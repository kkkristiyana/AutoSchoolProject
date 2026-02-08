using System.Collections.Generic;
using AutoSchoolProject.Models;

namespace AutoSchoolProject.ViewModels.Instructor
{
    public class InstructorDashboardViewModel
    {
        public string InstructorName { get; set; }

        public List<PracticeLesson> UpcomingLessons { get; set; }

        public List<PracticeLesson> PendingLessons { get; set; }

        public int CompletedLessonsCount { get; set; }
    }
}
