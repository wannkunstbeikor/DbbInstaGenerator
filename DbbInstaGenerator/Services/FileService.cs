using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace DbbInstaGenerator.Services;

public class FileDialogService
{
    /// <summary>
    /// Shows a save file dialog
    /// </summary>
    /// <param name="options"></param>
    /// <returns>The path of the file that was saved. Null if no file was saved</returns>
    public static async Task<IStorageFile?>? ShowSaveFileDialogAsync(FilePickerSaveOptions options)
    {
        TopLevel? topLevel;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        }
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            topLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView);
        }
        else
        {
            throw new Exception();
        }
        
        return await topLevel?.StorageProvider.SaveFilePickerAsync(options)!;
    }
}