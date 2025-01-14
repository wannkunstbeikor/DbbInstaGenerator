using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbbInstaGenerator.ApiClasses;
using DbbInstaGenerator.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace DbbInstaGenerator.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const string templateName = "DbbInstaGenerator.Resources.template.png";
    
    [ObservableProperty]
    private string scoreTemplate = string.Empty;
    
    [ObservableProperty]
    private string gameDayTemplate = string.Empty;

    [ObservableProperty]
    private string fontFamily = "Roboto";

    [ObservableProperty]
    private Bitmap? bitmap;

    public MemoryStream? Stream
    {
        get => m_stream;
        set
        {
            if (m_stream != value)
            {
                m_stream?.Dispose();
                m_stream = value;
                if (m_stream is not null)
                {
                    Bitmap = new Bitmap(m_stream);
                    m_stream.Position = 0;
                }
            }
        }
    }

    private MemoryStream? m_stream;
    
    Dictionary<DateTime, List<(string EkTeam, string OpTeam, bool IsHomeGame, string Score)>> gameDayResults = new();
    Dictionary<DateTime, List<(string EkTeam, string OpTeam, bool IsHomeGame, string Time)>> gameDays = new();
    
    Brush brushWhite = Brushes.Solid(Color.WhiteSmoke);
    Brush brushBlack = Brushes.Solid(Color.Black);

    int xGame = 40;
    int padGameDay = 40;
    int padGame = 10;
    private int startY = 200;
    private float padRect = 10;
    private float padBetween = 20;

    private FontCollection fonts = new();
    private IShareService m_shareService;
    
    Font title;
    Font header;
    Font text;
    Font boldText;

    public MainViewModel(IShareService inShareService)
    {
        m_shareService = inShareService;
        fonts.Add(GetResource("DbbInstaGenerator.Resources.Roboto-Bold.ttf"));
        fonts.Add(GetResource("DbbInstaGenerator.Resources.Roboto-Regular.ttf"));
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
            
            var ekTeam = homeName.Item2 ? homeName.Item1 : guestName.Item1;
            var opTeam = homeName.Item2 ? guestName.Item1 : homeName.Item1;

            if (!string.IsNullOrEmpty(match.Result))
            {
                gameDayResults.TryAdd(gameTime, new());
                gameDayResults[gameTime].Add((ekTeam, opTeam, homeName.Item2,
                    match.Abgesagt == true ? "Abgesagt" : match.Verzicht == true ? "Verzicht" : match.Result));
            }
            else
            {
                gameDays.TryAdd(gameTime, new());
                gameDays[gameTime].Add((ekTeam, opTeam, homeName.Item2, match.KickoffTime));
            }
        }
        
        
        title = fonts.Get(FontFamily).CreateFont(62, FontStyle.Bold);
        header = fonts.Get(FontFamily).CreateFont(42, FontStyle.Bold);
        text = fonts.Get(FontFamily).CreateFont(32);
        boldText = fonts.Get(FontFamily).CreateFont(32, FontStyle.Bold);
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
        
        using var templateImage = await LoadImageAsync(templateName);
        using var outputImage = new Image<Rgba32>(templateImage.Width, templateImage.Height);
        outputImage.Mutate(x => x.Fill(Color.White));
        //outputImage.Mutate(x => x.DrawImage(templateImage, 1));

        outputImage.Mutate(x => x.DrawCenteredText("Ergebnisse vom Wochenende", title, brushBlack, new PointF(outputImage.Width / 2, 75), true));
        
        float yPos = startY;

        FontRectangle ekRect = gameDayResults.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.EkTeam, new TextOptions(boldText)))).Pad(padRect);
        FontRectangle opRect = gameDayResults.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.OpTeam, new TextOptions(text)))).Pad(padRect);

        FontRectangle atRect = TextMeasurer.MeasureAdvance("vs", new TextOptions(boldText)).Pad(padRect);
        
        FontRectangle timeRect = gameDayResults.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.Score, new TextOptions(boldText)))).Pad(padRect);
        
        float offsetX = (outputImage.Width - (ekRect.Width + atRect.Width + opRect.Width + timeRect.Width +
                                              3 * padBetween)) / 2;

        foreach (var gameDay in gameDayResults)
        {
            yPos += 50;
            string s = gameDay.Key.ToString("dddd dd.MM.yyyy", new CultureInfo("de-DE")).ToUpper();
            var size = TextMeasurer.MeasureAdvance(s, new TextOptions(header));
            outputImage.Mutate(x =>
                x.DrawText(s, header, brushBlack, new PointF(outputImage.Width / 2 - size.Width / 2, yPos)));

            yPos += (int)header.Size + padGameDay;

            foreach ((string EkTeam, string OpTeam, bool IsHomeGame, string Score) g in gameDay.Value)
            {
                outputImage.Mutate(x =>
                {
                    // draw background
                    x.DrawRoundedRectangleWithCenteredText(
                        ekRect.Offset(offsetX + timeRect.Width + padBetween, yPos - (ekRect.Height - boldText.Size) / 2), Rgba32.ParseHex("#f05a5a"),
                        g.EkTeam, boldText, brushWhite);

                    x.DrawRoundedRectangleWithCenteredText(
                        atRect.Offset(offsetX + timeRect.Width + ekRect.Width + 2 * padBetween, yPos - (atRect.Height - boldText.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), g.IsHomeGame ? "vs" : "@", boldText, brushBlack);

                    x.DrawRoundedRectangleWithCenteredText(
                        opRect.Offset(offsetX + timeRect.Width + ekRect.Width + 3 * padBetween + atRect.Width, yPos - (opRect.Height - text.Size) / 2),
                        Rgba32.ParseHex("#f05a5a"), g.OpTeam, text, brushWhite);

                    x.DrawRoundedRectangleWithCenteredText(
                        timeRect.Offset(offsetX, yPos - (timeRect.Height - boldText.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), g.Score, boldText, brushBlack);
                });
                yPos += ekRect.Height + padGame;
            }
        }

        MemoryStream ms = new();
        await outputImage.SaveAsync(ms, JpegFormat.Instance);
        ms.Position = 0;
        Stream = ms;
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
        
        using var templateImage = await LoadImageAsync(templateName);
        using var outputImage = new Image<Rgba32>(templateImage.Width, templateImage.Height);
        outputImage.Mutate(x => x.Fill(Color.White));
        //outputImage.Mutate(x => x.DrawImage(templateImage, 1));
        
        outputImage.Mutate(x => x.DrawCenteredText("Spiele am Wochenende", title, brushBlack, new PointF(outputImage.Width / 2, 75), true));

        float yPos = startY;

        FontRectangle ekRect = gameDays.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.EkTeam, new TextOptions(boldText)))).Pad(padRect);
        FontRectangle opRect = gameDays.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.OpTeam, new TextOptions(text)))).Pad(padRect);

        FontRectangle atRect = TextMeasurer.MeasureAdvance("vs", new TextOptions(boldText)).Pad(padRect);
        
        FontRectangle timeRect = gameDays.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.Time, new TextOptions(boldText)))).Pad(padRect);
        
        float offsetX = (outputImage.Width - (ekRect.Width + atRect.Width + opRect.Width + timeRect.Width +
                                              3 * padBetween)) / 2;

        foreach (var gameDay in gameDays)
        {
            yPos += 50;
            string s = gameDay.Key.ToString("dddd dd.MM.yyyy", new CultureInfo("de-DE")).ToUpper();
            var size = TextMeasurer.MeasureAdvance(s, new TextOptions(header));
            outputImage.Mutate(x =>
                x.DrawText(s, header, brushBlack, new PointF(outputImage.Width / 2 - size.Width / 2, yPos)));

            yPos += header.Size + padGameDay;

            foreach ((string EkTeam, string OpTeam, bool IsHomeGame, string Time) g in gameDay.Value)
            {
                outputImage.Mutate(x =>
                {
                    // draw background
                    x.DrawRoundedRectangleWithCenteredText(
                        ekRect.Offset(offsetX + timeRect.Width + padBetween, yPos - (ekRect.Height - boldText.Size) / 2), Rgba32.ParseHex("#f05a5a"),
                        g.EkTeam, boldText, brushWhite);

                    x.DrawRoundedRectangleWithCenteredText(
                        atRect.Offset(offsetX + timeRect.Width + ekRect.Width + 2 * padBetween, yPos - (atRect.Height - boldText.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), g.IsHomeGame ? "vs" : "@", boldText, brushBlack);

                    x.DrawRoundedRectangleWithCenteredText(
                        opRect.Offset(offsetX + timeRect.Width + ekRect.Width + 3 * padBetween + atRect.Width, yPos - (opRect.Height - text.Size) / 2),
                        Rgba32.ParseHex("#f05a5a"), g.OpTeam, text, brushWhite);

                    x.DrawRoundedRectangleWithCenteredText(
                        timeRect.Offset(offsetX, yPos - (timeRect.Height - boldText.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), g.Time, boldText, brushBlack);
                });
                yPos += ekRect.Height + padGame;
            }
        }

        MemoryStream ms = new();
        await outputImage.SaveAsync(ms,PngFormat.Instance);
        ms.Position = 0;
        Stream = ms;
    }

    [RelayCommand]
    private void Share()
    {
        if (Stream is not null)
        {
            m_shareService.Share(Stream);
        }
    }
    
    [RelayCommand]
    private void ShareB()
    {
        if (Stream is not null)
        {
            m_shareService.ShareB(Stream);
        }
    }
}
