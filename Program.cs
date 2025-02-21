using Buratino.DI;
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
using Bronya.Entities;
using Bronya.Services;
using Bronya.Jobs.Structures;
using Quartz;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("credentials.json",
            optional: false,
            reloadOnChange: true);


        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        builder.Services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddSingleton(typeof(IRepository<>), typeof(PGRepository<>));

        builder.Services.AddTransient(typeof(IDomainService<>), typeof(DomainService<>));

        builder.Services.AddTransient(typeof(IDomainService<Account>), typeof(PersistentDomainService<Account>));
        builder.Services.AddTransient(typeof(IDomainService<WorkSchedule>), typeof(PersistentDomainService<WorkSchedule>));
        builder.Services.AddTransient(typeof(IDomainService<Table>), typeof(PersistentDomainService<Table>));
        builder.Services.AddTransient(typeof(IDomainService<Book>), typeof(PersistentDomainService<Book>));

        builder.Services.AddSingleton(typeof(LogToFileService), new LogToFileService());
        
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

        builder.Services.AddSingleton(typeof(IQuartzProvider), typeof(QuartzProvider));

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

        AppDomain.CurrentDomain.ProcessExit += (s, e) => OnStop();

        app.Lifetime.ApplicationStarted.Register(OnStarted);
        app.Run();
    }

    private static void OnStop()
    {
        Container.Get<LogToFileService>().Dispose();
    }

    public static void OnStarted()
    {
        new AuthorizeService(Container.Get<LogToFileService>(), new TGAPI(Container.Get<LogToFileService>(), "7434892034:AAHZlmmmNZlPdsPU1hye-JcKqYlzayb-VRI"));

        JobRegistrator.RegisterJobs();

        var accountDS = Container.GetDomainService<Account>(null);
        var account = accountDS.GetAll(x => x.Name == "Root").SingleOrDefault();
        if (account == default)
        {
            accountDS.Repository.Insert(new()
            {
                Id = AccountService.RootAccount.Id,
                Name = "Root",
            });
            accountDS.Repository.Insert(new()
            {
                Id = AccountService.MainTester.Id,
                TGTag = "morsw",
                TGChatId = "1029089379",
                Name = "Марсель",
            });
        }

        var roles = Container.GetDomainService<Role>(null);
        if (!roles.GetAll().Any())
        {
            roles.Save(new Role() { Name = "Administrator" });
            roles.Save(new Role() { Name = "Hostes" });
        }

        var workSchedules = Container.GetDomainService<WorkSchedule>(null);
        if (!workSchedules.GetAll().Any())
        {
            workSchedules.Save(new WorkSchedule());
        }
    }
}