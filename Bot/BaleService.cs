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
    long logGroupChatId = 5684598897;//گروه جلسات
    long channelId = 5760751221;//کانال 

    string channel_join_link = "ble.ir/join/4cGWyk4beZ";//آدرس عضویت در کانال
    string EnterButton = "Enter";
    BaleMethods client;
    public BaleService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        ScopeFactory = scopeFactory;
        Configuration = configuration;
        _apiToken = Configuration["BaleBot:ApiToken"];
        _HOST = Configuration["BaleBot:HOST"];
        url = $"https://tapi.bale.ai/bot{_apiToken}/";
        client = new BaleMethods(_apiToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteBotAsync(stoppingToken);
    }
    async Task ExecuteBotAsync(CancellationToken stoppingToken)
    {
        bool start = await StartBot();
        if (!start) return;

        while (start)
        {
            try
            {
                var Updates = await client.GetUpdatesAsync();
                if (Updates == null || !Updates.ok || Updates.result.Count == 0)
                {
                    await Task.Delay(2000);
                    continue;
                }

                foreach (var u in Updates.result)
                {
                    if (u.message != null)
                    {
                        await HandleMessage(u);
                    }
                    else if (u.callback_query != null)
                    {
                        await HandleCallbackQuery(u);
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

    async Task<bool> StartBot()
    {
        Console.WriteLine($"Host: {_HOST}\n Token: {_apiToken}");

        var response = await client.DeleteWebHook();
        if (response.Ok && response.Result)
        {
            Console.WriteLine("Webhook deleted successfully!");
        }
        else
        {
            Console.WriteLine("Failed to delete webhook.");
            return false;
        }

        response = await client.SetWebhookAsync(_HOST);
        if (response.Ok && response.Result)
        {
            Console.WriteLine("Webhook set successfully!");
        }
        else
        {
            Console.WriteLine("Failed to set webhook.");
            return false;
        }

        return true;
    }
    async Task HandleMessage(Result u)
    {

        await client.SendTextAsync(
                                    new sendMessage_InlineKeyboardButton_Parameter
                                    {
                                        chat_id = logGroupChatId,//گروه جلسات
                                        text = $"Id: {u.message.chat.id} با نام {u.message.chat.first_name} به ربات @bazshirazbot گفت:\n {(u.message.contact != null ? u.message.contact.phone_number : u.message.text)}",

                                    }

                                );
        if (u.message.contact != null)
        {
            await client.SendTextAsync(
                                    new TextMessage
                                    {
                                        ChatId = u.message.contact.user_id,
                                        Text = $"با تشکر شماره شما\n {u.message.contact.phone_number} \n می باشد",
                                    }
                           , "null");

            return;
        }

        if (u.message.text == "/start")
        {

            await client.SendTextAsync(
                                   new sendMessage_InlineKeyboardButton_Parameter
                                   {
                                       chat_id = u.message.chat.id,
                                       text = $" {u.message.chat.first_name} شما گفتید:\n {u.message.text}",
                                       reply_markup = new InlineKeyboardMarkup
                                       {
                                           inline_keyboard =
                                           [ [
                                                new InlineKeyboardButton { text= "ورود به ربات یا بازوی پیشکسوتان شیراز" ,callback_data=EnterButton},

                                           ],[
                                             new InlineKeyboardButton { text= "عضویت در کانال" ,url=channel_join_link},
                                                  new InlineKeyboardButton { text= "وبسایت" ,url="https://farsrms.ir/"}

                                           ]
                                       ]
                                       }

                                   }

                               );

            /* ReplyKeyboardBuidler replyKeyboardBuidler = new ReplyKeyboardBuidler();
             replyKeyboardBuidler.AddButton("ورود به ربات/بازو پیشکسوتان شیراز", EnterButton);
             await client.SendTextAsync(
                 new TextMessage
                 {
                     ChatId = u.message.chat.id,
                     Text = $" {u.message.chat.first_name} شما گفتید:\n {u.message.text}",
                     ReplyMarkup = replyKeyboardBuidler.Build(),
                 }
             );*/
        }
    }
    async Task HandleCallbackQuery(Result u)
    {

        await client.SendTextAsync(
            new TextMessage
            {
                ChatId = logGroupChatId,//گروه جلسات
                Text = $"Id: {u.callback_query.message.chat.id} با نام {u.callback_query.message.chat.first_name} در ربات @bazshirazbot روی دکمه: {u.callback_query.data} زد",
            }
        );
        //*******************

        var res = await client.GetChatMemberAsync(channelId, u.callback_query.message.chat.id);
        string isMember = "شما عضو کانال ";
        isMember += res.Ok ? "هستید" : $"نمی باشید\n برای عضویت در کانال روی این لینک {channel_join_link} کلیک کنید";


        if (!res.Ok)
        {
            await client.SendTextAsync(
                new TextMessage
                {
                    ChatId = u.callback_query.message.chat.id,
                    Text = $"{isMember} \n {u.callback_query.message.chat.first_name} ",
                }
            );
            return;
        }

        if (u.callback_query.data == EnterButton)
        {
            await client.SendTextAsync(
               new TextMessage
               {
                   ChatId = u.callback_query.message.chat.id,
                   Text = $"سلام {u.callback_query.message.chat.first_name}! \n لطفا برای احراز هویت شماره خود را با کلیک روی دکمه ارسال تماس در اختیار ما قرار دهید ",
               }, "ارسال شماره تماس"
           );
        }


    }
}
