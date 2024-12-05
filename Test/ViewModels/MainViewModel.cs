using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Test.ApiClasses;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace Test.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const string templateName = "Test.Resources.template.png";
    
    [ObservableProperty]
    private string scoreTemplate = string.Empty;
    
    [ObservableProperty]
    private string gameDayTemplate = string.Empty;

    [ObservableProperty]
    private string fontFamily = "Roboto";

    [ObservableProperty]
    private Bitmap? bitmap;
    
    Dictionary<DateTime, List<(string HomeTeam, string GuestTeam, string Score)>> gameDayResults = new();
    Dictionary<DateTime, List<(string HomeTeam, string GuestTeam, string Time)>> gameDays = new();
    
    Brush brushWhite = Brushes.Solid(Color.WhiteSmoke);
    Brush brushBlack = Brushes.Solid(Color.Black);

    int xGameDay = 50;
    int xGame = 70;
    int padGameDay = 40;
    int padGame = 10;
    private int startY = 200;

    private FontCollection fonts = new();

    public MainViewModel()
    {
        fonts.Add(GetResource("Test.Resources.Roboto-Bold.ttf"));
        fonts.Add(GetResource("Test.Resources.Roboto-Regular.ttf"));
    }

    public async Task LoadAsync()
    {
        gameDayResults.Clear();
        gameDays.Clear();
        HttpClient client = new();
        var response = await client.GetAsync(
            "https://cors-test.jonathan-kopmann.workers.dev/?https://www.basketball-bund.net/rest/club/id/886/actualmatches?justHome=false&rangeDays=6");

        if (!response.IsSuccessStatusCode)
        {
            var t = await response.Content.ReadAsStringAsync();
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        ApiResponse<ClubData>? c = JsonSerializer.Deserialize<ApiResponse<ClubData>>(responseBody);
        if (c is null)
        {
            return;
        }

        foreach (var match in c.Data.Matches)
        {
            DateTime gameTime = DateTime.ParseExact(match.KickoffDate, c.DateFormat, null);

            var homeName = GetTeamName(match.HomeTeam, match.LigaData, c.Data);
            var guestName = GetTeamName(match.GuestTeam, match.LigaData, c.Data);

            if (!string.IsNullOrEmpty(match.Result))
            {
                gameDayResults.TryAdd(gameTime, new List<(string HomeTeam, string GuestTeam, string Score)>());
                gameDayResults[gameTime].Add((homeName.Item1, guestName.Item1,
                    match.Abgesagt == true ? "Abgesagt" : match.Verzicht == true ? "Verzicht" : match.Result));
            }
            else
            {
                gameDays.TryAdd(gameTime, new List<(string HomeTeam, string GuestTeam, string Time)>());
                gameDays[gameTime].Add((homeName.Item1, guestName.Item1, match.KickoffTime));
            }
        }
    }
    
    private static (string, bool) GetTeamName(Team team, LigaData ligaData, ClubData c)
    {
        if (team.ClubId == c.Club.VereinId)
        {
            int n;
            if (ligaData.AkName == "Senioren")
            {
                if (!int.TryParse(team.Teamname[^1..], out n))
                {
                    n = 1;
                }

                return ($"H{n}", true);
            }

            string extra = string.Empty;
            if (int.TryParse(team.Teamname[^1..], out n))
            {
                extra = $" {n}";
            }
            
            return (ligaData.AkName + (ligaData.Geschlecht == "weiblich" ? "w" : string.Empty) + extra, true);
        }

        return (team.Teamname, false);
    }

    [RelayCommand]
    private async Task CreateScore()
    {
        await LoadAsync();
        Font title = fonts.Get(FontFamily).CreateFont(62, FontStyle.Bold);
        Font header = fonts.Get(FontFamily).CreateFont(42, FontStyle.Bold);
        Font text = fonts.Get(FontFamily).CreateFont(32);
        Font boldText = fonts.Get(FontFamily).CreateFont(32, FontStyle.Bold);
        
        using var templateImage = await LoadImageAsync(templateName);
        using var outputImage = new Image<Rgba32>(templateImage.Width, templateImage.Height);
        outputImage.Mutate(x => x.DrawImage(templateImage, 1));

        Size backgroundSize = new((int)(templateImage.Width / 2 * 0.7), (int)(text.Size * 1.5));

        outputImage.Mutate(x => x.DrawCenteredText("Ergebnisse vom Wochenende", title, brushBlack, new PointF(templateImage.Width / 2, 75), true));

        int yPos = startY;
        foreach (var gameDay in gameDayResults)
        {
            yPos += 50;
            string s = gameDay.Key.ToString("dddd dd.MM.yyyy", new CultureInfo("de-DE")).ToUpper();
            var size = TextMeasurer.MeasureAdvance(s, new TextOptions(header));
            outputImage.Mutate(x =>
                x.DrawText(s, header, brushBlack, new PointF(templateImage.Width / 2 - size.Width / 2, yPos)));

            yPos += (int)header.Size + padGameDay;

            foreach ((string HomeTeam, string GuestTeam, string Score) g in gameDay.Value)
            {
                var textSize = TextMeasurer.MeasureAdvance(g.Score.Remove(g.Score.IndexOf(':')),
                    new TextOptions(boldText));
                outputImage.Mutate(x =>
                {
                    // draw background
                    x.Fill(Rgba32.ParseHex("#f05a5a"), new RectangleF(new PointF(xGame, (int)(yPos -
                        (backgroundSize.Height - text.Size) / 2)), backgroundSize).ToRoundedRectangle(10f));
                    x.Fill(Rgba32.ParseHex("#f05a5a"), new RectangleF(new PointF(
                        templateImage.Width - xGame - backgroundSize.Width, (int)(yPos -
                            (backgroundSize.Height - text.Size) / 2)), backgroundSize).ToRoundedRectangle(10f));

                    // draw teams
                    x.DrawCenteredText(g.HomeTeam, text, brushWhite,
                        new PointF(xGame + backgroundSize.Width / 2, yPos));
                    x.DrawCenteredText(g.GuestTeam, text, brushWhite,
                        new PointF(templateImage.Width - xGame - backgroundSize.Width / 2, yPos));

                    // draw result in center
                    x.DrawText(g.Score, boldText, brushBlack,
                        new PointF(templateImage.Width / 2 - textSize.Width, yPos));
                });
                yPos += backgroundSize.Height + padGame;
            }
        }
        
        using (MemoryStream ms = new())
        {
            await outputImage.SaveAsync(ms,PngFormat.Instance);
            ms.Position = 0;
            Bitmap = new Bitmap(ms);
        }
    }

    private static Stream GetResource(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(name);
        if (stream is null)
        {
            throw new Exception("Missing embedded resource");
        }
        
        return stream;
    }

    private static async Task<Image> LoadImageAsync(string name)
    {
        await using var stream = GetResource(name);
        return await Image.LoadAsync(stream);
    }

    [RelayCommand]
    private async Task CreateGameDay()
    {
        await LoadAsync();
        Font title = fonts.Get(FontFamily).CreateFont(62, FontStyle.Bold);
        Font header = fonts.Get(FontFamily).CreateFont(42, FontStyle.Bold);
        Font text = fonts.Get(FontFamily).CreateFont(32);
        Font boldText = fonts.Get(FontFamily).CreateFont(32, FontStyle.Bold);
        
        using var templateImage = await LoadImageAsync(templateName);
        using var outputImage = new Image<Rgba32>(templateImage.Width, templateImage.Height);
        outputImage.Mutate(x => x.DrawImage(templateImage, 1));
        
        outputImage.Mutate(x => x.DrawCenteredText("Spiele am Wochenende", title, brushBlack, new PointF(templateImage.Width / 2, 75), true));
        
        Size backgroundSize = new((int)(templateImage.Width / 2 * 0.7), (int)(text.Size * 1.5));

        int yPos = startY;
        foreach (var gameDay in gameDays)
        {
            yPos += 50;
            string s = gameDay.Key.ToString("dddd dd.MM.yyyy", new CultureInfo("de-DE")).ToUpper();
            var size = TextMeasurer.MeasureAdvance(s, new TextOptions(header));
            outputImage.Mutate(x =>
                x.DrawText(s, header, brushBlack, new PointF(templateImage.Width / 2 - size.Width / 2, yPos)));

            yPos += (int)header.Size + padGameDay;

            foreach ((string HomeTeam, string GuestTeam, string Time) g in gameDay.Value)
            {
                outputImage.Mutate(x =>
                {
                    // draw background
                    x.Fill(Rgba32.ParseHex("#f05a5a"), new RectangleF(new PointF(xGame, (int)(yPos -
                        (backgroundSize.Height - text.Size) / 2)), backgroundSize).ToRoundedRectangle(10f));
                    x.Fill(Rgba32.ParseHex("#f05a5a"), new RectangleF(new PointF(
                        templateImage.Width - xGame - backgroundSize.Width, (int)(yPos -
                            (backgroundSize.Height - text.Size) / 2)), backgroundSize).ToRoundedRectangle(10f));

                    // draw teams
                    x.DrawCenteredText(g.HomeTeam, text, brushWhite,
                        new PointF(xGame + backgroundSize.Width / 2, yPos));
                    x.DrawCenteredText(g.GuestTeam, text, brushWhite,
                        new PointF(templateImage.Width - xGame - backgroundSize.Width / 2, yPos));

                    x.DrawCenteredText(g.Time, boldText, brushBlack, new PointF(templateImage.Width / 2, yPos));
                });
                yPos += backgroundSize.Height + padGame;
            }
        }

        using (MemoryStream ms = new())
        {
            await outputImage.SaveAsync(ms,PngFormat.Instance);
            ms.Position = 0;
            Bitmap = new Bitmap(ms);
        }
    }
}
