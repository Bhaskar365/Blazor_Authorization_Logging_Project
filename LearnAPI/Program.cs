using AutoMapper;
using LearnAPI.Container;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LearnAPI.Repos.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ICustomerService,CustomerService>();
builder.Services.AddTransient<IRefreshHandler,RefreshHandler>();
builder.Services.AddDbContext<LearndataContext>(o=>
    o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//builder.Services.AddAuthentication("BasicAuthentication").
//  AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication",null);

var _authKey = builder.Configuration.GetValue<string>("JwtSettings:securityKey");

builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(item => 
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});
    
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

app.MapGet("/minimalapi", ()=> "This is a text");

app.MapGet("/getText", (string x) => x + "is being called");

app.MapGet("/getcustomer", async (LearndataContext db) => 
{
    return await db.TblCustomers.ToListAsync();
});

app.MapGet("/getcustomerbycode/{code}", async (LearndataContext db, string code) =>
{
    return await db.TblCustomers.FindAsync(code);
});

app.MapPost("/createcustomer", async (LearndataContext db, TblCustomer customer) =>
{
    await db.TblCustomers.AddAsync(customer);
    await db.SaveChangesAsync();
});

app.MapPut("/updatecustomer/{code}", async (LearndataContext db, TblCustomer customer, string code) =>
{
    var existdata = await db.TblCustomers.FindAsync(code);
    if (existdata != null) 
    {
        existdata.Name = customer.Name;
        existdata.Email = customer.Email;
    }
    await db.SaveChangesAsync();
});

app.MapDelete("/removecustomer/{code}", async (LearndataContext db, string code) =>
{
    var existdata = await db.TblCustomers.FindAsync(code);
    if (existdata != null)
    {
        db.TblCustomers.Remove(existdata);
    } 
    await db.SaveChangesAsync();
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseCors();   // For cors with origin * , allow all, no policy name required

app.UseStaticFiles();

//app.UseCors("corspolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
