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
using System.Text.RegularExpressions;

namespace OpenRegion71Bot
{
    class Management
    {
        private static readonly IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultCookies().WithDefaultLoader());

        /// <summary>
        /// Страница помощи. Показывает права и доступные команды (команда /help)
        /// </summary>
        /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
        /// <param name="userId">Id пользователя (для проверки прав)</param>
        /// <returns></returns>
        public static async Task HelpPage(long chatId, int userId)
        {
            // команды, доступные разным ролям. !!!Команды не должны повторяться у разных ролей!!!
            Dictionary<int, string> ruleСommands = new Dictionary<int, string>(new[]
            {
                // администратор
                new KeyValuePair<int, string>(1, "/updatedistricts - обновление районов.\n" +
                                                 "/updateexecutors - обновление исполнителей."),
                // изменение обращений
                new KeyValuePair<int, string>(2, "/changeisp_xxxxxxx - изменить исполнителя по обращению, где хxxxxxx - это номер обращения."),
                // просмотр обращений
                new KeyValuePair<int, string>(3, "/id_xxxxxxx - информация по обращению, где хxxxxxx - это номер обращения."),
                // без прав
                new KeyValuePair<int, string>(100, "Вам не доступны команды."),
            });


            string rules = "Вам установлены права:", commands = "Вам доступны команды:";
            Dictionary<int, string> userRules = UserRules(userId);
            if (!userRules.ContainsKey(1))
            {
                foreach (var i in userRules.Keys)
                {
                    rules += "\n" + userRules[i];
                    commands += "\n" + ruleСommands[i];
                }
            }
            else
            {
                rules += "\n" + userRules[1];
                foreach (var i in ruleСommands.Keys)
                {
                    if (i == 100) break;
                    commands += "\n" + ruleСommands[i];
                }
            }
            await Controllers.HomeController.bot.SendTextMessageAsync(chatId, rules + "\n\n" + commands);
        }

