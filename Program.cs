using BalebotBlazor.Components;
using BalebotBlazor.Bot;
using Serilog;
try
{

    var builder = WebApplication.CreateBuilder(args);

    var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/errorlog.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
    Log.Information("Starting up the app");

    builder.Host.UseSerilog(logger);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddHostedService<BaleService>();

    var app = builder.Build();


    //app.UseWindowsService();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    //Add support to logging request with SERILOG
    app.UseSerilogRequestLogging();

    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}

catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

