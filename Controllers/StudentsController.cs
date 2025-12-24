using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;

namespace UniversityManagement.Controllers
{
    public class StudentsController : Controller
    {
        private readonly UniversityContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(UniversityContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Students
        public async Task<IActionResult> Index(string? studentIdSearch, string? firstNameSearch, string? lastNameSearch, int? courseId)
        {
            var students = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(studentIdSearch))
            {
                students = students.Where(s => s.StudentId.Contains(studentIdSearch));
            }

            if (!string.IsNullOrEmpty(firstNameSearch))
            {
                students = students.Where(s => s.FirstName.Contains(firstNameSearch));
            }

            if (!string.IsNullOrEmpty(lastNameSearch))
            {
                students = students.Where(s => s.LastName.Contains(lastNameSearch));
            }

            if (courseId.HasValue)
            {
                students = students.Where(s => s.Enrollments!.Any(e => e.CourseId == courseId.Value));
            }

            ViewData["StudentIdSearch"] = studentIdSearch;
            ViewData["FirstNameSearch"] = firstNameSearch;
            ViewData["LastNameSearch"] = lastNameSearch;
            ViewData["CourseId"] = courseId;

            return View(await students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments!)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student, IFormFile? profilePicture)
        {
            if (ModelState.IsValid)
            {
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/images");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(fileStream);
                    }

                    student.ProfilePicture = "/uploads/images/" + uniqueFileName;
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel,ProfilePicture")] Student student, IFormFile? profilePicture)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/images");
                        Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(student.ProfilePicture))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, student.ProfilePicture.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        student.ProfilePicture = "/uploads/images/" + uniqueFileName;
                    }

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                if (!string.IsNullOrEmpty(student.ProfilePicture))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, student.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/UploadDocument/5
        public async Task<IActionResult> UploadDocument(long? enrollmentId)
        {
            if (enrollmentId == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Students/UploadDocument/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(long enrollmentId, IFormFile? seminalDocument, IFormFile? projectDocument)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }

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

                    enrollment.SeminalUrl = "/uploads/documents/" + uniqueFileName;
                }
            }

            if (projectDocument != null && projectDocument.Length > 0)
            {
                var allowedExtensions = new[] { ".doc", ".docx", ".pdf" };
                var extension = Path.GetExtension(projectDocument.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/documents");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(projectDocument.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await projectDocument.CopyToAsync(fileStream);
                    }

                    enrollment.ProjectUrl = "/uploads/documents/" + uniqueFileName;
                }
            }

            _context.Update(enrollment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = enrollment.StudentId });
        }

        private bool StudentExists(long id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
