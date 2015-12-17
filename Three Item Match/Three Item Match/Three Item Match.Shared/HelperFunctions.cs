using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static System.Math;

namespace Three_Item_Match
{
    public static class HelperFunctions
    {

        public static List<T> AllChildrenOfType<T>(DependencyObject parent) where T : FrameworkElement
        {
            var _List = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is T)
                {
                    _List.Add(_Child as T);
                }
                _List.AddRange(AllChildrenOfType<T>(_Child));
            }
            return _List;
        }

        public static T FindControl<T>(DependencyObject parentContainer, string controlName) where T : FrameworkElement
        {
            var childControls = AllChildrenOfType<T>(parentContainer);
            var control = childControls.Where(x => x.Name.Equals(controlName)).Cast<T>().First();
            return control;
        }
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

        public static bool VisuallyApproximate(this double d1, double d2, double tolerance)
        {
            return Abs((d1 - d2) / d1) < tolerance;
        }

        public static bool VisuallyApproximate(this double d1, double d2) => d1.VisuallyApproximate(d2, 0.001);

        public static bool Contains(this Tuple<int,int, int> set, params int[] cards)
        {
            foreach (var card in cards)
            {
                if (!(set.Item1 == card || set.Item2 == card || set.Item3 == card))
                    return false;
            }
            return true;
        }
    }
}
