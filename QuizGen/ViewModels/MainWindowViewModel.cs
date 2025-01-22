using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net;
using System.Threading.Tasks;

using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

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

        private void OnClientAuthorizationStateChanged(TdApi.AuthorizationState state)
        {
            if(state is TdApi.AuthorizationStateWaitPhoneNumber || state is TdApi.AuthorizationStateClosed)
            {
                CurrentPage = new InputPhonePageViewModel();
            }
            else if (state is TdApi.AuthorizationStateWaitCode)
            {
                CurrentPage = new CheckCodePageViewModel();
            }
            else if (state is TdApi.AuthorizationStateWaitPassword)
            {
                CurrentPage = new TwoFAPageViewModel();
            }
            else if (state is TdApi.AuthorizationStateReady)
            {
                CurrentPage = new HomePageViewModel();
            }
        }
    }
}
