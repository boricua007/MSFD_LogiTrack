using MSFD_LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
    
builder.Services.AddEndpointsApiExplorer();   // Required for Swagger
builder.Services.AddSwaggerGen();             // Required for Swagger


// Register DbContext with SQLite
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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
