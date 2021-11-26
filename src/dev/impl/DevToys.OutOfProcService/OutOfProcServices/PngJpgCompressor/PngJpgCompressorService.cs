using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.OutOfProcService.API.Core.OOP;
using DevToys.OutOfProcService.Core;
using DevToys.Shared.AppServiceMessages.PngJpgCompressor;
using DevToys.Shared.Core;
using Windows.Storage;

namespace DevToys.OutOfProcService.OutOfProcServices.PngJpgCompressor
{
    [Export(typeof(IOutOfProcService))]
    [InputType(typeof(PngJpgCompressorWorkMessage))]
    internal class PngJpgCompressorService : OutOfProcServiceBase<PngJpgCompressorWorkMessage, PngJpgCompressorWorkResultMessage>
    {
        protected override async Task<PngJpgCompressorWorkResultMessage> ProcessMessageAsync(PngJpgCompressorWorkMessage inputMessage, CancellationToken cancellationToken)
        {
            string ectFullPath = Path.Combine(Constants.AssetsFolderFullPath, Constants.EfficientCompressionToolFileName);
            Assumes.IsTrue(File.Exists(ectFullPath), nameof(ectFullPath));

            Arguments.NotNull(inputMessage, nameof(inputMessage));
            Arguments.NotNullOrWhiteSpace(inputMessage.FilePath, nameof(inputMessage.FilePath));
            Assumes.IsTrue(File.Exists(inputMessage.FilePath), nameof(inputMessage.FilePath));

            string? inputFileExtension = Path.GetExtension(inputMessage.FilePath);
            Assumes.NotNullOrWhiteSpace(inputFileExtension, nameof(inputFileExtension));

            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            string localCacheFolderFullPath = localCacheFolder.Path;

            string tempFilePath = Path.Combine(localCacheFolderFullPath, inputMessage.MessageId!.ToString()! + inputFileExtension);

            File.Copy(inputMessage.FilePath, tempFilePath, true);

            try
            {
                var oldFileInfo = new FileInfo(tempFilePath);
                long fileSizeBeforeCompression = oldFileInfo.Length;

                var ectProcessStartInfo = new ProcessStartInfo(ectFullPath)
                {
                    Arguments = $"-5 --allfilters-c \"{tempFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var output = new StringBuilder();
                var errors = new StringBuilder();
                using (Process ectProcess = Process.Start(ectProcessStartInfo)!)
                {
                    ectProcess.OutputDataReceived += (s, e) =>
                    {
                        lock (output)
                        {
                            if (e.Data is not null)
                            {
                                output.AppendLine(e.Data);
                            }
                        }
                    };

                    ectProcess.ErrorDataReceived += (s, e) =>
                    {
                        lock (errors)
                        {
                            if (e.Data is not null)
                            {
                                errors.AppendLine(e.Data);
                            }
                        }
                    };

                    ectProcess.BeginErrorReadLine();
                    ectProcess.BeginOutputReadLine();

                    try
                    {
                        await ectProcess.WaitForExitAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        ectProcess.Kill();
                        throw;
                    }
                }

                var newFileInfo = new FileInfo(tempFilePath);
                long fileSizeAfterCompression = newFileInfo.Length;

                return new PngJpgCompressorWorkResultMessage
                {
                    ErrorMessage = errors.ToString(),
                    NewFileSize = fileSizeAfterCompression,
                    PercentageSaved = 1.0 - (fileSizeAfterCompression / (double)fileSizeBeforeCompression),
                    TempCompressedFilePath = tempFilePath
                };
            }
            catch (Exception)
            {
                // cleanup
                File.Delete(tempFilePath);
                throw;
            }
        }
    }
}
