using RealTimeBookingSystem.Hubs;
using RealTimeBookingSystem.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Required for API
builder.Services.AddSignalR();     // Required for Real-time

// Register Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<IBookingService, BookingService>();

// Register BroadcastService as Singleton and HostedService
builder.Services.AddSingleton<BroadcastService>();
builder.Services.AddSingleton<IBroadcastService>(provider => provider.GetRequiredService<BroadcastService>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<BroadcastService>());

// Register GameService as Singleton and HostedService
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<IGameService>(provider => provider.GetRequiredService<GameService>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<GameService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map endpoints
app.MapRazorPages();
app.MapControllers();
app.MapHub<BookingHub>("/bookingHub"); // SignalR Endpoint

app.Run();
