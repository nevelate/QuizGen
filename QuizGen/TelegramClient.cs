using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Td;
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

        private static event Action? OnReceivedMessageEvent;
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

        public static Task<User> GetUser()
        {
            var tcs = new TaskCompletionSource<User>();
            _client.Send(new GetMe(), new GetUserHandler(tcs));
            return tcs.Task;
        }

        public static Task SendMessage(long chatId, string message)
        {
            var tcs = new TaskCompletionSource();
            void Handler()
            {

                OnReceivedMessageEvent -= Handler;
                tcs.SetResult();
            }
            OnReceivedMessageEvent += Handler;

            InputMessageContent content = new InputMessageText(new FormattedText(message, null), null, true);

            _client.Send(new SendMessage(chatId, 0, null, null, null, content), null);

            return tcs.Task;
        }

        public static Task SendPoll(long chatId, Test test)
        {
            var tcs = new TaskCompletionSource();
            void Handler()
            {
                OnReceivedMessageEvent -= Handler;
                tcs.SetResult();
            }
            OnReceivedMessageEvent += Handler;

            var answers = new List<FormattedText> { new FormattedText(test.CorrectAnswer, []) };
            answers.AddRange(test.OtherAnswers.Select(t => new FormattedText(t, [])));

            InputMessagePoll poll = new InputMessagePoll(
                new FormattedText(test.Question, []),
                answers.ToArray(),
                false,
                new PollTypeQuiz(),
                0,
                0,
                false);

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

                    if (!message.IsOutgoing && message.ChatId == quizbotId)
                    {
                        OnReceivedMessageEvent?.Invoke();
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

        private class AuthorizationRequestHandler : ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(BaseObject @object)
            {
                if (@object is Error)
                {
                    OnAuthorizationStateUpdated(null); // repeat last action
                }
            }
        }

        private class GetUserHandler : ClientResultHandler
        {
            private TaskCompletionSource<User> _tcs;

            public GetUserHandler(TaskCompletionSource<User> tcs)
            {
                _tcs = tcs;
            }

            public void OnResult(BaseObject @object)
            {
                if (@object is User user)
                {
                    _tcs.SetResult(user);
                }
            }
        }
    }
}
