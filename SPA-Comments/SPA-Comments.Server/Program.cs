using Dal.Data;
using Dto.GraphQL.Comment;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Web;
using Service.Extentions;
using Service.GraphQL.Comment;
using Service.Validators.Comment;
using SPA_Comments.Server;
using System.Reflection;
 
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info($"Start {Assembly.GetEntryAssembly()?.GetName().Version}");
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()  // Разрешить запросы с любого домена
                  .AllowAnyMethod()  // Разрешить все HTTP методы (GET, POST и т.д.)
                  .AllowAnyHeader(); // Разрешить все заголовки
        });
    });
    builder.Services.AddDbContext<ChatDbContext>(options =>
    {
        var connString = builder.Configuration.GetConnectionString("DefaultConnection")
                         ?? "Server=mssql;Database=CommentsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";
        options.UseSqlServer(connString, x => x.MigrationsAssembly("Dal"));
    }, ServiceLifetime.Scoped);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddBlServices();
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    }, typeof(MappingProfile));
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateCommentDtoValidator>();

    // HotChocolate
    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddType<CommentDtoType>()
        .AddType<GetCommentListResponseType>()
        .AddFiltering()
        .AddSorting();
    builder.Host.UseNLog();
    var app = builder.Build();
    app.UseCors("AllowAll");
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Comments API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGraphQL("/graphql");

    app.MapFallbackToFile("index.html");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        try
        {
            db.Database.Migrate();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
        }
    }

    app.Run();

}
catch (Exception ex)
{
    logger.Error(ex);
    throw;
}