using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebHost.Customization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServicesSdk(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/carrinho" , async (Carrinho Carrinho , IDistributedCache redis) =>
{
    await redis.SetStringAsync(Carrinho.UsuarioId , JsonSerializer.Serialize(Carrinho));
    return true;
}).RequireAuthorization("Cliente");

app.MapGet("/carrinhos" , async (string usuarioId , IDistributedCache redis) =>
{
    var data = await redis.GetStringAsync(usuarioId);
    if(string.IsNullOrEmpty(data))
        return null;

    var carrinho = JsonSerializer.Deserialize<Carrinho>(data , new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = false ,

    });

    return carrinho;
}).RequireAuthorization("Cliente");

app.UseServiceApplicationAuth();

app.Run();
record Carrinho(string UsuarioId , List<Produto> Produtos);
record Produto(string Nome , int Quantidade , Decimal PrecoUnitario);