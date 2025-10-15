# TaskFlow API

TaskFlow is a Trello inspired task management API built as a personal portfolio project. It’s a simplified clone focused on showcasing clean architecture, CQRS, and scalable backend design in .NET 8.
## Features

*   **User Authentication**: Secure user registration and login using JWT (JSON Web Tokens).
*   **Board, Column, Task Management**: Full CRUD operations.
*   **Board Invinatation**: Owners can invite users to collaborate on boards with specific roles.
*   **Role-Based Access Control (RBAC)**: Boards can be set as public or private and support three permission levels: Owner, Editor, and Viewer.
*   **Task Reordering**: Endpoints to change tasks order and move them between columns.
*   **Commenting System**: Ability to add and manage comments on tasks.
*   **Email Service**: A service for sending email notifications for application events.
*   **API Versioning**: Support for multiple API versions.
*   **Admin Capabilities**: The system includes an `Admin` role, intended for user management endpoints.

## Tech Stack & Architecture

*   **Framework**: .NET 8, ASP.NET Core
*   **Architecture**: Clean Architecture, Repository & Unit of Work Pattern, CQRS with MediatR, Middleware, Behavior Pipelines
*   **Database**: PostgreSQL with Entity Framework Core
*   **Authentication**: JWT
*   **API Documentation**: Swagger (OpenAPI) with versioning support
*   **Email Service**: MailKit via SMTP for sending emails
*   **Testing**: xUnit, Moq, and AspNetCore.Mvc.Testing for integration tests
*   **Containerization**: Docker & Docker Compose
*   **Logging**: Serilog
*   **Object Mapping**: AutoMapper

### Project Structure

The solution follows the principles of **Clean Architecture** to ensure a separation of concerns, maintainability, and testability.

*   `TaskFlow.Domain`: Contains core business entities, enums, and domain logic interfaces. It has no dependencies on other layers.
*   `TaskFlow.Application`: Implements the application logic. It contains CQRS commands, queries, handlers, DTOs, and business rule validation. It depends only on the Domain layer.
*   `TaskFlow.Infrastructure`: Provides implementations for interfaces defined in the Application layer, such as repositories (using EF Core), authentication services (JWT), and email services. It depends on the Application layer.
*   `TaskFlow.API`: The presentation layer, an ASP.NET Core Web API project. It contains controllers, middleware, and configuration. It depends on the Application and Infrastructure layers.
*   `TaskFlow.Tests`: Contains unit and integration tests for the project, ensuring code quality and correctness.

## Getting Started

### Prerequisites

*   .NET 8 SDK
*   Docker and Docker Compose

### 1. Configuration

Create a `.env` file in the `TaskFlow.API` directory by copying the `TaskFlow.API/.env.example` file.

```bash
cp TaskFlow.API/.env.example TaskFlow.API/.env
```

Fill in the required values in the `.env` file, especially for the database connection and JWT secret.

```dotenv
# Database
POSTGRES_DB=taskflow
POSTGRES_USER=taskflow
POSTGRES_PASSWORD=your_strong_password

# JWT Configuration
JWT_SECRET=your_super_secret_jwt_key_that_is_long_and_secure
JWT_ISSUER=TaskFlow
JWT_AUDIENCE=TaskFlowUsers
# ... other variables
```

### 2. Running with Docker (Recommended)

The easiest way to get the application and its database running is with Docker Compose.

```bash
cd TaskFlow.API
docker-compose up --build
```

The API will be available at `http://localhost:5000`.

### 3. Running Locally (without Docker)

1.  Ensure you have a running PostgreSQL instance.
2.  Update the `ConnectionStrings:Default` and `Jwt:Secret` in `TaskFlow.API/appsettings.Development.json`.
3.  Navigate to the API project directory and run the application:

    ```bash
    cd TaskFlow.API
    dotnet run
    ```

## API Usage

Once the application is running, you can access the Swagger UI at the root URL: `http://localhost:5000`.

### Authentication

1.  **Register a new user**: Use the `POST /api/auth/register` endpoint.
2.  **Login**: Use the `POST /api/auth/login` endpoint with your credentials to receive an access token.
3.  **Authorize**: Click the "Authorize" button in Swagger and enter the token in the format `Bearer {your_token}` to access protected endpoints.

## Testing

To run the suite of unit and integration tests, execute the following command from the root directory of the solution:

```bash
dotnet test
```