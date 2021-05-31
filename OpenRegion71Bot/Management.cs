using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;


namespace OpenRegion71Bot
{
    class Management
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultCookies().WithDefaultLoader());

        /// <summary>
        /// Поиск ролей пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Возвращает словарь key = id роли, value = name роли</returns>
        private static Dictionary<int, string> UserRules(int userId)
        {
            using (var dbcontext = new DbContext())
            {
                return dbcontext.Users.Include(r => r.Rules).Where(u => u.Id == userId).FirstOrDefault().Rules.ToDictionary(k => k.Id, v => v.Name);
            }
        }

        /// <summary>
        /// Авторизоваться на портале 
        /// </summary>
        /// <returns>Авторизация прошла успешно или нет</returns>
        private static async Task<bool> Authorization()
        {
            try
            {
                await context.OpenAsync(ConfidentialData.OpenRegion71.AuthPage);

                // проверяем осталась ли авторизация (подходят ли куки) или нужна повторная авторизация
                if (context.Active.QuerySelector("form[name=form_auth]") != null)
                {
                    var form = context.Active.QuerySelector<IHtmlFormElement>("form");

                    // авторизуемся
                    await form.SubmitAsync(new
                    {
                        USER_LOGIN = ConfidentialData.OpenRegion71.Login,
                        USER_PASSWORD = ConfidentialData.OpenRegion71.Password,
                    });
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Авторизация пройдена.\n");
                }
                return true;
            }
            catch (Exception e)
            {
                await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Ошибка: {e}.\n");
                return false;
            }
        }

        /// <summary>
        /// Информация об обращении
        /// </summary>
        /// <param name="message">Объект telegram сообщения</param>
        /// <returns></returns>
        public static async Task InformationAboutMessage(Telegram.Bot.Types.Message message)
        {
            // проверяем наличие прав
            var rules = UserRules(message.From.Id);
            if (!(rules.ContainsKey(1) || rules.ContainsKey(3)))
            {
                await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, "Нет прав для данного действия.");
                return;
            }

            // проверяем ID обращения
            List<string> command = message.Text.Split("_").ToList();
            if (Int32.TryParse(command[1], out int result))
            {
                // пробуем авторизоваться
                if (!(await Authorization()))
                {
                    await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, "Ошибка авторизации.");
                    return;
                }

                await context.OpenAsync(ConfidentialData.OpenRegion71.ProblemPage + result);
                var form = context.Active.QuerySelector<IHtmlFormElement>("form#form_element_97_form");
                if (form != null)
                {
                    // получаем данные
                    string answer = "ID: " + form.QuerySelector("form tr:first-child td:nth-child(2)").TextContent
                            + "\n" + form.QuerySelector("tr#tr_PROPERTY_521 table tr span").TextContent
                            + " (" + form.QuerySelector("tr#tr_PROPERTY_520 option[selected]").TextContent + ")"
                            + "\nИсточник: " + form.QuerySelector("tr#tr_PROPERTY_531 table tr span").TextContent
                            + "\nАдрес: " + form.QuerySelector("tr#tr_PROPERTY_524 table tr input").GetAttribute("value")
                            + "\n\nСтатус: " + form.QuerySelector("tr#tr_PROPERTY_526 option[selected]").TextContent
                            + "\nДата создания: " + form.QuerySelector("form tr:nth-child(2) td:nth-child(2)").TextContent.Substring(0, 19)
                            + "\nПлановый срок: " + form.QuerySelector("tr#tr_PROPERTY_608 table tr input").GetAttribute("value")
                            + "\nПродленный срок: " + form.QuerySelector("tr#tr_PROPERTY_609 table tr input").GetAttribute("value")
                            + "\nДата ответа: " + form.QuerySelector("tr#tr_PROPERTY_1582 table tr input").GetAttribute("value");

                    // получаем название исполнителя
                    using (var dbcontext = new DbContext())
                    {
                        string ispol = form.QuerySelector("tr#tr_PROPERTY_527 table tr input").GetAttribute("value");
                        if (ispol != "")
                        {
                            var ispolDb = dbcontext.Executors.Find(Int32.Parse(ispol));
                            if (ispolDb != null) ispol = ispolDb.Name;
                        }
                        answer += "\n\nИсполнитель: " + ispol;
                    }

                    answer += "\n\nВопрос: \n" + form.QuerySelector("tr#tr_PREVIEW_TEXT_EDITOR textarea").TextContent
                            + "\n\nОтвет: \n" + form.QuerySelector("tr#tr_DETAIL_TEXT_EDITOR textarea").TextContent;

                    // выводим причину отклонения, если не пусто
                    string reason = form.QuerySelector("tr#tr_PROPERTY_613 table tr input").GetAttribute("value");
                    if (reason != "") answer += "\n\nПричина отклонения: \n" + reason;

                    await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, answer);

                    // пишем лог запроса
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Пользователю {message.From.Id} предоставлена информация по обращению {command[1]}.\n");

                    return;
                }

                else
                {
                    await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, "Обращение не найдено.");
                    return;
                }
            }
            else
            {
                await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, "Некорректный номер обращения.");
                return;
            }
        }

        public static async Task HelpPage(Telegram.Bot.Types.Message message)
        {
            // команды, доступные разным ролям. !!!Команды не должны повторяться у разных ролей!!!
            Dictionary<int, string> ruleСommands = new Dictionary<int, string>(new[]
            {
                // администратор
                new KeyValuePair<int, string>(1, "Команды пока не определены."),
                // изменение обращений
                new KeyValuePair<int, string>(2, "Команды пока не определены."),
                // просмотр обращений
                new KeyValuePair<int, string>(3, "/id_xxxxxxx - информация по обращению, где хxxxxxx - это номер обращения."),
                // без прав
                new KeyValuePair<int, string>(100, "Вам не доступны команды."),
            });


            string rules = "Вам установлены права:", commands = "Вам доступны команды:";
            Dictionary<int, string> userRules = UserRules(message.From.Id);
            if (!userRules.ContainsKey(1))
            {
                foreach (var i in userRules.Keys)
                {
                    rules += "\n- " + userRules[i];
                    commands += "\n- " + ruleСommands[i];
                }
            }
            else
            {
                rules += "\n- " + userRules[1];
                foreach (var i in ruleСommands.Keys)
                {
                    if (i == 100) break;
                    commands += "\n- " + ruleСommands[i];
                }
            }
            await Controllers.HomeController.bot.SendTextMessageAsync(message.Chat.Id, rules + "\n\n" + commands);
        }

        public class UpdateDataBase
        {
            /// <summary>
            /// Проверяет пользователя. Обновляет информацию о нем или добавляет в БД.
            /// </summary>
            /// <param name="message">Объект telegram сообщения</param>
            /// <returns></returns>
            public static async Task CheckUser(Telegram.Bot.Types.Message message)
            {
                var user = new DbData.User() { Id = message.From.Id, Name = (message.From.FirstName + message.From.LastName).Trim(), Nick = message.From.Username ?? "", IsBot = message.From.IsBot };
                bool added = false, updated = false;
                using (var dbcontext = new DbContext())
                {
                    var u = await dbcontext.Users.FindAsync(user.Id);
                    if (u == null) { user.Rules.Add(await dbcontext.Rules.FindAsync(100)); dbcontext.Users.Add(user); added = true; }
                    else if (!user.Equals(u)) { u.Name = user.Name; u.Nick = user.Nick; updated = true; }
                    await dbcontext.SaveChangesAsync();
                }

                // формируем сообщение для лога
                string strResult = String.Empty;
                if (added) strResult += $"{DateTime.Now} Добавлен пользователь {user.Id}: имя - {user.Name}, ник - {user.Nick}.\n";
                else if (updated) strResult += $"{DateTime.Now} Обновлена информация о пользователе {user.Id}: имя - {user.Name}, ник - {user.Nick}.\n";
                if (strResult != String.Empty) await File.AppendAllTextAsync(ConfidentialData.BotLogs, strResult);
            }
        }
    }
}
