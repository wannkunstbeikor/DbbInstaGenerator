using System.IO;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Provider;
using DbbInstaGenerator.Interfaces;

namespace DbbInstaGenerator.Android;

public class AndroidShareService : IShareService
{
    public void Share(MemoryStream inStream)
    {
        Bitmap? bitmap = BitmapFactory.DecodeStream(inStream);
        var filePath = MediaStore.Images.Media.InsertImage(Application.Context.ContentResolver, bitmap, null, null);
        
        // Instantiate an intent
        Intent intent = new Intent("com.instagram.share.ADD_TO_STORY");
        intent.SetPackage("com.instagram.android");

        // Attach your image to the intent from a URI
        Uri? backgroundAssetUri = Uri.Parse(filePath);
        intent.SetDataAndType(backgroundAssetUri, "image/jpeg");
        
        // Grant URI permissions for the image
        intent.SetFlags(ActivityFlags.GrantReadUriPermission);

        // Start activity
        MainActivity.Instance.StartActivity(intent);
    }

    public void ShareB(MemoryStream inStream)
    {
        Bitmap? bitmap = BitmapFactory.DecodeStream(inStream);
        var filePath = MediaStore.Images.Media.InsertImage(Application.Context.ContentResolver, bitmap, null, null);
        
        // Instantiate an intent
        Intent intent = new Intent("com.instagram.share.ADD_TO_STORY");
        intent.SetPackage("com.instagram.android");

        // Attach your image to the intent from a URI
        Uri? backgroundAssetUri = Uri.Parse(filePath);
        intent.SetType("image/jpeg");
        intent.PutExtra("interactive_asset_uri", backgroundAssetUri);

        // Grant URI permissions for the image
        intent.SetFlags(ActivityFlags.GrantReadUriPermission);

        // Start activity
        MainActivity.Instance.StartActivity(intent);
    }
}