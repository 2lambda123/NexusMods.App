namespace NexusMods.DataModel.Games;

/// <summary>
/// When implemented, enables support for detecting installations of your
/// <see cref="AGame"/> managed by EA Desktop (Origin Successor) launcher.
/// </summary>
/// <remarks>
/// Game detection is automatic provided the correct <see cref="EADesktopSoftwareIDs"/>
/// is applied.
/// </remarks>
// ReSharper disable once InconsistentNaming
public interface IEADesktopGame : IGame
{
    /// <summary>
    /// IDs for this game used in the 'EA Desktop' application.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public IEnumerable<string> EADesktopSoftwareIDs { get; }
}
