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
    private const string backgroundResourceName = "DbbInstaGenerator.Resources.template.png";
    
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

    private Dictionary<DateTime, List<(string EkTeam, string OpTeam, bool IsHomeGame, string Score)>> gameDayResults =
        new();
    private Dictionary<DateTime, List<(string EkTeam, string OpTeam, bool IsHomeGame, string Time)>> gameDays = new();

    private Brush brushWhite = Brushes.Solid(Color.WhiteSmoke);
    private Brush brushBlack = Brushes.Solid(Color.Black);

    private readonly int padAfterHeader = 40;
    private readonly int padAfterGame = 10;
    private readonly float padRect = 10;
    private readonly float padBetweenRects = 20;

    private readonly FontCollection fonts = new();
    private readonly IShareService shareService;

    private Font titleFont;
    private Font headerFont;
    private Font standardFont;
    private Font boldFont;

    private static readonly string request =
        "https://www.basketball-bund.net/rest/club/id/886/actualmatches?justHome=false&rangeDays=6";

    public MainViewModel(IShareService inShareService)
    {
        shareService = inShareService;
        fonts.Add(GetResource("DbbInstaGenerator.Resources.Roboto-Bold.ttf"));
        fonts.Add(GetResource("DbbInstaGenerator.Resources.Roboto-Regular.ttf"));
        
        // create fonts
        titleFont = fonts.Get(FontFamily).CreateFont(122, FontStyle.Bold);
        headerFont = fonts.Get(FontFamily).CreateFont(52, FontStyle.Bold);
        standardFont = fonts.Get(FontFamily).CreateFont(32);
        boldFont = fonts.Get(FontFamily).CreateFont(32, FontStyle.Bold);
    }

    private async Task LoadAsync()
    {
        // clear previous data
        gameDayResults.Clear();
        gameDays.Clear();
        
        HttpClient client = new();

        string uri;
        if (OperatingSystem.IsBrowser())
        {
            // we need to bypass cors
            uri = $"https://cors-test.jonathan-kopmann.workers.dev/?{request}";
        }
        else
        {
            uri = request;
        }

        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            var t = await response.Content.ReadAsStringAsync();
            // TODO: show some error
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        ApiResponse<ClubData>? c = JsonSerializer.Deserialize<ApiResponse<ClubData>>(responseBody);
        if (c is null)
        {
            // TODO: show some error
            return;
        }

        // go through all games returned by the api and add them to our dictionaries
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
                gameDays[gameTime].Add((ekTeam, opTeam, homeName.Item2, match.KickoffTime + " Uhr"));
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

    private async Task Render(string title, Dictionary<DateTime, List<(string EkTeam, string OpTeam, bool IsHome, string ScoreOrTime)>> data)
    {
        // load our background
        using var backgroundImage = await LoadImageAsync(backgroundResourceName);
        using var outputImage = new Image<Rgba32>(backgroundImage.Width, backgroundImage.Height);
        
        // draw the background
        outputImage.Mutate(x => x.DrawImage(backgroundImage, 1));

        // create rectangles that fit the largest text
        FontRectangle ekRect = data.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.EkTeam, new TextOptions(boldFont)))).Pad(padRect);
        FontRectangle opRect = data.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.OpTeam, new TextOptions(standardFont)))).Pad(padRect);
        FontRectangle atRect = TextMeasurer.MeasureAdvance("vs", new TextOptions(boldFont)).Pad(padRect);
        FontRectangle timeRect = data.Values.SelectMany(gameDay => gameDay).Aggregate(FontRectangle.Empty,
            (current, game) => FontRectangle.Union(current,
                TextMeasurer.MeasureAdvance(game.ScoreOrTime, new TextOptions(boldFont)))).Pad(padRect);
        
        // use the rects to calculate the offset of the x and y axis, so the stuff is centered
        float offsetX = (outputImage.Width - (ekRect.Width + atRect.Width + opRect.Width + timeRect.Width +
                                              3 * padBetweenRects)) / 2;
        float offsetY = (outputImage.Height - (data.Values.Count * (headerFont.Size + padAfterHeader) +
                                               (data.Values.Count - 1) * padAfterGame + data.Values.Sum(va =>
                                                   va.Count * ekRect.Height + (va.Count - 1) * padAfterGame))) / 2;

        // draw title at the top
        outputImage.Mutate(x => x.DrawCenteredText(title, titleFont, brushWhite, new PointF(outputImage.Width / 2, offsetY / 2), true));

        float yPos = offsetY;
        foreach (var day in data)
        {
            // draw day
            string s = day.Key.ToString("dddd dd.MM.yyyy", new CultureInfo("de-DE")).ToUpper();
            outputImage.Mutate(x =>
                x.DrawCenteredText(s, headerFont, brushWhite, new PointF(outputImage.Width / 2, yPos)));
            yPos += headerFont.Size + padAfterHeader;

            foreach ((string EkTeam, string OpTeam, bool IsHomeGame, string ScoreOrTime) game in day.Value)
            {
                float xPos = offsetX;
                outputImage.Mutate(x =>
                {
                    // draw ek team name
                    x.DrawRoundedRectangleWithCenteredText(
                        ekRect.Offset(xPos, yPos - (ekRect.Height - boldFont.Size) / 2), Rgba32.ParseHex("#f05a5a"),
                        game.EkTeam, boldFont, brushWhite);
                    xPos += ekRect.Width;

                    xPos += padBetweenRects;

                    // draw vs or @ depending on home/away game
                    x.DrawRoundedRectangleWithCenteredText(
                        atRect.Offset(xPos, yPos - (atRect.Height - boldFont.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), game.IsHomeGame ? "vs" : "@", boldFont, brushBlack);
                    xPos += atRect.Width;

                    xPos += padBetweenRects;

                    // draw opponent team name
                    x.DrawRoundedRectangleWithCenteredText(
                        opRect.Offset(xPos, yPos - (opRect.Height - standardFont.Size) / 2),
                        Rgba32.ParseHex("#f05a5a"), game.OpTeam, standardFont, brushWhite);
                    xPos += opRect.Width;

                    xPos += padBetweenRects;

                    // draw score or time of the game
                    x.DrawRoundedRectangleWithCenteredText(
                        timeRect.Offset(xPos, yPos - (timeRect.Height - boldFont.Size) / 2),
                        Rgba32.ParseHex("#cbcbcb"), game.ScoreOrTime, boldFont, brushBlack);
                    xPos += timeRect.Width;
                });
                yPos += ekRect.Height + padAfterGame;
            }

            yPos += padAfterHeader - padAfterGame;
        }

        // save the image to the stream so we can process it further if asked by the user
        MemoryStream ms = new();
        await outputImage.SaveAsync(ms);
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
    private async Task CreateScore()
    {
        await LoadAsync();

        await Render("Ergebnisse", gameDayResults);
    }

    [RelayCommand]
    private async Task CreateGameDay()
    {
        await LoadAsync();

        await Render("Spieltag", gameDays);
    }

    [RelayCommand]
    private void Share()
    {
        if (Stream is not null)
        {
            shareService.Share(Stream);
        }
    }
    
    [RelayCommand]
    private void ShareB()
    {
        if (Stream is not null)
        {
            shareService.ShareB(Stream);
        }
    }
}
