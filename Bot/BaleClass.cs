//https://docs.bale.ai/
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
public class BaleMethods
{
    private readonly string token;
    private readonly HttpClient client;
    private readonly string baseUrl;
    private readonly string fileBaseUrl;
    public BaleMethods(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidParameterException("token is invalid");
        }

        this.token = token;
        baseUrl = "https://tapi.bale.ai/" + token + "/";
        fileBaseUrl = "https://tapi.bale.ai/file/" + token + "/";
        client = new HttpClient();
    }


    public async Task<Response<bool>> SetWebhookAsync(string secureWebhookUrl)
    {
        if (string.IsNullOrEmpty(secureWebhookUrl))
        {
            throw new InvalidParameterException("web hook url is invalid, its null or empty");
        }

        string url = baseUrl + "setwebhook";
        var payload = new
        {
            url = secureWebhookUrl
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<bool>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
    }
    public async Task<Response<bool>> DeleteWebHook()
    {
        string requestUri = baseUrl + "deletewebhook";
        try
        {
            var response = client.GetAsync(requestUri).Result;
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<bool>>(responseBody);

            return result;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
    }

    public async Task<Root> GetUpdatesAsync(int? offset = 1)// if zero repeat getting update 
    {
        //var url = $"https://tapi.bale.ai/bot{token}/getUpdates";
        //https://tapi.bale.ai/bot1281856558:FF49UjcoVqjVQzjJkJYdM9w8KmqS5TS8DuG2GiQi/getUpdates
        string url = baseUrl + "getUpdates";
        var parameters = offset.HasValue ? new Dictionary<string, string> { { "offset", offset.Value.ToString() } } : null;
        var response = await client.GetAsync(url + (parameters != null ? $"?offset={parameters["offset"]}" : ""));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonResponse);

            return myDeserializedClass;
        }

        return null;
    }

    public async Task<Response> SendTextAsync(object message)
    {
        string url = baseUrl + "sendmessage";

        /* var payload = new
        {
            message = message
        };
        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");*/

        HttpContent content = ((message is HttpContent) ? ((HttpContent)message) : new StringContent(Serialize(message), Encoding.UTF8, "application/json"));

        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<RootSendMessage>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
        // return await Post<VoidType>(message, url);
    }

    /* public async Task<Response> SendTextAsync(TextMessage message, string RequestContact)
     {
         string url = baseUrl + "sendmessage";

         var payload = new
         {
             chat_id = message.ChatId,
             text = message.Text,
             reply_markup = new
             {
                 keyboard = new[]
                            {
                         new[]
                         {
                             RequestContact=="null"?null:
                             new { text = RequestContact, request_contact = true }
                         }
                     },
                 resize_keyboard = true,
                 one_time_keyboard = true
             }
         };

         var content = new StringContent(
             System.Text.Json.JsonSerializer.Serialize(payload),
             Encoding.UTF8,
             "application/json"
         );


         try
         {
             var response = await client.PostAsync(url, content);
             response.EnsureSuccessStatusCode();

             var responseBody = await response.Content.ReadAsStringAsync();
             var result = JsonConvert.DeserializeObject<Response<RootSendMessage>>(responseBody);

             return result;
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error setting webhook: {ex.Message}");
             return new Response<bool> { Ok = false, Result = false };
         }
     }*/

    public async Task<Response> SendTextAsync(sendMessage_InlineKeyboardButton_Parameter payload)
    {
        string url = baseUrl + "sendmessage";

        /*var payload = new
        {
            chat_id = message.ChatId,
            text = message.Text,
            reply_markup = new
            {
                keyboard = new[]
                           {
                        new[]
                        {
                            RequestContact=="null"?null:
                            new { text = RequestContact, request_contact = true }
                        }
                    },
                resize_keyboard = true,
                one_time_keyboard = true
            }
        };*/

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );


        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<RootSendMessage>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
    }

    public async Task<Response> SendTextAsync(sendMessage_ReplyKeyboardMarkup_Parameter payload)
    {
        string url = baseUrl + "sendmessage";


        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );


        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<RootSendMessage>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
    }

    public async Task<Response> SendInvoiceAsync(Invoice payload)
    {
        string url = baseUrl + "sendInvoice";


        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );


        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<RootSendMessage>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting webhook: {ex.Message}");
            return new Response<bool> { Ok = false, Result = false };
        }
    }


    /* public async Task<Message> SendInvoiceAsync(
         string chatId,
         string title,
         string description,
         string payload,
         string providerToken,
         LabeledPrice[] prices,
         string photoUrl = null,
         int? replyToMessageId = null,
         InlineKeyboardMarkup replyMarkup = null)
     {
         var invoice = new Invoice
         {
             Title = title,
             Description = description,
             Payload = payload,
             ProviderToken = providerToken,
             StartParameter = "create_order",
             Currency = "RUB", // یا هر ارز دیگری که مورد نظر است
             Prices = prices,
         };

         if (!string.IsNullOrEmpty(photoUrl))
         {
             using var stream = new MemoryStream(await new HttpClient().GetByteArrayAsync(photoUrl));
             await _botClient.SendPhotoAsync(chatId, new InputOnlineFile(stream, "invoice.jpg"), invoice);
         }
         else
         {
             await _botClient.SendInvoiceAsync(chatId, invoice, replyMarkup: replyMarkup, replyToMessageId: replyToMessageId);
         }


     }*/



    public async Task<Response> GetChatMemberAsync(long chatId, long userId)
    {
        //myid=497949405
        //bazit=1012519739
        //groupid=5684598897

        //var url = $"https://api.telegram.org/bot{_botToken}/getChatMember?chat_id={chatId}&user_id={userId}";
        string url = baseUrl + $"getChatMember?chat_id={chatId}&user_id={userId}";
        try
        {//https://github.com/TelegramBots/Telegram.Bot/blob/26d1a31a0c19f99d9be9fde385261fcea3f7dafb/src/Telegram.Bot/Requests/Available%20methods/Manage%20Chat/Get%20Chat/GetChatMemberRequest.cs#L4
         //https://tapi.bale.ai/1281856558:FF49UjcoVqjVQzjJkJYdM9w8KmqS5TS8DuG2GiQi/getChatMember?chat_id=5684598897&user_id=1012519739 
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<RootChatMember>>(responseBody);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting chat member: {ex.Message}");
            return new Response { Ok = false, Result = null };
        }
    }



    public static string Serialize(object obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new BaleContractResolver()
        };
        return JsonConvert.SerializeObject(obj, settings);
    }
}

