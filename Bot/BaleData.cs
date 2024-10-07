
using Newtonsoft.Json;

public class BaleMethods
{
    private static readonly HttpClient client = new HttpClient();
    public static async Task<Root> GetUpdatesAsync(string token, int? offset = 1)// if zero repeat getting update 
    {
        var url = $"https://tapi.bale.ai/bot{token}/getUpdates";
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
}
public record Response(bool Success, string Message);

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

