using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CompressorImagens.Services.ImagemService>();

// Adiciona o suporte ao CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        // Permite o frontend de outro domínio acessar a API
        policy.WithOrigins("https://localhost:44332") // URL do seu frontend, altere conforme necessário
              .AllowAnyMethod()  // Permite qualquer método HTTP
              .AllowAnyHeader()  // Permite qualquer cabeçalho
              .AllowCredentials(); // Permite credenciais (se necessário)
    });
});

// Adiciona o Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Imagens", Version = "v1" });
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// Adiciona serviços de controladores
builder.Services.AddControllers();

var app = builder.Build();

// Habilita o CORS
app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    // Habilita o Swagger no ambiente de desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Imagens v1");
        c.RoutePrefix = string.Empty; // Acessar diretamente a partir da raiz (http://localhost:5000/)
    });
}

app.UseRouting();

app.MapControllers();

app.Run();
