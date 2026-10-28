using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Flarial.Launcher.Views;

public sealed partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        BackgroundGif.Source = GifStreamSource.FromUriString("avares://Flarial.Launcher/Resources/background.gif");
        MessageBus.Current.Listen<PageTransitions>().Subscribe(PageTransition);
    }

    static void RestartClass(Control control, string cls)
    {
        control.Classes.Remove(cls);
        Dispatcher.UIThread.Post(() => control.Classes.Add(cls));
    }

    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    private double _currentPageY;

    private async void PageTransition(PageTransitions page)
    {
        // Entrance choreography for the whole options screen (sidebar + panel slide in).
        // Only (re)started on a genuine arrival: if the class is already on, this message
        // is a repeat (spam click) and the entrance must not restart mid-flight.
        if (page is PageTransitions.SettingsPage)
        {
            if (!RootGrid.Classes.Contains("enter"))
                Dispatcher.UIThread.Post(() => RootGrid.Classes.Add("enter"));
        }
        else if (page is PageTransitions.HomePage)
        {
            RootGrid.Classes.Remove("enter");
        }

        var selectedPageY = page switch
        {
            PageTransitions.SettingsGeneralPage => 0,
            PageTransitions.SettingsVersionsPage => 327,
            PageTransitions.SettingsConfigsPage => 654,
            _ => _currentPageY
        };

        if (Math.Abs(selectedPageY - _currentPageY) < 1)
            return;

        if (DataContext is not SettingsViewModel settingsViewModel) return;

        // One sub-page switch at a time: extra tab clicks that arrive while a switch
        // is still animating are dropped instead of restarting/queueing animations.
        // The clicked radio has already checked itself by now, so restore the check
        // to the page that is actually showing.
        if (settingsViewModel.IsAnimating)
        {
            GeneralPageButton.IsChecked = Math.Abs(_currentPageY - 0) < 1;
            VersionsPageButton.IsChecked = Math.Abs(_currentPageY - 327) < 1;
            return;
        }
        settingsViewModel.IsAnimating = true;

        PageTitle.Text = page switch
        {
            PageTransitions.SettingsGeneralPage => "GENERAL",
            PageTransitions.SettingsVersionsPage => "VERSIONS",
            PageTransitions.SettingsConfigsPage => "CONFIGS",
            _ => PageTitle.Text
        };
        RestartClass(PageTitle, "tj");
        RestartClass(TitleRule, "draw");

        UserControlGrid.IsEnabled = false;

        if (UserControlGrid.RenderTransform is not ScaleTransform) return;

        var generalMove = CreateMove(0 - selectedPageY);
        var versionsMove = CreateMove(327 - selectedPageY);
        var configsMove = CreateMove(654 - selectedPageY);

        if (_settings.PerformanceMode)
        {
            _ = generalMove.RunAsync(SettingsGeneralViewControl);
            _ = versionsMove.RunAsync(SettingsVersionsViewControl);
            _ = configsMove.RunAsync(SettingsConfigsViewControl);
        }
        else
        {
            var zoomOut = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.2),
                Easing = new QuadraticEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 0.9),
                            new Setter(ScaleTransform.ScaleYProperty, 0.9),
                        }
                    }
                }
            };

            var zoomIn = new Animation
            {
                Delay = TimeSpan.FromMilliseconds(500),
                Duration = TimeSpan.FromSeconds(0.2),
                Easing = new QuadraticEaseIn(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1.0),
                            new Setter(ScaleTransform.ScaleYProperty, 1.0),
                        }
                    }
                }
            };

            _ = zoomOut.RunAsync(UserControlGrid);
            _ = generalMove.RunAsync(SettingsGeneralViewControl);
            _ = versionsMove.RunAsync(SettingsVersionsViewControl);
            _ = configsMove.RunAsync(SettingsConfigsViewControl);
            _ = zoomIn.RunAsync(UserControlGrid);

            // The incoming pane fades in as it lands (the mock's paneIn).
            Control incoming = page switch
            {
                PageTransitions.SettingsVersionsPage => SettingsVersionsViewControl,
                PageTransitions.SettingsConfigsPage => SettingsConfigsViewControl,
                _ => SettingsGeneralViewControl
            };
            var paneIn = new Animation
            {
                Delay = TimeSpan.FromMilliseconds(250),
                Duration = TimeSpan.FromSeconds(0.35),
                Easing = new CubicEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 0.0) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, 1.0) } }
                }
            };
            _ = paneIn.RunAsync(incoming);

            await Task.Delay(700);
        }

        UserControlGrid.IsEnabled = true;
        _currentPageY = selectedPageY;
        settingsViewModel.IsAnimating = false;
        return;

        Animation CreateMove(double toY) => new()
        {
            Delay = TimeSpan.FromMilliseconds(_settings.PerformanceMode ? 0 : 250),
            Duration = TimeSpan.FromSeconds(_settings.PerformanceMode ? 0 : 0.2),
            Easing = new QuadraticEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(TranslateTransform.YProperty, toY) }
                }
            }
        };
    }
}