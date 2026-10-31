"""Log in and write lighthouse/headers.json with the auth cookie for Lighthouse --extra-headers."""
import json, os, re, urllib.request, urllib.parse

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
            cookies[n.strip()] = v.strip()

def fetch(path, data=None):
    url = BASE + path if path.startswith("/") else path
    h = {"Cookie": "; ".join(f"{k}={v}" for k, v in cookies.items())} if cookies else {}
    req = urllib.request.Request(url, data=data, headers=h, method="POST" if data else "GET")
    try:
        with opener.open(req, timeout=20) as r:
            store(r.headers)
            return r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        store(e.headers)
        if e.code in (301, 302, 303, 307, 308) and e.headers.get("Location"):
            return fetch(urllib.parse.urljoin(BASE, e.headers["Location"]))
        return e.read().decode("utf-8", "replace")

html = fetch("/Account/Login")
token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html).group(1)
html = fetch("/Account/Login", urllib.parse.urlencode({
    "Email": os.environ.get("STDMS_TEST_EMAIL", "admin@example.org"),
    "Password": os.environ.get("STDMS_TEST_PASSWORD", ""),
    "RememberMe": "false", "__RequestVerificationToken": token}).encode())
assert "Dashboard" in html, "login failed"
cookie_header = "; ".join(f"{k}={v}" for k, v in cookies.items())
with open(os.path.join(os.path.dirname(__file__), "headers.json"), "w") as f:
    json.dump({"Cookie": cookie_header}, f)
print("headers.json written, cookies:", ", ".join(cookies.keys()))
