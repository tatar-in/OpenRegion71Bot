using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRegion71Bot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace OpenRegion71Bot.Controllers
{
    public class HomeController : Controller
    {
        public static readonly TelegramBotClient bot = new TelegramBotClient(ConfidentialData.BotTelegramToken);
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            bot.OnMessage += Bot_OnMessage;
            bot.OnMessageEdited += Bot_OnMessageEdited;
            bot.OnCallbackQuery += Bot_OnCallbackQuery;
            bot.StartReceiving(); 

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e) 
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                // обновляем пользователя 
                await Management.UpdateDataBase.CheckUser(e.Message.From.Id, e.Message.From.FirstName, e.Message.From.LastName, e.Message.From.Username, e.Message.From.IsBot);

                // делим команду на составные
                List<string> command = e.Message.Text.Split("_").ToList();
                switch (command[0])
                {
                    case "/start":
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, $"Добрый день, {e.Message.From.FirstName}!\n" +
                            $"Доступные вам действия можно узнать /help.");
                        break;
                    case "/help":
                        await Management.HelpPage(e.Message.Chat.Id, e.Message.From.Id);
                        break;
                    case "/id":
                        await Management.InformationAboutMessage(e.Message.Chat.Id, e.Message.From.Id, e.Message.Text);
                        break;
                    case "/updatedistricts":
                        await Management.UpdateDataBase.DistrictsFromApi(e.Message.Chat.Id, e.Message.From.Id);
                        break;
                    case "/updateexecutors":
                        await Management.UpdateDataBase.ParseIspolnitels(e.Message.Chat.Id, e.Message.From.Id);
                        break;
                    case "/changeisp":
                        await Management.ChangingIspolnitel.ChangeIspolnitel(e.Message.Chat.Id, e.Message.From.Id, e.Message.MessageId, e.Message.Text);
                        break;
                    default:
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, "Неизвестная команда.", replyToMessageId: e.Message.MessageId);
                        break;
                }
            }
            else
            {
                await bot.SendTextMessageAsync(e.Message.Chat.Id, "Данный тип сообщений не поддерживается, разрешены только текстовые сообщения.", replyToMessageId: e.Message.MessageId);
            }
        }
        private async void Bot_OnMessageEdited(object sender, MessageEventArgs e)
        {
            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Изменение сообщений не поддерживается.", replyToMessageId: e.Message.MessageId);
        }
        private async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            // делим команду на составные
            List<string> command = e.CallbackQuery.Data.Split("_").ToList();
            switch(command[0])
            {
                case "/changeisp":
                    await Management.ChangingIspolnitel.ChangeIspolnitel(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.From.Id, e.CallbackQuery.Message.MessageId, e.CallbackQuery.Data);
                    break;
                default:
                    await bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Неизвестная команда.");
                    break;
            }
        }
    }
}

