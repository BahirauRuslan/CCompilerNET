using System;
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

        public TelegramListenerApp(string token)
        {
            _token = token;
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

            if (message?.Type == MessageType.Text)
            {
                ;
            }
            else if (message?.Type == MessageType.Document)
            {
                FileName = message.Document.FileName;
                var name = await CompileFile(message.Document.FileId);
                SendResultMessage(message.Chat.Id, name);
            }
        }

        private async Task<string> CompileFile(string fileId)
        {
            string outputFileName = null;

            using (Stream stream = new MemoryStream())
            {
                var file = await _client.GetInfoAndDownloadFileAsync(fileId, stream);
                stream.Position = 0;
                outputFileName = RunCompile(stream);
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
    }
}
