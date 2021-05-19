namespace OpenRegion71Bot
{
    class ConfidentialData
    {
        public static string BotTelegramToken { get => "скрытая строка"; }
        public static string BotDataBase { get => "скрытая строка"; }
        public static string BotLogs { get => "скрытая строка"; }
        public class OpenRegion71
        {
            public static string AuthPage { get => "скрытая строка"; }
            public static string Login { get => "скрытая строка"; }
            public static string Password { get => "скрытая строка"; }
            public static string ExecutorsStructurePages { get => "скрытая строка"; }
            public static string ExecutorStructureDetailPage { get => "скрытая строка"; }
            public static string ProblemPage { get => "скрытая строка"; }
            public static string SourceCategoryPages { get => "скрытая строка"; }
            public static string SourcePages { get => "скрытая строка"; }
            public class Api
            {
                private static string Key { get => "скрытая строка"; }
                public static string FRayons { get => "скрытая строка" + Key; }
                public static string Themes { get => "скрытая строка" + Key; }
                public static string Problems { get => "скрытая строка" + Key; }
            }
        }

    }
}
