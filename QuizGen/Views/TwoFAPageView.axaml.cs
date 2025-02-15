using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QuizGen.Views;

public partial class TwoFAPageView : UserControl
{
    public TwoFAPageView()
    {
        InitializeComponent();
        Loaded += TwoFAPageView_Loaded;
    }

    private void TwoFAPageView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TwoFactorBox.Focus();
    }
}