using Web.Api.Service;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;
using Web.Api.Repository;
using Web.Api.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1, 0);
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ReportApiVersions = true;
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "API v1", Version = "v1" });
	options.SwaggerDoc("v2", new OpenApiInfo { Title = "API v2", Version = "v2" });

	options.DocInclusionPredicate((docName, apiDesc) =>
	{
		if (!apiDesc.TryGetMethodInfo(out var methodInfo))
		{
			return false;
		}

		var versions = methodInfo.DeclaringType
			.GetCustomAttributes(true)
			.OfType<ApiVersionAttribute>()
			.SelectMany(attr => attr.Versions);

		return versions.Any(v => $"v{v.MajorVersion}" == docName);
	});
});

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IStreamService, StreamService>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddCronJob<AnalyticJob>(c =>
{
	c.TimeZoneInfo = TimeZoneInfo.Local;
	//c.CronExpression = @"*/1 * * * *"; // This is for local testing. will run after every 3 min
	c.CronExpression = @"0 0 * * *"; // Runs 12am every day

});
//builder.Services.AddSingleton<IDataRepository, DataRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
		options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
		options.DocExpansion(DocExpansion.List);
	});
}
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();
