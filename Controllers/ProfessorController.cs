using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;
using UniversityManagement.ViewModels;

namespace UniversityManagement.Controllers
{
    public class ProfessorController : Controller
    {
        private readonly UniversityContext _context;

        public ProfessorController(UniversityContext context)
        {
            _context = context;
        }

        private int? GetTeacherId()
        {
            return HttpContext.Session.GetInt32("TeacherId");
        }

        private bool CheckProfessorRole()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Professor" && GetTeacherId() != null;
        }

        // GET: Professor/Index
        public async Task<IActionResult> Index()
        {
            if (!CheckProfessorRole())
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherId = GetTeacherId();
            var courses = await _context.Courses
                .Where(c => c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId)
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .ToListAsync();

            return View(courses);
        }

        // GET: Professor/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int? id, int? year)
        {
            if (!CheckProfessorRole())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null) return NotFound();

            var teacherId = GetTeacherId();
            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(c => c.Id == id &&
                    (c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId));

            if (course == null) return NotFound();

            // Get available years
            var availableYears = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Year != null)
                .Select(e => e.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // If no year specified, use the latest year
            var selectedYear = year ?? (availableYears.Any() ? availableYears.First() : DateTime.Now.Year);

            // Get enrollments for the selected year
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Year == selectedYear)
                .Include(e => e.Student)
                .OrderBy(e => e.Student!.StudentId)
                .Select(e => new EnrollmentDetailViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.Student!.Id,
                    StudentIndex = e.Student.StudentId,
                    StudentName = e.Student.FirstName + " " + e.Student.LastName,
                    Semester = e.Semester,
                    Year = e.Year,
                    ExamPoints = e.ExamPoints,
                    SeminalPoints = e.SeminalPoints,
                    ProjectPoints = e.ProjectPoints,
                    AdditionalPoints = e.AdditionalPoints,
                    TotalPoints = (e.ExamPoints ?? 0) + (e.SeminalPoints ?? 0) +
                                  (e.ProjectPoints ?? 0) + (e.AdditionalPoints ?? 0),
                    Grade = e.Grade,
                    FinishDate = e.FinishDate,
                    SeminalUrl = e.SeminalUrl,
                    ProjectUrl = e.ProjectUrl
                })
                .ToListAsync();

            var viewModel = new ProfessorCourseViewModel
            {
                Course = course,
                SelectedYear = selectedYear,
                AvailableYears = availableYears,
                Enrollments = enrollments
            };

            return View(viewModel);
        }

        // GET: Professor/EditEnrollment/5
        public async Task<IActionResult> EditEnrollment(long? id)
        {
            if (!CheckProfessorRole())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null) return NotFound();

            var teacherId = GetTeacherId();
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.FirstTeacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.SecondTeacher)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id &&
                    (e.Course!.FirstTeacherId == teacherId || e.Course.SecondTeacherId == teacherId));

            if (enrollment == null) return NotFound();

            // Check if student is active
            if (enrollment.FinishDate != null)
            {
                TempData["Error"] = "Не можете да уредувате податоци за неактивен студент.";
                return RedirectToAction(nameof(CourseDetails), new { id = enrollment.CourseId });
            }

            return View(enrollment);
        }

        // POST: Professor/EditEnrollment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollment(long id, int? examPoints, int? seminalPoints,
            int? projectPoints, int? additionalPoints, int? grade, DateTime? finishDate)
        {
            if (!CheckProfessorRole())
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherId = GetTeacherId();
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id &&
                    (e.Course!.FirstTeacherId == teacherId || e.Course.SecondTeacherId == teacherId));

            if (enrollment == null) return NotFound();

            // Check if student is active
            if (enrollment.FinishDate != null)
            {
                TempData["Error"] = "Не можете да уредувате податоци за неактивен студент.";
                return RedirectToAction(nameof(CourseDetails), new { id = enrollment.CourseId });
            }

            enrollment.ExamPoints = examPoints;
            enrollment.SeminalPoints = seminalPoints;
            enrollment.ProjectPoints = projectPoints;
            enrollment.AdditionalPoints = additionalPoints;
            enrollment.Grade = grade;
            enrollment.FinishDate = finishDate;

            try
            {
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Податоците се успешно ажурирани.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Enrollments.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(CourseDetails), new { id = enrollment.CourseId });
        }
    }
}
