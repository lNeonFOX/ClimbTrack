# QA checklist

Record date, browser/device and result for each run.

| # | Scenario | Expected | Chrome | Firefox | Edge | Phone |
|---|---|---|---|---|---|---|
| 1 | Register with weak password | Clear error message |  |  |  |  |
| 2 | Register, sign out, sign in | Works; wrong password shows error |  |  |  |  |
| 3 | Open /journal signed out | Redirect to login, then back to /journal |  |  |  |  |
| 4 | Filter catalog by grade + wall + name | Results update; Reset clears filters |  |  |  |  |
| 5 | Navigate away and back to /routes | Filters are kept |  |  |  |  |
| 6 | Log climb from RouteCard | Journal opens with route preselected |  |  |  |  |
| 7 | Add climb with 0 attempts / future date | Validation message, nothing saved |  |  |  |  |
| 8 | Edit and delete a climb (with confirm) | List updates, message shown |  |  |  |  |
| 9 | Dashboard after logging sends | Numbers and chart update |  |  |  |  |
| 10 | Recommendations with empty history | V0-V1 routes |  |  |  |  |
| 11 | Two users: user B cannot see user A's data | Confirmed via journal and /api/climbs |  |  |  |  |
| 12 | Normal user opens /admin/routes | Access denied message |  |  |  |  |
| 13 | Admin creates duplicate route name | Friendly error |  |  |  |  |
| 14 | Resize to 375 px width | No horizontal scroll; menu collapses |  |  |  |  |
| 15 | Keyboard only (Tab / Enter) | All actions reachable, focus visible |  |  |  |  |
| 16 | Unknown URL | Not-found page |  |  |  |  |
