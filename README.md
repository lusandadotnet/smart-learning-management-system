# Smart Learning Management System

A secure, enterprise-grade Learning Management System built with **.NET 8**, featuring **Entra ID authentication**, **AI-powered chatbot** support, and **hybrid EF Core/Dapper** data access for optimal performance.

## Features

- **Multi-Role Authorization**: Secure access control with Entra ID supporting Student, Instructor, and Administrator roles
- **AI-Powered Tutor**: Integrated Azure OpenAI chatbot providing contextual course assistance
- **Hybrid Data Access**: EF Core for transactional operations, Dapper for high-performance read queries
- **RESTful API**: Clean architecture with comprehensive CRUD operations
- **Azure Cloud Deployment**: Production-ready infrastructure-as-code using Bicep
- **Comprehensive Testing**: xUnit test suite with 80%+ code coverage

## Architecture

```
SmartLMS/
├── Domain/              # Core entities and enums
│   ├── Entities/
│   └── Enums/
├── Application/         # Business logic layer
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
├── Infrastructure/      # Data access and API
│   ├── Controllers/
│   └── Persistance/
├── Tests/              # xUnit test suite
├── Deployment/         # Azure Bicep templates
└── Migrations/         # EF Core migrations
```

## Stack

- **Framework**: .NET 8 / ASP.NET Core Web API
- **Authentication**: Microsoft Entra ID (Azure AD)
- **Data Access**: 
  - Entity Framework Core 8 (writes)
  - Dapper 2.1 (high-performance reads)
- **Database**: Microsoft SQL Server
- **AI**: Azure OpenAI (GPT-4)
- **Cloud**: Microsoft Azure (App Service, Azure Functions, SQL Database)
- **Testing**: xUnit, Moq
- **IaC**: Bicep

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB for development)
- Azure subscription
- Entra ID tenant configured
- Azure OpenAI resource with GPT-4 deployment

##  Local Development Setup

### 1. Clone the Repository

```bash
git clone https://github.com/lusandadotnet/smart-learning-management-system.git
cd smart-learning-management-system
```

### 2. Configure App Settings

Update `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartLMSDb;Trusted_Connection=True;"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

### 3. Run Database Migrations

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5001`

### 5. Access Swagger UI

Navigate to `https://localhost:5001/swagger` to explore the API endpoints.

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

##  Azure Deployment

### Prerequisites

- Azure CLI installed
- Logged in to Azure (`az login`)
- Entra ID app registration configured

## Deploy Infrastructure

```bash
cd Deployment
chmod +x deploy.sh
./deploy.sh
```

The script will:
1. Create a resource group
2. Deploy App Service, SQL Database, and Azure OpenAI
3. Configure managed identity and role assignments
4. Deploy the application
5. Run database migrations

### Manual Deployment

```bash
# Create resource group
az group create --name rg-smartlms --location eastus

# Deploy Bicep template
az deployment group create \
  --resource-group rg-smartlms \
  --template-file main.bicep \
  --parameters \
    sqlAdminLogin=sqladmin \
    sqlAdminPassword='YourSecurePassword!' \
    entraIdTenantId='your-tenant-id' \
    entraIdClientId='your-client-id'
```

##  Database Schema

### Core Entities

- **Users**: Student, Instructor, Administrator with Entra ID integration
- **Courses**: Learning content organized by instructors
- **Modules**: Course subdivisions
- **Lessons**: Individual learning units with content
- **Enrollments**: Student-course relationships with status tracking
- **ChatSessions**: AI tutor conversation history
- **ChatMessages**: Individual messages in conversations
- **AiTutorConfiguration**: Course-specific AI settings

##  API Authentication

All endpoints (except anonymous course browsing) require a valid JWT token from Entra ID.

### Example Request

```bash
curl -X GET https://your-app.azurewebsites.net/api/courses/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Key API Endpoints

### Courses
- `GET /api/courses` - List all courses (public)
- `GET /api/courses/{id}` - Get course details
- `POST /api/courses` - Create course (Instructor)
- `PUT /api/courses/{id}` - Update course (Instructor)
- `DELETE /api/courses/{id}` - Delete course (Instructor)

### Enrollments
- `POST /api/enrollments` - Enroll student (Instructor)
- `GET /api/enrollments/my-enrollments` - Get my enrollments (Student)
- `PATCH /api/enrollments/{id}/status` - Update status (Instructor)

### AI Chat
- `POST /api/chat/send` - Send message to AI tutor (Student)
- `GET /api/chat/sessions` - Get my chat sessions (Student)
- `GET /api/chat/sessions/{id}/history` - Get session history (Student)

### Users
- `GET /api/user/me` - Get current user profile
- `GET /api/user` - List all users (Administrator)

##  Entra ID Role Configuration

Ensure your Entra ID app registration has the following app roles defined:

```json
{
  "appRoles": [
    {
      "displayName": "Student",
      "value": "Student",
      "allowedMemberTypes": ["User"]
    },
    {
      "displayName": "Instructor",
      "value": "Instructor",
      "allowedMemberTypes": ["User"]
    },
    {
      "displayName": "Administrator",
      "value": "Administrator",
      "allowedMemberTypes": ["User"]
    }
  ]
}
```
