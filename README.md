# Training API

A REST API for managing device resources

## Features
- CRUD operations for devices.
- Filter by brand and state.
- Validations: Cannot update creation time; cannot update name/brand or delete if in-use.
- Persisted to SQLite database (devices.db).
- API documented via Swagger.
- Unit tests with xUnit.
- Containerized with Docker.

## Requirements
- .NET 9 SDK
- C# 13
- Docker

## Setup and Run Locally
1. Clone the repository.
2. Navigate to `TrainingApi` and run `dotnet restore`.
3. Run `dotnet build`.
4. Run `dotnet run`.
5. Access the API at `https://localhost:5001` or `http://localhost:5000`.
6. Swagger UI: `https://localhost:5001/swagger`.

## Running Tests
1. Navigate to `TrainingApi.Tests`.
2. Run `dotnet test`.

## Docker Containerization
1. Build the image: `docker build -t training-api -f Dockerfile .`
2. Run the container: `docker run -d -p 8080:80 --name training-api-container training-api`
3. Access at `http://localhost:8080/api/devices`.
4. To persist DB outside container, use volume: `-v $(pwd)/data:/app`

## API Endpoints
- `POST /api/devices`: Create device.
- `GET /api/devices`: Get all devices.
- `GET /api/devices/{id}`: Get single device.
- `GET /api/devices/brand?brand={brand}`: Get by brand.
- `GET /api/devices/state?state={state}`: Get by state (Available, InUse, Inactive).
- `PUT /api/devices/{id}`: Full update.
- `PATCH /api/devices/{id}`: Partial update (JSON Patch format).
- `DELETE /api/devices/{id}`: Delete device.

## Notes
- Database: SQLite (file: devices.db).
- Tests use in-memory DB for isolation.
