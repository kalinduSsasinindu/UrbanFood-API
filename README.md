# DMCW (Digital Marketplace for Creative Works)

A full-stack e-commerce platform built with .NET Core and Angular, supporting both MongoDB and Oracle databases.

## Project Overview

DMCW is a modern e-commerce platform that allows users to:
- Browse and purchase products
- Manage orders and payments
- Create and manage seller profiles
- Review products and sellers
- Handle multiple payment methods
- Manage product variants and inventory

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

> **Note:** The Angular frontend is available in a separate repository: [DMCW-Frontend](https://github.com/your-username/dmcw-frontend)

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
1. Clone the frontend repository: [DMCW-Frontend](https://github.com/your-username/dmcw-frontend)
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

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 