using DevToys.Core.Tools.Metadata;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Tools;

[Export(typeof(SmartDetectionService))]
public sealed partial class SmartDetectionService
{
    private readonly ILogger _logger;
    private readonly IReadOnlyList<DetectorNode> _detectorHierarchy;
    private readonly Dictionary<string, List<GuiToolInstance>> _dataTypeToToolInstanceMap;

    [ImportingConstructor]
    public SmartDetectionService(
        [ImportMany] IEnumerable<Lazy<IDataTypeDetector, DataTypeDetectorMetadata>> dataTypeDetectors,
        ISettingsProvider settingsProvider,
        GuiToolProvider guiToolProvider)
    {
        _logger = this.Log();

        // Build a hierarchy of detectors based on their indicated base data type name.
        _detectorHierarchy = BuildDetectorNodeHierarchy(dataTypeDetectors);

        // Create a map of data types to tools.
        _dataTypeToToolInstanceMap = BuildDataTypeToToolInstanceMap(guiToolProvider, dataTypeDetectors);
    }

    /// <summary>
    /// Presumably the active tool in the app.
    /// </summary>
    public GuiToolInstance? ActiveToolInstance { private get; set; }

    /// <summary>
    /// Detects the best tools that could be used with the given <paramref name="rawData"/>.
    /// </summary>
    /// <param name="rawData">The raw data to use to detect the tools that could be used.</param>
    /// <param name="strict">When true, only returns tools that fit the best the given <paramref name="rawData"/>.
    /// When false, returns in priority the best tools that fit the given <paramref name="rawData"/>, then the first level of data type base that fit the <paramref name="rawData"/>.
    /// Example: Assuming the following data type dependencies: JWT-Header > JSON > Text. If <paramref name="rawData"/> is a JWT-Header and that <paramref name="strict"/> is true, only tools that strictly support JWT-Header data type will be return.
    /// if <paramref name="strict"/> is false, tools that support JWT-Header and JSON data types will be return, but tools that support Text won't be returned.</param>
    /// <returns>
    /// Assuming the following data type dependencies: JWT-Header > JSON > Text. If <paramref name="rawData"/> is a JWT-Header and that <paramref name="strict"/> is true, only tools that strictly support JWT-Header data type will be return.
    /// if <paramref name="strict"/> is false, tools that support JWT-Header and JSON data types will be return, but tools that support Text won't be returned.
    /// </returns>
    public async Task<IReadOnlyList<SmartDetectedTool>> DetectAsync(object? rawData, bool strict, CancellationToken cancellationToken)
    {
        var detectedTools = new List<SmartDetectedTool>();

        if (rawData == null)
        {
            return detectedTools;
        }

        // Make sure to be off the UI thread as this method may take many seconds to run.
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        GuiToolInstance? activeToolInstance = ActiveToolInstance;

        IReadOnlyList<(DetectorNode, DataDetectionResult)> succeededDetectors = await DetectAsync(cancellationToken, _detectorHierarchy, rawData);
        for (int i = 0; i < succeededDetectors.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            (DetectorNode detectorNode, DataDetectionResult dataDetectionResult) = succeededDetectors[i];

            // Add to the top of the list (high priority tool).
            if (_dataTypeToToolInstanceMap.TryGetValue(detectorNode.Detector.Metadata.DataTypeName, out List<GuiToolInstance>? toolList))
            {
                for (int j = 0; j < toolList.Count; j++)
                {
                    if (!strict && activeToolInstance is not null && string.Equals(toolList[j].InternalComponentName, activeToolInstance.InternalComponentName, StringComparison.Ordinal))
                    {
                        // When not strict, ignore the tool if it's the active one.
                        continue;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    detectedTools.Insert(0, new(toolList[j], detectorNode.Detector.Metadata.DataTypeName, dataDetectionResult.Data));
                }
            }

            if (!strict && !string.IsNullOrWhiteSpace(detectorNode.Detector.Metadata.DataTypeBaseName))
            {
                // Add to the end of the list (low priority tool).
                if (_dataTypeToToolInstanceMap.TryGetValue(detectorNode.Detector.Metadata.DataTypeBaseName, out toolList))
                {
                    for (int j = 0; j < toolList.Count; j++)
                    {
                        if (!strict && activeToolInstance is not null && string.Equals(toolList[j].InternalComponentName, activeToolInstance.InternalComponentName, StringComparison.Ordinal))
                        {
                            // When not strict, ignore the tool if it's the active one.
                            continue;
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                        detectedTools.Add(new(toolList[j], detectorNode.Detector.Metadata.DataTypeBaseName, dataDetectionResult.Data));
                    }
                }
            }
        }

        return detectedTools;
    }

    /// <summary>
    /// Detects data types from raw data using a hierarchy of detector nodes.
    /// </summary>
    /// <param name="detectorHierarchy">The hierarchy of detector nodes.</param>
    /// <param name="rawData">The raw data to detect data types from.</param>
    /// <param name="resultFromBaseDetector">The result from the base detector, if any.</param>
    /// <returns>A list of tuples containing a detector node and its corresponding detection result.</returns>
    private async Task<IReadOnlyList<(DetectorNode, DataDetectionResult)>> DetectAsync(
        CancellationToken cancellationToken,
        IReadOnlyList<DetectorNode> detectorHierarchy,
        object rawData,
        DataDetectionResult? resultFromBaseDetector = null)
    {
        var results = new List<(DetectorNode, DataDetectionResult)>();

        for (int i = 0; i < detectorHierarchy.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DetectorNode detectorNode = detectorHierarchy[i];

            // Detect data types from raw data using the current detector.
            DataDetectionResult resultFromDetector = await DetectAsync(detectorNode.Detector.Value, rawData, resultFromBaseDetector, cancellationToken);

            // If the detection was successful
            if (resultFromDetector.Success)
            {
                // If the current node has child detectors
                if (detectorNode.ChildrenDetectors?.Count > 0)
                {
                    // Recursively detect data types using child detectors
                    IReadOnlyList<(DetectorNode, DataDetectionResult)> resultFromDetectors
                        = await DetectAsync(
                            cancellationToken,
                            detectorNode.ChildrenDetectors,
                            rawData,
                            resultFromDetector);

                    // If no results were returned by child detectors
                    if (resultFromDetectors.Count == 0)
                    {
                        // Add the current detection result to the list of results
                        results.Add(new(detectorNode, resultFromDetector));
                    }
                    else
                    {
                        // Otherwise add all results returned by child detectors to the list of results
                        results.AddRange(resultFromDetectors);
                    }
                }
                else
                {
                    // If the current node does not have child detectors,
                    // add its detection result to the list of results
                    results.Add(new(detectorNode, resultFromDetector));
                }
            }
        }

        return results;
    }

    private async ValueTask<DataDetectionResult> DetectAsync(
        IDataTypeDetector detector,
        object rawData,
        DataDetectionResult? resultFromBaseDetector,
        CancellationToken cancellationToken)
    {
        try
        {
            return await detector.TryDetectDataAsync(rawData, resultFromBaseDetector, cancellationToken) ?? DataDetectionResult.Unsuccessful;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogDetectAsyncError(ex);
        }

        return DataDetectionResult.Unsuccessful;
    }

    /// <summary>
    /// Builds a hierarchy of detector nodes from a collection of detectors.
    /// </summary>
    /// <param name="detectors">The collection of detectors.</param>
    /// <returns>A list of root detector nodes.</returns>
    private List<DetectorNode> BuildDetectorNodeHierarchy(IEnumerable<Lazy<IDataTypeDetector, DataTypeDetectorMetadata>> detectors)
    {
        var result = new List<DetectorNode>();

        try
        {
            // Create a dictionary that maps data type names to detector nodes
            var lookup
                = detectors.ToDictionary(
                    node => node.Metadata.DataTypeName,
                    node => new DetectorNode(node));

            foreach (KeyValuePair<string, DetectorNode> node in lookup)
            {
                // Check if the current detector is supported by the current operating system
                if (!OSHelper.IsOsSupported(node.Value.Detector.Metadata.TargetPlatforms))
                {
                    // If not supported, ignore it.
                    Debug.WriteLine($"Ignoring '{node.Value.Detector.Metadata.DataTypeName}' data type detector as it isn't supported by the current OS.");
                }
                else if (string.IsNullOrEmpty(node.Value.Detector.Metadata.DataTypeBaseName))
                {
                    // If the current detector does not have a base data type name, add it to the result list as a root node
                    result.Add(node.Value);
                }
                else if (lookup.TryGetValue(node.Value.Detector.Metadata.DataTypeBaseName, out DetectorNode? parentNode))
                {
                    // If the current detector has a base data type name and its parent node exists in the dictionary,
                    // add it as a child to its parent node
                    parentNode.ChildrenDetectors ??= new();

                    parentNode.ChildrenDetectors.Add(node.Value);
                }
            }
        }
        catch (Exception ex)
        {
            LogBuildDetectorNodeHierarchyError(ex);
        }

        return result;
    }

    /// <summary>
    /// Builds a dictionary that maps data types to a list of GUI tool instances that accept them.
    /// </summary>
    /// <param name="guiToolProvider">The GUI tool provider.</param>
    /// <param name="detectors">The collection of detectors.</param>
    /// <returns>A dictionary that maps data types to a list of GUI tool instances that accept them.</returns>
    private Dictionary<string, List<GuiToolInstance>> BuildDataTypeToToolInstanceMap(
        GuiToolProvider guiToolProvider,
        IEnumerable<Lazy<IDataTypeDetector, DataTypeDetectorMetadata>> dataTypeDetectors)
    {
        // Create a new dictionary with case-insensitive string keys
        var dataTypeToToolInstanceMap = new Dictionary<string, List<GuiToolInstance>>(StringComparer.OrdinalIgnoreCase);

        // Iterate over all tools provided by the GUI tool provider
        for (int i = 0; i < guiToolProvider.AllTools.Count; i++)
        {
            GuiToolInstance tool = guiToolProvider.AllTools[i];

            // Iterate over all data types accepted by the current tool
            for (int j = 0; j < tool.AcceptedDataTypeNames.Count; j++)
            {
                string dataType = tool.AcceptedDataTypeNames[j];

                if (!string.IsNullOrWhiteSpace(dataType))
                {
                    if (dataTypeDetectors.Any(detector => string.Equals(detector.Metadata.DataTypeName, dataType, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        if (!dataTypeToToolInstanceMap.TryGetValue(dataType, out List<GuiToolInstance>? toolList))
                        {
                            toolList = new();
                            dataTypeToToolInstanceMap[dataType] = toolList;
                        }

                        toolList.Add(tool);
                    }
                    else
                    {
                        LogDataDetectorNotFound(tool.InternalComponentName, dataType);
                    }
                }
            }
        }

        return dataTypeToToolInstanceMap;
    }

    [LoggerMessage(0, LogLevel.Error, $"Error while running Smart Detection.")]
    partial void LogDetectAsyncError(Exception ex);

    [LoggerMessage(1, LogLevel.Error, $"Error while building data detectors.")]
    partial void LogBuildDetectorNodeHierarchyError(Exception ex);

    [LoggerMessage(2, LogLevel.Warning, "Unable to find the data type detector '{dataTypeName}' for the tool '{toolName}'.")]
    partial void LogDataDetectorNotFound(string toolName, string dataTypeName);

    private sealed class DetectorNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetectorNode"/> class with the specified detector.
        /// </summary>
        /// <param name="detector">The detector associated with this node.</param>
        internal DetectorNode(Lazy<IDataTypeDetector, DataTypeDetectorMetadata> detector)
        {
            Guard.IsNotNull(detector);
            Detector = detector;
        }

        /// <summary>
        /// Gets the detector associated with this node.
        /// </summary>
        internal Lazy<IDataTypeDetector, DataTypeDetectorMetadata> Detector { get; }

        /// <summary>
        /// Gets the list of child detectors for this node.
        /// </summary>
        internal List<DetectorNode>? ChildrenDetectors { get; set; }
    }
}
