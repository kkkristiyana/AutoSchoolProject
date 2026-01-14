namespace AutoSchoolProject.Models
{
    public class TestResultListovki
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int Score { get; set; }
        public DateTime Date { get; set; }
    }
}