using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;
using System.Threading.Tasks;
using Telegram.Td;

namespace QuizGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase currentPage;

        public MainWindowViewModel()
        {
            
        }

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
