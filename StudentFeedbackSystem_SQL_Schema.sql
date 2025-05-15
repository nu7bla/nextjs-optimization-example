-- SQL Server Database Schema for Student Feedback System in 3NF

-- Users table to store all users (students, teachers, admins)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NULL, -- Optional if using login code only
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('student', 'teacher', 'admin')),
    LoginCode NVARCHAR(20) UNIQUE NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Students table
CREATE TABLE Students (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Teachers table
CREATE TABLE Teachers (
    TeacherID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Subjects table
CREATE TABLE Subjects (
    SubjectID INT IDENTITY(1,1) PRIMARY KEY,
    SubjectName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);

-- Enrollments table linking students to subjects
CREATE TABLE Enrollments (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    SubjectID INT NOT NULL,
    FOREIGN KEY (StudentID) REFERENCES Students(StudentID),
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID),
    UNIQUE (StudentID, SubjectID)
);

-- SubjectTeachers table linking teachers to subjects
CREATE TABLE SubjectTeachers (
    SubjectTeacherID INT IDENTITY(1,1) PRIMARY KEY,
    SubjectID INT NOT NULL,
    TeacherID INT NOT NULL,
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID),
    FOREIGN KEY (TeacherID) REFERENCES Teachers(TeacherID),
    UNIQUE (SubjectID, TeacherID)
);

-- Feedback table to store feedback submissions
CREATE TABLE Feedback (
    FeedbackID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    TeacherID INT NOT NULL,
    SubmittedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Feedback UNIQUE (StudentID, TeacherID),
    FOREIGN KEY (StudentID) REFERENCES Students(StudentID),
    FOREIGN KEY (TeacherID) REFERENCES Teachers(TeacherID)
);

-- FeedbackAnswers table to store answers to 10 questions per feedback
CREATE TABLE FeedbackAnswers (
    AnswerID INT IDENTITY(1,1) PRIMARY KEY,
    FeedbackID INT NOT NULL,
    QuestionNumber INT NOT NULL CHECK (QuestionNumber BETWEEN 1 AND 10),
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    FOREIGN KEY (FeedbackID) REFERENCES Feedback(FeedbackID),
    UNIQUE (FeedbackID, QuestionNumber)
);

-- FeedbackComments table to store optional anonymous comments
CREATE TABLE FeedbackComments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    FeedbackID INT NOT NULL UNIQUE,
    CommentText NVARCHAR(MAX) NULL,
    FOREIGN KEY (FeedbackID) REFERENCES Feedback(FeedbackID)
);

-- Indexes for performance
CREATE INDEX IDX_Feedback_TeacherID ON Feedback(TeacherID);
CREATE INDEX IDX_FeedbackAnswers_FeedbackID ON FeedbackAnswers(FeedbackID);
CREATE INDEX IDX_FeedbackComments_FeedbackID ON FeedbackComments(FeedbackID);
