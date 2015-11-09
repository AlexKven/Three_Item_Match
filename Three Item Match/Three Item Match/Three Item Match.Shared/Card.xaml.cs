using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
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
            Storyboard.SetTarget(FlipAnimation, Projection);
            Storyboard.SetTargetProperty(FlipAnimation, "RotationY");
            FlipStoryboard.Children.Add(FlipAnimation);
        }

        private DoubleAnimation FlipAnimation = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(100) };
        Storyboard FlipStoryboard = new Storyboard();

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
            int num = (int)Face.Shape + 3 * (int)Face.Fill;
            Color color = new Color();
            switch (Face.Color)
            {
                case CardColor.Green:
                    color = Colors.Green;
                    break;
                case CardColor.Purple:
                    color = Colors.Purple;
                    break;
                case CardColor.Red:
                    color = Colors.Red;
                    break;
            }
            for (int i = 0; i < numRows; i++)
            {
                Rectangle colorRect = new Rectangle() { Fill = new SolidColorBrush(color), Margin = new Thickness(1,1,2,1) };
                Image imgCtrl = new Image() { Source = RenderedShapes[num], Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                Grid.SetRow(imgCtrl, 2 * i);
                Grid.SetRow(colorRect, 2 * i);
                ShapeGrid.Children.Add(colorRect);
                ShapeGrid.Children.Add(imgCtrl);
            }
        }

        private static ImageSource[] RenderedShapes = new ImageSource[9];

        public static async Task RenderShapes()
        {
            for (int i = 0; i < 9; i++)
            {
                int num = i;
                CardShape shape = (CardShape)(num % 3);
                num /= 3;
                CardFill fill = (CardFill)(num % 3);

                WriteableBitmap image = await WriteableBitmapExtensions.FromContent(null, new Uri($"ms-appx:///Assets/{shape.ToString()}.bmp"));
                image.ForEach((int x, int y, Color clr) =>
                {
                    if (clr == Colors.Black)
                        return Colors.Transparent;
                    else if (clr == Colors.White)
                        return Colors.White;
                    else if (fill == CardFill.Solid)
                        return Colors.Transparent;
                    else if (fill == CardFill.Empty)
                        return Colors.White;
                    else if (x % 64 < 32)
                        return Colors.Transparent;
                    else
                        return Colors.White;
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

        private bool _FaceUp = false;
        public bool FaceUp
        {
            get { return _FaceUp; }
            set
            {
                StopFlip();
                _FaceUp = value;
                FrontBorder.Visibility = FaceUp ? Visibility.Visible : Visibility.Collapsed;
                BackBorder.Visibility = FaceUp ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public void FlipTo(bool faceUp)
        {
            if (_FaceUp != faceUp)
            {
                Flip();
            }
        }

        private void StopFlip()
        {
            FlipStoryboard.Completed -= FlipStoryboard_Completed;
            if (FrontBorder.Visibility == (FaceUp ? Visibility.Visible : Visibility.Collapsed))
                FlipStoryboard.SkipToFill();
            else
            {
                FlipStoryboard.Seek(TimeSpan.Zero);
                FrontBorder.Visibility = FaceUp ? Visibility.Visible : Visibility.Collapsed;
                BackBorder.Visibility = FaceUp ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void Flip()
        {
            StopFlip();
            _FaceUp = !FaceUp;

            FlipAnimation.To = 90;
            FlipAnimation.From = 0;

            FlipStoryboard.Completed += FlipStoryboard_Completed;
            FlipStoryboard.Begin();
        }

        private void FlipStoryboard_Completed(object sender, object e)
        {
            FrontBorder.Visibility = FaceUp ? Visibility.Visible : Visibility.Collapsed;
            BackBorder.Visibility = FaceUp ? Visibility.Collapsed : Visibility.Visible;
            FlipStoryboard.Completed -= FlipStoryboard_Completed;
            FlipAnimation.To = 0;
            FlipAnimation.From = -90;
            FlipStoryboard.Begin();
        }
    }
}
