using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.ViewModels;

namespace UniversityManagement.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly UniversityContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentDashboardController(UniversityContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private long? GetStudentId()
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (long.TryParse(studentIdString, out long studentId))
            {
                return studentId;
            }
            return null;
        }

        private bool CheckStudentRole()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Student" && GetStudentId() != null;
        }

        // GET: StudentDashboard/Index
        public async Task<IActionResult> Index()
        {
            if (!CheckStudentRole())
            {
                return RedirectToAction("Login", "Account");
            }

            var studentId = GetStudentId();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.FirstTeacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.SecondTeacher)
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Course!.Title)
                .Select(e => new StudentEnrollmentViewModel
                {
                    EnrollmentId = e.Id,
                    CourseId = e.Course!.Id,
                    CourseTitle = e.Course.Title,
                    CourseCredits = e.Course.Credits,
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
                    ProjectUrl = e.ProjectUrl,
                    FirstTeacherName = e.Course.FirstTeacher != null ?
                        e.Course.FirstTeacher.FirstName + " " + e.Course.FirstTeacher.LastName : null,
                    SecondTeacherName = e.Course.SecondTeacher != null ?
                        e.Course.SecondTeacher.FirstName + " " + e.Course.SecondTeacher.LastName : null
                })
                .ToListAsync();

            ViewBag.Student = student;
            return View(enrollments);
        }

        // GET: StudentDashboard/EditUrls/5
        public async Task<IActionResult> EditUrls(long? id)
        {
            if (!CheckStudentRole())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null) return NotFound();

            var studentId = GetStudentId();
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == studentId);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // POST: StudentDashboard/EditUrls/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUrls(long id, string? projectUrl, IFormFile? seminalDocument)
        {
            if (!CheckStudentRole())
            {
                return RedirectToAction("Login", "Account");
            }

            var studentId = GetStudentId();
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == studentId);

            if (enrollment == null) return NotFound();

            // Update ProjectURL (GitHub link)
            if (!string.IsNullOrWhiteSpace(projectUrl))
            {
                enrollment.ProjectUrl = projectUrl;
            }

            // Upload Seminal Document (doc, docx, pdf)
            if (seminalDocument != null && seminalDocument.Length > 0)
            {
                var allowedExtensions = new[] { ".doc", ".docx", ".pdf" };
                var extension = Path.GetExtension(seminalDocument.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/documents");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(seminalDocument.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await seminalDocument.CopyToAsync(fileStream);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(enrollment.SeminalUrl))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, enrollment.SeminalUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    enrollment.SeminalUrl = "/uploads/documents/" + uniqueFileName;
                }
                else
                {
                    TempData["Error"] = "Дозволени се само .doc, .docx и .pdf формати.";
                    return RedirectToAction(nameof(EditUrls), new { id });
                }
            }

            try
            {
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Линковите се успешно ажурирани.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Enrollments.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
