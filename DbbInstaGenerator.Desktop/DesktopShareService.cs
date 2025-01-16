using System.IO;
using Avalonia.Platform.Storage;
using DbbInstaGenerator.Interfaces;
using DbbInstaGenerator.Services;

namespace DbbInstaGenerator.Desktop;

public class DesktopShareService : IShareService
{
    public async void Share(MemoryStream inStream)
    {
        var f = await FileDialogService.ShowSaveFileDialogAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ".png",
            Title = "Save Image",
            SuggestedFileName = "image.png",
            FileTypeChoices = [new FilePickerFileType(".png")]
        })!;
        if (f is null)
        {
            return;
        }

        await using var stream = await f.OpenWriteAsync();
        await inStream.CopyToAsync(stream);
    }

    public void ShareB(MemoryStream inStream)
    {
        throw new System.NotImplementedException();
    }
}