-- Seed default instructor: admin / Admin123!
-- Hash placeholder replaced at runtime by DatabaseInitializer
INSERT OR IGNORE INTO Instructors (Username, PasswordHash, DisplayName, Email, CreatedAt, UpdatedAt)
VALUES (
    'admin',
    '{{ADMIN_HASH}}',
    'Admin Instructor',
    'admin@weblearn.local',
    datetime('now'),
    datetime('now')
);

-- Seed course
INSERT OR IGNORE INTO Courses (Id, Title, Description, InstructorId, IsPublished, CreatedAt, UpdatedAt)
VALUES (
    1,
    'Computer Networks Fundamentals',
    'A comprehensive introduction to computer networking concepts, protocols, and technologies.',
    1,
    1,
    datetime('now'),
    datetime('now')
);

-- Seed units
INSERT OR IGNORE INTO Units (Id, CourseId, Title, Description, SortOrder, CreatedAt, UpdatedAt)
VALUES
(1, 1, 'Unit 1: Network Fundamentals', 'Core concepts of computer networking including the OSI model and network topologies.', 1, datetime('now'), datetime('now')),
(2, 1, 'Unit 2: Network Protocols', 'Essential networking protocols including TCP/IP, DNS, DHCP, HTTP, and HTTPS.', 2, datetime('now'), datetime('now'));

-- Seed lessons (XML content will be loaded from sample files or set as placeholder)
INSERT OR IGNORE INTO Lessons (Id, Title, XmlContent, InstructorId, CreatedAt, UpdatedAt)
VALUES
(1, 'Introduction to Networking', '<lesson title="Introduction to Networking" level="beginner"><objective>Understand what a computer network is</objective><objective>Identify the types of networks</objective><summary>This lesson introduces the basics of computer networking.</summary></lesson>', 1, datetime('now'), datetime('now')),
(2, 'The OSI Model', '<lesson title="The OSI Model" level="intermediate"><objective>Describe the 7 layers of the OSI model</objective><objective>Explain the function of each layer</objective><summary>The OSI model provides a framework for understanding network communication.</summary></lesson>', 1, datetime('now'), datetime('now')),
(3, 'Network Topologies', '<lesson title="Network Topologies" level="beginner"><objective>Identify common network topologies</objective><objective>Compare advantages and disadvantages of each topology</objective><summary>Network topology describes the arrangement of network elements.</summary></lesson>', 1, datetime('now'), datetime('now')),
(4, 'TCP/IP Protocol Suite', '<lesson title="TCP/IP Protocol Suite" level="intermediate"><objective>Understand the TCP/IP model layers</objective><objective>Explain how TCP ensures reliable delivery</objective><summary>TCP/IP is the foundational protocol suite of the internet.</summary></lesson>', 1, datetime('now'), datetime('now')),
(5, 'DNS and DHCP', '<lesson title="DNS and DHCP" level="intermediate"><objective>Explain how DNS resolves domain names</objective><objective>Describe DHCP address assignment</objective><summary>DNS and DHCP are essential services for network operation.</summary></lesson>', 1, datetime('now'), datetime('now')),
(6, 'HTTP and HTTPS', '<lesson title="HTTP and HTTPS" level="intermediate"><objective>Describe the HTTP request/response cycle</objective><objective>Explain how HTTPS provides security</objective><summary>HTTP and HTTPS are the protocols that power the web.</summary></lesson>', 1, datetime('now'), datetime('now'));

-- Assign lessons to units
INSERT OR IGNORE INTO UnitLessons (UnitId, LessonId, SortOrder)
VALUES
(1, 1, 1),
(1, 2, 2),
(1, 3, 3),
(2, 4, 1),
(2, 5, 2),
(2, 6, 3);
