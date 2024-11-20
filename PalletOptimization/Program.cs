using DotNetEnv;
using PalletOptimization.Data;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Controllers;

// Load environment variables from the .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Added the .env as Server_Password, Used dependency injection(DI) to manage AppDbContext, instead of manually creating it as an object each time.
// The "options" represent an instance of DbContextOptionsBuilder its a class from EntityFrameworkCore to set up database stuff. 
//We are basically writing var builder = new DbContextOptionsBuilder<AppDbContext>();
//builder.UseSqlServer("Server_Password"); its just the lambda function is cleaner than this shit. 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("Server_Password")));

// Register PalletController (PalletController depends on AppDbContext, so DI will inject AppDbContext instance into the PalletController constructor when u use it.)
//Scoped means that one instance is created per HTTP request.
builder.Services.AddScoped<PalletsController>();

//Builds the application
var app = builder.Build();

// Create a DI scope for testing CRUD operations
// This scope is independent of the HTTP pipeline
using (var scope = app.Services.CreateScope())
{
    // Get AppDbContext instance from the DI container
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Test: Add a new PalletGroup to the database
        Console.WriteLine("Testing database insertion...");
        var newPalletGroup = new PalletOptimization.Models.PalletGroup
        {
            Length = 1200,
            Width = 800,
            Height = 150,
            BaseWeight = 20,
            MaxWeight = 5000
        };
        context.PalletGroups.Add(newPalletGroup); // Add PalletGroup to the database context
        context.SaveChanges(); // Save changes to the database
        Console.WriteLine($"PalletGroup added with ID: {newPalletGroup.Id}");

        // Test: Retrieve and display all PalletGroups from the database
        var allPalletGroups = context.PalletGroups.ToList();
        Console.WriteLine("PalletGroups in the database:");
        foreach (var pallet in allPalletGroups)
        {
            Console.WriteLine($"ID: {pallet.Id}, Length: {pallet.Length}, Width: {pallet.Width}, Height: {pallet.Height}, BaseWeight: {pallet.BaseWeight}, BaseMaxWeight: {pallet.MaxWeight}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while testing the database: {ex.Message}");
    }
}

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

app.Run();