/// <summary>
/// other classes
/// </summary>
public class TextMessage : BaseInput
{
    public string Text { get; set; }

    public ReplyKeyboard ReplyMarkup { get; set; }
}
public class ReplyKeyboard
{
    public IEnumerable<IEnumerable<InlineKeyboardItem>> InlineKeyboard { get; set; }

    public static ReplyKeyboardBuidler Create()
    {
        return new ReplyKeyboardBuidler();
    }
}

public class InlineKeyboardItem
{
    public string Text { get; set; }

    public string Url { get; set; }

    public string CallbackData { get; set; }

    public string SwitchInlineQuery { get; set; }

    public string SwitchInlineQueryCurrentChat { get; set; }

    public InlineKeyboardItem(string text, string callbackData)
    {
        Text = text;
        CallbackData = callbackData;
    }
}

public class ReplyKeyboardBuidler
{
    private List<List<InlineKeyboardItem>> _inlineKeyboard;

    private List<InlineKeyboardItem> _row;

    public ReplyKeyboardBuidler()
    {
        _inlineKeyboard = new List<List<InlineKeyboardItem>>();
        _row = new List<InlineKeyboardItem>();
    }

    public ReplyKeyboardBuidler AddButton(string text, string callbackData)
    {
        _row.Add(new InlineKeyboardItem(text, callbackData));
        return this;
    }

    public ReplyKeyboard Build()
    {
        if (_row.Count > 0)
        {
            _inlineKeyboard.Add(_row);
        }

        return new ReplyKeyboard
        {
            InlineKeyboard = _inlineKeyboard
        };
    }
}

public class BaseInput
{
    public long ChatId { get; set; }

    public long? ReplyToMessageId { get; set; }

    public BaseInput(long chatId, long? replyToMessageId = null)
    {
        ChatId = chatId;
        ReplyToMessageId = replyToMessageId;
    }

    public BaseInput()
    {
    }

    public BaseInput(long chatId)
    {
        ChatId = chatId;
    }
}


internal class BaleContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return GetGapNamingConvention(propertyName);
    }

    public static string GetGapNamingConvention(string propertyName)
    {
        string text = "";
        for (int i = 0; i < propertyName.Length; i++)
        {
            char c = propertyName[i];
            if (c >= 'A' && c <= 'Z')
            {
                if (text != "")
                {
                    text += "_";
                }

                text += (char)(c + 32);
            }
            else
            {
                text += c;
            }
        }

        return text;
    }
}

public class InvalidParameterException : Exception
{
    public InvalidParameterException()
    {
    }

    public InvalidParameterException(string message)
        : base(message)
    {
    }
}
//public record Response(bool Success, string Message);

public class Response<T> : Response
{
    public new T Result { get; set; }
}
public class Response
{
    public bool Ok { get; set; }

    public long Errorcode { get; set; }

    public string Description { get; set; }

    public Result Result { get; set; }

