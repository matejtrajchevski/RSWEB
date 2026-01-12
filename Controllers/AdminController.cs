using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Data;
using UniversityManagement.Models;
using UniversityManagement.ViewModels;
using UniversityManagement.Helpers;

namespace UniversityManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly UniversityContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(UniversityContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool CheckAdminRole()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }
        public async Task<IActionResult> Index()
        {
            if (!CheckAdminRole())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();

            return View();
        }

        #region Students Management

        public async Task<IActionResult> Students()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            var students = await _context.Students.ToListAsync();
            return View(students);
        }

        public IActionResult CreateStudent()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student, IFormFile? profilePicture)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

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
                return RedirectToAction(nameof(Students));
            }
            return View(student);
        }

        public async Task<IActionResult> EditStudent(long? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(long id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel,ProfilePicture")] Student student, IFormFile? profilePicture)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id != student.Id) return NotFound();

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
                    if (!_context.Students.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Students));
            }
            return View(student);
        }

        public async Task<IActionResult> DeleteStudent(long? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(long id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
                var enrollments = await _context.Enrollments.Where(e => e.StudentId == id).ToListAsync();
                _context.Enrollments.RemoveRange(enrollments);

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
            return RedirectToAction(nameof(Students));
        }
        public IActionResult ImportStudents()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStudents(IFormFile csvFile)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (csvFile == null || csvFile.Length == 0)
            {
                ViewBag.Error = "Ве молиме изберете CSV датотека.";
                return View();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Ве молиме изберете валидна CSV датотека.";
                return View();
            }

            try
            {
                using (var stream = csvFile.OpenReadStream())
                {
                    var studentsData = CsvImportHelper.ParseStudentsWithCredentialsCsv(stream);

                    if (studentsData.Count == 0)
                    {
                        ViewBag.Error = "Не се пронајдени валидни податоци во CSV датотеката.";
                        return View();
                    }
                    foreach (var studentData in studentsData)
                    {
                        var student = new Student
                        {
                            StudentId = studentData.StudentId,
                            FirstName = studentData.FirstName,
                            LastName = studentData.LastName,
                            EnrollmentDate = studentData.EnrollmentDate,
                            CurrentSemester = studentData.CurrentSemester,
                            AcquiredCredits = studentData.AcquiredCredits,
                            EducationLevel = studentData.EducationLevel
                        };
                        _context.Students.Add(student);
                    }
                    await _context.SaveChangesAsync();
                    var addedStudents = await _context.Students
                        .Where(s => studentsData.Select(sd => sd.StudentId).Contains(s.StudentId))
                        .ToListAsync();

                    foreach (var studentData in studentsData)
                    {
                        var student = addedStudents.FirstOrDefault(s => s.StudentId == studentData.StudentId);
                        if (student != null)
                        {
                            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == studentData.Username);
                            if (existingUser == null)
                            {
                                var user = new User
                                {
                                    Username = studentData.Username,
                                    Password = studentData.Password,
                                    Role = "Student",
                                    StudentId = student.Id
                                };
                                _context.Users.Add(user);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();

                    ViewBag.Success = $"Успешно се внесени {studentsData.Count} студенти и креирани нивните корисници.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Грешка при обработка на датотеката: {ex.Message}";
                return View();
            }

            return View();
        }

        #endregion

        #region Teachers Management

        public async Task<IActionResult> Teachers()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            var teachers = await _context.Teachers.ToListAsync();
            return View(teachers);
        }

        public IActionResult CreateTeacher()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher([Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher, IFormFile? profilePicture)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/images");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(fileStream);
                    }

                    teacher.ProfilePicture = "/uploads/images/" + uniqueFileName;
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Teachers));
            }
            return View(teacher);
        }

        public async Task<IActionResult> EditTeacher(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate,ProfilePicture")] Teacher teacher, IFormFile? profilePicture)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id != teacher.Id) return NotFound();

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

                        if (!string.IsNullOrEmpty(teacher.ProfilePicture))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, teacher.ProfilePicture.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        teacher.ProfilePicture = "/uploads/images/" + uniqueFileName;
                    }

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teachers.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Teachers));
            }
            return View(teacher);
        }

        public async Task<IActionResult> DeleteTeacher(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost, ActionName("DeleteTeacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacherConfirmed(int id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.TeacherId == id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }
                var coursesAsFirstTeacher = await _context.Courses
                    .Where(c => c.FirstTeacherId == id)
                    .ToListAsync();

                foreach (var course in coursesAsFirstTeacher)
                {
                    course.FirstTeacherId = null;
                }

                var coursesAsSecondTeacher = await _context.Courses
                    .Where(c => c.SecondTeacherId == id)
                    .ToListAsync();

                foreach (var course in coursesAsSecondTeacher)
                {
                    course.SecondTeacherId = null;
                }

                if (!string.IsNullOrEmpty(teacher.ProfilePicture))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, teacher.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Teachers.Remove(teacher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Teachers));
        }
        public IActionResult ImportTeachers()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTeachers(IFormFile csvFile)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (csvFile == null || csvFile.Length == 0)
            {
                ViewBag.Error = "Ве молиме изберете CSV датотека.";
                return View();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Ве молиме изберете валидна CSV датотека.";
                return View();
            }

            try
            {
                using (var stream = csvFile.OpenReadStream())
                {
                    var teachersData = CsvImportHelper.ParseTeachersWithCredentialsCsv(stream);

                    if (teachersData.Count == 0)
                    {
                        ViewBag.Error = "Не се пронајдени валидни податоци во CSV датотеката.";
                        return View();
                    }
                    foreach (var teacherData in teachersData)
                    {
                        var teacher = new Teacher
                        {
                            FirstName = teacherData.FirstName,
                            LastName = teacherData.LastName,
                            Degree = teacherData.Degree,
                            AcademicRank = teacherData.AcademicRank,
                            OfficeNumber = teacherData.OfficeNumber,
                            HireDate = teacherData.HireDate
                        };
                        _context.Teachers.Add(teacher);
                    }
                    await _context.SaveChangesAsync();
                    var addedTeachers = await _context.Teachers
                        .Where(t => teachersData.Select(td => td.FirstName + " " + td.LastName)
                            .Contains(t.FirstName + " " + t.LastName))
                        .ToListAsync();

                    foreach (var teacherData in teachersData)
                    {
                        var teacher = addedTeachers.FirstOrDefault(t =>
                            t.FirstName == teacherData.FirstName && t.LastName == teacherData.LastName);

                        if (teacher != null)
                        {
                            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == teacherData.Username);
                            if (existingUser == null)
                            {
                                var user = new User
                                {
                                    Username = teacherData.Username,
                                    Password = teacherData.Password,
                                    Role = "Professor",
                                    TeacherId = teacher.Id
                                };
                                _context.Users.Add(user);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();

                    ViewBag.Success = $"Успешно се внесени {teachersData.Count} професори и креирани нивните корисници.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Грешка при обработка на датотеката: {ex.Message}";
                return View();
            }

            return View();
        }

        #endregion

        #region Courses Management

        public async Task<IActionResult> Courses()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            var courses = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .ToListAsync();
            return View(courses);
        }

        public IActionResult CreateCourse()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Courses));
            }
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        public async Task<IActionResult> EditCourse(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id != course.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Courses));
            }
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        public async Task<IActionResult> DeleteCourse(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseConfirmed(int id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Courses));
        }
        public IActionResult ImportCourses()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCourses(IFormFile csvFile)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (csvFile == null || csvFile.Length == 0)
            {
                ViewBag.Error = "Ве молиме изберете CSV датотека.";
                return View();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Ве молиме изберете валидна CSV датотека.";
                return View();
            }

            try
            {
                using (var stream = csvFile.OpenReadStream())
                {
                    var courses = CsvImportHelper.ParseCoursesCsv(stream);

                    if (courses.Count == 0)
                    {
                        ViewBag.Error = "Не се пронајдени валидни податоци во CSV датотеката.";
                        return View();
                    }

                    _context.Courses.AddRange(courses);
                    await _context.SaveChangesAsync();

                    ViewBag.Success = $"Успешно се внесени {courses.Count} предмети.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Грешка при обработка на датотеката: {ex.Message}";
                return View();
            }

            return View();
        }

        #endregion

        #region Enrollment Management
        public async Task<IActionResult> EnrollStudents(int? id, int? filterSemester, string? filterEducationLevel)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == id)
                .Select(e => e.StudentId)
                .ToListAsync();

            var studentsQuery = _context.Students
                .Where(s => !enrolledStudentIds.Contains(s.Id));
            if (filterSemester.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.CurrentSemester == filterSemester.Value);
            }

            if (!string.IsNullOrEmpty(filterEducationLevel))
            {
                studentsQuery = studentsQuery.Where(s => s.EducationLevel == filterEducationLevel);
            }

            var availableStudents = await studentsQuery
                .Select(s => new StudentCheckboxViewModel
                {
                    Id = s.Id,
                    StudentId = s.StudentId,
                    FullName = s.FirstName + " " + s.LastName,
                    CurrentSemester = s.CurrentSemester,
                    EducationLevel = s.EducationLevel,
                    IsSelected = false
                })
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            var viewModel = new BulkEnrollmentViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Year = DateTime.Now.Year,
                Semester = "Зимски",
                AvailableStudents = availableStudents,
                FilterSemester = filterSemester,
                FilterEducationLevel = filterEducationLevel
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudents(BulkEnrollmentViewModel model)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (model.SelectedStudentIds != null && model.SelectedStudentIds.Any())
            {
                foreach (var studentId in model.SelectedStudentIds)
                {
                    var enrollment = new Enrollment
                    {
                        CourseId = model.CourseId,
                        StudentId = studentId,
                        Year = model.Year,
                        Semester = model.Semester
                    };

                    _context.Enrollments.Add(enrollment);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageEnrollments), new { id = model.CourseId });
        }
        public async Task<IActionResult> ManageEnrollments(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent(long enrollmentId, DateTime? finishDate)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                enrollment.FinishDate = finishDate ?? DateTime.Now;
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageEnrollments), new { id = enrollment?.CourseId });
        }

        #endregion

        #region User Management

        public async Task<IActionResult> Users()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            var users = await _context.Users
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> CreateUser()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Students = await _context.Students.ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Users));
            }

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Students = await _context.Students.ToListAsync();

            return View(user);
        }

        public async Task<IActionResult> EditUser(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Students = await _context.Students.ToListAsync();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Корисникот е успешно ажуриран.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Users));
            }

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            ViewBag.Students = await _context.Students.ToListAsync();

            return View(user);
        }

        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Teacher)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Users));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateUsersForAll()
        {
            if (!CheckAdminRole()) return RedirectToAction("Login", "Account");

            int createdCount = 0;
            var teachersWithoutUser = await _context.Teachers
                .Where(t => !_context.Users.Any(u => u.TeacherId == t.Id))
                .ToListAsync();

            foreach (var teacher in teachersWithoutUser)
            {
                var username = $"{teacher.FirstName}.{teacher.LastName}".ToLower().Replace(" ", "");
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (existingUser == null)
                {
                    var user = new User
                    {
                        Username = username,
                        Password = "prof123",
                        Role = "Professor",
                        TeacherId = teacher.Id
                    };
                    _context.Users.Add(user);
                    createdCount++;
                }
            }
            var studentsWithoutUser = await _context.Students
                .Where(s => !_context.Users.Any(u => u.StudentId == s.Id))
                .ToListAsync();

            foreach (var student in studentsWithoutUser)
            {
                var username = $"{student.FirstName}.{student.LastName}".ToLower().Replace(" ", "");
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (existingUser == null)
                {
                    var user = new User
                    {
                        Username = username,
                        Password = "student123",
                        Role = "Student",
                        StudentId = student.Id
                    };
                    _context.Users.Add(user);
                    createdCount++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Креирани се {createdCount} нови корисници.";
            return RedirectToAction(nameof(Users));
        }

        #endregion
    }
}
