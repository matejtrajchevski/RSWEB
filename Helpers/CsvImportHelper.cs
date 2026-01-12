using System.Globalization;
using UniversityManagement.Models;
using UniversityManagement.ViewModels;

namespace UniversityManagement.Helpers
{
    public class CsvImportHelper
    {
        public static List<Student> ParseStudentsCsv(Stream csvStream)
        {
            var students = new List<Student>();
            using (var reader = new StreamReader(csvStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 7) continue;

                    try
                    {
                        var student = new Student
                        {
                            StudentId = values[0].Trim(),
                            FirstName = values[1].Trim(),
                            LastName = values[2].Trim(),
                            EnrollmentDate = DateTime.Parse(values[3].Trim()),
                            CurrentSemester = int.Parse(values[4].Trim()),
                            AcquiredCredits = int.Parse(values[5].Trim()),
                            EducationLevel = values[6].Trim()
                        };

                        students.Add(student);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return students;
        }

        public static List<Teacher> ParseTeachersCsv(Stream csvStream)
        {
            var teachers = new List<Teacher>();
            using (var reader = new StreamReader(csvStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 5) continue;

                    try
                    {
                        var teacher = new Teacher
                        {
                            FirstName = values[0].Trim(),
                            LastName = values[1].Trim(),
                            Degree = values[2].Trim(),
                            AcademicRank = values[3].Trim(),
                            OfficeNumber = values[4].Trim(),
                            HireDate = values.Length > 5 && !string.IsNullOrWhiteSpace(values[5])
                                ? DateTime.Parse(values[5].Trim())
                                : null
                        };

                        teachers.Add(teacher);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return teachers;
        }

        public static List<Course> ParseCoursesCsv(Stream csvStream)
        {
            var courses = new List<Course>();
            using (var reader = new StreamReader(csvStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 4) continue;

                    try
                    {
                        var course = new Course
                        {
                            Title = values[0].Trim(),
                            Credits = int.Parse(values[1].Trim()),
                            Semester = int.Parse(values[2].Trim()),
                            Programme = values[3].Trim()
                        };

                        courses.Add(course);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return courses;
        }

        public static List<StudentImportViewModel> ParseStudentsWithCredentialsCsv(Stream csvStream)
        {
            var students = new List<StudentImportViewModel>();
            using (var reader = new StreamReader(csvStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 9) continue;

                    try
                    {
                        var student = new StudentImportViewModel
                        {
                            StudentId = values[0].Trim(),
                            FirstName = values[1].Trim(),
                            LastName = values[2].Trim(),
                            EnrollmentDate = DateTime.Parse(values[3].Trim()),
                            CurrentSemester = int.Parse(values[4].Trim()),
                            AcquiredCredits = int.Parse(values[5].Trim()),
                            EducationLevel = values[6].Trim(),
                            Username = values[7].Trim(),
                            Password = values[8].Trim()
                        };

                        students.Add(student);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return students;
        }

        public static List<TeacherImportViewModel> ParseTeachersWithCredentialsCsv(Stream csvStream)
        {
            var teachers = new List<TeacherImportViewModel>();
            using (var reader = new StreamReader(csvStream))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 8) continue;

                    try
                    {
                        var teacher = new TeacherImportViewModel
                        {
                            FirstName = values[0].Trim(),
                            LastName = values[1].Trim(),
                            Degree = values[2].Trim(),
                            AcademicRank = values[3].Trim(),
                            OfficeNumber = values[4].Trim(),
                            HireDate = !string.IsNullOrWhiteSpace(values[5])
                                ? DateTime.Parse(values[5].Trim())
                                : null,
                            Username = values[6].Trim(),
                            Password = values[7].Trim()
                        };

                        teachers.Add(teacher);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            return teachers;
        }
    }
}
