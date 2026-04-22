# Technology Stack and Architecture Overview

This presentation provides a brief overview of the key technologies and architectural patterns used in the Student Assessment Tracker application.

## Core Technologies & Architecture

### 1. Multilayered Architecture

Our application is built using a multilayered architecture, which separates concerns and improves maintainability. This includes:

-   **Presentation Layer:** Handles user interaction (API endpoints).
-   **Application Layer:** Contains business logic, services, DTOs and validation.
-   **Domain Layer:** Represents the core business entities and rules.
-   **Infrastructure Layer:** Manages data access and external services.

### 2. ASP.NET Core
The backend is powered by ASP.NET Core 8, a high-performance, cross-platform framework for building modern, cloud-based, internet-connected applications. It serves as the foundation for our RESTful API.

### 3. Entity Framework Core
We use Entity Framework Core as our Object-Relational Mapper (ORM) to interact with a SQL Server LocalDB database. It simplifies data access by allowing us to work with .NET objects instead of writing raw SQL queries.

### 4. Angular 21
The front-end is a single-page application (SPA) built with Angular 21. Angular allows us to create a dynamic, responsive and modern user interface for a seamless user experience.

## Key Libraries & Tools

### 5. DTOs (Data Transfer Objects)

DTOs are used to transfer data between the client and server and between different layers of the application. This helps to decouple layers and prevent exposing our internal domain models.

### 6. AutoMapper

AutoMapper is a library that simplifies the process of mapping between our domain entities and DTOs. It automates the tedious task of writing boilerplate code to copy data from one object to another.

### 7. FluentValidation

For robust and readable validation, we use FluentValidation. It allows us to define clear and concise validation rules for our DTOs, ensuring data integrity.

### 8. DataTables

The DataTables library is integrated into our Angular front-end to provide advanced interaction controls for our HTML tables, including searching, sorting and pagination.

## Testing & Documentation

### 9. Swagger UI

Our API is documented and can be tested interactively using Swagger UI. It automatically generates a user-friendly interface from our code, making it easy to explore and test the API endpoints.

### 10. Postman

Postman is used for more in-depth API testing. We have created a Postman collection with a set of pre-configured requests to test all API functionalities and ensure everything is working as expected.

### 11. Serilog

Serilog is used for structured application-wide logging. Logs are written to the `StudentAssessmentTrackerAPI/Logs/` folder and support JSON-structured output for easy diagnostics.

**Benefits:**
- Structured logging (JSON format)
- Automatic file rotation
- Easy debugging and monitoring
- Production diagnostics

### What It Is
A JavaScript library that adds interactive features to HTML tables: sorting, filtering, pagination and search.

### Where Used in Code
**Package.json Dependency** - [StudentApp/package.json](StudentApp/package.json)
```json
{
  "dependencies": {
    "datatables.net": "^2.3.7",
    "datatables.net-dt": "^2.3.7"
  }
}
```

### How It Works
1. Frontend fetches student data from API (Application Programming Interface)
2. Datatables renders it in an interactive table
3. Users can:
   - Sort by any column (Grade, Score, etc.)
   - Search/filter students
   - View data in pages
   - Export to CSV/Excel

**Frontend Integration**: Used in Angular components to display the student grid

---

## 5. Angular 21 Frontend Tool

### What It Is
A modern JavaScript framework for building interactive web applications. It handles the user interface and communication with the backend API.

### Where Used in Code
**Main Application** - [StudentApp/](StudentApp/)

**Project Structure:**
```
StudentApp/
 src/
    main.ts          (Application entry point)
    index.html       (Main page)
    app/             (Components, services, etc.)
 package.json         (Dependencies)
 angular.json         (Configuration)
 tsconfig.json        (TypeScript settings)
```

**Key Technologies:**
- Components: Reusable UI pieces
- Services: Handle API communication
- Routing: Navigate between pages
- Forms: Handle user input

**Frontend to Backend Communication:**
```
Angular App  HTTP Requests  ASP.NET Core API  Database
(StudentApp)                  (Port 5000)
```

---

## 6. Multilayered Architecture

### What It Is
A software design pattern that separates code into distinct layers, each with a specific responsibility.

### The Four Layers in Your Project

#### **Layer 1: Presentation Layer**
Shows data to users and collects input
- Location: [Presentation/Controllers/](Presentation/Controllers/)
- Handles HTTP requests/responses
- Returns DTOs to frontend

#### **Layer 2: Application Layer**
Contains business logic and validation rules
- Location: [Application/](Application/)
- Contains:
  - **Services** - Business logic (calculate grades, averages)
  - **Validators** - FluentValidation rules
  - **DTOs** - Data transfer objects
  - **Mappings** - AutoMapper profiles

#### **Layer 3: Domain Layer**
Defines core business entities and interfaces
- Location: [Domain/](Domain/)
- Contains:
  - **Entities** - Student, Teacher classes
  - **Interfaces** - IRepository contract

