using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static System.Math;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Three_Item_Match
{
    public sealed partial class Card : UserControl
    {
        public Card()
        {
            this.InitializeComponent();
            OnFaceChanged();
        }

        private CardFace _Face;
        public CardFace Face
        {
            get { return _Face; }
            set
            {
                _Face = value;
                OnFaceChanged();
            }
        }

        private void OnFaceChanged()
        {
            int margin = 10;
            int numRows = (int)Face.Number + 1;
            ShapeGrid.Children.Clear();
            ShapeGrid.RowDefinitions.Clear();
            ShapeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            if (numRows >= 2)
            {
                ShapeGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(margin) });
                ShapeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            if (numRows >= 3)
            {
                ShapeGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(margin) });
                ShapeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            int num = (int)Face.Shape + 3 * (int)Face.Fill + 9 * (int)Face.Color;
            for (int i = 0; i < numRows; i++)
            {
                Image imgCtrl = new Image() { Source = RenderedShapes[num], Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                Grid.SetRow(imgCtrl, 2 * i);
                ShapeGrid.Children.Add(imgCtrl);
            }
        }

        private static ImageSource[] RenderedShapes = new ImageSource[27];

        public static async Task RenderShapes()
        {
            for (int i = 0; i < 27; i++)
            {
                int num = i;
                CardShape shape = (CardShape)(num % 3);
                num /= 3;
                CardFill fill = (CardFill)(num % 3);
                num /= 3;
                CardColor color = (CardColor)(num % 3);

                WriteableBitmap image = await WriteableBitmapExtensions.FromContent(null, new Uri($"ms-appx:///Assets/{shape.ToString()}.bmp"));
                Color realColor = new Color();
                switch (color)
                {
                    case CardColor.Green:
                        realColor = Colors.Green;
                        break;
                    case CardColor.Red:
                        realColor = Colors.Red;
                        break;
                    case CardColor.Purple:
                        realColor = Colors.Purple;
                        break;
                }
                image.ForEach((int x, int y, Color clr) =>
                {
                    if (clr == Colors.Black)
                        return realColor;
                    else if (clr == Colors.White)
                        return Colors.Transparent;
                    else if (fill == CardFill.Solid)
                        return realColor;
                    else if (fill == CardFill.Empty)
                        return Colors.Transparent;
                    else if (x % 64 < 32)
                        return realColor;
                    else
                        return Colors.Transparent;
                });
                RenderedShapes[i] = image;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0 && e.NewSize.Width > 0)
            {
                double xScl = e.NewSize.Width / 230;
                double yScl = e.NewSize.Height / 330;
                double scl = Min(yScl, xScl);
                Scale.ScaleX = scl;
                Scale.ScaleY = scl;
                MainGrid.Margin = new Thickness((e.NewSize.Width - 200 * scl) / 2, (e.NewSize.Height - 300 * scl) / 2, 0, 0);
            }
        }
    }
}
