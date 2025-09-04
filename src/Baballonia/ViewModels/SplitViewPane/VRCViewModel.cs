using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Baballonia.Contracts;
using Baballonia.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Baballonia.ViewModels.SplitViewPane;

public partial class VrcViewModel : ViewModelBase
{
    private static readonly string BaballoniaModulePath;

    static VrcViewModel()
    {
        var moduleFiles = Directory.EnumerateFiles(Utils.CustomLibsDirectory, "*.json");
        foreach (var moduleFile in moduleFiles)
        {
            var contents = File.ReadAllText(moduleFile);
            var possibleBabbleConfig = JsonSerializer.Deserialize<ModuleConfig>(contents);
            if (possibleBabbleConfig != null)
            {
                BaballoniaModulePath = moduleFile;
            }
        }
    }

    [ObservableProperty]
    [property: SavedSetting("VRC_UseNativeTracking", false)]
    private bool _useNativeVrcEyeTracking;

    [ObservableProperty]
    [property: SavedSetting("VRC_SelectedModuleMode", "Both")]
    private string? _selectedModuleMode;

    partial void OnSelectedModuleModeChanged(string? oldValue, string? newValue)
    {
        if (string.IsNullOrEmpty(BaballoniaModulePath)) return;

        var oldConfig = JsonSerializer.Deserialize<ModuleConfig>(File.ReadAllText(BaballoniaModulePath));
        var newConfig = newValue switch
        {
            "Both" => new ModuleConfig(oldConfig!.Host, oldConfig.Port, true, true),
            "Eyes" => new ModuleConfig(oldConfig!.Host, oldConfig.Port, true, false),
            "Face" => new ModuleConfig(oldConfig!.Host, oldConfig.Port, false, true),
            _ => throw new InvalidOperationException()
        };

        File.WriteAllText(BaballoniaModulePath, JsonSerializer.Serialize(newConfig));
    }
}
