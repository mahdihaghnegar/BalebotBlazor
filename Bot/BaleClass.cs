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

        /*var payload = new
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

    public static string Serialize(object obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new BaleContractResolver()
        };
        return JsonConvert.SerializeObject(obj, settings);
    }
}

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

    public InlineKeyboardItem(string text)
    {
        Text = text;
        CallbackData = text;
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

    public ReplyKeyboardBuidler AddButton(string text)
    {
        _row.Add(new InlineKeyboardItem(text));
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

    public int Errorcode { get; set; }

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


public class CallbackQuery
{
    public string id { get; set; }
    public From from { get; set; }
    public Message message { get; set; }
    public string inline_message_id { get; set; }
    public string chat_instance { get; set; }
    public string data { get; set; }
}

public class Chat
{
    public int id { get; set; }
    public string type { get; set; }
    public string username { get; set; }
    public string first_name { get; set; }
    public Photo photo { get; set; }
}

public class From
{
    public int id { get; set; }
    public bool is_bot { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string username { get; set; }
}

public class Message
{
    public int message_id { get; set; }
    public From from { get; set; }
    public int date { get; set; }
    public Chat chat { get; set; }
    public string text { get; set; }
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
    public int update_id { get; set; }
    public Message message { get; set; }
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
    public List<ResultSendMessage> result { get; set; }
}
public class ResultSendMessage
{
    public int message_id { get; set; }
    public From from { get; set; }
    public int date { get; set; }
    public Chat chat { get; set; }
    public string text { get; set; }
}