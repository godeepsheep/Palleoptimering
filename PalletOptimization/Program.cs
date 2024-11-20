using DotNetEnv;
using PalletOptimization.Data;
using PalletOptimization.Utilities;
using Microsoft.EntityFrameworkCore;
using PalletOptimization.Controllers;


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

var app = builder.Build();

// Test CRUD stuff here if needed
//CreateScope starts a new DI for this block of code.
//GetRequiredService<PalletsController>() tells the DI to give an instance of PalletsController.
//The DI container makes an AppDbContext and injects it in to PalletsController constructor.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.PalletGroups.Add(new PalletOptimization.Models.PalletGroup
    {
        Length = 1200,
        Width = 800,
        Height = 150,
        BaseWeight = 20,
        BaseMaxWeight = 5000,
    });
    context.SaveChanges();
    
    //test 1
    
    //Test 2
    
    
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