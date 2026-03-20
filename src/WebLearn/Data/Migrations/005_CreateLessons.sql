CREATE TABLE IF NOT EXISTS Lessons (
    Id INTEGER PRIMARY KEY,
    Title TEXT NOT NULL,
    XmlContent TEXT NOT NULL,
    InstructorId INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (InstructorId) REFERENCES Instructors(Id)
);
