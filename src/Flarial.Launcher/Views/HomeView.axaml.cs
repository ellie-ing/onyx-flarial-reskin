using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Labs.Gif;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public sealed partial class HomeView : UserControl
{
    bool _launchArmed;
    HomeViewModel? _viewModel;

    public HomeView()
    {
        InitializeComponent();
        BackgroundGif.Source = GifStreamSource.FromUriString("avares://Flarial.Launcher/Resources/background.gif");

        // The launching overlay may only appear once the user actually presses Launch.
        // IsLaunching also starts out true while the launcher initializes at boot, so
        // binding the overlay straight to it flashed the launching screen on startup.
        LaunchButton.Click += (_, _) =>
        {
            // Arm the latch WITHOUT running UpdateLaunchingOverlay here: the
            // command may not have flipped IsLaunching yet, and the update
            // would disarm the latch again before the launch even starts.
            _launchArmed = true;

            if (_viewModel?.IsLaunching is true)
                LaunchingOverlay.Classes.Add("show");
        };

        DataContextChanged += (_, _) =>
        {
            if (_viewModel is not null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _viewModel = DataContext as HomeViewModel;

            if (_viewModel is not null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            UpdateLaunchingOverlay();
        };
    }

    void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(HomeViewModel.IsLaunching))
            UpdateLaunchingOverlay();
    }

    void UpdateLaunchingOverlay()
    {
        var launching = _viewModel?.IsLaunching ?? false;

        if (launching && _launchArmed)
            LaunchingOverlay.Classes.Add("show");
        else
            LaunchingOverlay.Classes.Remove("show");

        if (!launching)
            _launchArmed = false;
    }
}
