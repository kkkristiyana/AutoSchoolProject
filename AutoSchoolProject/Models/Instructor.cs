namespace AutoSchoolProject.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }

        public ICollection<PracticeLesson> PracticeLessons { get; set; }
    }
}