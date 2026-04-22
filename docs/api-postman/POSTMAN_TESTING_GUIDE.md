# Postman Setup and Testing Guide

## API Status
- **Server**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`
- **Database**: SQL Server LocalDB (`StudentAssessmentTrackerDev`)

---

## Step 1: Import the Collection and Environment

1. Open Postman
2. Click **Import**
3. Select `StudentAssessmentTracker.postman_collection.json` (project root)
4. Click **Import** again
5. Select `StudentAssessmentTracker.postman_environment.json` (project root)
6. Select **StudentAssessmentTracker** from the environment dropdown (top right)

The collection includes all Admin, Teacher, Student, Assessment, Report, and ClassGroup endpoints. Tokens are saved automatically to environment variables when you run any login request.

---

## Step 2: Get the Admin Token

1. Expand the **Admins** folder in the collection
2. Run **POST Login** with:
   ```json
   { "email": "admin@tracker.local", "password": "Admin@1234" }
   ```
3. The token is automatically saved to `adminToken`
4. All Admin-protected requests will now work

---

## Recommended Test Order (Full Flow)

Run requests in this order to set up all roles and verify end-to-end functionality:

| # | Request | Folder | Notes |
|---|---|---|---|
| 1 | POST /api/admins/login | Admins | Saves adminToken |
| 2 | POST /api/admins/teachers | Admins | Create teacher (no password) |
| 3 | POST /api/teachers/activate | Teachers | Teacher sets own password |
| 4 | POST /api/teachers/login | Teachers | Saves teacherToken |
| 5 | POST /api/admins/students | Admins | Create student record |
| 6 | POST /api/students/{sid}/teachers/{tid} | Students | Assign teacher to student |
| 7 | POST /api/students/activate | Students | Student sets own password |
| 8 | POST /api/students/login | Students | Saves studentToken |
| 9 | GET /api/students | Students | Teacher sees their student |
| 10 | POST /api/students/{id}/assessments | StudentAssessments | Add assessment |
| 11 | GET /api/students/{id} | Students | Verify assessment on student detail |
| 12 | GET /api/reports/students/{id}/pdf | Reports | Download PDF report |

---

## Test Scenarios

### Test 1: Create a Teacher (Admin JWT)

```json
POST /api/admins/teachers
Authorization: Bearer {{adminToken}}

{
  "idPassportNo": "T001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phone": "12345678",
  "subjectName": "Mathematics"
}
```
Expected: `201 Created` with teacher ID and `isActive: false`

---

### Test 2: Activate a Teacher (public)

```json
POST /api/teachers/activate

{
  "email": "jane.smith@school.com",
  "password": "Teacher@123",
  "confirmPassword": "Teacher@123"
}
```
Expected: `200 OK` with a teacher JWT token

---

### Test 3: Create a Student (Admin JWT)

```json
POST /api/admins/students
Authorization: Bearer {{adminToken}}

{
  "idPassportNo": "S001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phone": "87654321",
  "gradeName": "Grade 10"
}
```
Expected: `201 Created` with `studentUniqueId` (e.g. `STU-XXXXXXXX`) and `isActive: false`

---

### Test 4: Add an Assessment (Teacher JWT)

```json
POST /api/students/{studentId}/assessments
Authorization: Bearer {{teacherToken}}

{
  "name": "Term 1 Test",
  "score": 42,
  "maxScore": 50,
  "dueDate": "2026-05-01T00:00:00",
  "instructions": "Complete all sections",
  "isAssigned": true
}
```
Expected: `201 Created` with the assessment object

---

### Test 5: Bulk Import Students (Admin JWT)

```json
POST /api/admins/students/bulk
Authorization: Bearer {{adminToken}}

[
  {
    "idPassportNo": "ID001",
    "firstName": "Alice",
    "lastName": "Brown",
    "email": "alice.brown@school.com",
    "phone": "11111111",
    "gradeName": "Grade 9"
  },
  {
    "idPassportNo": "ID002",
    "firstName": "Bob",
    "lastName": "Jones",
    "email": "bob.jones@school.com",
    "phone": "22222222",
    "gradeName": "Grade 10"
  }
]
```
Expected: `200 OK` with `{ totalRows, successCount, failureCount, results: [...] }`

---

### Test 6: Forgot Password — Teacher (public)

```json
POST /api/teachers/forgot-password

