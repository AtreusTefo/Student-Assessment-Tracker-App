# Postman Testing - Quick Start Reference

## 🎯 Current Status

✅ **API is RUNNING** on: `http://localhost:5000`  
✅ **Postman Collection**: [StudentAssessmentTracker.postman_collection.json](StudentAssessmentTracker.postman_collection.json)  
✅ **All 5 Endpoints**: Ready for testing

---

## ⚡ 30-Second Setup

### 1. Open Postman
```
Download: https://www.postman.com/downloads/
```

### 2. Import Collection
- Click **Import** (top-left)
- Select file: `StudentAssessmentTracker.postman_collection.json`
- Done! ✅

### 3. Start Testing
- Expand **"Students"** folder
- Click any endpoint
- Click **"Send"**
- View response

---

## 📋 Test Requests (Copy-Paste Ready)

### 1️⃣ GET All Students
```
GET http://localhost:5000/api/students
```
**Expected**: `200 OK` with empty array `[]`

---

### 2️⃣ CREATE Student
```
POST http://localhost:5000/api/students

Body (JSON):
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
**Expected**: `201 Created` with student object including **ID**

**Note**: Save the ID from response (e.g., `"id": 1`)

---

### 3️⃣ GET Student by ID
```
GET http://localhost:5000/api/students/1
```
(Replace `1` with ID from previous response)

**Expected**: `200 OK` with student details

---

### 4️⃣ UPDATE Student
```
PUT http://localhost:5000/api/students/1

Body (JSON):
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
(Replace `1` with student ID)

**Expected**: `200 OK` with updated student

---

### 5️⃣ DELETE Student
```
DELETE http://localhost:5000/api/students/1
```
(Replace `1` with student ID)

**Expected**: `204 No Content` (empty response)

---

### 6️⃣ VERIFY Deletion
```
GET http://localhost:5000/api/students
```
**Expected**: `200 OK` with empty array `[]`

---

## 🔗 Alternative: Test in Scalar UI

Instead of Postman, use the built-in Scalar interface:

```
Open in Browser: http://localhost:5000/scalar/v1
```

**Features**:
- Browse all endpoints
- Fill in parameters
- Send requests
- View responses
- No installation needed

---

## 📊 Success Indicators

After testing, you should see:

| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| Get All | GET | 200 | Returns array (empty initially) |
| Create | POST | 201 | Returns student with ID |
| Get by ID | GET | 200 | Returns single student |
| Update | PUT | 200 | Returns updated student |
| Delete | DELETE | 204 | Empty response |
| Get All (after delete) | GET | 200 | Returns empty array |

---

## ⚙️ Postman Headers (Auto-Included)

The collection includes these headers automatically:

```
Content-Type: application/json
Accept: application/json
```

You don't need to add these - they're already configured!

---

## 🚨 Troubleshooting

### "127.0.0.1 refused to connect"
**Solution**: 
- Check API is running in terminal
- Ensure port 5000 is available
- Verify no firewall blocking

### "404 Not Found"
**Solution**:
- Check URL is exactly: `http://localhost:5000/api/students`
- Verify endpoint spelling
- Make sure `{{base_url}}` variable is set to `http://localhost:5000`

### "400 Bad Request"
**Solution**:
- Verify JSON syntax (use Postman's JSON validation)
- Check all required fields are present
- Ensure data types match (strings, numbers, etc.)

### Variable `{{base_url}}` Shows as Literal
**Solution**:
- Click the collection name to select it
- Make sure you're in collection scope
- Refresh Postman

---

## 💻 API Endpoints Summary

```
Base URL: http://localhost:5000/api/students

GET    /api/students          → Get all students
GET    /api/students/{id}     → Get specific student
POST   /api/students          → Create new student
PUT    /api/students/{id}     → Update student
DELETE /api/students/{id}     → Delete student
```

---

## 📁 Student Properties

When creating/updating, use these fields:

| Field | Type | Example |
|-------|------|---------|
| firstName | string | "John" |
| lastName | string | "Doe" |
| email | string | "john@example.com" |
| phoneNumber | string | "+1-555-0100" |
| enrollmentDate | date | "2024-01-15" |
| program | string | "Computer Science" |
| totalScore | decimal | 85.5 |
| averageScore | decimal | 85.5 |
| performanceLevel | string | "Good" |

**Required Fields**: firstName, lastName, email, totalScore, averageScore, performanceLevel

---

## 🎬 Example Workflow

### Test 1: Create Student
1. Select: **Create New Student**
2. Click: **Send**
3. Copy: Student ID from response (e.g., `1`)

### Test 2: Retrieve Student  
1. Select: **Get Student by ID**
2. Change ID in URL to `1`
3. Click: **Send**
4. Should show John Doe's details

### Test 3: Update Student
1. Select: **Update Student**
2. Change ID in URL to `1`
3. Modify body (e.g., firstName to "Jane")
4. Click: **Send**
5. Should show updated details

### Test 4: Delete Student
1. Select: **Delete Student**
2. Change ID in URL to `1`
3. Click: **Send**
4. Should return 204 No Content

### Test 5: Verify Gone
1. Select: **Get All Students**
2. Click: **Send**
3. Should show empty array `[]`

---

## 🔑 Key Points

✅ **Collection URL**: Uses variable `{{base_url}}` = `http://localhost:5000`  
✅ **Headers**: Automatically included (no manual setup needed)  
✅ **Sample Data**: Pre-filled in request bodies  
✅ **All 5 Endpoints**: Included in collection  
✅ **Ready to Test**: No additional configuration required  

---

## 📚 Documentation Files

- 📖 Full Guide: [API_SETUP_TESTING_GUIDE.md](API_SETUP_TESTING_GUIDE.md)
- 📋 Detailed Testing: [POSTMAN_TESTING_GUIDE.md](POSTMAN_TESTING_GUIDE.md)
- 📊 Implementation Report: [IMPLEMENTATION_COMPLETION_REPORT.md](IMPLEMENTATION_COMPLETION_REPORT.md)

---

## 🎯 You're Ready!

Your Student Assessment Tracker API is fully set up and ready for testing with Postman. Everything is configured and working. Just import the collection and start testing!

**Questions?** Check the detailed guides linked above.

**Version**: 1.0  
**Status**: ✅ API Running  
**Last Updated**: February 18, 2026