    public static Response CreateInstance<T>(string description)
    {
        if (typeof(T) == typeof(VoidType))
        {
            return new Response
            {
                Description = description
            };
        }

        return new Response<T>
        {
            Description = description
        };
    }
}
public class VoidType
{
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
//https://docs.bale.ai/
public class CallbackQuery
{
    public string id { get; set; }
    public User from { get; set; }
    public Message message { get; set; }
    public string inline_message_id { get; set; }
    public string chat_instance { get; set; }
    public string data { get; set; }
}

public class Chat
{
    public long id { get; set; }
    public string type { get; set; }
    public string username { get; set; }
    public string first_name { get; set; }
    public Photo photo { get; set; }
}

public class User
{
    public long id { get; set; }
    public bool is_bot { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string username { get; set; }
}

/*public class Result_SendMessage
{
    public long message_id { get; set; }
    public User from { get; set; }
    public long date { get; set; }
    public Chat chat { get; set; }
    public string text { get; set; }

}*/
public class Message
{
    public long message_id { get; set; }
    public User from { get; set; }
    public long date { get; set; }
    public Chat chat { get; set; }
    public User forward_from { get; set; }
    public Chat forward_from_chat { get; set; }
    public long forward_from_message_id { get; set; }
    public long forward_date { get; set; }
    public Message reply_to_message { get; set; }
    public long edite_date { get; set; }
    public string text { get; set; }
    public Contact contact { get; set; }
    public Location location { get; set; }
    public Invoice invoice { get; set; }
    public InlineKeyboardMarkup reply_markup { get; set; }
}

public class Photo
{
    public string small_file_id { get; set; }
    public string small_file_unique_id { get; set; }
    public string big_file_id { get; set; }
    public string big_file_unique_id { get; set; }
}

public class Result
{
    public long update_id { get; set; }
    public Message message { get; set; }
    public Message edited_message { get; set; }
    public CallbackQuery callback_query { get; set; }

}

public class Root
{
    public bool ok { get; set; }
    public List<Result> result { get; set; }
}

/// <summary>
/// SendMessage
/// </summary>
public class RootSendMessage
{
    public bool ok { get; set; }
    // public List<Result_SendMessage> result { get; set; }
    public List<Message> result { get; set; }
}
public class Contact
{
    public string phone_number { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public long user_id { get; set; }
}

public class Location
{
    public float longitude { get; set; }
    public float latitude { get; set; }
}



public class RootChatMember
{
    public bool ok { get; set; }
    public ResultChatMember result { get; set; }
}

public class ResultChatMember
{
    public string status { get; set; }
    public User user { get; set; }
    //public bool is_anonymous { get; set; }
    public bool is_member { get; set; }
    public bool can_send_messages { get; set; }
    public bool can_send_media_messages { get; set; }
    public bool can_send_audios { get; set; }
    public bool can_send_documents { get; set; }
    public bool can_send_photos { get; set; }
    public bool can_send_videos { get; set; }
    public bool can_send_video_notes { get; set; }
    public bool can_send_voice_notes { get; set; }
    public bool can_send_polls { get; set; }
    public bool can_send_other_messages { get; set; }
    public bool can_add_web_page_previews { get; set; }
    public bool can_change_info { get; set; }
    public bool can_invite_users { get; set; }
    public bool can_pin_messages { get; set; }
    public bool can_manage_topics { get; set; }
    public long until_date { get; set; }
}


public class LabeledPrice
{//https://core.telegram.org/bots/api#labeledprice
    public string label { get; set; }
    public long amount { get; set; }
}
public class Invoice
{
    [Required]
    public long chat_id { get; set; }

    [Required]
    public string title { get; set; }

    [Required]
    public string description { get; set; }

    [Required]
    public string payload { get; set; }

    [Required]
    public string provider_token { get; set; }

    [Required]
    public LabeledPrice[] prices { get; set; }
    public string photo_url { get; set; }
    public long reply_to_message_id { get; set; }
    public ReplyKeyboard reply_markup { get; set; }
}


public class sendMessage_InlineKeyboardButton_Parameter
{
    [Required]
    public long chat_id { get; set; }
    [Required]
    public string text { get; set; }
    public long reply_to_message_id { get; set; }
    public InlineKeyboardMarkup reply_markup { get; set; }

}

public class InlineKeyboardMarkup
{
    // https://core.telegram.org/bots/api#inlinekeyboardmarkup
    public InlineKeyboardButton[][] inline_keyboard { get; set; }
}
public class InlineKeyboardButton
{
    public string text { get; set; }
    public string url { get; set; }
    public string callback_data { get; set; }
}
public class sendMessage_ReplyKeyboardMarkup_Parameter
{
    [Required]
    public long chat_id { get; set; }
    [Required]
    public string text { get; set; }
    public long reply_to_message_id { get; set; }
    public ReplyKeyboardMarkup reply_markup { get; set; }

}
public class ReplyKeyboardMarkup
{
    public KeyboardButton[][] keyboard { get; set; }
}

public class KeyboardButton
{
    public string text { get; set; }
    public bool request_contact { get; set; }
    public bool request_location { get; set; }

}