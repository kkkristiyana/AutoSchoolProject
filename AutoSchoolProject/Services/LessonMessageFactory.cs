namespace AutoSchoolProject.Services
{
    public static class LessonMessageFactory
    {
        public const string StudentPrefix = "[КУРСИСТ]";
        public const string InstructorPrefix = "[ИНСТРУКТОР]";

        public static string ForStudent(string message)
            => $"{StudentPrefix} {message}";

        public static string ForInstructor(string message)
            => $"{InstructorPrefix} {message}";

        public static bool IsStudentMessage(string? note)
            => !string.IsNullOrWhiteSpace(note)
               && note.StartsWith(StudentPrefix, StringComparison.OrdinalIgnoreCase);

        public static bool IsInstructorMessage(string? note)
            => !string.IsNullOrWhiteSpace(note)
               && note.StartsWith(InstructorPrefix, StringComparison.OrdinalIgnoreCase);

        public static string StripPrefix(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return string.Empty;
            }

            return note
                .Replace(StudentPrefix, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(InstructorPrefix, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();
        }
    }
}
