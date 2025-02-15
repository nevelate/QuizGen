using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QuizGen.Views;

public partial class CheckCodePageView : UserControl
{
    public CheckCodePageView()
    {
        InitializeComponent();
        Loaded += CheckCodePageView_Loaded;
    }

    private void CheckCodePageView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CodeBox.Focus();
    }
}