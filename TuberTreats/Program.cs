using System.Reflection.Metadata.Ecma335;
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
    new TuberTopping {Id=1, TuberOrderId=1, ToppingId = 1},
    new TuberTopping {Id=2, TuberOrderId=1, ToppingId=2},
    new TuberTopping {Id =3, TuberOrderId=2, ToppingId=5}
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

//____________
//TuberOrder
//____________
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
    if (tuberOrders.FirstOrDefault(to => to.Id == id) == null)
    {
        return Results.NotFound();
    }

    List<int> toppingId = tuberToppings.Where(tt => tt.TuberOrderId == id)
    .Select(tt => tt.ToppingId).ToList();

    return Results.Ok(tuberOrders.Where(to => to.Id == id)
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
    })
    );
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
    .Select(tt => tt.ToppingId).ToList();

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

app.MapPut("/api/tuberOrders/{id}", (int id, TuberOrder tuberOrder) =>
{
    TuberOrder tuberOrderToUpdate = tuberOrders.FirstOrDefault(to => to.Id == id);
    //check if order is valid
    if (tuberOrder.Id != id || tuberOrderToUpdate == null)
    {
        return Results.BadRequest();
    }

    else tuberOrders[id - 1] = tuberOrder;
    return Results.NoContent();
});

app.MapPost("/api/tuberOrders/{id}/complete", (int id) =>
{
    TuberOrder tuberOrderToComplete = tuberOrders.FirstOrDefault(to => to.Id == id);
    if (tuberOrderToComplete == null)
    {
        return Results.BadRequest($"There is no order with id {id}");
    }

    tuberOrderToComplete.DeliveredOnDate = DateTime.Now;
    tuberOrders[id - 1] = tuberOrderToComplete;
    return Results.Created();
});

//____________________
//Topping
//____________________
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

//____________________
//TuberToppings
//____________________
app.MapGet("/api/tubertoppings", () =>
{
    return tuberToppings.Select(tt => new TuberToppingDto
    {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        ToppingId = tt.ToppingId
    });
});

app.MapPost("/api/tubertoppings/", (TuberTopping tuberTopping) =>
{
    tuberTopping.Id = tuberToppings.Max(tt => tt.Id) + 1;
    //Check if orderId and topping Id are valid.
    if (tuberOrders.FirstOrDefault(to => to.Id == tuberTopping.TuberOrderId) == null)
    {
        return Results.BadRequest($"There is no tuber order with id {tuberTopping.TuberOrderId}");
    }

    if (toppings.FirstOrDefault(t => t.Id == tuberTopping.ToppingId) == null)
    {
        return Results.BadRequest($"There is no topping with id {tuberTopping.ToppingId}");
    }

    tuberToppings.Add(tuberTopping);
    return Results.Created($"/api/tuberToppings/{tuberTopping.Id}", new TuberToppingDto
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});

//remove topping from order
app.MapDelete("/api/tuberTopping/{id}", (int id) =>
{
    TuberTopping tuberToppingToDelete = tuberToppings.FirstOrDefault(to => to.Id == id);
    if (tuberToppingToDelete == null)
    {
        return Results.BadRequest($"There is no tuber topping with id{id}");
    }

    tuberToppings.Remove(tuberToppingToDelete);
    return Results.NoContent();
});


//____________
//Customers
//____________

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

app.MapPost("/api/customers/", (Customer customer) =>
{
    customer.Id = customers.Max(c => c.Id) + 1;
    customers.Add(customer);

    return Results.Created($"/api/customers/{customer.Id}", new CustomerDto
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });
});

app.MapDelete("/api/customers/{id}", (int id) =>
{
    Customer customerToDelete = customers.FirstOrDefault(c => c.Id == id);
    if (customerToDelete == null) { return Results.BadRequest($"There is no customer with id {id}"); }

    customers.Remove(customerToDelete);
    return Results.NoContent();
});


//____________
//TuberDrivers
//____________
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