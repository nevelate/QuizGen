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
        private readonly static Td.ClientResultHandler _defaultHandler = new DefaultHandler();

        private static TdApi.AuthorizationState? _authorizationState = null;

        public static Action<TdApi.AuthorizationState> OnAuthorizationStateChangedEvent;

        static TelegramClient()
        {
            Td.Client.Execute(new TdApi.SetLogVerbosityLevel(0));
            if (Td.Client.Execute(new TdApi.SetLogStream(new TdApi.LogStreamFile("tdlib.log", 1 << 27, false))) is TdApi.Error)
            {
                throw new System.IO.IOException("Write access to the current directory is required");
            }
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
            _client.Send(new TdApi.SendMessage(chatId, 0, null, null, null, content), _defaultHandler);
        }

        public static void SendPoll(long chatId, Test test)
        {
            TdApi.InputMessagePoll poll = new TdApi.InputMessagePoll(test.Question, test.Answers.ToArray(), false, new TdApi.PollTypeQuiz(), 0, 0, false);

            _client.Send(new TdApi.SendMessage(chatId, 0, null, null, null, poll), _defaultHandler);
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
            _client.Send(new TdApi.LogOut(), _defaultHandler);
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

        private class DefaultHandler : Td.ClientResultHandler
        {
            void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
            {
                
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
