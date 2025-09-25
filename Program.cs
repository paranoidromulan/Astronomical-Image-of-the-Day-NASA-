using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using HttpClient client = new();


client.DefaultRequestHeaders.Accept.Clear();
Random rand = new Random();
DateTime today = DateTime.Today;
while (true)
{

    Console.WriteLine("Welcome to the NASA image downloader!");
    Console.WriteLine("To download an image press 1, to exit the program press 2");
    string response = Console.ReadLine();

    if (response == "1")
    {
        Console.WriteLine("Would you like to provide a date(1), donwload the image of today(2), download the image of yesterday (3) or choose a random date(4)");
        string howtosearch = Console.ReadLine();

        if (howtosearch == "1")
        {
            Console.WriteLine("Please insert the date to download an image (for example 2023-06-06)");
            string date = Console.ReadLine();

            var apodResponse = await ProcessApodAsync(client, date);

            DownloadIfImageAsync(client, apodResponse);
        }
        else if (howtosearch == "2")
        {
            string todaydate = DateTime.Today.ToString("yyyy-MM-dd");
            var apodToday = await ProcessApodAsync(client, todaydate);

            DownloadIfImageAsync(client, apodToday);

        }
        else if (howtosearch == "3")
        {
            string yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            var apodYesterday = await ProcessApodAsync(client, yesterday);

            DownloadIfImageAsync(client, apodYesterday);
        }
        else if (howtosearch == "4")
        {
            DateTime start = new DateTime(1995, 6, 16);
            DateTime end = today;

            int rangeDays = (end - start).Days;
            DateTime randomDate = start.AddDays(rand.Next(rangeDays + 1));

            Console.WriteLine($"Random date: {randomDate:yyyy-MM-dd}");
            string randomDateString = randomDate.ToString("yyyy-MM-dd");

            var apodRandom = await ProcessApodAsync(client, randomDateString);

            DownloadIfImageAsync(client, apodRandom);

        }
        else
        {
            Console.WriteLine("Please provide a valid option");
        }
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
async Task DownloadIfImageAsync(HttpClient client, ApodResponse apod)
{
    if (apod != null && apod.Media_Type == "image")
        await DownloadImageAsync(client, apod);
    else
        Console.WriteLine("No image found (maybe it's a video)");
}
static async Task<ApodResponse> ProcessApodAsync(HttpClient client, string date)
{
    string url = $"https://api.nasa.gov/planetary/apod?api_key=eHVVF1gTlDryhpxq8qDRh3cddhdPHSWK4cE6faWk&date={date}";
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