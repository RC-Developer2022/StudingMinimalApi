using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("connect/token" , (Autenticacao autenticacao , IConfiguration configuration) =>
{
    if(autenticacao.Usuario == "Cliente" && autenticacao.Senha == "123")
        return new Token(configuration).Create("Cliente");

    return "Usuario não encontrado!";
});

app.Run();

record Autenticacao(string Usuario , string Senha);

public class Token(IConfiguration configuration)
{
    public object Create(string role)
    {
        var key = Encoding.ASCII.GetBytes(configuration["Autenticacao:key"]);

        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Role , role)
            }) ,
            Expires = DateTime.UtcNow.AddHours(3) ,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key) , SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandle = new JwtSecurityTokenHandler();
        var token = tokenHandle.CreateToken(tokenConfig);
        var tokenString = tokenHandle.WriteToken(token);
        return new
        {
            token = tokenString
        };
    }
}