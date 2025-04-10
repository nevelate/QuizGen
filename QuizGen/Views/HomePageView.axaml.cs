using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QuizGen.ViewModels;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System.Linq;
using System.Configuration;
using FluentAvalonia.UI.Controls;
using System.IO;

namespace QuizGen.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        Loaded += HomePageView_Loaded;
    }

    private void HomePageView_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HomePageViewModel vm) vm.TopLevel = TopLevel.GetTopLevel(this);

        if (AppSettings.GetSetting("Theme") != null)
        {
            (ThemeMenuItem.GetLogicalChildren().ElementAt(int.Parse(AppSettings.GetSetting("Theme"))) as MenuItem).IsChecked = true;
        }
        if (AppSettings.GetSetting("Backdrop") != null)
        {
            (BackdropMenuItem.GetLogicalChildren().ElementAt(int.Parse(AppSettings.GetSetting("Backdrop"))) as MenuItem).IsChecked = true;
        }
    }

    private void ShowTitleTip(object? sender, RoutedEventArgs e)
    {        
        TitleTip.IsOpen = true;
    }

    private void ShowGroupByTip(object? sender, RoutedEventArgs e)
    {
        GroupByTip.IsOpen = true;
    }

    private void ShowLongTestByTip(object? sender, RoutedEventArgs e)
    {
        LongTestTip.IsOpen = true;
    }

    private async void ShowAboutUserDialog(object? sender, RoutedEventArgs e)
    {
        var user = await TelegramClient.GetUser();
        await new ContentDialog()
        {
            Title = "Account info",
            Content = $"{user.FirstName} {user.LastName}\n" +
            $"@{user.Usernames.ActiveUsernames.FirstOrDefault()}\n+" +
            user.PhoneNumber,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "OK",
        }.ShowAsync();
    }

    private void LogOut(object? sender, RoutedEventArgs e)
    {
        TelegramClient.LogOut();
    }

    private void Exit(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void ShowAboutDialog(object? sender, RoutedEventArgs e)
    {
        new ContentDialog()
        {
            Title = "About",
            Content = "QuizGen by nevelate\n" +
            "Version 1.0",
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "OK",
        }.ShowAsync();
    }

    private void OpenGithubPage(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/nevelate/QuizGen");
    }

    private void OpenIssuesPage(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/nevelate/QuizGen/issues");
    }

    private void OpenPluginsFolder(object? sender, RoutedEventArgs e)
    {
        OpenUrl("file:///" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/QuizGen/Plugins");
    }

    private void ChangeBackdrop(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MenuItem menuItem)
        {
            var mainWindow = this.GetLogicalAncestors().First(l => l is MainWindow) as MainWindow;
            mainWindow?.ChangeTransparency((sender as Control).GetLogicalChildren().IndexOf(menuItem));
            AppSettings.SetSetting("Backdrop", (sender as Control).GetLogicalChildren().IndexOf(menuItem).ToString());
        }
    }

    private void ChangeTheme(object? sender, RoutedEventArgs e)
    {
        if (e.Source is MenuItem menuItem)
        {
            App.Current.RequestedThemeVariant = (sender as Control).GetLogicalChildren().IndexOf(menuItem) switch
            {
                1 => ThemeVariant.Default,
                2 => ThemeVariant.Light,
                3 => ThemeVariant.Dark,
            };
            AppSettings.SetSetting("Theme", (sender as Control).GetLogicalChildren().IndexOf(menuItem).ToString());
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}