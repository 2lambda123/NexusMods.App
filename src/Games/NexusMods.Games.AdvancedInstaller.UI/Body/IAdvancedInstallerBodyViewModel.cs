﻿using NexusMods.App.UI;

namespace NexusMods.Games.AdvancedInstaller.UI;

public interface IAdvancedInstallerBodyViewModel : IViewModel
{
    public IAdvancedInstallerModContentViewModel ModContentViewModel { get; }

    public IAdvancedInstallerPreviewViewModel PreviewSectionViewModel { get; }
}
