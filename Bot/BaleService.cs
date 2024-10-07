
using BaleLib;
using BaleLib.Models.Parameters;
using Newtonsoft.Json.Linq;


namespace BalebotBlazor.Bot;

public record Response(bool Success, string Message);

public class BaleService : BackgroundService
{
    private readonly string _apiToken;
    private readonly string _HOST;

    private string apiUrl;
    private static readonly HttpClient client = new HttpClient();
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
        apiUrl = "https://tapi.bale.ai/bot" + _apiToken + "/";

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await ExecuteBotAsync(stoppingToken);
    }

    private async Task<dynamic> GetUpdatesAsync(int? offset = null)
    {
        //https://tapi.bale.ai/bot1281856558:FF49UjcoVqjVQzjJkJYdM9w8KmqS5TS8DuG2GiQi/getUpdates

        /*var response = await client.GetStringAsync(apiUrl + "getUpdates");
        
        return JsonConvert.DeserializeObject(response);*/
        var url = $"https://tapi.bale.ai/bot{_apiToken}/getUpdates";
        var parameters = offset.HasValue ? new Dictionary<string, string> { { "offset", offset.Value.ToString() } } : null;
        var response = await client.GetAsync(url + (parameters != null ? $"?offset={parameters["offset"]}" : ""));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(jsonResponse)["result"];
            return result;//.ToList();
        }

        return new List<JToken>();
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
                var Updates = await GetUpdatesAsync();// await client.GetUpdatesAsync();
                if (Updates == null || Updates.Count == 0)
                {
                    await Task.Delay(5000);
                    continue;
                }

                foreach (var u in Updates)
                {
                    if (u.message!.text != null)
                    {
                        ReplyKeyboardBuidler replyKeyboardBuidler = new ReplyKeyboardBuidler();
                        replyKeyboardBuidler.AddButton("شروع");
                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = u.message.chat.id,
                                Text = $"سلام {u.Message.chat.firstName} شما گفتید:\n {u.message.text}",
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