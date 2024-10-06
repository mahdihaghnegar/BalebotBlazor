
namespace BalebotBlazor.Bot;

public record Response(bool Success, string Message);

public class BaleService : BackgroundService
{
    private readonly string _apiToken;
    private readonly string _HOST;
    private static Dictionary<long, string> userStates = new Dictionary<long, string>();
    private static Dictionary<long, string> userPhones = new Dictionary<long, string>();

    private readonly IServiceScopeFactory ScopeFactory;
    private readonly IConfiguration Configuration;
    public BaleService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        this.ScopeFactory = scopeFactory;
        this.Configuration = configuration;
        _apiToken = Configuration["BaleBot:ApiToken"];
        _HOST = Configuration["BaleBot:HOST"];

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await ExecuteBotAsync(stoppingToken);
    }
    async Task ExecuteBotAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Host: {_HOST} - Token: {_apiToken} ");
    }
}