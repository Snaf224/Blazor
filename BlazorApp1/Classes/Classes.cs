using Telegram.Bot.Types;

namespace BlazorApp1.Classes
{
    // Дополнительные классы
    public class FileItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class UpdatesResponse
    {
        public List<Update> Result { get; set; }
    }

}
