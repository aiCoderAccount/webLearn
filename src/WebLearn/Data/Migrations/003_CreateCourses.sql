CREATE TABLE IF NOT EXISTS Courses (
    Id INTEGER PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    InstructorId INTEGER NOT NULL,
    IsPublished INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (InstructorId) REFERENCES Instructors(Id)
);
