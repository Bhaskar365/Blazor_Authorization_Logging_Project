using AutoMapper;
using LearnAPI.Container;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ICustomerService,CustomerService>();
builder.Services.AddDbContext<LearndataContext>(o=>
    o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

builder.Services.AddAuthentication("BasicAuthentication").
    AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication",null);

var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddCors(p => p.AddPolicy("corspolicy", build => 
{
    build.WithOrigins("http://domain.com").AllowAnyMethod().AllowAnyHeader();
}));

// multiple cors domain declaration and implementation possible here
//  or in controller as EnableCors("corspolicy1") . To disable specific CORS policy DisableCors() is used

//builder.Services.AddCors(p => p.AddDefaultPolicy(build =>
//{
//    build.WithOrigins("http://domain1.com").AllowAnyMethod().AllowAnyHeader();
//}));
//---------------------------------------------------------------------------------------//

// all allow cors policy

//builder.Services.AddCors(p => p.AddPolicy("corspolicy1", build =>
//{
//    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
//}));
//---------------------------------------------------------------------------------------//



string logpath = builder.Configuration.GetSection("Logging:Logpath").Value;

var _logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logpath)
    .CreateLogger();

builder.Logging.AddSerilog(_logger);

var _jwtSetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtSetting);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors();   // For cors with origin * , allow all, no policy name required



app.UseCors("corspolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
