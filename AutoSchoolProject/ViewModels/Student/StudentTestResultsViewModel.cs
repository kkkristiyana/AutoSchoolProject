namespace AutoSchoolProject.ViewModels.Student
{
    public class StudentTestResultsViewModel
    {
        public string? FullName { get; set; }
        public string? CourseName { get; set; }

        public List<TestResultRowViewModel> Results { get; set; } = new();
    }

    public class TestResultRowViewModel
    {
        public DateTime Date { get; set; }
        public int Score { get; set; }
    }
}
