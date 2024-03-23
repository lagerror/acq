using acq.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//增加调用日志过滤器
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(RequestLoggingFilter));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//增加serilog日志
builder.Host.AddSerilLog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
