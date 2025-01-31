using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Td.Api;
using TestParser;
using Td = Telegram.Td;

namespace QuizGen
{
    internal static class TelegramClient
    {
        private static Td.Client _client = null!;
        private const long quizbotId = 983000232;

        private static AuthorizationState? _authorizationState = null;

        public static event Action<long>? OnReceivedMessageEvent;
        public static event Action<AuthorizationState>? OnAuthorizationStateChangedEvent;
        public static event Action<long, List<KeyboardButton>>? OnShowKeyboardEvent;

        static TelegramClient()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Td.Client.Run();
            }).Start();

            _client = Td.Client.Create(new UpdateHandler());
        }

        public static Task SendMessage(long chatId, string message)
        {
            var tcs = new TaskCompletionSource();

            void Handler(long chatId)
            {
                if (chatId == quizbotId)
                {
                    tcs.SetResult();
                    OnReceivedMessageEvent -= Handler;
                }
            }

            InputMessageContent content = new InputMessageText(new FormattedText(message, null), null, true);

            OnReceivedMessageEvent += Handler;
            _client.Send(new SendMessage(chatId, 0, null, null, null, content), null);

            return tcs.Task;
        }

        public static Task SendPoll(long chatId, Test test)
        {
            var tcs = new TaskCompletionSource();

            void Handler(long chatId)
            {
                if (chatId == quizbotId)
                {
                    tcs.SetResult();
                    OnReceivedMessageEvent -= Handler;
                }
            }

            InputMessagePoll poll = new InputMessagePoll(
                new FormattedText(test.Question, null),
                test.Answers.Select(s => new FormattedText(s, null)).ToArray(),
                false,
                new PollTypeQuiz(),
                0,
                0,
                false);

            OnReceivedMessageEvent += Handler;
            _client.Send(new SendMessage(chatId, 0, null, null, null, poll), null);

            return tcs.Task;
        }

        public static void SetPhoneNumber(string phoneNumber)
        {
            _client.Send(new SetAuthenticationPhoneNumber(phoneNumber, null), new AuthorizationRequestHandler());
        }

        public static void CheckCode(string code)
        {
            _client.Send(new CheckAuthenticationCode(code), new AuthorizationRequestHandler());
        }

        public static void CheckPassword(string password)
        {
            _client.Send(new CheckAuthenticationPassword(password), new AuthorizationRequestHandler());
        }

        public static void LogOut()
        {
            _client.Send(new LogOut(), null);
        }

        private static void OnAuthorizationStateUpdated(AuthorizationState authorizationState)
        {
            if (authorizationState != null)
            {
                _authorizationState = authorizationState;
            }
            if (_authorizationState is AuthorizationStateWaitTdlibParameters)
            {
                SetTdlibParameters request = new SetTdlibParameters();
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
            void Td.ClientResultHandler.OnResult(BaseObject @object)
            {
                if (@object is UpdateAuthorizationState state)
                {
                    OnAuthorizationStateUpdated(state.AuthorizationState);
                }
                if (@object is UpdateNewMessage updateNewMessage)
                {
                    var message = updateNewMessage.Message;

                    if (!updateNewMessage.Message.IsOutgoing)
                    {
                        OnReceivedMessageEvent?.Invoke(message.ChatId);
                    }

                    if (message.ReplyMarkup is ReplyMarkupShowKeyboard showKeyboard)
                    {
                        var keyboard = new List<KeyboardButton>();

                        foreach (var row in showKeyboard.Rows)
                        {
                            foreach (var keyboardButton in row)
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
            void Td.ClientResultHandler.OnResult(BaseObject @object)
            {
                if (@object is Error)
                {
                    OnAuthorizationStateUpdated(null); // repeat last action
                }
            }
        }
    }
}
