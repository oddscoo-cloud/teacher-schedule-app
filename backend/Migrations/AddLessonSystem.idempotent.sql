IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502101225_InitialCreate'
)
BEGIN
    CREATE TABLE [Teachers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502101225_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Login] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [TeacherId] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502101225_InitialCreate'
)
BEGIN
    CREATE TABLE [Lessons] (
        [Id] int NOT NULL IDENTITY,
        [Day] nvarchar(max) NOT NULL,
        [Start] nvarchar(max) NOT NULL,
        [End] nvarchar(max) NOT NULL,
        [LessonName] nvarchar(max) NOT NULL,
        [TeacherId] int NOT NULL,
        CONSTRAINT [PK_Lessons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lessons_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502101225_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Lessons_TeacherId] ON [Lessons] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502101225_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502101225_InitialCreate', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502102652_Users'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Login', N'Password', N'Role', N'TeacherId') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([Id], [Login], [Password], [Role], [TeacherId])
    VALUES (1, N''admin'', N''admin'', N''admin'', NULL),
    (2, N''teacher1'', N''123'', N''teacher'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Login', N'Password', N'Role', N'TeacherId') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502102652_Users'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502102652_Users', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502112238_SeedUsers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502112238_SeedUsers', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502112751_SeedTeachers'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Teachers]'))
        SET IDENTITY_INSERT [Teachers] ON;
    EXEC(N'INSERT INTO [Teachers] ([Id], [Name])
    VALUES (1, N''John Doe''),
    (2, N''Jane Smith'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Teachers]'))
        SET IDENTITY_INSERT [Teachers] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502112751_SeedTeachers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502112751_SeedTeachers', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502121923_SeedTeacher3'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Login', N'Password', N'Role', N'TeacherId') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([Id], [Login], [Password], [Role], [TeacherId])
    VALUES (3, N''teacher2'', N''1234'', N''teacher'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Login', N'Password', N'Role', N'TeacherId') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502121923_SeedTeacher3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502121923_SeedTeacher3', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [Schools] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(255) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Schools] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [UserEntities] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(255) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] int NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_UserEntities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [TeacherEntities] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [SchoolId] int NULL,
        [FirstName] nvarchar(255) NOT NULL,
        [LastName] nvarchar(255) NOT NULL,
        [Bio] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TeacherEntities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeacherEntities_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_TeacherEntities_UserEntities_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [WorkspaceMemberships] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [SchoolId] int NULL,
        [WorkspaceType] int NOT NULL,
        [IsDefault] bit NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WorkspaceMemberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkspaceMemberships_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_WorkspaceMemberships_UserEntities_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserEntities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [StudentEntities] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [SchoolId] int NULL,
        [StandaloneTeacherId] int NULL,
        [FirstName] nvarchar(255) NOT NULL,
        [LastName] nvarchar(255) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentEntities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentEntities_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_StudentEntities_TeacherEntities_StandaloneTeacherId] FOREIGN KEY ([StandaloneTeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_StudentEntities_UserEntities_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE TABLE [StudentTeacherAssignments] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [TeacherId] int NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_StudentTeacherAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentTeacherAssignments_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentTeacherAssignments_TeacherEntities_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_StudentEntities_SchoolId] ON [StudentEntities] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_StudentEntities_StandaloneTeacherId] ON [StudentEntities] ([StandaloneTeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_StudentEntities_UserId] ON [StudentEntities] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentTeacherAssignments_StudentId_TeacherId] ON [StudentTeacherAssignments] ([StudentId], [TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_StudentTeacherAssignments_TeacherId] ON [StudentTeacherAssignments] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_TeacherEntities_SchoolId] ON [TeacherEntities] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_TeacherEntities_UserId] ON [TeacherEntities] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserEntities_Email] ON [UserEntities] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_WorkspaceMemberships_SchoolId] ON [WorkspaceMemberships] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    CREATE INDEX [IX_WorkspaceMemberships_UserId] ON [WorkspaceMemberships] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113105704_InitializeCleanArchitecture'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261113105704_InitializeCleanArchitecture', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261113110108_AddTeacherManagementControllers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261113110108_AddTeacherManagementControllers', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    ALTER TABLE [StudentTeacherAssignments] ADD [IsSchool] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentEntities]') AND [c].[name] = N'UserId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [StudentEntities] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [StudentEntities] ALTER COLUMN [UserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    ALTER TABLE [StudentEntities] ADD [DateOfBirth] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    ALTER TABLE [StudentEntities] ADD [Email] nvarchar(255) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    ALTER TABLE [StudentEntities] ADD [Patronymic] nvarchar(255) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    UPDATE student
    SET Email = [user].Email
    FROM StudentEntities AS student
    INNER JOIN UserEntities AS [user] ON [user].Id = student.UserId
    WHERE student.Email IS NULL;

    UPDATE assignment
    SET IsSchool = 1
    FROM StudentTeacherAssignments AS assignment
    INNER JOIN StudentEntities AS student ON student.Id = assignment.StudentId
    INNER JOIN TeacherEntities AS teacher ON teacher.Id = assignment.TeacherId
    WHERE student.SchoolId IS NOT NULL AND teacher.SchoolId = student.SchoolId;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE TABLE [Contacts] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(255) NOT NULL,
        [LastName] nvarchar(255) NOT NULL,
        [Email] nvarchar(255) NULL,
        [PhoneNumber] nvarchar(50) NULL,
        CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [SchoolId] int NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE TABLE [StudentNotes] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [TeacherId] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Goals] nvarchar(max) NULL,
        [Progress] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_StudentNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentNotes_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentNotes_TeacherEntities_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE TABLE [StudentContacts] (
        [StudentId] int NOT NULL,
        [ContactId] int NOT NULL,
        [Relationship] nvarchar(100) NULL,
        CONSTRAINT [PK_StudentContacts] PRIMARY KEY ([StudentId], [ContactId]),
        CONSTRAINT [FK_StudentContacts_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentContacts_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE TABLE [StudentCourseAssignmentEntity] (
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        CONSTRAINT [PK_StudentCourseAssignmentEntity] PRIMARY KEY ([StudentId], [CourseId]),
        CONSTRAINT [FK_StudentCourseAssignmentEntity_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentCourseAssignmentEntity_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE INDEX [IX_StudentEntities_Email] ON [StudentEntities] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Courses_SchoolId_Name] ON [Courses] ([SchoolId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE INDEX [IX_StudentContacts_ContactId] ON [StudentContacts] ([ContactId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE INDEX [IX_StudentCourseAssignmentEntity_CourseId] ON [StudentCourseAssignmentEntity] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE INDEX [IX_StudentNotes_StudentId_TeacherId] ON [StudentNotes] ([StudentId], [TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    CREATE INDEX [IX_StudentNotes_TeacherId] ON [StudentNotes] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118160954_AddStudentManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118160954_AddStudentManagement', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    EXEC(N'DELETE FROM [Teachers]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    EXEC(N'DELETE FROM [Teachers]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    EXEC(N'DELETE FROM [Users]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    EXEC(N'DELETE FROM [Users]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    EXEC(N'DELETE FROM [Users]
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120132948_RemoveLegacyDemoAccounts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261120132948_RemoveLegacyDemoAccounts', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE TABLE [Homeworks] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [TeacherId] int NOT NULL,
        [SchoolId] int NULL,
        [CourseId] int NULL,
        [Title] nvarchar(255) NOT NULL,
        [Instructions] nvarchar(4000) NULL,
        [DueAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Homeworks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Homeworks_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Homeworks_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Homeworks_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Homeworks_TeacherEntities_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE TABLE [HomeworkSubmissions] (
        [Id] int NOT NULL IDENTITY,
        [HomeworkId] int NOT NULL,
        [AttemptNumber] int NOT NULL,
        [Text] nvarchar(max) NULL,
        [LinksJson] nvarchar(max) NOT NULL,
        [Grade] int NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [GradedAt] datetime2 NULL,
        CONSTRAINT [PK_HomeworkSubmissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HomeworkSubmissions_Homeworks_HomeworkId] FOREIGN KEY ([HomeworkId]) REFERENCES [Homeworks] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE TABLE [HomeworkAttachments] (
        [Id] int NOT NULL IDENTITY,
        [SubmissionId] int NOT NULL,
        [StorageName] nvarchar(80) NOT NULL,
        [DisplayName] nvarchar(255) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [SizeBytes] bigint NOT NULL,
        CONSTRAINT [PK_HomeworkAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HomeworkAttachments_HomeworkSubmissions_SubmissionId] FOREIGN KEY ([SubmissionId]) REFERENCES [HomeworkSubmissions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE INDEX [IX_HomeworkAttachments_SubmissionId] ON [HomeworkAttachments] ([SubmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE INDEX [IX_Homeworks_CourseId] ON [Homeworks] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE INDEX [IX_Homeworks_SchoolId] ON [Homeworks] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE INDEX [IX_Homeworks_StudentId_CreatedAt] ON [Homeworks] ([StudentId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE INDEX [IX_Homeworks_TeacherId_CourseId] ON [Homeworks] ([TeacherId], [CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HomeworkSubmissions_HomeworkId_AttemptNumber] ON [HomeworkSubmissions] ([HomeworkId], [AttemptNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070531_AddHomeworkManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121070531_AddHomeworkManagement', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE TABLE [StudentLessonCharges] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [SourceLessonId] int NOT NULL,
        [ChargeType] int NOT NULL,
        [LessonStartsAt] datetimeoffset NOT NULL,
        [ChargedAt] datetimeoffset NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [RecordedByUserId] int NOT NULL,
        CONSTRAINT [PK_StudentLessonCharges] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentLessonCharges_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentLessonCharges_UserEntities_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE TABLE [StudentPayments] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaidLessonCount] int NOT NULL,
        [PaidAt] datetimeoffset NOT NULL,
        [RecordedAt] datetimeoffset NOT NULL,
        [RecordedByUserId] int NOT NULL,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_StudentPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentPayments_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentPayments_UserEntities_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE TABLE [StudentPriceHistory] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [EffectiveFrom] datetimeoffset NOT NULL,
        [RecordedAt] datetimeoffset NOT NULL,
        [RecordedByUserId] int NOT NULL,
        CONSTRAINT [PK_StudentPriceHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentPriceHistory_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentPriceHistory_UserEntities_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE TABLE [StudentPaymentAdjustments] (
        [Id] int NOT NULL IDENTITY,
        [PaymentId] int NOT NULL,
        [AmountDelta] decimal(18,2) NOT NULL,
        [PaidLessonCountDelta] int NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [RecordedAt] datetimeoffset NOT NULL,
        [RecordedByUserId] int NOT NULL,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_StudentPaymentAdjustments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentPaymentAdjustments_StudentPayments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [StudentPayments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentPaymentAdjustments_UserEntities_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE INDEX [IX_StudentLessonCharges_RecordedByUserId] ON [StudentLessonCharges] ([RecordedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentLessonCharges_StudentId_SourceLessonId] ON [StudentLessonCharges] ([StudentId], [SourceLessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentPaymentAdjustments_PaymentId_IdempotencyKey] ON [StudentPaymentAdjustments] ([PaymentId], [IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE INDEX [IX_StudentPaymentAdjustments_RecordedByUserId] ON [StudentPaymentAdjustments] ([RecordedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE INDEX [IX_StudentPayments_RecordedByUserId] ON [StudentPayments] ([RecordedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentPayments_StudentId_IdempotencyKey] ON [StudentPayments] ([StudentId], [IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE INDEX [IX_StudentPriceHistory_RecordedByUserId] ON [StudentPriceHistory] ([RecordedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StudentPriceHistory_StudentId_EffectiveFrom] ON [StudentPriceHistory] ([StudentId], [EffectiveFrom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121070839_AddStudentFinance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121070839_AddStudentFinance', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignmentEntity] DROP CONSTRAINT [FK_StudentCourseAssignmentEntity_Courses_CourseId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignmentEntity] DROP CONSTRAINT [FK_StudentCourseAssignmentEntity_StudentEntities_StudentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignmentEntity] DROP CONSTRAINT [PK_StudentCourseAssignmentEntity];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    EXEC sp_rename N'[StudentCourseAssignmentEntity]', N'StudentCourseAssignments', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    EXEC sp_rename N'[StudentCourseAssignments].[IX_StudentCourseAssignmentEntity_CourseId]', N'IX_StudentCourseAssignments_CourseId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignments] ADD CONSTRAINT [PK_StudentCourseAssignments] PRIMARY KEY ([StudentId], [CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE TABLE [LessonSeries] (
        [Id] int NOT NULL IDENTITY,
        [TeacherId] int NOT NULL,
        [StudentId] int NOT NULL,
        [SchoolId] int NULL,
        [CourseId] int NULL,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [Duration] time NOT NULL,
        [Title] nvarchar(255) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [Timezone] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_LessonSeries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LessonSeries_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonSeries_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonSeries_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonSeries_TeacherEntities_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE TABLE [LessonEvents] (
        [Id] int NOT NULL IDENTITY,
        [SeriesId] int NULL,
        [TeacherId] int NOT NULL,
        [StudentId] int NOT NULL,
        [SchoolId] int NULL,
        [CourseId] int NULL,
        [ScheduledAtUtc] datetime2 NOT NULL,
        [Title] nvarchar(255) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [Status] int NOT NULL,
        [Duration] time NOT NULL,
        [Timezone] nvarchar(100) NOT NULL,
        [Notes] nvarchar(4000) NULL,
        [StatusReason] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [ModifiedAtUtc] datetime2 NOT NULL,
        [ModifiedByUserId] int NULL,
        [RescheduledFromId] int NULL,
        [SeriesInstanceIndex] int NULL,
        CONSTRAINT [PK_LessonEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LessonEvents_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_LessonEvents_RescheduledFromId] FOREIGN KEY ([RescheduledFromId]) REFERENCES [LessonEvents] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_LessonSeries_SeriesId] FOREIGN KEY ([SeriesId]) REFERENCES [LessonSeries] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_TeacherEntities_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherEntities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonEvents_UserEntities_ModifiedByUserId] FOREIGN KEY ([ModifiedByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE TABLE [LessonReschedules] (
        [Id] int NOT NULL IDENTITY,
        [OriginalLessonId] int NOT NULL,
        [NewLessonId] int NOT NULL,
        [RescheduleReason] nvarchar(500) NULL,
        [RescheduleDateUtc] datetime2 NOT NULL,
        [RescheduleByUserId] int NOT NULL,
        CONSTRAINT [PK_LessonReschedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LessonReschedules_LessonEvents_NewLessonId] FOREIGN KEY ([NewLessonId]) REFERENCES [LessonEvents] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LessonReschedules_LessonEvents_OriginalLessonId] FOREIGN KEY ([OriginalLessonId]) REFERENCES [LessonEvents] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LessonReschedules_UserEntities_RescheduleByUserId] FOREIGN KEY ([RescheduleByUserId]) REFERENCES [UserEntities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_CourseId] ON [LessonEvents] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_ModifiedByUserId] ON [LessonEvents] ([ModifiedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_RescheduledFromId] ON [LessonEvents] ([RescheduledFromId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_SchoolId] ON [LessonEvents] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LessonEvents_SeriesId_SeriesInstanceIndex] ON [LessonEvents] ([SeriesId], [SeriesInstanceIndex]) WHERE [SeriesId] IS NOT NULL AND [SeriesInstanceIndex] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_SeriesId_Status] ON [LessonEvents] ([SeriesId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_StudentId] ON [LessonEvents] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonEvents_TeacherId_StudentId_ScheduledAtUtc] ON [LessonEvents] ([TeacherId], [StudentId], [ScheduledAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LessonReschedules_NewLessonId] ON [LessonReschedules] ([NewLessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LessonReschedules_OriginalLessonId] ON [LessonReschedules] ([OriginalLessonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonReschedules_RescheduleByUserId] ON [LessonReschedules] ([RescheduleByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonSeries_CourseId] ON [LessonSeries] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonSeries_SchoolId] ON [LessonSeries] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonSeries_StudentId] ON [LessonSeries] ([StudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    CREATE INDEX [IX_LessonSeries_TeacherId_StudentId_StartDate] ON [LessonSeries] ([TeacherId], [StudentId], [StartDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignments] ADD CONSTRAINT [FK_StudentCourseAssignments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    ALTER TABLE [StudentCourseAssignments] ADD CONSTRAINT [FK_StudentCourseAssignments_StudentEntities_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [StudentEntities] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261121085749_AddLessonSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121085749_AddLessonSystem', N'10.0.0');
END;

COMMIT;
GO
