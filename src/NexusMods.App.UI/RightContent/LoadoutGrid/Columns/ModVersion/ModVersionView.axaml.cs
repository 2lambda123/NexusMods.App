﻿using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace NexusMods.App.UI.RightContent.LoadoutGrid.Columns.ModVersion;

public partial class ModVersionView : ReactiveUserControl<IModVersionViewModel>
{
    public ModVersionView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(view => view.ViewModel!.Version)
                .BindToUi<string, ModVersionView, string>(this, view => view.VersionTextBlock.Text)
                .DisposeWith(d);
        });
    }
}

