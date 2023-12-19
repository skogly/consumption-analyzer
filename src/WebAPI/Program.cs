var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Services.AddWebServices(env);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapGet("/powerPrices", (ILogger<WebApplication> logger, IMediator mediator, IPowerPriceService powerPriceService) =>
{
    logger.LogInformation("Getting power prices");
    var powerPrices = powerPriceService.PowerPrices();
    return Results.Ok(powerPrices);
});

app.MapPost("/powerPrices", (ILogger<WebApplication> logger, IMediator mediator, IPowerPriceService powerPriceService) =>
{
    logger.LogInformation("Setting power prices for today to normal");
    powerPriceService.SetAllPowerPricesToNormal();
    return Results.Ok();
});

app.Run();
