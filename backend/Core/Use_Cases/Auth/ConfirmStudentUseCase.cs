namespace backend.Core.Use_Cases.Auth;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;

public class ConfirmStudentUseCase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceMembershipRepository _membershipRepository;

    public ConfirmStudentUseCase(
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        ISchoolRepository schoolRepository,
        IUserRepository userRepository,
        IWorkspaceMembershipRepository membershipRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _schoolRepository = schoolRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
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
            if (student.SchoolId.HasValue && student.SchoolId != schoolId ||
                student.StandaloneTeacherId.HasValue && !student.SchoolId.HasValue)
                return (false, "Student belongs to another workspace");

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

            if (schoolId.HasValue)
            {
                if (teacher.SchoolId != schoolId)
                    return (false, "Teacher does not belong to this school");

                var teacherMembership = await _membershipRepository.GetUserSchoolMembershipAsync(
                    teacher.UserId,
                    schoolId.Value);
                if (teacherMembership == null)
                    return (false, "Teacher is not a member of this school");
            }
            else if (teacher.SchoolId.HasValue ||
                     !(await _membershipRepository.GetByUserIdAsync(teacher.UserId))
                         .Any(membership => membership.WorkspaceType == WorkspaceType.Standalone))
            {
                return (false, "Teacher is not a member of this standalone workspace");
            }

            if (!schoolId.HasValue &&
                (student.TeacherAssignments.Any(assignment => assignment.IsSchool) ||
                 student.TeacherAssignments.Any(assignment => !assignment.IsSchool && assignment.TeacherId != teacherId) ||
                 student.StandaloneTeacherId.HasValue && student.StandaloneTeacherId != teacherId))
                return (false, "Student belongs to another workspace");

            // Check if assignment already exists
            var existingAssignment = student.TeacherAssignments
                .FirstOrDefault(ta => ta.TeacherId == teacherId.Value);

            if (existingAssignment == null)
            {
                var assignment = new StudentAssignmentEntity
                {
                    StudentId = studentId,
                    TeacherId = teacherId.Value,
                    IsSchool = schoolId.HasValue,
                    AssignedAt = DateTime.UtcNow
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
        var studentUser = student.UserId.HasValue
            ? await _userRepository.GetByIdAsync(student.UserId.Value)
            : null;
        if (studentUser != null)
        {
            studentUser.Status = AccountStatus.Active;
            await _userRepository.UpdateAsync(studentUser);
        }

        await _studentRepository.UpdateAsync(student);

        return (true, "Student confirmed and activated");
    }
}
