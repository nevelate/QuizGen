using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net;
using System.Threading.Tasks;
using Telegram.Td.Api;

using Td = Telegram.Td;

namespace QuizGen.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? currentPage;

        public MainWindowViewModel()
        {
            TelegramClient.OnAuthorizationStateChangedEvent += OnClientAuthorizationStateChanged;
        }

        [RelayCommand]
        private async Task Identificate(string phoneNumber)
        {
            await TelegramClient.SetPhoneNumber(phoneNumber);
        }

        [RelayCommand]
        private async Task Authentificate(string code)
        {
            await TelegramClient.CheckCode(code);
        }

        [RelayCommand]
        private async Task TwoFactorCheck(string password)
        {
            await TelegramClient.CheckPassword(password);
        }

        private void OnClientAuthorizationStateChanged(AuthorizationState state)
        {
            if(state is AuthorizationStateWaitPhoneNumber || state is AuthorizationStateClosed)
            {
                CurrentPage = new InputPhonePageViewModel();
            }
            else if (state is AuthorizationStateWaitCode)
            {
                CurrentPage = new CheckCodePageViewModel();
            }
            else if (state is AuthorizationStateWaitPassword)
            {
                CurrentPage = new TwoFAPageViewModel();
            }
            else if (state is AuthorizationStateReady)
            {
                CurrentPage = new HomePageViewModel();
            }
        }
    }
}
