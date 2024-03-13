namespace DevToys.Tools.Models;

/// <summary>
/// Defines the different list comparaison mode
/// </summary>
internal enum ListComparisonMode
{
    /// <summary>
    /// Get elements which are presents in List A and List B
    /// </summary>
    AInterB,

    /// <summary>
    /// Get elements which are presents in List A or List B
    /// </summary>
    AUnionB,

    /// <summary>
    /// Get elements which are presents in List A but not in List B
    /// /// </summary>
    AOnly,

    /// <summary>
    ///Get elements which are presents in List B but not in List A
    /// </summary>
    BOnly
}
