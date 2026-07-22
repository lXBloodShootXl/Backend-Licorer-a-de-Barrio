using Microsoft.EntityFrameworkCore;
using LICORERIA.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Intentar obtener DATABASE_URL (Railway)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string? connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convertir de formato postgres:// a formato Npgsql
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    connectionString = $"Host={uri.Host};" +
                       $"Port={uri.Port};" +
                       $"Database={uri.AbsolutePath.TrimStart('/')};" +
                       $"Username={userInfo[0]};" +
                       $"Password={userInfo[1]};" +
                       $"SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // Usar configuración local (appsettings.json)
    connectionString = builder.Configuration.GetConnectionString("LICORERIAContext");
}

// Configurar DbContext
builder.Services.AddDbContext<LICORERIA_DBContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    }));
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyApp", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
    });
});

// Añadir controladores, Swagger y la API de endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar repositorios
//Sólo en Clean Architecture,  no usaremos Clean Architecture
//builder.Services.AddScoped<IPersonaRepositorio, PersonaRepositorio>();

var app = builder.Build();

// Aplicar migraciones al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LICORERIA_DBContext>();
    try
    {
        dbContext.Database.Migrate(); // Aplica migraciones si no existen
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error aplicando migraciones: " + ex.Message);
    }

    // Ejecutar creación de vistas en la base de datos
    //await CrearVistas(dbContext);
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    //c.RoutePrefix = string.Empty; // Esto hace que Swagger esté en la raíz (puedes ajustarlo si necesitas otro lugar)
});

// Middleware
app.UseCors("MyApp");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();