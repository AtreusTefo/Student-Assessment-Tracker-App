# Postman Testing - Ready Checklist

Use this checklist before starting a testing session.

---

## Pre-Flight Checks

- [ ] API is running: dotnet run in StudentAssessmentTrackerAPI/
- [ ] API responds: GET http://localhost:5000/swagger loads in browser
- [ ] Database is up: no Cannot open database error in the dotnet console
- [ ] Postman collection is imported
- [ ] Postman environment is imported and selected

---

## Token Checklist

- [ ] Admin token obtained: POST /api/admins/login (admin@tracker.local / Admin@1234)
- [ ] Teacher token obtained: POST /api/teachers/login (after teacher activation)
- [ ] Student token obtained: POST /api/students/login (after student activation)

---

## Data Setup Checklist

- [ ] At least one teacher created by admin
- [ ] Teacher account activated (POST /api/teachers/activate)
- [ ] At least one student created by admin
- [ ] Teacher assigned to student (POST /api/students/{sid}/teachers/{tid})
- [ ] Student account activated (POST /api/students/activate)

---

## Common Request Examples

### Authenticate
```
POST /api/admins/login
Content-Type: application/json
Body: { "email": "admin@tracker.local", "password": "Admin@1234" }
```

### Create Teacher (Admin JWT required)
```
POST /api/admins/teachers
Authorization: Bearer adminToken
Content-Type: application/json
Body:
{
  "idPassportNo": "ID001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phone": "12345678",
  "subjectName": "Mathematics"
}
```

### Activate Teacher (public)
```
POST /api/teachers/activate
Content-Type: application/json
Body:
{
  "email": "jane.smith@school.com",
  "password": "Teacher@1234",
  "confirmPassword": "Teacher@1234"
}
```

### Create Student (Admin JWT required)
```
POST /api/admins/students
Authorization: Bearer adminToken
Content-Type: application/json
Body:
{
  "idPassportNo": "ID002",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phone": "87654321",
  "gradeName": "Grade 10"
}
```

### Add Assessment (Teacher JWT required)
```
POST /api/students/{studentId}/assessments
Authorization: Bearer teacherToken
Content-Type: application/json
Body:
{
  "name": "Test 1",
  "score": 75,
  "maxScore": 100,
  "dueDate": "2026-05-01",
  "isAssigned": true
}
```

---

## Validation Rules (Quick Reference)

| Field | Rule |
|---|---|
| Phone | Exactly 8 digits, numeric only |
| Email | Must be lowercase in database; unique per role |
| Password | 8-20 characters |
| Score | 0 <= Score <= MaxScore |
| MaxScore | Must be greater than 0 |
| StudentUniqueId | Format: STU-XXXXXXXX (8 hex chars) |
| Bulk import | Maximum 500 rows per request |
