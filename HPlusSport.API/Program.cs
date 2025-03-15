using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // This prevents sending error message back to the caller
        //options.SuppressModelStateInvalidFilter = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShopContext>(options =>
{
    options.UseInMemoryDatabase("Shop");
});

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/products", async (ShopContext _context) =>
{
    return await _context.Products.ToArrayAsync();
});

app.MapGet("/products/{id}", async (int id, ShopContext _context) =>
{
    var product = await _context.Products.FindAsync(id);


    if (product == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

app.MapGet("/products/available", async (ShopContext _context) =>
{
    var availableProducts = await _context.Products.Where(x => x.IsAvailable == true).ToArrayAsync();
    if (availableProducts == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(availableProducts);
}).WithName("GetProduct");

app.MapDelete("/products/{id}", async (int id, ShopContext _context) =>
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return Results.NotFound();

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();

    return Results.Ok(product);
});

app.MapPut("/products/{id}", async (int id, Product product, ShopContext _context) =>
{
    if (id != product.Id)
    {
        return Results.BadRequest();
    }

    _context.Entry(product).State = EntityState.Modified;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!_context.Products.Any(x => x.Id == id))
        {
            return Results.NotFound();
        }
        else
        {
            throw;
        }
    }
    return Results.NoContent();
});

app.MapPost("/products", async (Product product, ShopContext _context) =>
{
    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return Results.CreatedAtRoute(
        "GetProduct",
        new { id = product.Id },
        product
        );
});

app.MapPost("/products/{ids}", async (int[] ids, ShopContext _context) =>
{
    var products = new List<Product>();
    foreach (var id in ids)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return Results.NotFound();
        products.Add(product);
    }

    _context.Products.RemoveRange(products);
    await _context.SaveChangesAsync();
    
    return Results.Ok(products);
});

app.Run();
