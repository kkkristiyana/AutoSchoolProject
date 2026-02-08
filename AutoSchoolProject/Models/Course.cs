namespace AutoSchoolProject.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }     
        public decimal Price { get; set; }

        public ICollection<PracticeLesson>? PracticeLessons { get; set; }
        public ICollection<Student>? Students { get; set; }
        public int RequiredPracticeLessons { get; set; } = 31;
    }
}