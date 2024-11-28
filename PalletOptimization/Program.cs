using DotNetEnv;
using PalletOptimization.Data;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Controllers;

// Load environment variables from the .env file
Env.Load();
//Webapp (We are able to attach things to our application, like controllers and such)
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
builder.Services.AddScoped<PalletGroupController>(); //Here we are adding a palletscontroller to our application. 
//AddScoped Whenever someone (like an HTTP request) needs this thing (like an instance), give them their very own copy.
//But only while they’re here. When they leave, throw it away.
//When your controller (like PalletsController) needs access to a database (AppDbContext), AddScoped() makes sure a new database connection is provided for every HTTP request.
//While CreateScope() isnt tied to an HTTP request, so any background services or tests. 

//Builds the application
var app = builder.Build();

// Create a DI scope for testing CRUD operations
// This scope is independent of the HTTP pipeline
using (var scope = app.Services.CreateScope())
{
    // Get AppDbContext instance from the DI container
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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