#### **Layer 4: Infrastructure Layer**
Handles data access and external services
- Location: [Infrastructure/](Infrastructure/)
- Contains:
  - **DbContext** - Database communication
  - **Repositories** - Data access logic
  - Implements Domain interfaces

```
REQUEST FLOW:

Angular App
     HTTP
Presentation Layer (StudentsController)
    
Application Layer (StudentService, StudentValidator)
    
Domain Layer (Student Entity, IRepository)
    
Infrastructure Layer (StudentRepository, DbContext)
    
Database

```

**Benefits:**
- Separation of concerns (each layer has one job)
- Testability (can test each layer independently)
- Maintainability (easy to find and modify code)
- Scalability (easy to add new features)

---

## 7. Swagger UI

### What It Is
An interactive API documentation tool. It reads your API code and automatically generates documentation that developers can test directly in the browser.

### Where Used in Code
**Configured in** [Program.cs](Program.cs) - Lines 51-72

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Assessment Tracker API",
        Version = "v1",
        Description = "REST API for Student Assessment Tracking System"
    });

    // Include XML documentation
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "StudentAssessmentTracker.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});
```

### Access It
When your application runs:
- Visit: `http://localhost:5000/swagger/ui`
- See all API endpoints
- Test requests directly
- View response models

**What It Shows:**
```
GET    /api/students          - Fetch all students
POST   /api/students          - Create a student
GET    /api/students/{id}     - Get one student
PUT    /api/students/{id}     - Update a student
DELETE /api/students/{id}     - Delete a student
```

---

## 8. Postman

### What It Is
A tool for testing APIs. Instead of writing code, you click buttons to send requests and see responses.

### How It's Used
**Postman Collection** - [StudentAssessmentTracker.postman_collection.json](StudentAssessmentTracker.postman_collection.json)

### Testing Workflow
1. Create a request (GET, POST, PUT, DELETE)
2. Enter the endpoint URL: `http://localhost:5000/api/students`
3. Add request body (JSON):
   ```json
   {
     "firstName": "John",
     "lastName": "Doe",
     "email": "john@example.com",
     "assessment1": 18,
     "assessment2": 19,
     "assessment3": 17
   }
   ```
4. Click "Send"
5. View the response (with status code, headers, body)

### Benefits
- Manual API testing before frontend is ready
- Debug issues without running full application
- Share test collection with team
- Automate API tests

---

## Architecture Diagram

```

                   ANGULAR FRONTEND                      
              (StudentApp - TypeScript)                   
                                                          
  Components  Services  Datatables (Display)          

                      HTTP Requests/Responses
                     

           PRESENTATION LAYER (Controllers)              
     StudentsController.cs, TeacherController.cs         

                     
                     

           APPLICATION LAYER (Services)                  
  Validators (FluentValidation)                          
  Mappings (AutoMapper)                                  
  Services (Business Logic)                              
  DTOs (Data Transfer Objects)                           

                     
                     

            DOMAIN LAYER (Entities)                      
  Student.cs, Teacher.cs                                 
  IRepository (Interface)                                

                     
                     

        INFRASTRUCTURE LAYER (Data Access)               
  StudentRepository.cs                                   
  ApplicationDbContext.cs (Entity Framework)             

                     
                     
                
                 DATABASE
                

TOOLS & FEATURES:
 Serilog: Logs all operations to ~/Logs/
 Swagger UI: http://localhost:5000/swagger/ui
 Postman: Test API endpoints before frontend
```

---

## Summary Table

| Technology | Purpose | Location |
|-----------|---------|----------|
| **FluentValidation** | Validate user input | `Application/Validators/` |
| **AutoMapper** | Map entities to DTOs | `Application/Mappings/` |
| **DTOs** | Transfer data between layers | `Application/DTOs/` |
| **Serilog** | Log application events | `Program.cs`  `Logs/` |
| **Datatables** | Interactive table UI | `StudentApp/package.json` |
| **Angular** | Frontend web app | `StudentApp/src/` |
| **Multilayered Architecture** | Organize code by responsibility | Presentation  Application  Domain  Infrastructure |
| **Swagger** | API documentation & testing | `Program.cs`  `/swagger/ui` |
| **Postman** | Manual API testing | `StudentAssessmentTracker.postman_collection.json` |

---

## How To Use Everything Together

### 1. **Start the Backend**
   - Runs ASP.NET Core API on port 5000
   - Serilog logs all activity

### 2. **Start the Frontend**
   - Angular app on port 4200
   - Makes HTTP calls to port 5000

### 3. **Test with Swagger**
   - Visit `/swagger/ui` to test each endpoint
   - See what data is sent/received

### 4. **Verify with Postman**
   - Import the collection
   - Test each endpoint manually

### 5. **Frontend Communication**
   - Angular Services call the API
   - ValidationRules check input
   - AutoMapper converts responses
   - Datatables displays the results
   - Serilog tracks everything

---

**Everything works together to create a modern, professional, scalable application!**
