using MSFD_LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

    
builder.Services.AddMemoryCache(); 
builder.Services.AddEndpointsApiExplorer();   // Required for Swagger
builder.Services.AddSwaggerGen(options =>
{
    // Adds an Authorize button to Swagger UI for testing JWT-protected endpoints
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token returned from /api/auth/login (no need to type \"Bearer \" prefix)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});


// Register DbContext with SQLite
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db"));

// Register Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Enforce a stronger password policy than the Identity defaults
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;

        // Lock accounts after repeated failed logins to mitigate brute-force attacks
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<LogiTrackContext>()
    .AddDefaultTokenProviders();


// PART 3 - Step 5: Security Review
// Register JWT Bearer authentication so [Authorize] validates the tokens issued by AuthController.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Keeps the entered bearer token across page reloads for easier manual testing
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseDefaultFiles();  // Serves wwwroot/index.html at the root URL
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



//  *** TESTING ***
// STEP 2 TEST BLOCK
var Item = new InventoryItem
{
    Name = "Pallet Jack",
    Quantity = 12,
    Location = "Warehouse A"
};

Item.DisplayInfo();
// -------------------------------



// STEP 3 TEST BLOCK
var order = new Order
{
    OrderId = 1001,
    CustomerName = "Samir"
};

// Add items
order.AddItem(new InventoryItem { ItemId = 1, Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" });
order.AddItem(new InventoryItem { ItemId = 2, Name = "Forklift", Quantity = 3, Location = "Warehouse B" });

// Remove one item
order.RemoveItem(1);

// Print summary
Console.WriteLine(order.GetOrderSummary());
// -------------------------------



// STEP 5: Seed and Test Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
    context.Database.EnsureCreated();   // Guarantees the SQLite file exists before seeding

    // Seed initial data if necessary
    if (!context.InventoryItems.Any())
    {
        context.InventoryItems.AddRange(
            new InventoryItem { Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" },
            new InventoryItem { Name = "Forklift", Quantity = 3, Location = "Warehouse B" }
        );
        context.SaveChanges();
    }

    // Retrieve and print items (confirms persistence)
    foreach (var inv in context.InventoryItems)
    {
        inv.DisplayInfo();
    }
}
// -------------------------------


// STEP 6: Save and Retrieve Order with Items
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();

    // Ensure database exists
    context.Database.EnsureCreated();

    // Only seed if no orders exist
    if (!context.Orders.Any())
    {
        var seedOrder = new Order
        {
            CustomerName = "Samir"
        };

        seedOrder.AddItem(new InventoryItem { Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" });
        seedOrder.AddItem(new InventoryItem { Name = "Forklift", Quantity = 3, Location = "Warehouse B" });

        context.Orders.Add(seedOrder);
        context.SaveChanges();
    }

    // Retrieve order with items using Include
    var savedOrder = context.Orders
        .Include(o => o.Items)
        .FirstOrDefault();

    if (savedOrder != null)
    {
        Console.WriteLine(savedOrder.GetOrderSummary());
        foreach (var item in savedOrder.Items)
        {
            item.DisplayInfo();
        }
    }
}

// -------------------------------
//  *** END TESTING ***

app.Run();
