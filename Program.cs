using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using HttpClient client = new();

client.DefaultRequestHeaders.Accept.Clear();
while (true)
{

    Console.WriteLine("Welcome to the NASA image downloader!");
    Console.WriteLine("To download an image press 1, to exit the program press 2");
    string response = Console.ReadLine();

    if (response == "1")
    {
        Console.WriteLine("Please insert the date to download an image (for example 2023-06-06)");
        string date = Console.ReadLine();
        var apodResponse = await ProcessApodAsync(client, date);

        await DownloadImageAsync(client, apodResponse);


    }
    else if (response == "2")
    {
        break;
    }
    else
    {
        Console.WriteLine("Please provide a valid option!");
    }

}
static async Task<ApodResponse> ProcessApodAsync(HttpClient client, string date)
{
    string url = $"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={date}";
    try
    {
        var apod = await client.GetFromJsonAsync<ApodResponse>(url);
        return apod;
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("429"))
    {
        Console.WriteLine("Rate limit exceeded. Please try again later or use your own API key.");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching APOD: {ex.Message}");
        return null;
    }
    
    
}
async Task DownloadImageAsync(HttpClient client, ApodResponse apod)
{
    
    string imageUrl = apod.Hdurl ?? apod.Url;
    if (string.IsNullOrEmpty(imageUrl))
    {
        return;
    }

    var date = DateTime.Parse(apod.Date);

    string folder = Path.Combine(date.Year.ToString(), date.Month.ToString("D2"));
    Directory.CreateDirectory(folder);

    string filePath = Path.Combine(folder, $"{date:yyyy-MM-dd}.jpg");

    using var imageStream = await client.GetStreamAsync(imageUrl);
    using var fileStream = File.Create(filePath);
    await imageStream.CopyToAsync(fileStream);
}