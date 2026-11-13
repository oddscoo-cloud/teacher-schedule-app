namespace backend.Core.Use_Cases.Auth;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;

public class ConfirmStudentUseCase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUserRepository _userRepository;

    public ConfirmStudentUseCase(
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        ISchoolRepository schoolRepository,
        IUserRepository userRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _schoolRepository = schoolRepository;
        _userRepository = userRepository;
    }

    public async Task<(bool success, string message)> ExecuteAsync(
        int studentId,
        int? schoolId = null,
        int? teacherId = null)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
            return (false, "Student not found");

        if (schoolId.HasValue)
        {
            var school = await _schoolRepository.GetByIdAsync(schoolId.Value);
            if (school == null)
                return (false, "School not found");

            student.SchoolId = schoolId.Value;
        }

        if (teacherId.HasValue)
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId.Value);
            if (teacher == null)
                return (false, "Teacher not found");

            // Check if assignment already exists
            var existingAssignment = student.TeacherAssignments
                .FirstOrDefault(ta => ta.TeacherId == teacherId.Value);

            if (existingAssignment == null)
            {
                var assignment = new StudentTeacherAssignmentEntity
                {
                    StudentId = studentId,
                    TeacherId = teacherId.Value
                };
                student.TeacherAssignments.Add(assignment);
            }

            // For standalone mode
            if (teacher.SchoolId == null && student.StandaloneTeacherId == null)
            {
                student.StandaloneTeacherId = teacherId.Value;
            }
        }

        // Activate student account
        student.Status = AccountStatus.Active;
        var studentUser = await _userRepository.GetByIdAsync(student.UserId);
        if (studentUser != null)
        {
            studentUser.Status = AccountStatus.Active;
            await _userRepository.UpdateAsync(studentUser);
        }

        await _studentRepository.UpdateAsync(student);

        return (true, "Student confirmed and activated");
    }
}
