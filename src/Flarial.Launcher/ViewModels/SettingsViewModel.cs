namespace Flarial.Launcher.ViewModels;

public sealed class SettingsViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    // Lets the settings chrome (header) bind the same account/version data as the home screen.
    public HomeViewModel HomeViewModel => mainWindowViewModel.HomeViewModel;

    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new(mainWindowViewModel);
}