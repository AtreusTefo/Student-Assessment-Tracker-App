# ✅ POSTMAN IMPORT & TESTING - COMPLETE INSTRUCTIONS

## 🎯 WHAT YOU NEED TO DO

### 1️⃣ Download & Open Postman (1 minute)
- Visit: https://www.postman.com/downloads/
- Download and install (or use web version)
- Open Postman application

### 2️⃣ Import the Collection (2 minutes)
**Location**: `c:\Users\User\Desktop\StudentAssessmentTracker\StudentAssessmentTracker.postman_collection.json`

**Steps**:
1. Click **Import** button (top-left of Postman)
2. Click **File** tab in the import dialog
3. Navigate to and select the JSON file above
4. Click **Import** → Collection loads! ✅

### 3️⃣ Test the API (5 minutes)
See **Testing Flow** section below for step-by-step instructions.

---

## 📥 POSTMAN COLLECTION DETAILS

**File**: StudentAssessmentTracker.postman_collection.json

**Contains**:
- ✅ 5 REST API endpoints (CRUD operations)
- ✅ Pre-configured request headers
- ✅ Sample request bodies
- ✅ Base URL variable: `{{base_url}}` = `http://localhost:5000`
- ✅ Detailed endpoint descriptions
- ✅ Expected response codes documented

**Endpoints Included**:
```
1. GET    /api/students           → Retrieve all students
2. POST   /api/students           → Create new student
3. GET    /api/students/{id}      → Get specific student
4. PUT    /api/students/{id}      → Update student
5. DELETE /api/students/{id}      → Delete student
```

---

## 🧪 COMPLETE TESTING FLOW

Follow these steps IN ORDER in Postman:

### Step 1: Get All Students (Initial State)
```
Request:  GET http://localhost:5000/api/students

In Postman:
1. Left sidebar → Expand "Students" folder
2. Click "Get All Students"
3. Click "Send" button
4. View Response Panel (bottom)

Expected:
- Status: 200 OK
- Body: [] (empty array)
```

---

### Step 2: Create a Student
```
Request:  POST http://localhost:5000/api/students

In Postman:
1. Click "Create New Student"
2. Review the request body (already filled with example data)
3. Click "Send"
4. View response

Expected:
- Status: 201 Created
- Body includes: "id": 1

✅ SAVE THIS ID! You'll use it in next tests.
```

**Sample Request Body**:
```json
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

**Sample Response**:
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  ...
}
```

---

### Step 3: Get Student by ID
```
Request:  GET http://localhost:5000/api/students/1

In Postman:
1. Click "Get Student by ID"
2. In the URL, replace "1" with ID from Step 2
   (If ID was 1, URL stays: http://localhost:5000/api/students/1)
3. Click "Send"
4. View response

Expected:
- Status: 200 OK
- Body: Full student object with matching ID
```

---

### Step 4: Update the Student
```
Request:  PUT http://localhost:5000/api/students/1

In Postman:
1. Click "Update Student"
2. Replace "1" in URL with your student ID
3. Modify the request body (change any field, e.g., firstName: "Jane")
4. Click "Send"
5. View response

Expected:
- Status: 200 OK
- Body: Updated student object showing your changes
```

**Example Modified Body**:
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

---

### Step 5: Get All Students (Verify Update)
```
Request:  GET http://localhost:5000/api/students

In Postman:
1. Click "Get All Students"
2. Click "Send"
3. View response

Expected:
- Status: 200 OK
- Body: Array with 1 student showing YOUR updated data
- First name should be "Jane" (or whatever you changed it to)
```

---

### Step 6: Delete the Student
```
Request:  DELETE http://localhost:5000/api/students/1

In Postman:
1. Click "Delete Student"
2. Replace "1" in URL with your student ID
3. Click "Send"
4. View response

Expected:
- Status: 204 No Content
- Body: (empty - no content returned)
```

---

### Step 7: Verify Deletion
```
Request:  GET http://localhost:5000/api/students

In Postman:
1. Click "Get All Students"
2. Click "Send"
3. View response

Expected:
- Status: 200 OK
- Body: [] (empty array - student is gone!)

✅ DELETION CONFIRMED!
```

---

## ✅ SUCCESS CHECKLIST

After completing all steps, verify:

