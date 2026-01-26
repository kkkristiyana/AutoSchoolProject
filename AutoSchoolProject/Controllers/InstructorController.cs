using AutoSchoolProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoSchoolProject.Controllers
{
    public class InstructorController : Controller
    {
        private readonly InstructorService _instructorService;

        public InstructorController(InstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        public IActionResult Details(int id)
        {
            var model = _instructorService.GetInstructorDetails(id);

            if (model == null)
                return NotFound();

            return View(model);
        }
    }
}
