namespace OpenRegion71Bot
{
    class ConfidentialData
    {
        public static string BotTelegramToken { get => "скрыто"; }
        public static string BotDataBase { get => "BDB.db"; }
        public static string BotLogs { get => "logs.txt"; }
        public class OpenRegion71
        {
            public static string AuthPage { get => "скрыто"; }
            public static string Login { get => "скрыто"; }
            public static string Password { get => "скрыто"; }
            public static string ExecutorsStructurePages { get => "https://or71.ru/bitrix/admin/iblock_section_admin.php?IBLOCK_ID=232&type=messages&lang=ru&find_section_section=0&SIZEN_1=200&PAGEN_1="; }
            public static string ExecutorStructureDetailPage { get => "https://or71.ru/bitrix/admin/iblock_section_edit.php?IBLOCK_ID=232&type=messages&lang=ru&from=iblock_section_admin&find_section_section=0&ID="; }
            public static string ProblemPage { get => "https://or71.ru/bitrix/admin/iblock_element_edit.php?IBLOCK_ID=97&type=messages&lang=ru&find_section_section=0&WF=Y&ID="; }
            public static string SourceCategoryPages { get => "https://or71.ru/bitrix/admin/iblock_section_admin.php?IBLOCK_ID=96&type=information&lang=ru&find_section_section=0&SIZEN_1=100&PAGEN_1="; }
            public static string SourcePages { get => "https://or71.ru/bitrix/admin/iblock_element_admin.php?IBLOCK_ID=96&type=information&lang=ru&SIZEN_1=500&PAGEN_1=1&find_section_section="; }
            public class Api
            {
                private static string Key { get => "скрыто"; }
                public static string FRayons { get => "https://or71.ru/api.php?action=getfias&" + Key; }
                public static string Themes { get => "https://or71.ru/api.php?action=getcategory&" + Key; }
                public static string Problems { get => "https://or71.ru/api.php?action=getissues&" + Key; }
            }
        }
    }
}
