using System.Text.Json.Serialization;
public record class ApodResponse
{
    public string Date { get; set; }
    public string Url { get; set; }
    public string Hdurl { get; set; }

    [JsonPropertyName("media_type")]
    public string Media_Type { get; set; }
    public string Title { get; set; }


}