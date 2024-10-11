/*using BaleLib;
using BaleLib.Models.Parameters;*/

namespace BalebotBlazor.Bot;
public class BaleService : BackgroundService
{
    private readonly string _apiToken;
    private readonly string _HOST;
    private readonly string url;

    private static Dictionary<long, string> userStates = new Dictionary<long, string>();
    private static Dictionary<long, string> userPhones = new Dictionary<long, string>();

    private readonly IServiceScopeFactory ScopeFactory;
    private readonly IConfiguration Configuration;
    public BaleService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        ScopeFactory = scopeFactory;
        Configuration = configuration;
        _apiToken = Configuration["BaleBot:ApiToken"];
        _HOST = Configuration["BaleBot:HOST"];
        url = $"https://tapi.bale.ai/bot{_apiToken}/";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteBotAsync(stoppingToken);
    }
    async Task ExecuteBotAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Host: {_HOST}\n Token: {_apiToken}");
        BaleMethods client = new BaleMethods(_apiToken);
        //BaleClient oldclient = new BaleClient(_apiToken);
        //oldclient.DeleteWebHook();

        var response = await client.DeleteWebHook();
        if (response.Ok && response.Result)
        {
            Console.WriteLine("Webhook deleted successfully!");
        }
        else
        {
            Console.WriteLine("Failed to delete webhook.");
            return;
        }

        response = await client.SetWebhookAsync(_HOST);
        if (response.Ok && response.Result)
        {
            Console.WriteLine("Webhook set successfully!");
        }
        else
        {
            Console.WriteLine("Failed to set webhook.");
            return;
        }

        var groupChatId = 5684598897;//گروه جلسات
        var channelId = 5760751221;//کانال 
        while (true)
        {
            try
            {
                var Updates = await client.GetUpdatesAsync();// await oldclient.GetUpdatesAsync();
                if (Updates == null || !Updates.ok || Updates.result.Count == 0)
                {
                    await Task.Delay(2000);
                    continue;
                }

                foreach (var u in Updates.result)
                {
                    if (u.message != null)
                    {
                        ReplyKeyboardBuidler replyKeyboardBuidler = new ReplyKeyboardBuidler();
                        replyKeyboardBuidler.AddButton("ورود به ربات/بازو پیشکسوتان شیراز", "enter");
                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = u.message.chat.id,
                                Text = $" {u.message.chat.first_name} شما گفتید:\n {u.message.text}",
                                ReplyMarkup = replyKeyboardBuidler.Build(),
                            }
                        );

                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = groupChatId,//گروه جلسات
                                Text = $"Id: {u.message.chat.id} با نام {u.message.chat.first_name} به ربات @bazshirazbot گفت:\n {u.message.text}",
                            }
                        );
                    }
                    else if (u.callback_query != null)
                    {

                        /*
                        ble.ir/join/4cGWyk4beZ
                        5760751221
                        */
                        var res = await client.GetChatMemberAsync(channelId, u.callback_query.message.chat.id);
                        string isMember = "شما عضو کانال ";
                        isMember += res.Ok ? "هستید" : "نمی باشید\n برای عضویت در کانال روی این لینک ble.ir/join/4cGWyk4beZ کلیک کنید";

                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = u.callback_query.message.chat.id,
                                Text = $"{isMember} \n {u.callback_query.message.chat.first_name} \n شما روی کلید : {u.callback_query.data} زدید ",
                            }
                        );

                        await client.SendTextAsync(
                            new TextMessage
                            {
                                ChatId = groupChatId,//گروه جلسات
                                Text = $"Id: {u.callback_query.message.chat.id} با نام {u.message.chat.first_name} به ربات @bazshirazbot گفت:\n {u.message.text}",
                            }
                        );
                    }
                }

                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

}
