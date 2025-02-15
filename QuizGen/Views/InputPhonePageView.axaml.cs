using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace QuizGen.Views;

public partial class InputPhonePageView : UserControl
{
    public InputPhonePageView()
    {
        InitializeComponent();
        Loaded += InputPhonePageView_Loaded;
    }

    private void InputPhonePageView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TelephoneBox.Focus();
    }
}