# Smart LMS API Documentation

## Authentication

All API requests require a valid JWT token obtained from Microsoft Entra ID, except for publicly accessible endpoints.

### Get Access Token

```bash
# Using Azure CLI
az account get-access-token --resource <your-client-id>
```

## API Examples

### 1. Create a Course (Instructor Only)

```bash
curl -X POST https://localhost:5001/api/courses \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Introduction to Python",
    "description": "Learn Python programming from scratch"
  }'
```

**Response:**
```json
{
  "id": 1,
  "title": "Introduction to Python",
  "description": "Learn Python programming from scratch",
  "isPublished": false,
  "instructorId": 1,
  "instructorName": "John Doe",
  "createdAt": "2026-05-02T10:00:00Z",
  "moduleCount": 0,
  "enrollmentCount": 0
}
```

### 2. Get All Courses (Public)

```bash
curl -X GET https://localhost:5001/api/courses
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Introduction to Python",
    "description": "Learn Python programming from scratch",
    "isPublished": true,
    "instructorId": 1,
    "instructorName": "John Doe",
    "createdAt": "2026-05-02T10:00:00Z",
    "moduleCount": 3,
    "enrollmentCount": 25
  }
]
```

### 3. Enroll a Student (Instructor/Admin Only)

```bash
curl -X POST https://localhost:5001/api/enrollments \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": 1,
    "userId": 5
  }'
```

**Response:**
```json
{
  "id": 10,
  "userId": 5,
  "studentName": "Jane Smith",
  "studentEmail": "jane@example.com",
  "courseId": 1,
  "courseTitle": "Introduction to Python",
  "status": "Active",
  "enrolledAt": "2026-05-02T10:30:00Z",
  "completedAt": null
}
```

### 4. Send Message to AI Tutor (Student Only)

```bash
curl -X POST https://localhost:5001/api/chat/send \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": 1,
    "lessonId": 3,
    "message": "Can you explain what a variable is in Python?"
  }'
```

**Response:**
```json
{
  "response": "A variable in Python is a named container that stores data values. You can think of it as a labeled box where you put information. In Python, you create a variable by assigning a value to a name using the equals sign, like this: x = 5 or name = 'Alice'. The variable then holds that value and you can use it throughout your program."
}
```

### 5. Get My Enrollments (Student)

```bash
curl -X GET https://localhost:5001/api/enrollments/my-enrollments \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
[
  {
    "id": 10,
    "userId": 5,
    "studentName": "Jane Smith",
    "studentEmail": "jane@example.com",
    "courseId": 1,
    "courseTitle": "Introduction to Python",
    "status": "Active",
    "enrolledAt": "2026-05-02T10:30:00Z",
    "completedAt": null
  }
]
```

### 6. Get My Courses (Instructor)

```bash
curl -X GET https://localhost:5001/api/courses/my-courses \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Introduction to Python",
    "description": "Learn Python programming from scratch",
    "isPublished": true,
    "instructorId": 1,
    "instructorName": "John Doe",
    "createdAt": "2026-05-02T10:00:00Z",
    "moduleCount": 3,
    "enrollmentCount": 25
  }
]
```

### 7. Get Chat History

```bash
curl -X GET https://localhost:5001/api/chat/sessions/5/history \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
[
  {
    "role": "User",
    "content": "Can you explain what a variable is in Python?",
    "timestamp": "2026-05-02T11:00:00Z"
  },
  {
    "role": "Assistant",
    "content": "A variable in Python is a named container that stores data values...",
    "timestamp": "2026-05-02T11:00:05Z"
  }
]
```

### 8. Update Course (Instructor)

```bash
curl -X PUT https://localhost:5001/api/courses/1 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Advanced Python Programming",
    "isPublished": true
  }'
```

**Response:** 204 No Content

### 9. Delete Course (Instructor/Admin)

```bash
curl -X DELETE https://localhost:5001/api/courses/1 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:** 204 No Content

### 10. Get Current User Profile

```bash
curl -X GET https://localhost:5001/api/user/me \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "id": 5,
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "role": "Student",
  "profilePictureUrl": "https://example.com/photos/jane.jpg",
  "createdAt": "2026-01-15T08:00:00Z"
}
```

## Status Codes

- `200 OK`: Request succeeded
- `201 Created`: Resource created successfully
- `204 No Content`: Request succeeded with no response body
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Missing or invalid authentication token
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Rate Limiting

The API implements standard rate limiting:
- 100 requests per minute per user
- 1000 requests per hour per user

Exceeding these limits returns a `429 Too Many Requests` response.

## Pagination

List endpoints support pagination using query parameters:

```bash
curl "https://localhost:5001/api/courses?page=1&pageSize=20"
```

## Error Response Format

```json
{
  "message": "Course not found",
  "statusCode": 404,
  "timestamp": "2026-05-02T12:00:00Z"
}
```
