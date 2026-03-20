# WebLearn

A web-based learning system where instructors can create structured courses with units and lessons. Lesson content is written in a custom XML format and rendered as interactive HTML with quizzes, definitions, code blocks, diagrams, and exercises. Students can browse published courses and work through lessons.

## Tech Stack

- **Framework:** ASP.NET Core 8.0 MVC (C#)
- **Database:** SQLite (via Dapper micro-ORM)
- **Authentication:** Session-based with BCrypt password hashing
- **Frontend:** Bootstrap 5.3, Bootstrap Icons
- **Migrations:** SQL scripts applied automatically on startup

## Lesson XML Format

Lessons are written in a custom XML schema. Here is a complete example:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<lesson title="Introduction to Networking" level="beginner">

  <objective>Define what a computer network is and explain its purpose</objective>
  <objective>Identify the main types of computer networks (LAN, WAN, MAN)</objective>

  <definition term="Computer Network">
    A collection of interconnected devices that can communicate and share resources.
  </definition>

  <note type="info">
    The internet is the world's largest computer network.
  </note>

  <note type="tip">
    Remember: LAN = Local, MAN = Metropolitan, WAN = Wide.
  </note>

  <note type="warning">
    Do not confuse the OSI model with the TCP/IP model.
  </note>

  <example title="Home Network">
    A typical home network includes a router, smartphones, laptops, and a printer
    — all connected and sharing an internet connection.
  </example>

  <diagram alt="Network Types" caption="Common network types by size">
    PAN (Personal) — ~10 meters
    LAN (Local)    — ~1 km
    MAN (Metro)    — ~50 km
    WAN (Wide)     — Global
  </diagram>

  <codeblock language="plaintext">
    ping 192.168.1.1
  </codeblock>

  <quiz title="Check Your Understanding">
    <question type="multiple-choice" text="Which network type covers a single building?">
      <answer correct="true">LAN</answer>
      <answer correct="false">WAN</answer>
      <answer correct="false">MAN</answer>
    </question>
    <question type="true-false" text="The Internet is an example of a WAN.">
      <answer correct="true">True</answer>
      <answer correct="false">False</answer>
    </question>
    <question type="short-answer" text="What does DNS stand for?">
      <answer correct="true">Domain Name System</answer>
    </question>
  </quiz>

  <exercise difficulty="easy">
    List all the network-connected devices in your home and identify the network type.
  </exercise>

  <summary>
    <item>A computer network connects devices to share data and resources</item>
    <item>Networks are classified by scope: PAN, LAN, MAN, WAN</item>
  </summary>
</lesson>
```

### Supported XML Elements

| Element | Attributes | Description |
|---|---|---|
| `<lesson>` | `title`, `level` (beginner/intermediate/advanced) | Root element |
| `<objective>` | — | Learning objective |
| `<definition>` | `term` | Term and definition |
| `<note>` | `type` (info/tip/warning) | Callout box |
| `<example>` | `title` | Worked example |
| `<diagram>` | `alt`, `caption`, `src` (optional) | ASCII diagram or image |
| `<codeblock>` | `language` | Code snippet |
| `<quiz>` | `title` | Quiz container |
| `<question>` | `type` (multiple-choice/true-false/short-answer), `text` | Quiz question |
| `<answer>` | `correct` (true/false) | Answer option |
| `<exercise>` | `difficulty` (easy/medium/hard) | Hands-on exercise |
| `<summary>` | — | Lesson summary with `<item>` children |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Install Dependencies and Run

```bash
# Clone the repository
git clone <repo-url>
cd WebLearn

# Run the application (dependencies are restored automatically)
dotnet run --project src/WebLearn
```

The app will start at **http://localhost:5084**. The SQLite database and all tables are created automatically on first run.

### Default Instructor Account

A seed account is created on first startup:

- **Username:** `admin`
- **Password:** `Admin123!`

Log in at `/Auth/Login` to access the instructor dashboard where you can create courses, units, and lessons.