{ "email": "jane.smith@school.com" }
```
Expected: `200 OK` (always, regardless of whether email exists — anti-enumeration)

Effect: Teacher's password is nulled. Teacher must re-activate via `POST /api/teachers/activate`.

---

### Test 7: Forgot Password — Student (public, dual-factor)

```json
POST /api/students/forgot-password

{
  "studentUniqueId": "STU-XXXXXXXX",
  "email": "john.doe@school.com"
}
```
Expected: `200 OK`. Both StudentUniqueId and email must match — if email is wrong, the reset is silently rejected.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| 401 Unauthorized | Run the login request for your role; check the environment variable is set |
| 403 Forbidden | You are using the wrong role token for this endpoint |
| 400 Bad Request | Check Swagger UI for required field formats. Phone must be exactly 8 digits |
| 500 Internal Server Error | Check the dotnet console. Logs are in `StudentAssessmentTrackerAPI/Logs/` |


---

## Step 1: Import the Postman Collection

### Option A: Using Postman UI (Recommended)

1. **Open Postman** (Download from https://www.postman.com/downloads/ if needed)

2. **Click "Import"** button in the top-left corner

3. **Choose "File"** tab in the import dialog

4. **Browse and select**:
   ```
   StudentAssessmentTracker.postman_collection.json
   ```
   Located at: `c:\Users\User\Desktop\StudentAssessmentTracker\StudentAssessmentTracker.postman_collection.json`

5. **Click Import** - collection will load with all 5 endpoints

### Option B: Using Postman Web

1. Go to https://web.postman.co/
2. Click **Import**
3. Select the JSON file
4. Collection imports automatically

---

## Step 2: Configure Environment (Optional)

The collection comes pre-configured with:
- **Variable**: `base_url` = `http://localhost:5000`

If your API runs on a different port:

1. **Click the collection** to expand it
2. **Click the three dots (⋯)** next to the collection name
3. **Select "Edit"**
4. **Go to "Variables" tab**
5. **Find** `base_url` variable
6. **Change "Current Value"** to your API URL (e.g., `http://localhost:5001`)
7. **Save**

---

## Step 3: Test the Endpoints

### Test 1: Get All Students

1. **Expand** "Students" folder
2. **Click** "Get All Students"
3. **Check** the URL shows: `{{base_url}}/api/students`
4. **Click "Send"**
5. **Expected Response**: 
   - Status: **200 OK**
   - Body: Empty array `[]` (if no students yet)

```json
// Example successful response
[]
```

---

### Test 2: Create a Student

1. **Click** "Create New Student"
2. **Review** the request body (pre-filled with example data)
3. **Click "Send"**
4. **Expected Response**: 
   - Status: **201 Created**
   - Body: Student object with assigned ID

```json
// Example response (save the ID for next tests)
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1-555-0100",
  "totalScore": 85.5,
  "averageScore": 85.5,
  "performanceLevel": "Good"
}
```

**💡 Important**: Copy the `id` value (e.g., 1) from the response for the next tests!

---

### Test 3: Get Student by ID

1. **Click** "Get Student by ID"
2. **Replace** the ID in the URL path (last segment)
   - Original: `{{base_url}}/api/students/1`
   - Change **1** to the ID from your previous response
3. **Click "Send"**
4. **Expected Response**: 
   - Status: **200 OK**
   - Body: Student details

```json
// Example response
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1-555-0100",
  "totalScore": 85.5,
  "averageScore": 85.5,
  "performanceLevel": "Good"
}
```

---

### Test 4: Update Student

1. **Click** "Update Student"
2. **Update** the ID in the URL (same as Get by ID)
3. **Modify** the request body with new data:

```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phoneNumber": "+1-555-0199",
  "enrollmentDate": "2024-02-15",
  "program": "Data Science",
  "totalScore": 95.0,
  "averageScore": 95.0,
  "performanceLevel": "Excellent"
}
```

4. **Click "Send"**
5. **Expected Response**: 
   - Status: **200 OK**
   - Body: Updated student object

---

### Test 5: Get All Students (Verify Update)

1. **Click** "Get All Students"
2. **Click "Send"**
3. **Expected Response**: 
   - Status: **200 OK**
   - Body: Array with updated student

