using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QuizGen.ViewModels;

namespace QuizGen.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        Loaded += HomePageView_Loaded;
    }

    private void HomePageView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(DataContext is HomePageViewModel vm) vm.TopLevel = TopLevel.GetTopLevel(this);
    }

    private void ShowTeachingTip(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InfoTip.IsOpen = true;
    }
}