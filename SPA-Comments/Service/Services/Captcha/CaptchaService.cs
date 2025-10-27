using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Numerics;

namespace Service.Services.Captcha;

public class CaptchaService : ICaptchaService
{
    private readonly Font _font;

    public CaptchaService()
    {
        // Загружаем локальный шрифт Roboto из wwwroot/fonts
        var fontCollection = new FontCollection();
        FontFamily family;
        using (var stream = File.OpenRead("wwwroot/fonts/Roboto.ttf"))
        {
            family = fontCollection.Add(stream);
        }

        _font = family.CreateFont(32, FontStyle.Bold);
    }

    public (byte[] Image, string Text) GenerateCaptcha()
    {
        var text = GenerateRandomText(5);

        using var image = new Image<Rgba32>(150, 60);

        image.Mutate(ctx =>
        {
            // Заливка фона
            ctx.Fill(Color.LightGray);

            // Рисуем текст
            ctx.DrawText(text, _font, Color.DarkBlue, new PointF(10, 10));

            // Можно добавить линии или шум
            AddNoise(ctx, image.Width, image.Height);
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return (ms.ToArray(), text);
    }

    private static string GenerateRandomText(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = new Random();
        return new string(Enumerable.Range(0, length).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }

    private static void AddNoise(IImageProcessingContext ctx, int width, int height)
    {
        var rnd = new Random();
        for (int i = 0; i < 20; i++)
        {
            int x1 = rnd.Next(width);
            int y1 = rnd.Next(height);
            int x2 = rnd.Next(width);
            int y2 = rnd.Next(height);
            ctx.DrawLine(Color.Gray, 1, new PointF[] { new PointF(x1, y1), new PointF(x2, y2) });
        }
    }
}
