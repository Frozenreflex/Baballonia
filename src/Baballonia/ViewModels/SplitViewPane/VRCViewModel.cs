using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls;
using Baballonia.Contracts;
using Baballonia.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Baballonia.ViewModels.SplitViewPane;

public partial class VrcViewModel : ViewModelBase
{
    public ILocalSettingsService LocalSettingsService { get; }

    [ObservableProperty]
    [property: SavedSetting("VRC_UseNativeTracking", false)]
    private bool _useNativeVrcEyeTracking;

    [ObservableProperty]
    [property: SavedSetting("VRC_SelectedModuleMode", "Face")]
    private string? _selectedModuleMode;

    public VrcViewModel()
    {
        LocalSettingsService = Ioc.Default.GetRequiredService<ILocalSettingsService>();
        LocalSettingsService.Load(this);

        PropertyChanged += (_, _) =>
        {
            LocalSettingsService.Save(this);
        };
    }
}
