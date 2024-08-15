using SD.LLBLGen.Pro.DQE.SqlServer;
using SD.LLBLGen.Pro.ORMSupportClasses;
using Microsoft.Data.SqlClient;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development",
          builder =>
              builder
                .AllowAnyHeader()
                .WithExposedHeaders("X-Message-Code", "Content-Type")
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true)
          );
});



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

RuntimeConfiguration.AddConnectionString("ConnectionString.SQL Server (SqlClient)", "data source=124.158.6.137,1434;initial catalog=icorp2024;User ID=sa;Password=Icorp@2024;TrustServerCertificate=True");
RuntimeConfiguration.ConfigureDQE<SQLServerDQEConfiguration>(
    c => c.SetDefaultCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2012)
          .AddDbProviderFactory(typeof(SqlClientFactory))
          .SetTraceLevel(System.Diagnostics.TraceLevel.Verbose));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Development");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
