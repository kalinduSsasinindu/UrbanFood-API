# DMCW (Digital Marketplace for Creative Works)

A full-stack e-commerce platform built with .NET Core and Angular, supporting both MongoDB and Oracle databases. This project was developed as part of my university's Database Management module, showcasing the implementation of dual database systems in a real-world application.

## Project Context

This project was developed as a coursework assignment for the Database Management module at NIBM. The primary objective was to demonstrate proficiency in:
- Database design and implementation
- Working with different types of databases (NoSQL and Relational)
- Database integration in a real-world application
- Performance optimization and data modeling

> **Note:** This is a work in progress. While the database structure and basic CRUD operations are implemented, some advanced features and optimizations are still under development.

## Current Implementation Status

### Database Implementation
- **MongoDB**: Fully implemented and actively used in the project
  - All CRUD operations are functional
  - Generic repository pattern implemented
  - Proper dependency injection setup
  - Working with all entity types

- **Oracle**: Partially implemented but not currently in use
  - OracleDBContext and OracleRepository classes are implemented
  - Table creation scripts are ready
  - Facing issues with Oracle Cloud connection
  - Currently using MongoDB as the primary database

### Technical Details
- The project uses a generic repository pattern for MongoDB
- Entity mapping is handled through the `FilteredMongoCollection<T>` class
- Services are injected with `MongoDBContext` for data access
- Oracle implementation is ready but requires Oracle Cloud configuration

## Project Overview

DMCW is a modern e-commerce platform that allows users to:
- Browse and purchase products
- Manage orders and payments
- Create and manage seller profiles
- Review products and sellers
- Handle multiple payment methods
- Manage product variants and inventory

## Technical Insights

### Database Architecture
The project implements a hybrid database approach:
- **MongoDB**: Used for flexible document storage, particularly for:
  - Product catalogs with varying attributes
  - User-generated content (reviews, comments)
  - Session data and caching
- **Oracle**: Used for structured data requiring:
  - Complex transactions
  - Data integrity constraints
  - ACID compliance
  - Complex queries and reporting

### Implementation Challenges
- **Data Synchronization**: Maintaining consistency between MongoDB and Oracle
- **Transaction Management**: Handling distributed transactions across different database systems
- **Performance Optimization**: Balancing the strengths of both databases
- **Schema Design**: Creating flexible yet efficient data models

### Future Improvements
1. **Data Synchronization**: Implement a robust synchronization mechanism between MongoDB and Oracle
2. **Caching Layer**: Add Redis for improved performance
3. **Search Optimization**: Implement Elasticsearch for better product search
4. **Microservices**: Break down into smaller, focused services
5. **Real-time Updates**: Implement WebSocket for live updates

## Tech Stack

### Backend (.NET Core API)
- ASP.NET Core 8.0
- MongoDB for flexible document storage
- Oracle Database for structured data
- JWT Authentication
- Google OAuth Integration
- Cloudinary for image management
- RESTful API design

### Frontend (Angular)
- Angular 15+
- Material Design
- Responsive UI
- State management with NgRx
- Real-time updates
- Secure authentication flow

