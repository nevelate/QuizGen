using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QuizGen.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

    private void ShowTeachingTip(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        InfoTip.IsOpen = true;
    }
}