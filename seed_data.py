"""Seed the STDMS LocalDB with demo data (90+ students and related records).

Usage:  py -3.13 seed_data.py
Safe to re-run: exits early if the seed students already exist.
Requires: pip install pyodbc, and the app database (STDMSDB) migrated.
"""
import random
import pyodbc
from datetime import date, timedelta

random.seed(42)

cn = pyodbc.connect(
    "DRIVER={ODBC Driver 17 for SQL Server};SERVER=(localdb)\\MSSQLLocalDB;"
    "DATABASE=STDMSDB;Trusted_Connection=yes;TrustServerCertificate=yes;",
    timeout=10,
)
cur = cn.cursor()

cur.execute("SELECT COUNT(*) FROM Students WHERE Email LIKE 'student%@stdms.local'")
if cur.fetchone()[0] >= 90:
    print("Seed data already present - nothing to do.")
    raise SystemExit

# --- Departments -----------------------------------------------------------
new_departments = ["Mathematics", "Physics", "Chemistry",
                   "Engineering", "Medicine", "Literature"]
dept_ids = [r[0] for r in cur.execute("SELECT Id FROM Departments").fetchall()]
for name in new_departments:
    cur.execute("INSERT INTO Departments (Name) OUTPUT INSERTED.Id VALUES (?)", name)
    dept_ids.append(int(cur.fetchone()[0]))

# --- Instructors ------------------------------------------------------------
first = ["Ahmed", "Sara", "Mohamed", "Fatma", "Omar", "Mona", "Khaled", "Nour",
         "Ali", "Huda", "Youssef", "Laila", "Tarek", "Dina", "Karim", "Salma",
         "Hassan", "Rania", "Mostafa", "Aya", "Sherif", "Nadia", "Adel", "Iman",
         "Walid", "Heba", "Samir", "Ghada", "Amr", "Rasha"]
last = ["Hassan", "Ibrahim", "Ali", "Mahmoud", "Youssef", "Khalil", "Nasser",
        "Aziz", "Farouk", "Mansour", "Saleh", "Hamdy", "Sharif", "Adel",
        "Rashad", "Fathy", "Osman", "Barakat", "Hegazy", "Soliman", "Naguib",
        "Zaki", "Helmy", "Samaha", "Fikry", "Gaber", "Metwally", "Shawky"]

instructor_ids = []
for i in range(18):
    name = f"Dr. {random.choice(first)} {random.choice(last)}"
    hire = date(2008, 1, 1) + timedelta(days=random.randint(0, 6000))
    cur.execute("INSERT INTO Instructors (Name, HireDate) OUTPUT INSERTED.Id VALUES (?, ?)", name, hire)
    instructor_ids.append(int(cur.fetchone()[0]))

# --- Office assignments (one per instructor) --------------------------------
buildings = ["Main Building", "Science Hall", "Engineering Block", "Medical Wing", "Arts Tower"]
for iid in instructor_ids:
    loc = f"{random.choice(buildings)} - Room {random.randint(101, 430)}"
    cur.execute("INSERT INTO OfficeAssignments (InstructorId, Location) VALUES (?, ?)", iid, loc)

# --- Courses -----------------------------------------------------------------
course_names = ["Database Systems", "Data Structures", "Operating Systems",
                "Computer Networks", "Linear Algebra", "Calculus II",
                "Organic Chemistry", "Quantum Physics", "Microeconomics",
                "Marketing Principles", "Human Anatomy", "Thermodynamics",
                "Signals and Systems", "Modern Poetry", "Statistics",
                "Software Engineering", "Machine Learning", "Ethics in Medicine"]
course_ids = []
for name in course_names:
    cur.execute("INSERT INTO Courses (Name, Credits) OUTPUT INSERTED.Id VALUES (?, ?)", name, random.choice([2, 3, 3, 4]))
    course_ids.append(int(cur.fetchone()[0]))

# --- Students (90) ------------------------------------------------------------
student_ids = []
base_dob = date(1999, 1, 1)
used_names = set()
for i in range(1, 91):
    while True:
        name = f"{random.choice(first)} {random.choice(last)}"
        if name not in used_names:
            used_names.add(name)
            break
    email = f"student{i:03d}@stdms.local"
    dob = base_dob + timedelta(days=random.randint(0, 2900))
    cur.execute("INSERT INTO Students (Name, Email, DateOfBirth, DepartmentId) OUTPUT INSERTED.Id VALUES (?, ?, ?, ?)", name, email, dob, random.choice(dept_ids))
    student_ids.append(int(cur.fetchone()[0]))

# --- Enrollments (2-3 courses per student) ------------------------------------
grades = ["A", "A-", "B+", "B", "B-", "C+", "C", "IP", "IP"]
enrollments = []
for sid in student_ids:
    for cid in random.sample(course_ids, random.randint(2, 3)):
        cur.execute("INSERT INTO Enrollments (StudentId, CourseId, Grade) VALUES (?, ?, ?)",
                    sid, cid, random.choice(grades))
        enrollments.append((sid, cid))

# --- Attendance (6 sessions per enrollment, ~90% present) ---------------------
attendance_rows = 0
for sid, cid in enrollments:
    for w in range(6):
        d = date(2026, 9, 6) + timedelta(days=7 * w + random.randint(0, 2))
        cur.execute("INSERT INTO Attendances (StudentId, CourseId, Date, IsPresent) VALUES (?, ?, ?, ?)",
                    sid, cid, d, 1 if random.random() < 0.9 else 0)
        attendance_rows += 1

cn.commit()
for t in ["Departments", "Instructors", "Courses", "Students", "Enrollments", "Attendances", "OfficeAssignments"]:
    cur.execute(f"SELECT COUNT(*) FROM [{t}]")
    print(f"{t}: {cur.fetchone()[0]}")
print(f"Attendance rows added: {attendance_rows}")
print("Seed complete.")

