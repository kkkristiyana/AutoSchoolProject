namespace AutoSchoolProject.ViewModels.Common
{
    public class UserMessageItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class UserMessagesViewModel
    {
        public string Heading { get; set; } = "Съобщения";
        public string EmptyMessage { get; set; } = "Няма съобщения.";
        public List<UserMessageItemViewModel> Messages { get; set; } = new();
    }
}