```json
[
  {
    "id": 1,
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@example.com",
    "totalScore": 95.0,
    "averageScore": 95.0,
    "performanceLevel": "Excellent"
  }
]
```

---

### Test 6: Delete Student

1. **Click** "Delete Student"
2. **Update** the ID in the URL
3. **Click "Send"**
4. **Expected Response**: 
   - Status: **204 No Content**
   - Body: Empty (no content to return)

---

### Test 7: Verify Deletion

1. **Click** "Get All Students"
2. **Click "Send"**
3. **Expected Response**: 
   - Status: **200 OK**
   - Body: Empty array `[]` (student is gone)

```json
[]
```

---

## 📊 API Testing Checklist

Use this checklist to track your tests:

- [ ] **Get All Students** (GET) - Returns empty array initially
- [ ] **Create New Student** (POST) - Returns 201 Created with student ID
- [ ] **Get Student by ID** (GET) - Returns specific student
- [ ] **Update Student** (PUT) - Returns 200 OK with updated data
- [ ] **Get All Students** (GET) - Shows updated student
- [ ] **Delete Student** (DELETE) - Returns 204 No Content
- [ ] **Get All Students** (GET) - Shows empty array (deletion confirmed)

---

## 🔗 Testing the Scalar Documentation UI

Scalar provides an interactive API documentation interface:

1. **Open Browser**: Go to `http://localhost:5000/scalar/v1`
2. **Browse Endpoints**: Left sidebar shows all endpoints
3. **Click on Endpoint**: See full documentation
4. **Fill Parameters**: Enter data in form fields
5. **Click "Send Request"**: Execute directly from the UI
6. **View Response**: See results immediately

**Advantages**:
- No additional software needed
- Live documentation
- Interactive testing
- Beautiful UI from scalar.com

---

## 📝 Request/Response Examples

### Create Student Request
```http
POST /api/students HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1-555-0100",
  "enrollmentDate": "2024-01-15",
  "program": "Computer Science",
  "totalScore": 85.5,
  "averageScore": 85.5,
  "performanceLevel": "Good"
}
```

### Create Student Response (201)
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1-555-0100",
  "totalScore": 85.5,
  "averageScore": 85.5,
  "performanceLevel": "Good"
}
```

---

## 🐛 Troubleshooting

### "Connection Refused" Error
- **Problem**: API not running
- **Solution**: Start API with: `dotnet run`

### "404 Not Found"
- **Problem**: Wrong endpoint URL
- **Solution**: Check URL in request matches: `http://localhost:5000/api/students`

### "400 Bad Request"
- **Problem**: Invalid request body
- **Solution**: Verify JSON syntax and required fields are present

### "500 Internal Server Error"
- **Problem**: Server error processing request
- **Solution**: Check API logs in terminal, verify all fields are valid

### Variable Not Working
- **Problem**: `{{base_url}}` shows as literal text
- **Solution**: Ensure collection is selected, not just a request

---

## 💡 Tips & Tricks

1. **Save Responses**: Use "Save response" to store API responses for reference
2. **Create Tests**: Add tests under "Tests" tab to validate responses automatically
3. **Use Variables**: Store student IDs in variables for multi-step tests
4. **Generate Code**: Click "Code" to see equivalent code in your language
5. **Set Pre-request Scripts**: Run setup code before each request

---

## 🎯 What You'll Accomplish

After running all tests, you'll have:
- ✅ Verified API is running
- ✅ Tested all 5 CRUD endpoints
- ✅ Created sample student data
- ✅ Retrieved and updated data
- ✅ Deleted test data
- ✅ Confirmed API functionality

---

## 📚 Next Steps

1. **Explore Scalar UI**: `http://localhost:5000/scalar/v1`
2. **Review API Controller**: [StudentsController.cs](../Presentation/Controllers/StudentsController.cs)
3. **Check Logs**: View API logs in terminal for each request
4. **Create More Tests**: Add students with different data
5. **Test Edge Cases**: Try invalid inputs to see error responses

---

## 🚀 Success!

Once all tests pass, your Student Assessment Tracker API is fully operational with:
- ✅ Full CRUD functionality
- ✅ Scalar documentation
- ✅ Postman testing support
- ✅ DataTables frontend integration

**Ready to deploy!**
