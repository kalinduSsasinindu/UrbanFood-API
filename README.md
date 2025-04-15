# DMCW Project

## Configuration Setup

1. Create a new `appsettings.Development.json` file in the DMCW.API project with your actual configuration values:

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

2. Make sure to add `appsettings.Development.json` to your `.gitignore` file to keep your credentials secure.

3. For production, use environment variables or a secure configuration management system.

## Security Notes

- Never commit sensitive information to version control
- Use environment variables for production deployments
- Regularly rotate your API keys and credentials
- Follow the principle of least privilege for database access 