        /// <summary>
        /// Информация об обращении (команда /id_xxxxxxx)
        /// </summary>
        /// <param name="message">Объект telegram сообщения</param>
        /// <returns></returns>
        public static async Task InformationAboutMessage(long chatId, int userId, string commands)
        {
            // проверяем наличие прав
            if (!IsRule(chatId, userId, 3).Result) return;

            List<string> command = commands.Split("_").ToList();
            
            // проверяем ID обращения
            if (!IsProblem(command[1], chatId).Result) return;
            try
            {
                // пробуем авторизоваться
                if (!Authorization(chatId).Result) return;

                await context.OpenAsync(ConfidentialData.OpenRegion71.ProblemPage + command[1]);
                var form = context.Active.QuerySelector<IHtmlFormElement>("form#form_element_97_form");
                if (form != null)
                {
                    // получаем данные
                    string answer = "ID: " + form.QuerySelector("form tr:first-child td:nth-child(2)").TextContent
                            + "\n" + form.QuerySelector("tr#tr_PROPERTY_521 table tr span").TextContent
                            + " (" + form.QuerySelector("tr#tr_PROPERTY_520 option[selected]").TextContent.Replace(".", "").Trim() + ")"
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

                    // получаем обращение из АПИ для отправки фото
                    var problem = MessageFromApi(Int32.Parse(command[1])).Result;

                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, answer);

                    if (problem.ProblemPhotos != null) await Controllers.HomeController.bot.SendMediaGroupAsync(problem.ProblemPhotos.Split(";").Select(ph => new Telegram.Bot.Types.InputMediaPhoto(new Telegram.Bot.Types.InputMedia(ph)) { Caption = "Обращение " + problem.Id + ". Фото пользователя." }), chatId);
                    if (problem.AnswerPhotos != null) await Controllers.HomeController.bot.SendMediaGroupAsync(problem.AnswerPhotos.Split(";").Select(ph => new Telegram.Bot.Types.InputMediaPhoto(new Telegram.Bot.Types.InputMedia(ph)) { Caption = "Обращение " + problem.Id + ". Фото исполнителя." }), chatId);

                    // пишем лог запроса
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Пользователю {userId} предоставлена информация по обращению {command[1]}.\n");
                }
                else
                {
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Не удалось получить форму обращения.");
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} В методе InformationAboutMessage не удалось получить форму обращения.\n");
                }
            }
            catch
            {
                string error = "Возникла ошибка в методе InformationAboutMessage.";
                await Controllers.HomeController.bot.SendTextMessageAsync(chatId, error);
                await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} {error}.\n");
            }
        }

        /// <summary>
        /// Класс содержит методы для операций замены исполнителя в обращении
        /// </summary>
        public class ChangingIspolnitel
        {
            /// <summary>
            /// Метод для интерактивного взаимодействия с пользователем для операции по замене исполнителя в обращении
            /// </summary>
            /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
            /// <param name="userId">Id пользователя (для проверки прав)</param>
            /// <param name="messageId">Id сообщения, которое подвергается изменению после выбора пользователя</param>
            /// <param name="commands">Команда, которую задал пользователь</param>
            /// <returns></returns>
            public static async Task ChangeIspolnitel(long chatId, int userId, int messageId, string commands)
            {
                // проверяем наличие прав на изменение обращения
                if (!IsRule(chatId, userId, 2).Result) return;

                List<string> command = commands.Split("_").ToList();
                
                // проверяем команду
                if (command.Count == 2)
                {
                    // проверяем обращение
                    if (!IsProblem(command[1], chatId).Result) return;

                    // проверяем статус обращения
                    var problem = MessageFromApi(Int32.Parse(command[1])).Result;
                    if (!(problem.StatusId == 290 || problem.StatusId == 292 || problem.StatusId == 293 || problem.StatusId == 294))
                    {
                        await Controllers.HomeController.bot.SendTextMessageAsync(chatId, $"Действие не возможно, обращение не находится в работе. Статус обращения - {problem.StatusName}.");
                        return;
                    }

                    // выдаем перечень исполнителей
                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(ShortNameIspolnitels().Select(isp => new[]
                    {
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton() { Text = isp.Value, CallbackData = command[0] + "_"+ command[1] + "_"+ isp.Key }
                    }));
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Выберите нового исполнителя из списка", replyMarkup: keyboard);
                }
                if (command.Count == 3)
                {
                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                    {
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton() { Text = "Да", CallbackData = commands + "_yes" },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton() { Text = "Нет", CallbackData = commands + "_no" }
                    });
                    await Controllers.HomeController.bot.EditMessageTextAsync(chatId, messageId, "Вы уверены, что хотите по обращению " + command[1] + " изменить исполнителя на " + Ispolnitels()[Int32.Parse(command[2])] + " ?", replyMarkup: keyboard);
                }
                else if (command.Count == 4 && command[3] == "no")
                {
                    await Controllers.HomeController.bot.EditMessageTextAsync(chatId, messageId, "Вы отменили изменение исполнителя по обращению " + command[1] + ".", replyMarkup: null);
                }
                else if (command.Count == 4 && command[3] == "yes")
                {
                    await Controllers.HomeController.bot.EditMessageTextAsync(chatId, messageId, "Пошел работать...", replyMarkup: null);
                    
                    // запускаем операцию по замене исполнителя в обращении
                    await ChangingIspolnitelInProblem(chatId, Int32.Parse(command[1]), Int32.Parse(command[2]));
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Готово. Проверьте /id_" + command[1] + ".");

                    // пишем логи
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Пользователь {userId} передал обращение {command[1]} другому исполнителю - {Ispolnitels()[Int32.Parse(command[2])]}.\n");
                }
                //else
                //{
                //    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Неверная команда.");
                //}
            }

            /// <summary>
            /// Метод производит действия по изменению исполнителя в обращения, проставляя и затирая необходимые поля
            /// </summary>
            /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
            /// <param name="problemId">Id обращения, в котором нужно изменить исполнителя</param>
            /// <param name="ispolnitelId">Id нового исполнителя</param>
            /// <returns></returns>
            private static async Task ChangingIspolnitelInProblem(long chatId, int problemId, int ispolnitelId)
            {
                // пробуем авторизоваться
                if (!Authorization(chatId).Result) return;

                // получаем исполнителя из базы
                DbData.Executor ispolnitel;
                using (var dbcontext = new DbContext()) { ispolnitel = dbcontext.Executors.Find(ispolnitelId); }

                try
                {
                    // пошли на страницу и начинаем замену
                    await context.OpenAsync(ConfidentialData.OpenRegion71.ProblemPage + problemId);
                    var form = context.Active.QuerySelector<IHtmlFormElement>("form#form_element_97_form[name=form_element_97_form]");

                    // проверяем, удалось ли получить форму
                    if (form == null)
                    {
                        await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Не удалось получить форму обращения.");
                        await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} В методе ChangingIspolnitelInProblem не удалось получить форму обращения.\n");
                        return;
                    }

                    form.QuerySelector("tr#tr_PROPERTY_527 table input").SetAttribute("value", ispolnitel.Id.ToString());
                    form.QuerySelector("tr#tr_PROPERTY_526 option[selected]").RemoveAttribute("selected");
                    form.QuerySelector("tr#tr_PROPERTY_526 option[value=290]").SetAttribute("selected", "");
                    form.QuerySelector("tr#tr_PROPERTY_1582 input").SetAttribute("value", "");
                    form.QuerySelectorAll("tr#tr_PROPERTY_5799 option[selected]").ToList().ForEach(q => q.RemoveAttribute("selected"));
                    form.QuerySelector("tr#tr_PROPERTY_5799 option[value=" + ispolnitel.StructureId + "]").SetAttribute("selected", "");
                    form.QuerySelector("tr#tr_PROPERTY_5810 option[selected]").RemoveAttribute("selected");
                    form.QuerySelector("tr#tr_PROPERTY_5810 option[value=" + ispolnitel.StructureId + "]").SetAttribute("selected", "");
                    form.QuerySelector("tr#tr_PROPERTY_5806 option[selected]").RemoveAttribute("selected");
                    form.QuerySelector("tr#tr_PROPERTY_5807 option[selected]").RemoveAttribute("selected");

                    await form.SubmitAsync(new { save = "Сохранить" });
                }
                catch
                {
                    string error = "Возникла ошибка в методе ChangingIspolnitelInProblem.";
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, error);
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} {error}.\n");
                }
            }
        }

        /// <summary>
        /// Класс содержит методы обновления и добавления информации в БД
        /// </summary>
        public class UpdateDataBase
        {
            /// <summary>
            /// Проверяет пользователя. Обновляет информацию о нем или добавляет в БД.
            /// </summary>
            /// <param name="message">Объект telegram сообщения</param>
            /// <returns></returns>
            public static async Task CheckUser(int userId, string firstName, string lastName, string nick, bool isBot)
            {
                var user = new DbData.User() { Id = userId, Name = (firstName + " " + lastName).Trim(), Nick = nick ?? "", IsBot = isBot };
                bool added = false, updated = false;
                using (var dbcontext = new DbContext())
                {
                    var u = await dbcontext.Users.FindAsync(user.Id);
                    if (u == null) { user.Rules.Add(await dbcontext.Rules.FindAsync(100)); dbcontext.Users.Add(user); added = true; }
                    else if (!u.Equals(user)) { u.Name = user.Name; u.Nick = user.Nick; updated = true; }
                    await dbcontext.SaveChangesAsync();
                }

                // формируем сообщение для лога
                string strResult = String.Empty;
                if (added) strResult += $"{DateTime.Now} Добавлен пользователь {user.Id}: имя - {user.Name}, ник - {user.Nick}.\n";
                else if (updated) strResult += $"{DateTime.Now} Обновлена информация о пользователе {user.Id}: имя - {user.Name}, ник - {user.Nick}.\n";
                if (strResult != String.Empty) await File.AppendAllTextAsync(ConfidentialData.BotLogs, strResult);
            }

            /// <summary>
            /// Получаем перечень районов из АПИ и сохраняем в БД (команда /updatedistricts)
            /// </summary>
            /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
            /// <param name="userId">Id пользователя (для проверки прав)</param>
            /// <returns><returns>
            public static async Task DistrictsFromApi(long chatId, int userId)
            {
                // проверяем наличие прав
                if (!IsRule(chatId, userId, 1).Result) return;
                
                try
                {
                    // запрос к API
                    HttpClient client = new HttpClient();
                    HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("TARGET", "") }); // пустой TARGET выдает список ФИАС районов
                    var result = await client.PostAsync(ConfidentialData.OpenRegion71.Api.FRayons, content);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    Deserialize.FRayon obj = JsonSerializer.Deserialize<Deserialize.FRayon>(resultContent);

                    // фиксируем результаты задачи
                    Dictionary<string, int> taskResult = new Dictionary<string, int>(new[]
                    {
                        new KeyValuePair<string, int>("parsed", obj.RESULTS.Count),
                        new KeyValuePair<string, int>("added", 0),
                        new KeyValuePair<string, int>("updated", 0)
                    });

                    // пишем в базу полученные районы
                    using (var dbcontext = new DbContext())
                    {
                        foreach (var district in obj.RESULTS)
                        {
                            var dist = new DbData.District() { Id = district.ID, Name = district.NAME };
                            var d = dbcontext.Districts.Find(dist.Id);
                            if (d == null) { dbcontext.Districts.Add(dist); taskResult["added"]++; }
                            else if (!d.Equals(dist)) { d.Name = dist.Name; taskResult["updated"]++; }
                        }
                        await dbcontext.SaveChangesAsync();
                    }

                    // формируем сообщение для лога и ответа
                    string strResult = $"Районы: спарсил {taskResult["parsed"]}, добавил в БД {taskResult["added"]}, обновил в БД {taskResult["updated"]}.\n";
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId,strResult);
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, DateTime.Now + " " + strResult);
                }
                catch
                {
                    string error = "Возникла ошибка в методе DistrictsFromApi.";
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, error);
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} {error}.\n");
                    return;
                }
            }

            /// <summary>
            /// Парсим структуру исполнителей и сохраняем в БД (команда /updateexecutors)
            /// </summary>
            /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
            /// <param name="userId">Id пользователя (для проверки прав)</param>
            /// <returns></returns>
            public static async Task ParseIspolnitels(long chatId, int userId)
            {
                // проверяем наличие прав
                if (!IsRule(chatId, userId, 1).Result) return;

                try
                {
                    // пробуем авторизоваться
                    if (!Authorization(chatId).Result) return;
                    
                    // открываем страницу и получаем количество строк
                    await context.OpenAsync(ConfidentialData.OpenRegion71.ExecutorsStructurePages + "1");
                    var rows = context.Active.QuerySelectorAll<IHtmlTableRowElement>("div#adm-workarea div.adm-list-table-layout table.adm-list-table tbody tr");

                    // фиксируем результаты задачи
                    Dictionary<string, int> taskResult = new Dictionary<string, int>(new[]
                    {
                        new KeyValuePair<string, int>("parsed", 0),
                        new KeyValuePair<string, int>("added", 0),
                        new KeyValuePair<string, int>("updated", 0)
                    });

                    using (var dbcontext = new DbContext())
                    {
                        foreach (var row in rows)
                        {
                            taskResult["parsed"]++;

                            // получаем id категории и переходим на детальную страницу
                            int catId = row.QuerySelector("a.adm-list-table-icon-link[title='Перейти в список подразделов']").GetAttribute("href").Split("&").Where(c => c.Contains("find_section_section=")).Select(s => Int32.Parse(s.Replace("find_section_section=", String.Empty))).FirstOrDefault();
                            await context.OpenAsync(ConfidentialData.OpenRegion71.ExecutorStructureDetailPage + catId);

                            // проверяем заполнено ли поле Отвественный
                            string user = context.Active.QuerySelector("div#adm-workarea form select[name=UF_USER] option[selected]")?.TextContent;
                            if (user != null)
                            {
                                int usId = Int32.Parse(Regex.Match(user, @"\d+(?=\D)").Value);
                                string userLogin = Regex.Match(user, @"\((\w|\W)+(?=\))").Value[1..];
                                string userName = context.Active.QuerySelector("div#adm-workarea form tr#tr_NAME input[name=NAME]").GetAttribute("value");

                                var executor = new DbData.Executor()
                                {
                                    Id = usId,
                                    Login = userLogin,
                                    Name = userName,
                                    ShortName = Regex.Replace(userName, @"(Администрация муниципального образования|по Тульской области|Тульской области)", "").Trim(),
                                    StructureId = catId,
                                    Activity = context.Active.QuerySelector("div#adm-workarea form tr#tr_ACTIVE input[name=ACTIVE][checked]") != null,
                                };

                                // сохраняем данные в БД
                                var ex = dbcontext.Executors.Find(executor.Id);
                                if (ex == null) { dbcontext.Executors.Add(executor); taskResult["added"]++; }
                                else if (!ex.Equals(executor))
                                {
                                    ex.Name = executor.Name;
                                    ex.ShortName = executor.ShortName;
                                    ex.Login = executor.Login;
                                    ex.Activity = executor.Activity;
                                    ex.StructureId = executor.StructureId;
                                    taskResult["updated"]++;
                                }
                            }
                            // если поле Ответственный не задано, но исполнитель есть в БД, то меняем активность (чтобы не светить его)
                            else if (dbcontext.Executors.Where(ex => ex.StructureId == catId).FirstOrDefault() != null)
                            {
                                string userName = context.Active.QuerySelector("div#adm-workarea form tr#tr_NAME input[name=NAME]").GetAttribute("value");
                                var ex = dbcontext.Executors.Where(ex => ex.StructureId == catId).FirstOrDefault();
                                ex.Activity = false;
                                ex.Name = userName;
                                ex.ShortName = Regex.Replace(userName, @"(Администрация муниципального образования|по Тульской области|Тульской области|истерств.|итет.|авлени.|еральн...|иальн...|ономическ...|ударственн...)", "").Trim();
                            }
                        }
                        dbcontext.SaveChanges();
                    }

                    // формируем сообщение для лога и ответа
                    string strResult = $"Исполнители: спарсил {taskResult["parsed"]}, добавил в БД {taskResult["added"]}, обновил в БД {taskResult["updated"]}.\n";
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, strResult);
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, DateTime.Now + " " + strResult);
                }
                catch
                {
                    string error = "Возникла оишбка в методе ParseIspolnitels.";
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, error);
                    await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} {error}.\n");
                    return;
                }
            }
        }

        /// <summary>
        /// Авторизоваться на портале 
        /// </summary>
        /// <returns>Авторизация прошла успешно или нет</returns>
        private static async Task<bool> Authorization(long chatId)
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
            catch
            {
                await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Ошибка авторизации.");
                await File.AppendAllTextAsync(ConfidentialData.BotLogs, $"{DateTime.Now} Ошибка авторизации.\n");
                return false;
            }
        }

        /// <summary>
        /// Выдает список прав пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Возвращает словарь key = id роли, value = name роли</returns>
        private static Dictionary<int, string> UserRules(int userId)
        {
            using var dbcontext = new DbContext();
            return dbcontext.Users.Include(r => r.Rules).Where(u => u.Id == userId).FirstOrDefault().Rules.ToDictionary(k => k.Id, v => v.Name);
        }

        /// <summary>
        /// Выдает список исполнителей (id и name)
        /// </summary>
        /// <returns>Возвращает словарь key = id исполнителя, value = name исполнителя</returns>
        private static Dictionary<int, string> Ispolnitels()
        {
            using var dbcontext = new DbContext();
            return dbcontext.Executors.Where(ex => ex.Activity == true).OrderBy(ex => ex.Name).ToDictionary(k => k.Id, v => v.Name);
        }

        /// <summary>
        /// Выдает список исполнителей с короткими названиями (id и short name)
        /// </summary>
        /// <returns>Возвращает словарь key = id исполнителя, value = short name исполнителя</returns>
        private static Dictionary<int, string> ShortNameIspolnitels()
        {
            using var dbcontext = new DbContext();
            return dbcontext.Executors.Where(ex => ex.Activity == true).OrderBy(ex => ex.Name).ToDictionary(k => k.Id, v => v.ShortName);
        }

        /// <summary>
        /// Получает информацию по обращению из АПИ
        /// </summary>
        /// <param name="problemId">Номер обращения</param>
        /// <returns>Возвращает объект DbData.Problem или null. если обращение не найдено</returns>
        private static async Task<DbData.Problem> MessageFromApi(int problemId)
        {
            HttpClient client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("JSON", "{\"ID\":\"" + problemId + "\"}") });
            var result = await client.PostAsync(ConfidentialData.OpenRegion71.Api.Problem, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Deserialize.Problem>(resultContent).RESULTS[0];
            if (obj == null) { return null; }
            else
            {
                var problem = new DbData.Problem() { Id = Int32.Parse(obj.ID) };
                if (obj.CATEGORY_NAME != null) problem.CategoryName = obj.CATEGORY_NAME;
                if (Int32.TryParse(obj.CATEGORY, out int res)) problem.CategoryId = res;
                if (obj.THEME_NAME != null) problem.ThemeName = obj.THEME_NAME;
                if (Int32.TryParse(obj.THEME, out res)) problem.ThemeId = res;
                if (obj.ADDRESS != null) problem.Adress = obj.ADDRESS;
                if (obj.SOURCE != null) problem.SourceName = obj.SOURCE;
                if (Int32.TryParse(obj.SOURCE_ID, out res)) problem.SourceId = res;
                if (DateTime.TryParse(obj.QUEST.DATE, out DateTime date)) problem.CreateDate = date;
                if (DateTime.TryParse(obj.ANSWER.DATE, out date)) problem.AnswerDate = date;
                problem.ProblemText = obj.QUEST.TEXT;
                if (obj.ANSWER.TEXT != "nulled") problem.AnswerText = obj.ANSWER.TEXT;
                if (obj.ANSWER.STATUS != null) problem.StatusName = obj.ANSWER.STATUS;
                if (Int32.TryParse(obj.ANSWER.STATUS_ID, out res)) problem.StatusId = res;
                if (obj.ANSWER.FIO != null) problem.IspolnitelName = obj.ANSWER.FIO;
                if (obj.FRAYON != null) problem.DistrictId = obj.FRAYON;
                if (Int32.TryParse(obj.CHILD_ISSUE, out res)) problem.ChildID = res;
                if (Int32.TryParse(obj.PARENT_ISSUE, out res)) problem.ParentID = res;
                if (obj.QUEST.PHOTOS != null) { foreach (var photo in obj.QUEST.PHOTOS) problem.ProblemPhotos = problem.ProblemPhotos == null ? photo.BIG : (problem.ProblemPhotos + ";" + photo.BIG); }
                if (obj.ANSWER.PHOTOS != null) { foreach (var photo in obj.ANSWER.PHOTOS) problem.AnswerPhotos = problem.AnswerPhotos == null ? photo.BIG : (problem.AnswerPhotos + ";" + photo.BIG); }

                return problem;
            }
        }

        /// <summary>
        /// Ищет обращение
        /// </summary>
        /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
        /// <param name="problem">Предполагаемый номер обращения</param>
        /// <returns>true - обращение найдено, false - обращение не найдено</returns>
        private static async Task<bool> IsProblem(string problem, long chatId)
        {
            if(Int32.TryParse(problem, out int result))
            {
                var pr = await MessageFromApi(result);
                if (pr == null)
                {
                    await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Обращение не найдено.");
                    return false;
                }
                else { return true; }
            }
            else
            {
                await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Некорректный номер обращения.");
                return false;
            }
        }

        /// <summary>
        /// Проверяет, доступно ли пользователю данное действие (есть ли соответствующие права)
        /// </summary>
        /// <param name="chatId">Id чата с пользователем (для отправки ответа)</param>
        /// <param name="userId">Id пользователя (для проверки прав)</param>
        /// <param name="needRule">Необходимая группа прав для выбранного действия</param>
        /// <returns>true - действие разрешено, false - действие запрещено</returns>
        private static async Task<bool> IsRule(long chatId, int userId, params int[] needRule)
        {
            var rules = UserRules(userId);
            if (rules.ContainsKey(1)) { return true; }
            else
            {
                foreach (int r in needRule)
                {
                    if (rules.ContainsKey(r)) { return true; }
                }
                await Controllers.HomeController.bot.SendTextMessageAsync(chatId, "Нет прав для данного действия.");
                return false;
            }
        }
    }
}
