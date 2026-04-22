# Postman Testing - Start Here

## What You Need

| Item | Location |
|---|---|
| Postman collection | StudentAssessmentTracker.postman_collection.json (project root) |
| Postman environment | StudentAssessmentTracker.postman_environment.json (project root) |
| API base URL | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

---

## 3-Step Quick Start

### Step 1: Start the API
```powershell
cd C:\Users\Developer.03\Desktop\Student-Assessment-Tracker\StudentAssessmentTrackerAPI
dotnet run
```
Wait for: Now listening on: http://localhost:5000

### Step 2: Import into Postman
1. Open Postman.
2. Click Import.
3. Import the collection JSON file.
4. Import the environment JSON file.
5. Select the StudentAssessmentTracker environment from the top-right dropdown.

### Step 3: Get the Admin Token
1. Open the Admins folder in the collection.
2. Run POST Login.
3. The token is automatically saved to the adminToken environment variable.
4. All Admin-protected requests will now work.

---

## First Test Run (Recommended Order)

1. POST /api/admins/login - get admin token
2. POST /api/admins/teachers - create a teacher
3. POST /api/teachers/activate - activate the teacher account
4. POST /api/teachers/login - get teacher token
5. POST /api/admins/students - create a student
6. POST /api/students/{sid}/teachers/{tid} - assign teacher to student
7. POST /api/students/activate - activate student account
8. POST /api/students/login - get student token
9. GET /api/students - verify teacher sees their assigned student
10. POST /api/students/{id}/assessments - add an assessment
11. GET /api/students/{id} - verify assessment appears

---

## Troubleshooting

| Problem | Solution |
|---|---|
| 401 Unauthorized | Run the login request for your role first; check environment variable is set |
| 400 Bad Request | Check the Swagger UI for required fields and formats |
| 500 Internal Server Error | Ensure dotnet run is running and database is up |
| Token expired | Re-run the login request to get a fresh token |
| API not reachable | Confirm API is running on http://localhost:5000 |

For detailed endpoint documentation, see POSTMAN_TESTING_GUIDE.md.

---
