using Buratino.DI;
using Buratino.Entities;
using Buratino.API;
using Buratino.Models.DomainService.DomainStructure;
using Buratino.Models.DomainService;
using Buratino.Repositories.Implementations.Postgres;
using Buratino.Repositories.Implementations;
using Buratino.Repositories.RepositoryStructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.FileProviders;
using vkteams.Services;
using Bronya.Entities;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddSingleton(typeof(IRepository<>), typeof(PGRepository<>));

        builder.Services.AddSingleton(typeof(IDomainService<>), typeof(DefaultDomainService<>));

        //for PG
        builder.Services.AddSingleton(typeof(IPGSessionFactory), typeof(PGSessionFactory));

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = PathString.FromUriComponent("/Security/LoginPage");
                options.ReturnUrlParameter = "url";
            });
        builder.Services.AddControllers(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        });

        var app = builder.Build();

        Container.Configure(app.Services);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(x =>
        {
            x.MapDefaultControllerRoute();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                           Path.Combine(builder.Environment.ContentRootPath, "Images")),
            RequestPath = "/Images"
        });

        //new MigrateData();

        app.Lifetime.ApplicationStarted.Register(OnStarted);
        app.Run();
    }

    public static void OnStarted()
    {
        LogService logService = new LogService();
        new BronyaService(logService, new TGAPI(logService, "7434892034:AAHZlmmmNZlPdsPU1hye-JcKqYlzayb-VRI"));

        Container.GetDomainService<WorkSchedule>().Save(new WorkSchedule()
        {
            StartDate = DateTime.Now.AddDays(-5),
        });

        var accounts = Container.GetDomainService<Account>();
        if (!accounts.GetAll().Any(x => x.Name == "Root"))
        {
            Account entity = new Account()
            {
                Name = "Root",
            };
            accounts.Save(entity);
        }

        //Переопределение сравнения сущностей в управляемом коде
        var exist = accounts.GetAll().First();
        var newacc = new Account()
        {
            Id = exist.Id
        };
        if (exist == newacc)
        {

        }

        Container.GetDomainService<RoleAccountLink>().CascadeSave(new RoleAccountLink()
        {
            Account = accounts.GetAll().First(),
            Role = new Role()
            {
                Name = "Administrator"
            }
        });

        var list = Container.GetDomainService<RoleAccountLink>().GetAll();
    }
}