> **Note:** The Angular frontend is available in a separate repository: [DMCW-Frontend](https://github.com/kalinduSsasinindu/Urban-Food-Web)

## Features

- **Dual Database Support**
  - MongoDB for flexible document storage
  - Oracle for structured data and complex queries

- **User Management**
  - Secure authentication with JWT
  - Google OAuth integration
  - Role-based access control
  - User profiles and seller accounts

- **Product Management**
  - Product variants and options
  - Inventory tracking
  - Image management with Cloudinary
  - Product reviews and ratings

- **Order Processing**
  - Multi-step checkout
  - Multiple payment methods
  - Order tracking
  - Shipping address management

- **Seller Features**
  - Seller profiles
  - Store management
  - Order fulfillment
  - Performance analytics

## Authentication Implementation

### Google OAuth2 Authentication

The project implements Google OAuth2 as the primary authentication mechanism. This provides a secure and user-friendly way to authenticate users.

#### Features
- Secure Google OAuth2 integration
- JWT token generation and validation
- Role-based access control
- Refresh token support
- Automatic user profile creation

#### Implementation Details
1. **Configuration**
   ```json
   "Authentication": {
     "Google": {
       "ClientId": "your-google-client-id",
       "ClientSecret": "your-google-client-secret",
       "WebRedirectLocalUrl": "http://localhost:4200/",
       "WebRedirectUrl": "http://localhost:4200/"
     }
   }
   ```

2. **Authentication Flow**
   - User clicks "Sign in with Google"
   - Redirected to Google consent screen
   - After consent, Google redirects back with authorization code
   - Backend exchanges code for tokens
   - JWT token generated and returned to frontend
   - Frontend stores token for subsequent requests

3. **Security Features**
   - Secure token storage
   - Token expiration handling
   - Automatic token refresh
   - CSRF protection
   - Secure cookie handling

4. **User Management**
   - Automatic user profile creation on first login
   - Role assignment based on Google account
   - Profile information synchronization
   - Session management

#### Usage in Services
```csharp
[Authorize]
public class YourService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public YourService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
```

#### Frontend Integration
The Angular frontend includes:
- Google OAuth2 client integration
- Token management service
- Auth guard for protected routes
- HTTP interceptor for token injection
- User profile management

## Getting Started

### Prerequisites
- .NET Core 8.0 SDK
- Node.js and npm
- MongoDB
- Oracle Database
- Angular CLI

### Backend Setup
1. Clone the repository
2. Create a new `appsettings.Development.json` file in the DMCW.API project with your actual configuration values:

```json
{
  "ConnectionStrings": {
    "MongoDB": "your-mongodb-connection-string",
    "DatabaseName": "your-database-name",
    "OracleDB": "your-oracle-connection-string"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

3. Make sure to add `appsettings.Development.json` to your `.gitignore` file to keep your credentials secure.

4. Run the application:
```bash
cd DMCW.API
dotnet run
```

### Frontend Setup
1. Clone the frontend repository: [DMCW-Frontend](https://github.com/kalinduSsasinindu/Urban-Food-Web)
2. Install dependencies:
```bash
npm install
```
3. Start the development server:
```bash
ng serve
```

## Security Notes

- Never commit sensitive information to version control
- Use environment variables for production deployments
- Regularly rotate your API keys and credentials
- Follow the principle of least privilege for database access

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Oracle Database Implementation Guide

### Setting Up OracleDBContext in Services

1. **Dependency Injection Setup**
   In your `Program.cs` or `Startup.cs`, add the OracleDBContext service:
   ```csharp
   builder.Services.AddScoped<OracleDBContext>();
   ```

2. **Using OracleDBContext in Services**
   Inject OracleDBContext into your service classes:
   ```csharp
   public class YourService
   {
       private readonly OracleDBContext _oracleContext;
       
       public YourService(OracleDBContext oracleContext)
       {
           _oracleContext = oracleContext;
       }
   }
   ```

3. **Accessing Repositories**
   Use the context to access repositories for different entities:
   ```csharp
   // Get products repository
   var productsRepo = _oracleContext.Products;
   
   // Get users repository
   var usersRepo = _oracleContext.Users;
   
   // Get orders repository
   var ordersRepo = _oracleContext.Orders;
   ```

4. **Performing CRUD Operations**
   ```csharp
   // Find operations
   var product = await productsRepo.FindOneAsync("WHERE ID = :id", 
       new Dictionary<string, object> { { "id", productId } });
   
   var products = await productsRepo.FindAsync("WHERE PRICE > :price", 
       new Dictionary<string, object> { { "price", 100 } });
   
   // Insert operations
   var newProductId = await productsRepo.InsertOneAsync(newProduct);
   
   // Update operations
   var updates = new Dictionary<string, object>
   {
       { "Price", 150 },
       { "Name", "Updated Product" }
   };
   await productsRepo.UpdateOneAsync(productId, updates);
   
   // Delete operations
   await productsRepo.SoftDeleteOneAsync(productId);
   ```

5. **Direct SQL Execution**
   For complex queries, use the direct execution methods:
   ```csharp
   // Execute query
   var result = await _oracleContext.ExecuteQueryAsync(
       "SELECT * FROM PRODUCTS WHERE PRICE > :price",
       new OracleParameter("price", 100));
   
   // Execute non-query
   await _oracleContext.ExecuteNonQueryAsync(
       "UPDATE PRODUCTS SET PRICE = :price WHERE ID = :id",
       new OracleParameter("price", 150),
       new OracleParameter("id", productId));
   ```

### Important Notes
- Ensure your Oracle connection string is properly configured in `appsettings.json`
- The Oracle implementation includes soft delete functionality
- All queries automatically include user filtering for user-owned entities
- Use transactions for complex operations that require atomicity

