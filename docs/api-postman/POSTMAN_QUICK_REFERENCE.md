# Postman Quick Reference

## Current API Status
- Server: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Database: SQL Server LocalDB (StudentAssessmentTrackerDev)

---

## 30-Second Setup

1. Open Postman.
2. Click Import.
3. Import `StudentAssessmentTracker.postman_collection.json` (project root).
4. Import `StudentAssessmentTracker.postman_environment.json` (project root).
5. Select the StudentAssessmentTracker environment (top-right dropdown).
6. Run Admin Login first  token is saved automatically.

---

## Must-Do First: Get Tokens

### Admin Token
```
POST http://localhost:5000/api/admins/login
Body: { "email": "admin@tracker.local", "password": "Admin@1234" }
```
Token is auto-saved to `adminToken` environment variable.

### Teacher Token (after teacher is activated)
```
POST http://localhost:5000/api/teachers/login
Body: { "email": "teacher@school.com", "password": "..." }
```
Token is auto-saved to `teacherToken`.

### Student Token (after student is activated)
```
POST http://localhost:5000/api/students/login
Body: { "studentUniqueId": "STU-XXXXXXXX", "password": "..." }
```
Token is auto-saved to `studentToken`.

---

## Key Endpoint Groups

### Admin Operations (requires adminToken)
```
GET    /api/admins/teachers                  List all teachers
POST   /api/admins/teachers                  Create teacher
PUT    /api/admins/teachers/{id}             Update teacher
DELETE /api/admins/teachers/{id}             Delete teacher
GET    /api/admins/students                  List all students
POST   /api/admins/students                  Create student
PUT    /api/admins/students/{id}             Update student
DELETE /api/admins/students/{id}             Delete student
POST   /api/admins/students/bulk             Bulk import students (JSON)
POST   /api/admins/students/bulk-csv         Bulk import students (CSV)
POST   /api/admins/teachers/bulk             Bulk import teachers (JSON)
POST   /api/admins/teachers/bulk-csv         Bulk import teachers (CSV)
GET    /api/admins/audit-logs/{entity}/{id}  View audit log
```

### Teacher Operations (requires teacherToken)
```
GET  /api/students                           List assigned students
GET  /api/students/{id}                      Student detail
POST /api/students/{id}/assessments          Add assessment
PUT  /api/students/{id}/assessments/{aid}    Edit assessment
DEL  /api/students/{id}/assessments/{aid}    Delete assessment
POST /api/assessments/bulk                   Bulk create assessments
GET  /api/reports/students/{id}/csv          Export student CSV
GET  /api/reports/students/{id}/pdf          Export student PDF
GET  /api/reports/students/csv               Export all students CSV
```

### Public Endpoints (no token required)
```
POST /api/admins/login                       Admin login
POST /api/teachers/activate                  Activate teacher account
POST /api/teachers/login                     Teacher login
POST /api/teachers/forgot-password           Reset teacher password
POST /api/students/activate                  Activate student account
POST /api/students/login                     Student login
POST /api/students/forgot-password           Reset student password
```

---

## Expected Status Codes

| Operation | Expected Code |
|---|---|
| GET (found) | 200 OK |
| POST (created) | 201 Created |
| PUT (updated) | 200 OK |
| DELETE | 204 No Content |
| Validation error | 400 Bad Request |
| Unauthorized | 401 Unauthorized |
| Not found | 404 Not Found |
| Duplicate / conflict | 409 Conflict |

---

## Adding Authorization Header Manually

If not using the environment variables, add this header to every protected request:
```
Key:   Authorization
Value: Bearer <your-token-here>
```

---
