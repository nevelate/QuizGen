using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.UI.Windowing;
using System.Collections.Generic;
using System.Configuration;

namespace QuizGen.Views
{
    public partial class MainWindow : AppWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            Loaded += MainWindow_Loaded;                     
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var appSettings = ConfigurationManager.AppSettings;

            if (appSettings["Theme"] != null)
            {
                App.Current.RequestedThemeVariant = int.Parse(appSettings["Theme"]) switch
                {
                    1 => ThemeVariant.Default,
                    2 => ThemeVariant.Light,
                    3 => ThemeVariant.Dark,
                };
            }

            if (appSettings["Backdrop"] != null)
            {
                ChangeTransparency(int.Parse(appSettings["Backdrop"]));
            }
            else
            {
                ChangeTransparency(2);
            }
        }

        public void ChangeTransparency(int index)
        {
            switch (index)
            {
                case 1: // Default
                    if (this.TryFindResource("ApplicationPageBackgroundThemeBrush", App.Current?.ActualThemeVariant, out var value))
                    {
                        var brush = value as SolidColorBrush;
                        Background = brush;
                    }
                    TransparencyLevelHint = new List<WindowTransparencyLevel>()
                    {
                        WindowTransparencyLevel.None,
                    };
                    AccentAcrylicBorder.IsVisible = false;
                    AcrylicBorder.IsVisible = false;
                    break;
                case 2: // Mica
                    Background = Brushes.Transparent;
                    TransparencyLevelHint = new List<WindowTransparencyLevel>()
                        {
                            WindowTransparencyLevel.Mica,
                        };
                    AccentAcrylicBorder.IsVisible = false;
                    AcrylicBorder.IsVisible = false;
                    break;
                case 3: // Acrylic
                    Background = Brushes.Transparent;
                    TransparencyLevelHint = new List<WindowTransparencyLevel>()
                        {
                            WindowTransparencyLevel.AcrylicBlur,
                        };
                    AccentAcrylicBorder.IsVisible = false;
                    AcrylicBorder.IsVisible = true;
                    break;
                case 4: // Acrylic (Accent)
                    Background = Brushes.Transparent;
                    TransparencyLevelHint = new List<WindowTransparencyLevel>()
                        {
                            WindowTransparencyLevel.AcrylicBlur,
                        };
                    AccentAcrylicBorder.IsVisible = true;
                    AcrylicBorder.IsVisible = false;
                    break;
            }
        }
    }
}