
using BaleLib;
using BaleLib.Models.Parameters;

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
        Console.WriteLine($"Host: {_HOST}\n Token: {_apiToken}");

        BaleClient client = new BaleClient(_apiToken);
        client.DeleteWebHook();
        var res = await client.SetWebhookAsync(_HOST);
        if (!res.Ok) return;

        while (true)
        {
            try
            {
                var Updates = await client.GetUpdatesAsync();
                if (Updates.Result == null || Updates.Result.Count == 0)
                {
                    await Task.Delay(5000);
                    continue;
                }

                foreach (var u in Updates.Result)
                {
                    if (u.Message != null)
                    {
                        ReplyKeyboardBuidler replyKeyboardBuidler = new ReplyKeyboardBuidler();
                        replyKeyboardBuidler.AddButton("شروع");
                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = u.Message.Chat.Id,
                                Text = $"سلام {u.Message.Chat.FirstName} شما گفتید:\n {u.Message.Text}",
                                ReplyMarkup = replyKeyboardBuidler.Build(),
                            }

                        );
                    }
                }
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex);
            }
        }
    }
}