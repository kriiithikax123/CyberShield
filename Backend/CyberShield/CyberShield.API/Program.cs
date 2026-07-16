using CyberShield.API.Data;
using CyberShield.API.Interfaces;
using CyberShield.API.Models;
using CyberShield.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.Configure<VirusTotalSettings>(
    builder.Configuration.GetSection("VirusTotal"));

builder.Services.AddScoped<IUrlScanService, UrlScanService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();