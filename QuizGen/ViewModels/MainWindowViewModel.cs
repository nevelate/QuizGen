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
        private ViewModelBase currentPage;

        public MainWindowViewModel()
        {
            TelegramClient.OnAuthorizationStateChangedEvent += OnClientAuthorizationStateChanged;
        }

        [RelayCommand]
        private void Identificate(string? phoneNumber)
        {
            TelegramClient.SetPhoneNumber(phoneNumber);
        }

        [RelayCommand]
        private void Authentificate(string? code)
        {
            TelegramClient.CheckCode(code);
        }

        [RelayCommand]
        private void TwoFactorCheck(string? password)
        {
            TelegramClient.CheckPassword(password);
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
