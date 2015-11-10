using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Three_Item_Match
{
    public static class HelperFunctions
    {
        public static async Task SaveImage(RenderTargetBitmap rtb, string fileName)
        {
            var pixelBuffer = await rtb.GetPixelsAsync();

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".png", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    (uint)rtb.PixelWidth,
                    (uint)rtb.PixelHeight, 96d, 96d,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }
        }
    }
}
