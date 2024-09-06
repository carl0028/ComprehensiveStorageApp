using ComprehensiveStorageApp.ComprehensiveStorageApp;
using ComprehensiveStorageApp.Services;
//using ComprehensiveStorageApp.Services.ComprehensiveStorageApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register your storage services 
builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<TableStorageService>();
builder.Services.AddScoped<QueueStorageService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<OrderProcessingService>();

// Configure AppSettings to read from appsettings.json
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create tables and queues on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var tableStorageService = scope.ServiceProvider.GetRequiredService<TableStorageService>();
        var queueStorageService = scope.ServiceProvider.GetRequiredService<QueueStorageService>();

        await tableStorageService.CreateTablesIfNotExistsAsync();
        await queueStorageService.CreateQueueIfNotExistsAsync("order-processing");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>(); // Get the logger
    logger.LogError(ex, "An error occurred during startup."); // Log the error using the logger
}

await app.RunAsync();