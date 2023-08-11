using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using JetBrains.Annotations;
using MessageBox.Avalonia;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Scarab.Interfaces;
using Scarab.Util;

namespace Scarab.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [Notify]
    private readonly ISettings _settings;
    private readonly IModSource _mods;

    public static string[] Languages => new[] {
        "en-US",
        "fr",
        "hu-HU",
        "pt-BR",
        "zh"
    };

    public ReactiveCommand<Unit, Unit> ChangePath { get; }

    [Notify]
    private string? _selected;

    public SettingsViewModel(ISettings settings, IModSource mods)
    {
        _settings = settings;
        _mods = mods;
        
        Selected = settings.PreferredCulture;

        ChangePath = ReactiveCommand.CreateFromTask(ChangePathAsync);

        this.WhenAnyValue(x => x.Selected)
            .Subscribe(item =>
            {
                if (string.IsNullOrEmpty(item))
                    return;

                settings.PreferredCulture = item;
                
                settings.Apply();
                settings.Save();
            });
    }
    
    private async Task ChangePathAsync()
    {
        string? path = await PathUtil.SelectPathFallible();

        if (path is null)
            return;

        _settings.ManagedFolder = path;
        _settings.Save();

        await _mods.Reset();

        await MessageBoxManager.GetMessageBoxStandardWindow(Resources.MLVM_ChangePathAsync_Msgbox_Title,
            Resources.MLVM_ChangePathAsync_Msgbox_Text).Show();

        // Shutting down is easier than re-doing the source and all the items.
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

}