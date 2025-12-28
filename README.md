# Real-Time Booking System

A real-time booking system built with ASP.NET Core, SignalR, and Redis that allows multiple users to compete for booking blocks in a game-like environment.

## Features

- **Real-time Updates**: Uses SignalR for instant synchronization across all connected clients
- **Competitive Booking**: Multiple users can attempt to book blocks simultaneously
- **Game Mechanics**: Includes countdown timers, reward blocks with different values, and game states
- **Redis Backend**: Leverages Redis for high-performance state management and atomic operations
- **Responsive UI**: Interactive grid-based interface with visual feedback

## Technology Stack

- ASP.NET Core (.NET 10.0)
- SignalR for real-time communication
- Redis for distributed state management
- Razor Pages for UI
- StackExchange.Redis client library

## Prerequisites

- .NET 10.0 SDK
- Redis server (running on localhost:6379 or configured connection string)

## Setup

1. Clone the repository:
```bash
git clone https://github.com/shefat2002/RealTimeBookingSystem.git
cd RealTimeBookingSystem
```

2. Ensure Redis is running:
```bash
redis-server
```

3. Update the Redis connection string in `appsettings.json` if needed:
```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

4. Build and run the application:
```bash
dotnet restore
dotnet build
dotnet run --project RealTimeBookingSystem
```

5. Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## How It Works

- Users connect to the booking hub and can book available blocks
- The system uses Redis atomic operations to ensure only one user can book each block
- Games start automatically when minimum players join
- Reward blocks have different values and types
- All bookings are synchronized in real-time across all connected clients

## Project Structure

- `Controllers/` - API endpoints for booking operations
- `Hubs/` - SignalR hub for real-time communication
- `Services/` - Business logic (BookingService, GameService, BroadcastService)
- `Pages/` - Razor Pages UI
- `wwwroot/` - Static files and client-side assets

## License

This project is open source and available for educational purposes.
