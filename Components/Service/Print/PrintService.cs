using SkiaSharp;
using System.Drawing;
using System.Drawing.Printing;
using System.Diagnostics;
using TaskManagerWeb.Components.Helper;

public class PrintService(CommonLib CommonLib)
{
    private static string taskName = "PrintService";
    private static MessageLevel settingLevel => DebugParameters.MsgLvl_PrintService;

    public void CreateImage(string TextToPrint, SKImage ImageToPrint)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"CreateImage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            using var skSurface = SKSurface.Create(new SKImageInfo(500, 500));
            var canvas = skSurface.Canvas;
            canvas.Clear(SKColors.White);

            var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 24
            };

            canvas.DrawText(TextToPrint, 20, 40, textPaint);

            if (ImageToPrint != null)
            {
                var imageBitmap = SKBitmap.FromImage(ImageToPrint);
                canvas.DrawBitmap(imageBitmap, new SKRect(20, 60, 420, 460));
            }

            using var image = skSurface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray();
            File.WriteAllBytes("wwwroot/output.png", bytes);
            // PrintImage("C:/Users/drkel/OneDrive/Documents/GitHub/TaskManagerWeb/wwwroot/output.png");
            // WindowPrint("wwwroot/output.png");
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    public void PrintImage(string ImagePath)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"PrintImage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ImagePath,
                UseShellExecute = true
            };

            using Process process = Process.Start(startInfo)!;
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

#pragma warning disable CA1416 // Validate platform compatibility
    public void WindowPrint(string ImagePath)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"WindowPrint triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            PrintDocument printDoc = new();
            printDoc.PrintPage += (sender, ev) => PrintPage(sender, ev, ImagePath);
            printDoc.Print();
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private void PrintPage(object Sender, PrintPageEventArgs EV, string ImagePath)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"PrintPage triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            Image img = Image.FromFile(ImagePath);
            Point loc = new Point(100, 100);
            EV.Graphics!.DrawImage(img, loc);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility
}