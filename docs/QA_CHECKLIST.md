# QA checklist

Record date, browser/device and result for each run.

| # | Scenario | Expected| Browser | |
|---|---|---|---|---|
| 1 | Register with weak password | Clear error message | ![alt text](image-1.png) | |
| 2 | Register, sign out, sign in | Works; wrong password shows error | ![alt text](image-3.png) | |
| 3 | Filter catalog by grade + wall + name | Results update; Reset clears filters | ![alt text](image-4.png) | |
| 4 | Navigate away and back to /routes | Filters are kept 
| 5 | Log climb from RouteCard | Journal opens with route preselected | ![alt text](image-5.png) | |
| 6 | Add climb with 0 attempts / future date | Validation message, nothing saved | ![alt text](image-6.png) | |
| 7 | Edit and delete a climb (with confirm) | List updates, message shown | ![alt text](image-7.png) | |
| 8 | Dashboard after logging sends | Numbers and chart update | ![alt text](image-8.png) | |
| 9 | Recommendations with history | Vn-Vn+1 routes | ![alt text](image-9.png) | |
| 12 | Admin creates duplicate route name | Friendly error | ![alt text](image-10.png) | |
