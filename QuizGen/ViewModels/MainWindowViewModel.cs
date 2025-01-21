using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Telegram.Td;

namespace QuizGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
#pragma warning disable CA1822 // Mark members as static
        public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

        [RelayCommand]
        private async Task Identificate(string? telephoneNumber)
        {
            await Task.Delay(500);
        }

        [RelayCommand]
        private async Task Authentificate(string? code)
        {
            await Task.Delay(500);
        }

        [RelayCommand]
        private async Task TwoFactorCheck(string? password)
        {
            await Task.Delay(500);
        }
    }
}
