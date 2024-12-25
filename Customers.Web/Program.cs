using System.ComponentModel.DataAnnotations;
using Customers.Web.Customers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CustomerData>();

var app = builder.Build();

app.MapCustomerEndpoints();

app.MapGet("/brewcoffee", (HttpResponse response) =>
    {
        response.StatusCode = StatusCodes.Status418ImATeapot;
        response.ContentType = "text/plain";
        return response.WriteAsync("I'm a a teapot - I cannot brew coffee");
    })
    .WithName("BrewCoffee")
    .WithOpenApi();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();