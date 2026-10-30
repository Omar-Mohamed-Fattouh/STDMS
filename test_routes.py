"""Smoke-test every MVC GET route with an authenticated session.

Usage: python test_routes.py
Expects the app on http://localhost:5265 and the bootstrap admin below.
Creates no data; GET requests only.
"""
import re
import sys
import urllib.request
import urllib.parse
import http.cookiejar

BASE = "http://localhost:5265"
import os
EMAIL = os.environ.get("STDMS_TEST_EMAIL", "admin@example.org")
PASSWORD = os.environ.get("STDMS_TEST_PASSWORD", "")

cookies = {}


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


opener = urllib.request.build_opener(NoRedirect)


def store_cookies(headers):
    for raw in headers.get_all("Set-Cookie") or []:
        pair = raw.split(";", 1)[0]
        if "=" in pair:
            name, value = pair.split("=", 1)
            if value:
                cookies[name.strip()] = value.strip()
            else:
                cookies.pop(name.strip(), None)


def fetch(path, data=None):
    url = BASE + path if path.startswith("/") else path
    headers = {}
    if cookies:
        headers["Cookie"] = "; ".join(f"{k}={v}" for k, v in cookies.items())
    req = urllib.request.Request(url, data=data, headers=headers,
                                 method="POST" if data else "GET")
    try:
        with opener.open(req, timeout=20) as resp:
            store_cookies(resp.headers)
            return resp.status, resp.geturl(), resp.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        store_cookies(e.headers)
        body = e.read().decode("utf-8", "replace")
        if e.code in (301, 302, 303, 307, 308) and e.headers.get("Location"):
            loc = e.headers["Location"]
            loc = urllib.parse.urljoin(BASE, loc)
            # follow one redirect manually so cookies ride along
            return fetch(loc)
        return e.code, e.geturl(), body


# --- login -----------------------------------------------------------------
status, url, html = fetch("/Account/Login")
token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
assert token, "no antiforgery token on login page"
data = urllib.parse.urlencode({
    "Email": EMAIL, "Password": PASSWORD, "RememberMe": "false",
    "__RequestVerificationToken": token.group(1),
}).encode()
status, url, html = fetch("/Account/Login", data)
print(f"LOGIN -> {status} {url}")
if "Dashboard" not in html and "/Account/Login" in url:
    print("LOGIN FAILED")
    sys.exit(1)

ROUTES = [
    "/", "/Student", "/Student/Create", "/Student/Details/1", "/Student/Edit/1", "/Student/Delete/1",
    "/Department", "/Department/Create", "/Department/Details/1", "/Department/Edit/1", "/Department/Delete/1",
    "/Course", "/Course/Create", "/Course/Details/1", "/Course/Edit/1", "/Course/Delete/1",
    "/Instructor", "/Instructor/Create", "/Instructor/Details/1", "/Instructor/Edit/1", "/Instructor/Delete/1",
    "/Enrollment", "/Enrollment/Create", "/Enrollment/Details/1", "/Enrollment/Edit/1", "/Enrollment/Delete/1",
    "/Attendance", "/Attendance/Create", "/Attendance/Details/1", "/Attendance/Edit/1", "/Attendance/Delete/1",
    "/OfficeAssignment", "/OfficeAssignment/Create", "/OfficeAssignment/Details/1",
    "/OfficeAssignment/Edit/1", "/OfficeAssignment/Delete/1",
    "/Admin/Users", "/Admin/Users/Create", "/Admin/CourseAssignments",
    "/Home/Privacy", "/Student/Details/99999", "/totally-missing-page",
]

failures = 0
for route in ROUTES:
    status, url, html = fetch(route)
    title = re.search(r"<title>(.*?)</title>", html, re.S)
    title = title.group(1).strip() if title else "(no title)"
    # count real <option>s inside each <select> to spot empty dropdowns
    selects = re.findall(r"<select.*?</select>", html, re.S)
    empty_selects = sum(1 for s in selects if len(re.findall(r"<option[^>]*value=\"[^\"]+\"", s)) == 0 and "All " not in s and "filter" not in s.lower())
    flag = ""
    if status >= 500 or "Internal Server Error" in title or "unhandled exception" in html.lower():
        flag = "  <-- SERVER ERROR"
        failures += 1
    elif empty_selects:
        flag = f"  <-- {empty_selects} EMPTY DROPDOWN(S)"
        failures += 1
    print(f"{status} {route:38} {title:45} selects={len(selects)}{flag}")

print(f"\n{failures} problem route(s)")
sys.exit(1 if failures else 0)
