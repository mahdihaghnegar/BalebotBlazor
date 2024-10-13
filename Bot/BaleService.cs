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
    private readonly long logGroupChatId = 5684598897;//گروه جلسات
    private readonly long channelId = 5760751221;//کانال 

    private readonly string channel_join_link = "ble.ir/join/4cGWyk4beZ";//آدرس عضویت در کانال
    private readonly string EnterButton = "Enter";
    private static readonly string BackButton = "برگشت";
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
                        //check if chat_id is Channel Member
                        await isChannelMember(u.message.chat.id, u.message.chat.first_name);

                        await HandleMessage(u);
                    }
                    else if (u.callback_query != null)
                    {

                        //check if chat_id is Channel Member
                        await isChannelMember(u.callback_query.message.chat.id, u.callback_query.message.chat.first_name);

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
                                   new sendMessage_ReplyKeyboardMarkup_Parameter
                                   {
                                       chat_id = u.message.contact.user_id,
                                       text = $"با تشکر شماره شما\n {u.message.contact.phone_number} \n می باشد",
                                       reply_markup = null
                                   }
                               );
            /*await client.SendTextAsync(
                                    new TextMessage
                                    {
                                        ChatId = u.message.contact.user_id,
                                        Text = $"با تشکر شماره شما\n {u.message.contact.phone_number} \n می باشد",
                                    }
                           , "null");*/

            // Clear user state
            userStates.Remove(u.message.chat.id);

            return;
        }

        InlineKeyboardMarkup StartInlineKeyboardMarkup = new InlineKeyboardMarkup
        {
            inline_keyboard =
                                                  [ [
                                                new InlineKeyboardButton { text= "ورود به ربات یا بازوی پیشکسوتان شیراز" ,callback_data=EnterButton},

                                           ],[
                                             new InlineKeyboardButton { text= "عضویت در کانال" ,url=channel_join_link},
                                                  new InlineKeyboardButton { text= "وبسایت" ,url="https://farsrms.ir/"},

                                           ]
                                              ]
        };

        InlineKeyboardMarkup BackInlineKeyboardMarkup = new InlineKeyboardMarkup
        {
            inline_keyboard =
                                                 [ [
                                                new InlineKeyboardButton { text= BackButton ,callback_data=BackButton},

                                           ],
                                              ]
        };

        if (u.message.text == BackButton)
        {
            // Clear user state
            userStates.Remove(u.message.chat.id);

            await client.SendTextAsync(
                                   new sendMessage_InlineKeyboardButton_Parameter
                                   {
                                       chat_id = u.message.chat.id,
                                       text = "خوش برگشتید! لطفا روی یک دکمه کلیک کنید!",
                                       reply_markup = StartInlineKeyboardMarkup
                                   }
                               );
        }
        else if (u.message.text == "/start" || u.message.text == "start" || u.message.text == "salam" || u.message.text == "/شروع" || u.message.text == "شروع" || u.message.text == "/سلام" || u.message.text == "سلام")
        {
            await client.SendTextAsync(
                                   new sendMessage_InlineKeyboardButton_Parameter
                                   {
                                       chat_id = u.message.chat.id,
                                       text = "  با سلام و احترام \n" + "به ربات ما خوش آمدید! لطفا روی یک دکمه کلیک کنید!",// $" {u.message.chat.first_name} شما گفتید:\n {u.message.text}",
                                       reply_markup = StartInlineKeyboardMarkup
                                   }
                               );

        }
        else if (userStates.ContainsKey(u.message.chat.id))
        {
            await userStatesSolver(u.message);
        }
        else if (u.message.text != null)
        {
            await client.SendTextAsync(
                                     new sendMessage_InlineKeyboardButton_Parameter
                                     {
                                         chat_id = u.message.chat.id,
                                         text = "لطفا روی کلیدی کلیک نمایید",
                                         reply_markup = BackInlineKeyboardMarkup
                                     }
                                 );
        }
        else if (u.message.text is null) return;
    }
    async Task userStatesSolver(Message msg)
    {
        if (userStates[msg.chat.id] == "ExpectingNationalCode")
        {
            await ExpectingNationalCode(msg);
        }
        else if (userStates[msg.chat.id] == "ExpectingMobile")
        {
            await ExpectingMobile(msg);
        }
    }

    async Task ExpectingNationalCode(Message msg)
    {
        /*if (msg.text == null || !msg.text.IsValidIranianNationalCode())
        {
            await bot.SendTextMessageAsync(
            chatId: msg.Chat.Id,
            text: $"کد ملی نادرست می باشد. لطفا مجددا تلاش نمایید");
            return;
        }

        var nationalCode = msg.Text;
        var response = await GetNameByCodemeliAsync(long.Parse(nationalCode), userPhones[msg.Chat.Id]);

        await bot.SendTextMessageAsync(
            chatId: msg.Chat.Id,
            text: response.Message
        );

        if (response.Success)
        {
            //set userStates
            userStates[msg.Chat.Id] = "NEXT";

        }*/
    }

    async Task ExpectingMobile(Message msg)
    {
        if (msg.contact == null)
        {
            await client.SendTextAsync(
                                    new sendMessage_InlineKeyboardButton_Parameter
                                    {
                                        chat_id = msg.chat.id,
                                        text = $"{msg.chat.first_name} عزیز\n لطفا برای ارزیابی ورود با موبایل خودتان، فقط بر روی دکمه \n ارسال شماره تماس \n در پایین صفحه کلیک نمایید و از تایپ  یا ارسال آن خودداری فرمایید",
                                        //reply_markup = BackInlineKeyboardMarkup
                                    }
                                );

            return;
        }
        /*
                ReplyKeyboardMarkup requestBackKeyboard = new ReplyKeyboardMarkup(

                                new[]   {
                                         new KeyboardButton(BackText),
                                             })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                };

                // Clear user state
                userStates.Remove(msg.Chat.Id);

                var phoneNumber = msg.Contact.PhoneNumber;
                userPhones[msg.Chat.Id] = phoneNumber;

                var response = await CheckByNumberAsync(phoneNumber);
                if (response.Success)
                {
                    await bot.SendTextMessageAsync(
                        chatId: msg.Chat.Id,
                        text: response.Message
                          , replyMarkup: requestBackKeyboard
                    );

                    //set userStates
                    userStates[msg.Chat.Id] = "ExpectingNationalCode";
                }
                else
                {
                    await bot.SendTextMessageAsync(
                       chatId: msg.Chat.Id,
                       text: response.Message
                   );
                }*/
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



        if (u.callback_query.data == EnterButton)
        {    //set userStates
            userStates[u.callback_query.message.chat.id] = "ExpectingMobile";


            await client.SendTextAsync(
               new TextMessage
               {
                   ChatId = u.callback_query.message.chat.id,
                   Text = $"سلام {u.callback_query.message.chat.first_name}! \n لطفا برای احراز هویت شماره خود را با کلیک روی دکمه ارسال تماس در اختیار ما قرار دهید ",
               }, "ارسال شماره تماس"
           );
        }


    }

    async Task<bool> isChannelMember(long chat_id, string first_name)
    {
        var res = await client.GetChatMemberAsync(channelId, chat_id// u.callback_query.message.chat.id
        );
        string isMember = "شما عضو کانال ";
        isMember += res.Ok ? "هستید" : $"نمی باشید\n لطفا ! \n برای عضویت در کانال، دکمه  * عضویت در کانال  * را بزنید";

        if (!res.Ok)
        {
            await client.SendTextAsync(
                 new sendMessage_InlineKeyboardButton_Parameter
                 {
                     chat_id = chat_id,
                     text = $" {first_name} گرامی! \n {isMember}   ",
                     reply_markup = new InlineKeyboardMarkup
                     {
                         inline_keyboard =
                                           [ [
                                             new InlineKeyboardButton { text= "عضویت در کانال" ,url=channel_join_link},
                                           ]
                                       ]
                     }
                 }
            );
        }
        return res.Ok;
    }
}
