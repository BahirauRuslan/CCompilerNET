using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace CCompiler
{
    public class TelegramListenerApp : BaseApp
    {
        private TelegramBotClient _client;
        private readonly string _token;

        private HashSet<long> _chats;
        private Dictionary<long, string> _fileNames;

        public TelegramListenerApp(string token)
        {
            _token = token;
            _chats = new HashSet<long>();
            _fileNames = new Dictionary<long, string>();
        }

        public override void Run()
        {
            _client = new TelegramBotClient(_token);
            _client.OnMessage += BotOnMessageReceived;
            _client.OnMessageEdited += BotOnMessageReceived;

            _client.StartReceiving();
            Console.ReadLine();
            _client.StopReceiving();
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message?.Text != null &&
                _chats.Contains(message.Chat.Id) &&
                !_fileNames.ContainsKey(message.Chat.Id))
            {
                _fileNames.Add(message.Chat.Id, message.Text);

                await _client.SendTextMessageAsync(message.Chat.Id,
                    "Now send source code as text message");
            }
            else if (message?.Text != null &&
                _chats.Contains(message.Chat.Id) &&
                _fileNames.ContainsKey(message.Chat.Id))
            {
                var name = CompileSourceCode(message.Text, _fileNames[message.Chat.Id]);
                SendResultMessage(message.Chat.Id, name);
                ClearChatInfo(message.Chat.Id);
            }
            else if (message?.Type == MessageType.Document &&
                _chats.Contains(message.Chat.Id))
            {
                var name = await CompileFile(message.Document.FileId, message.Document.FileName);
                SendResultMessage(message.Chat.Id, name);
                ClearChatInfo(message.Chat.Id);
            }
            else
            {
                _chats.Add(message.Chat.Id);
                await _client.SendTextMessageAsync(message.Chat.Id,
                    "Send file (extension .c) or file name like file.c");
            }
        }

        private string CompileSourceCode(string sourceCode, string fileName)
        {
            string outputFileName = null;

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(sourceCode);
                writer.Flush();
                stream.Position = 0;
                outputFileName = RunCompile(stream, fileName);
            }

            return outputFileName;
        }

        private async Task<string> CompileFile(string fileId, string fileName)
        {
            string outputFileName = null;

            using (var stream = new MemoryStream())
            {
                var file = await _client.GetInfoAndDownloadFileAsync(fileId, stream);
                stream.Position = 0;
                outputFileName = RunCompile(stream, fileName);
            }

            return outputFileName;
        }

        private async void SendResultMessage(long chatId, string outputFileName)
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                await _client.SendTextMessageAsync(chatId, "Complilation failed");
            }
            else
            {
                using (var stream = new FileStream(outputFileName, FileMode.Open))
                {
                    var input = new InputOnlineFile(stream);
                    input.FileName = outputFileName;
                    await _client.SendDocumentAsync(chatId, input);
                }
            }
        }

        private void ClearChatInfo(long chatId)
        {
            if (_chats.Contains(chatId))
            {
                _chats.Remove(chatId);
            }

            if (_fileNames.ContainsKey(chatId))
            {
                _fileNames.Remove(chatId);
            }
        }
    }
}
