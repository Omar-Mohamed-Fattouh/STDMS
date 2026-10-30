"""Full CRUD smoke test for every module. Creates throwaway records and
deletes them again, leaving the seeded data untouched."""
import os, re, sys, urllib.request, urllib.parse

BASE = "http://localhost:5265"
cookies = {}

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

opener = urllib.request.build_opener(NoRedirect)

def store(headers):
    for raw in headers.get_all("Set-Cookie") or []:
        pair = raw.split(";", 1)[0]
        if "=" in pair:
            n, v = pair.split("=", 1)
            cookies[n.strip()] = v.strip() if v else cookies.pop(n.strip(), None)

def fetch(path, data=None):
    url = BASE + path if path.startswith("/") else path
    h = {"Cookie": "; ".join(f"{k}={v}" for k, v in cookies.items())} if cookies else {}
    req = urllib.request.Request(url, data=data, headers=h, method="POST" if data else "GET")
    try:
        with opener.open(req, timeout=20) as r:
            store(r.headers); return r.status, r.geturl(), r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        store(e.headers); body = e.read().decode("utf-8", "replace")
        if e.code in (301,302,303,307,308) and e.headers.get("Location"):
            return fetch(urllib.parse.urljoin(BASE, e.headers["Location"]))
        return e.code, e.geturl(), body

def tok(html):
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    assert m, "no token"; return m.group(1)

def create(path, fields):
    _, _, html = fetch(path)
    return fetch(path, urllib.parse.urlencode({**fields, "__RequestVerificationToken": tok(html)}).encode())

def delete(path, id_):
    _, _, html = fetch(f"{path}/Delete/{id_}")
    return fetch(f"{path}/Delete/{id_}", urllib.parse.urlencode({"__RequestVerificationToken": tok(html)}).encode())

def find_id(list_url, pattern):
    _, _, html = fetch(list_url)
    m = re.search(pattern, html)
    return m.group(1) if m else None

fails = []
def check(name, cond):
    print(("PASS " if cond else "FAIL ") + name)
    if not cond: fails.append(name)

# login
_, _, html = fetch("/Account/Login")
s, u, html = fetch("/Account/Login", urllib.parse.urlencode({
    "Email": os.environ.get("STDMS_TEST_EMAIL", "admin@example.org"),
    "Password": os.environ.get("STDMS_TEST_PASSWORD", ""),
    "RememberMe": "false", "__RequestVerificationToken": tok(html)}).encode())
check("login", "Dashboard" in html)

# Department
s, u, _ = create("/Department/Create", {"Name": "Smoke Dept"})
did = find_id("/Department", r"/Department/Details/(\d+)\"[^>]*>Details")
# details links are ordered; find the one on the Smoke Dept row instead
_, _, html = fetch("/Department")
m = re.search(r"Smoke Dept.*?/Department/Details/(\d+)", html, re.S)
did = m.group(1) if m else did
check("create department", bool(m))
if did: s, u, _ = delete("/Department", did); check("delete department", find_id("/Department", r"Smoke Dept") is None)

# Course
s, u, _ = create("/Course/Create", {"Name": "Smoke Course", "Credits": "3"})
_, _, html = fetch("/Course")
m = re.search(r"Smoke Course.*?/Course/Details/(\d+)", html, re.S)
check("create course", bool(m))
cid = m.group(1) if m else None
if cid: delete("/Course", cid); check("delete course", find_id("/Course", r"Smoke Course") is None)

# Instructor + OfficeAssignment
s, u, _ = create("/Instructor/Create", {"Name": "Dr. Smoke Test", "HireDate": "2020-01-01"})
_, _, html = fetch("/Instructor")
m = re.search(r"Dr\. Smoke Test.*?/Instructor/(?:Details|Edit)/(\d+)", html, re.S)
check("create instructor", bool(m))
iid = m.group(1) if m else None
if iid:
    s, u, html = create("/OfficeAssignment/Create", {"InstructorId": iid, "Location": "Smoke Room 1"})
    check("create office assignment", "Smoke Room 1" in fetch("/OfficeAssignment")[2])
    delete("/OfficeAssignment", iid)
    check("delete office assignment", "Smoke Room 1" not in fetch("/OfficeAssignment")[2])
    delete("/Instructor", iid)
    check("delete instructor", "Dr. Smoke Test" not in fetch("/Instructor")[2])

# Attendance (against seeded student 1 / course 1), then delete it
before = fetch("/Attendance")[2].count("Details")
s, u, html = create("/Attendance/Create", {"StudentId": "1", "CourseId": "1", "Date": "2026-10-30", "IsPresent": "true"})
_, _, html = fetch("/Attendance?courseId=1&date=2026-10-30")
m = re.search(r"/Attendance/Details/(\d+)", html)
check("create attendance", bool(m))
if m:
    delete("/Attendance", m.group(1))
    check("delete attendance", "/Attendance/Details/" + m.group(1) not in fetch("/Attendance?courseId=1&date=2026-10-30")[2])

print("\n" + ("ALL CRUD TESTS PASSED" if not fails else f"FAILURES: {fails}"))
sys.exit(1 if fails else 0)