- [ ] Step 1: GET all returned 200 with empty array
- [ ] Step 2: POST create returned 201 and student ID
- [ ] Step 3: GET by ID returned 200 with correct student
- [ ] Step 4: PUT update returned 200 with new data
- [ ] Step 5: GET all returned 1 student with updates
- [ ] Step 6: DELETE returned 204
- [ ] Step 7: GET all returned 200 with empty array

**If all checked: Your API is fully functional! 🎉**

---

## 🔍 POSTMAN TIPS

**Finding Requests**:
- Look in left sidebar under "Students" folder
- Each endpoint is a separate "Request"

**Using Variables**:
- `{{base_url}}` automatically expands to `http://localhost:5000`
- No need to manually edit URLs (unless your API is on different port)

**Viewing Responses**:
- Click "Response" tab (usually selected by default)
- Shows Status code, Headers, Body
- Use "Pretty" button to format JSON nicely

**Modifying Requests**:
- Click endpoint to open request
- Edit body under "Body" tab
- Edit URL parameters as needed
- Click "Send" to execute

**Keyboard Shortcut**:
- Press **Ctrl+Enter** to send request (faster than clicking)

---

## 🌐 ALTERNATIVE: Scalar UI

If you prefer NOT to use Postman:

**Open in Browser**:
```
http://localhost:5000/scalar/v1
```

**Same testing**:
- Left sidebar shows endpoints
- Click endpoint to expand
- Fill in parameters/body
- Click "Send Request"
- View response below

**No Postman installation needed!**

---

## 🚀 QUICK REFERENCE

| Step | Method | URL | Expected Status |
|------|--------|-----|-----------------|
| 1 | GET | `/api/students` | 200 |
| 2 | POST | `/api/students` | 201 |
| 3 | GET | `/api/students/1` | 200 |
| 4 | PUT | `/api/students/1` | 200 |
| 5 | GET | `/api/students` | 200 |
| 6 | DELETE | `/api/students/1` | 204 |
| 7 | GET | `/api/students` | 200 |

---

## ❓ FREQUENTLY ASKED QUESTIONS

**Q: How do I import the collection into Postman?**  
A: Click Import → Select File → Choose the JSON file → Click Import

**Q: Where is the collection file?**  
A: `c:\Users\User\Desktop\StudentAssessmentTracker\StudentAssessmentTracker.postman_collection.json`

**Q: What if I get "Connection refused"?**  
A: The API is not running. In terminal, run: `dotnet run`

**Q: Do I need to set up the base URL?**  
A: No! It's pre-configured in the collection as `{{base_url}}` = `http://localhost:5000`

**Q: What if my API runs on a different port?**  
A: Edit the collection → Variables tab → Change `base_url` value

**Q: How do I know the student ID from the create request?**  
A: Look in the POST response, you'll see: `"id": 1` (or whatever number)

**Q: Can I test without Postman?**  
A: Yes! Use Scalar UI at `http://localhost:5000/scalar/v1`

---

## 📞 SUPPORT

**For Postman Help**:
- https://learning.postman.com/docs/
- https://www.postman.com/product/rest-client/

**For API Issues**:
- Check terminal where API is running for error logs
- Verify JSON syntax in request body
- Check that all required fields are included

**Documentation**:
- [POSTMAN_TESTING_GUIDE.md](POSTMAN_TESTING_GUIDE.md) - Detailed walkthrough
- [POSTMAN_QUICK_REFERENCE.md](POSTMAN_QUICK_REFERENCE.md) - Quick lookup
- [API_SETUP_TESTING_GUIDE.md](API_SETUP_TESTING_GUIDE.md) - Complete reference

---

## 🎯 YOU'RE ALL SET!

Everything is configured and ready:
- ✅ API running on port 5000
- ✅ Collection file created and ready
- ✅ All endpoints pre-configured
- ✅ Sample data included
- ✅ Full documentation provided

**Next Step**: Open Postman and import the collection!

---

## 📊 API SUMMARY

**Server**: http://localhost:5000  
**Base API**: http://localhost:5000/api  
**Documentation**: http://localhost:5000/scalar/v1  
**Collection File**: StudentAssessmentTracker.postman_collection.json  

**Endpoints**: 5 (GET, POST, GET by ID, PUT, DELETE)  
**Database**: In-memory (data resets on app restart)  
**Status**: ✅ Running and tested

---

**Ready to begin? Open Postman and import the collection! 🚀**

*Last Updated: February 18, 2026*  
*Status: ✅ Ready for Testing*
