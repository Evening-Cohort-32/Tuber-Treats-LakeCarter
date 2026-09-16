using Microsoft.AspNetCore.Components;
using TuberTreats.Models;
using TuberTreats.Models.DTO;

// __Data__
List<TuberDriver> tuberDrivers = new List<TuberDriver>
{
    new TuberDriver {Id=1, Name="Mark"},
    new TuberDriver {Id=2, Name="Lake"},
    new TuberDriver {Id=3, Name = "Juju"}
};

List<Customer> customers = new List<Customer>
{
    new Customer {Id=1, Name="Bob", Address="555 Johnson Ln"},
    new Customer {Id=2, Name= "John", Address="679 Holly Rd"},
    new Customer {Id=3, Name="Fox", Address="1999 Space Rd"},
    new Customer {Id = 4, Name="Link", Address= "468 Hyrule Ln"}
};

List<Topping> toppings = new List<Topping>
{
    new Topping {Id=1, Name="Beans"},
    new Topping {Id=2, Name="Butter"},
    new Topping {Id=3, Name="Tuna"},
    new Topping {Id=4, Name="sour Cream"},
    new Topping {Id=5, Name="Bacon"},
};

List<TuberOrder> tuberOrders = new List<TuberOrder>
{
    new TuberOrder {Id = 1, CustomerId=1, TuberDriverId=1,OrderPlacedOnDate=new DateTime(2026,2,13), DeliveredOnDate= new DateTime(2026,2,13) },
    new TuberOrder {Id = 2, CustomerId=2, TuberDriverId=2,OrderPlacedOnDate=new DateTime(2026,4,1), DeliveredOnDate= new DateTime(2026,4,2) },
    new TuberOrder {Id = 3, CustomerId=3, TuberDriverId=3,OrderPlacedOnDate=new DateTime(2026,5,20), DeliveredOnDate= new DateTime(2026,5,20) }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping {Id=1, TuberOrderId=1, TuberToppingId = 1},
    new TuberTopping {Id=2, TuberOrderId=1, TuberToppingId=2},
    new TuberTopping {Id =3, TuberOrderId=2, TuberToppingId=5}
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here

//TuberOrder
app.MapGet("/api/tuberOrders", () =>
{
    return tuberOrders.Select(o => new TuberOrderDto
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        TuberDriverId = o.TuberDriverId,
        OrderPlacedOnDate = o.OrderPlacedOnDate,
        DeliveredOnDate = o.DeliveredOnDate
    });
});

app.MapGet("/api/tuberOrders/{id}", (int id) =>
{
    List<int> toppingId = tuberToppings.Where(tt => tt.TuberOrderId == id)
    .Select(tt => tt.TuberToppingId).ToList();

    return tuberOrders.Where(to => to.Id == id)
    .Select(to => new TuberOrderDto
    {
        Id = to.Id,
        CustomerId = to.CustomerId,
        TuberDriverId = to?.TuberDriverId,
        OrderPlacedOnDate = to.OrderPlacedOnDate,
        DeliveredOnDate = to?.DeliveredOnDate,
        Customer = customers.Where(c => c.Id == to.CustomerId)
        .Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address
        }).First(),
        Driver = to.TuberDriverId == null ? null : tuberDrivers.Where(d => d.Id == to.TuberDriverId)
        .Select(d => new TuberDriverDto
        {
            Id = d.Id,
            Name = d.Name
        }).First(),
        Toppings = toppings.Where(t => toppingId.Contains(t.Id))
        .Select(t => new ToppingDto
        {
            Id = t.Id,
            Name = t.Name
        }).ToList()
    });
});

app.MapPost("/api/tuberorders", (TuberOrder tuberOrder) =>
{
    //Check if valid customer was provided
    Customer customer = customers.FirstOrDefault(c => c.Id == tuberOrder.CustomerId);
    if (customer == null) { return Results.BadRequest(); }

    //Create id for order
    tuberOrder.Id = tuberOrders.Max(to => to.Id) + 1;
    //set OrderPlacedOnDate
    tuberOrder.OrderPlacedOnDate = DateTime.Today;

    //Add order to database
    tuberOrders.Add(tuberOrder);


    List<int> toppingId = tuberToppings.Where(tt => tt.TuberOrderId == tuberOrder.Id)
    .Select(tt => tt.TuberToppingId).ToList();

    return Results.Created($"/api/tuberOrders/{tuberOrder.Id}", new TuberOrderDto
    {
        Id = tuberOrder.Id,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = null,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        DeliveredOnDate = null,
        Customer = customers.Where(c => c.Id == tuberOrder.CustomerId)
        .Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address
        }).First(),
        Driver = null,
        Toppings = toppings?.Where(t => toppingId.Contains(t.Id))
        .Select(t => new ToppingDto
        {
            Id = t.Id,
            Name = t.Name
        }).ToList()
    });
});



//Topping
app.MapGet("/api/toppings/", () =>
{
    return toppings.Select(t => new ToppingDto
    {
        Id = t.Id,
        Name = t.Name
    });
});

app.MapGet("/api/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);

    if (topping == null)
    {
        return Results.NotFound();
    }
    else return Results.Ok(new ToppingDto
    {
        Id = topping.Id,
        Name = topping.Name
    });
});


//TuberToppings
app.MapGet("/api/tubertoppings", () =>
{
    return tuberToppings.Select(tt => new TuberToppingDto
    {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        TuberToppingId = tt.TuberToppingId
    });
});

app.MapGet("/api/tubertoppings/{id}", (int id) =>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if (tuberTopping == null)
    {
        return Results.NotFound();
    }
    else return Results.Ok(new TuberToppingDto
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        TuberToppingId = tuberTopping.TuberToppingId
    });
});


//Customers
app.MapGet("api/customers/", () =>
{
    return customers.Select(c => new CustomerDto
    {
        Id = c.Id,
        Address = c.Address,
        Name = c.Name
    });
});

app.MapGet("api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    else return Results.Ok(new CustomerDto
    {
        Id = customer.Id,
        Address = customer.Address,
        Name = customer.Name,
        TuberOrders = tuberOrders.Where(to => to.CustomerId == customer.Id)
        .Select(to => new TuberOrderDto
        {
            Id = to.Id,
            CustomerId = to.CustomerId,
            TuberDriverId = to.TuberDriverId,
            OrderPlacedOnDate = to.OrderPlacedOnDate,
            DeliveredOnDate = to.DeliveredOnDate,
        }).ToList()
    });
});


//TuberDrivers
app.MapGet("/api/tuberdrivers", () =>
{
    return tuberDrivers.Select(td => new TuberDriverDto
    {
        Id = td.Id,
        Name = td.Name
    });
});

app.MapGet("/api/tuberdrivers/{id}", (int id) =>
{
    TuberDriver tuberDriver = tuberDrivers.FirstOrDefault(td => td.Id == id);

    if (tuberDriver == null)
    {
        return Results.NotFound();
    }
    else return Results.Ok(new TuberDriverDto
    {
        Id = tuberDriver.Id,
        Name = tuberDriver.Name
    });
});


app.Run();
//don't touch or move this!
public partial class Program { }