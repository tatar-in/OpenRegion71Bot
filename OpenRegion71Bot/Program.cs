using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace OpenRegion71Bot
{
    class Program
    {
        private static readonly TelegramBotClient bot = new TelegramBotClient(ConfidentialData.BotTelegramToken);
        static void Main(string[] args)
        {
            bot.OnMessage += Bot_OnMessage;
            bot.OnMessageEdited += Bot_OnMessageEdited;
            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }
        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                if (e.Message.Text == "как дела")
                {
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "спасибо, хорошо", replyToMessageId: e.Message.MessageId);
                }
                else
                {
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Неизвестная команда", replyToMessageId: e.Message.MessageId);
                }
            }
            else
            {
                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Данный тип сообщений не поддерживается, разрешены только текстовые сообщения.", replyToMessageId: e.Message.MessageId);
            }
        }
        private static async void Bot_OnMessageEdited(object sender, MessageEventArgs e)
        {
            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Изменение сообщений не поддерживается.", replyToMessageId: e.Message.MessageId);
        }
    }
}
