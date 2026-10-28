using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Flarial.Launcher.Views;

// ReSharper disable once PartialTypeWithSinglePart
public sealed partial class MainWindow : Window
{
    public static Canvas? ToolTipLayerInstance { get; private set; }

    readonly App _application;
    readonly AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        _application = (App)Application.Current!;
        _settings = _application.Settings;

        ToolTipLayerInstance = ToolTipLayer;
        WindowDecorations = WindowDecorations.None;
        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaTitleBarHeightHint = -1;

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(static e => e == WindowStateArgs.Minimize)
            .Subscribe(_ => WindowState = WindowState.Minimized);

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(static e => e == WindowStateArgs.Close)
            .Subscribe(_ => Close());

        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);

        /*
            - Prevents external apps from messing up the launcher's size and position.
            - For devices with handheld frontends, this prevents the launcher from misbehaving.
        */

        this.GetObservable(WindowStateProperty).Subscribe(_ =>
        {
            if (_ is WindowState.Minimized) return;
            else WindowState = WindowState.Normal;
        });
    }

    void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        try { BeginMoveDrag(args); }
        catch { }
    }

    bool _pageTransitionRunning;

    private async void PageTransition(PageTransitions page)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // One page transition at a time: messages that arrive while a swap is still
        // running are dropped, so spam-clicking a nav button can neither restart the
        // running transition nor queue up extra ones behind it.
        if (_pageTransitionRunning) return;
        _pageTransitionRunning = true;

        vm.IsAnimating = true;

        // Shield the whole window from clicks while a transition runs: the wipe slabs
        // are hit-test visible only for the duration of the swap.
        PageWipeGrid.IsHitTestVisible = true;

        try
        {
            await RunPageTransition(page);
        }
        finally
        {
            PageWipeGrid.IsHitTestVisible = false;
            vm.IsAnimating = false;
            _pageTransitionRunning = false;
        }
    }

    private async Task RunPageTransition(PageTransitions page)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var tasks = new List<Task>();
        Animation? homeViewAnimation;
        Animation? settingsViewAnimation;

        switch (page)
        {
            case PageTransitions.SettingsPage:
                if (_settings.PerformanceMode)
                {
                    homeViewAnimation = (Animation?)_application.Resources["PerformanceHomePageLeaveTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["PerformanceSettingsPageEnterTransition"];
                }
                else
                {
                    homeViewAnimation = (Animation?)_application.Resources["HomePageLeaveTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["SettingsPageEnterTransition"];
                }
                break;
            case PageTransitions.HomePage:
                if (_settings.PerformanceMode)
                {
                    homeViewAnimation = (Animation?)_application.Resources["PerformanceHomePageEnterTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["PerformanceSettingsPageLeaveTransition"];
                }
                else
                {
                    homeViewAnimation = (Animation?)_application.Resources["HomePageEnterTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["SettingsPageLeaveTransition"];
                }
                break;
            case PageTransitions.SettingsGeneralPage:
            case PageTransitions.SettingsVersionsPage:
            case PageTransitions.SettingsConfigsPage:
            default:
                vm.IsAnimating = false;
                return;
        }

        if (homeViewAnimation is not null)
            tasks.Add(homeViewAnimation.RunAsync(HomeViewControl));

        if (settingsViewAnimation is not null)
            tasks.Add(settingsViewAnimation.RunAsync(SettingsViewControl));

        // The ONYX wipe: skewed slabs sweep over the swap (skipped in performance mode,
        // where the page animations above are already instant).
        if (!_settings.PerformanceMode)
        {
            if (_application.Resources["PageWipeWhiteTransition"] is Animation wipeWhite)
                tasks.Add(wipeWhite.RunAsync(WipeWhite));

            if (_application.Resources["PageWipeDarkTransition"] is Animation wipeDark)
                tasks.Add(wipeDark.RunAsync(WipeDark));
        }

        await Task.WhenAll(tasks);

        vm.IsAnimating = false;
    }

    async void OnLoaded(object? sender, RoutedEventArgs args)
    {
        Loaded -= OnLoaded;
        ((MainWindowViewModel)DataContext!).OnLoaded();
    }

}