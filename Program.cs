using Microsoft.EntityFrameworkCore;
using MinimalApiDotNet.Data;
using MinimalApiDotNet.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new System.Version())), ServiceLifetime.Scoped);

var app = builder.Build();

/// <summary>
/// In case you don't want to use Swagger in your deployed API,
/// remove the comment of the if statement below;
/// </summary>
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapGet("/product", async (
    MinimalContextDb context) =>
    await context.Products.ToListAsync())
    .WithName("GetProduct")
    .WithTags("Product");

app.MapGet("/product/{id}", async (
    Guid id,
    MinimalContextDb context) =>

    await context.Products.FindAsync(id)
        is Product product
            ? Results.Ok(product)
            : Results.NotFound())
    .Produces<Product>(StatusCodes.Status200OK)
    .Produces<Product>(StatusCodes.Status404NotFound)
    .WithName("GetProductById")
    .WithTags("Product");

app.MapPost("/product", async (
    MinimalContextDb context,
    Product product) =>
   {
       if (!MiniValidator.TryValidate(product, out var errors))
           return Results.ValidationProblem(errors);

       context.Products.Add(product);
       var result = await context.SaveChangesAsync();

       return result > 0
        // ? Results.Created($"/product/{product.Id}", product)
        ? Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product)
        : Results.BadRequest("An error occurred while trying to save the product");
   
   }).ProducesValidationProblem()
    .Produces<Product>(StatusCodes.Status201Created)
    .Produces<Product>(StatusCodes.Status400BadRequest)
    .WithName("PostProduct")
    .WithTags("Product");


app.MapPut("/product/{id}", async (
    Guid id,
    MinimalContextDb context,
    Product product) =>
{
    var existentProduct = await context.Products.AsNoTracking<Product>()
                                                .FirstOrDefaultAsync(p => p.Id == id);

    if (existentProduct == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(product, out var errors))
        return Results.ValidationProblem(errors);

    context.Products.Update(product);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("An error occurred while trying to save the product");

}).ProducesValidationProblem()
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutProduct")
.WithTags("Product");

app.MapDelete("/product/{id}", async (
        Guid id,
        MinimalContextDb context) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null) return Results.NotFound();

    context.Products.Remove(product);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("An error occurred while trying to save the product");

}).Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("DeleteProduct")
    .WithTags("Product");

app.Run();
