using System;
using System.IO;
using DbbInstaGenerator.Interfaces;

namespace DbbInstaGenerator.iOS;

public class iOSShareService : IShareService
{
    private const string InstagramUrlScheme = "instagram-stories://share";
    
    public void Share(MemoryStream inStream)
    {
        var urlScheme = new NSUrl(InstagramUrlScheme);

        if (UIApplication.SharedApplication.CanOpenUrl(urlScheme))
        {
            var backgroundImage = UIImage.LoadFromData(inStream.ToArray());
            var pasteboardItems = new[] { new NSDictionary("com.instagram.sharedSticker.backgroundImage", backgroundImage.AsPNG()) };

            var pasteboardOptions = new NSDictionary(UIPasteboard.OptionExpirationDate, NSDate.Now.AddSeconds(60 * 5));

            UIPasteboard.General.SetItems(pasteboardItems, pasteboardOptions);

            UIApplication.SharedApplication.OpenUrl(urlScheme, new NSDictionary(), null);
        }
        else
        {
            // Handle error cases
        }
    }
    
    public void ShareB(MemoryStream inStream)
    {
        throw new NotImplementedException();
    }
}