"""POST-level test: create + delete a throwaway student and enrollment."""
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

def token(html):
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    assert m, "no antiforgery token"; return m.group(1)

def post_form(path, fields, html_for_token=None):
    _, _, html = fetch(path) if html_for_token is None else (0, "", html_for_token)
    fields = {**fields, "__RequestVerificationToken": token(html)}
    return fetch(path, urllib.parse.urlencode(fields).encode())

# login
s, u, html = fetch("/Account/Login")
s, u, html = fetch("/Account/Login", urllib.parse.urlencode({
    "Email": os.environ.get("STDMS_TEST_EMAIL", "admin@example.org"),
    "Password": os.environ.get("STDMS_TEST_PASSWORD", ""),
    "RememberMe": "false", "__RequestVerificationToken": token(html)}).encode())
assert "Dashboard" in html, "login failed"
print("login OK")

# create student
s, u, html = post_form("/Student/Create", {
    "Name": "Smoke Test Student", "Email": "smoke.test@example.com",
    "DateOfBirth": "2002-03-04", "DepartmentId": "1"})
ok = u.rstrip("/").endswith("/Student")  # successful POST redirects to the list
print(f"create student -> {s} {u} redirected_to_list={ok}")
if not ok:
    msgs = re.findall(r'<span[^>]*text-danger[^>]*>([^<]*)</span>', html)
    summary = re.search(r'validation-summary[^>]*>(.*?)</div>', html, re.S)
    print("field errors:", [m for m in msgs if m.strip()])
    print("summary:", re.sub(r'<[^>]+>', ' ', summary.group(1)).strip() if summary else "(none)")
    sys.exit(1)

# find new student id from its Details link
s, u, html = fetch("/Student?search=Smoke")
m = re.search(r'/Student/Details/(\d+)', html)
assert m, "new student not findable via search"; sid = m.group(1)
print(f"student id = {sid}")

# create enrollment for that student in course 1
s, u, html = post_form("/Enrollment/Create", {
    "StudentId": sid, "CourseId": "1", "Grade": "A"})
ok = "/Enrollment" in u
print(f"create enrollment -> {s} {u}")
s, u, html = fetch(f"/Enrollment?search=Smoke")
m2 = re.search(r'/Enrollment/Details/(\d+)', html)
print(f"enrollment listed={'Smoke Test Student' in html} id={m2.group(1) if m2 else '?'}")
if m2:
    eid = m2.group(1)
    _, _, del_html = fetch(f"/Enrollment/Delete/{eid}")
    s, u, html = fetch(f"/Enrollment/Delete/{eid}", urllib.parse.urlencode(
        {"__RequestVerificationToken": token(del_html)}).encode())
    print(f"delete enrollment -> {s} {u}")

# delete the throwaway student
_, _, del_html = fetch(f"/Student/Delete/{sid}")
s, u, html = fetch(f"/Student/Delete/{sid}", urllib.parse.urlencode(
    {"__RequestVerificationToken": token(del_html)}).encode())
s2, u2, html2 = fetch("/Student?search=Smoke")
gone = "Smoke Test Student" not in html2
print(f"delete student -> {s} {u} gone={gone}")
print("POST SMOKE TEST PASSED" if gone else "CLEANUP FAILED")
sys.exit(0 if gone else 1)
