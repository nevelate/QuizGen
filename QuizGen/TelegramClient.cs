using QuizGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

namespace QuizGen
{
    internal static class TelegramClient
    {
        private static Td.Client? _client = null;

        private static TdApi.AuthorizationState? _authorizationState = null;

        public static Action<TdApi.AuthorizationState> OnAuthorizationStateChangedEvent;
        public static Action<long, List<TdApi.KeyboardButton>> OnShowKeyboardEvent;

        static TelegramClient()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Td.Client.Run();
            }).Start();

            _client = Td.Client.Create(new UpdateHandler());
        }

        public static void SendMessage(long chatId, string message)
        {
            TdApi.InputMessageContent content = new TdApi.InputMessageText(new TdApi.FormattedText(message, null), null, true);
            _client.Send(new TdApi.SendMessage(chatId, 0, null, null, null, content), null);
        }

        public static void SendPoll(long chatId, Test test)
        {
            TdApi.InputMessagePoll poll = new TdApi.InputMessagePoll(test.Question, test.Answers.ToArray(), false, new TdApi.PollTypeQuiz(), 0, 0, false);

            _client.Send(new TdApi.SendMessage(chatId, 0, null, null, null, poll), null);
        }

        public static void SetPhoneNumber(string phoneNumber)
        {
            _client.Send(new TdApi.SetAuthenticationPhoneNumber(phoneNumber, null), new AuthorizationRequestHandler());
        }

        public static void CheckCode(string code)
        {
            _client.Send(new TdApi.CheckAuthenticationCode(code) , new AuthorizationRequestHandler());
        }

        public static void CheckPassword(string password)
        {
            _client.Send(new TdApi.CheckAuthenticationPassword(password), new AuthorizationRequestHandler());
        }

        public static void LogOut()
        {
            _client.Send(new TdApi.LogOut(), null);
        }

        private static void OnAuthorizationStateUpdated(TdApi.AuthorizationState authorizationState)
        {
            if (authorizationState != null)
            {
                _authorizationState = authorizationState;
            }
            if (_authorizationState is TdApi.AuthorizationStateWaitTdlibParameters)
            {
                TdApi.SetTdlibParameters request = new TdApi.SetTdlibParameters();
                request.DatabaseDirectory = "tdlib_db";

                request.UseMessageDatabase = true;
                request.UseSecretChats = false;
                request.ApiId = Secrets.Api_id;
                request.ApiHash = Secrets.Api_hash;
                request.SystemLanguageCode = "en";
                request.DeviceModel = "Desktop";
                request.ApplicationVersion = "1.0";

                _client.Send(request, new AuthorizationRequestHandler());
            }
            else
            {
                OnAuthorizationStateChangedEvent?.Invoke(_authorizationState);
            }
        }

        private class UpdateHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object is TdApi.UpdateAuthorizationState state)
                {
                    OnAuthorizationStateUpdated(state.AuthorizationState);
                }
                if (@object is TdApi.UpdateNewMessage updateNewMessage)
                {
                    var message = updateNewMessage.Message;

                    if (message.ReplyMarkup is TdApi.ReplyMarkupShowKeyboard showKeyboard)
                    {
                        var keyboard = new List<TdApi.KeyboardButton>();

                        foreach(var row in showKeyboard.Rows)
                        {
                            foreach(var keyboardButton in row)
                            {
                                keyboard.Add(keyboardButton);
                            }
                        }

                        OnShowKeyboardEvent?.Invoke(message.ChatId, keyboard);
                    }
                }
            }
        }

        private class AuthorizationRequestHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                if (@object is TdApi.Error)
                {
                    OnAuthorizationStateUpdated(null); // repeat last action
                }
            }
        }
    }
}
