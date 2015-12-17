using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Three_Item_Match
{
    public sealed partial class Card : DependencyObject
    {
        public Card()
        {
            this.InitializeComponent();
        }

        public const double WIDTH = 300;
        public const double HEIGHT = 200;

        private bool FaceVisible = false;

        private DoubleAnimation FlipAnimation = new DoubleAnimation() { };
        Storyboard FlipStoryboard = new Storyboard();

        public Card(Image sourceImage, CardFace face)
        {
            SourceImage = sourceImage;
            Face = face;
            SourceImage.Projection = Projection;
            SourceImage.Width = 200;
            SourceImage.Height = 300;
            SetImage("Back.png");

            Storyboard.SetTarget(FlipAnimation, Projection);
            Storyboard.SetTargetProperty(FlipAnimation, "RotationY");
            FlipStoryboard.Children.Add(FlipAnimation);
        }

        private void SetImage(string imageName)
        {
            BitmapImage img = new BitmapImage();
            string pth = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Assets\\" + imageName;
            img.UriSource = new Uri(pth, UriKind.Absolute);
            SourceImage.Source = img;
        }

        public Image SourceImage { get; private set; }
        public CardFace Face { get; private set; }

        private bool _FaceUp = false;
        public bool FaceUp
        {
            get { return _FaceUp; }
            set
            {
                StopFlip();
                _FaceUp = value;
                SetImage(value ? "Card" + Face.ToInt().ToString() + ".png" : "Back.png");
            }
        }
        
        private PlaneProjection Projection = new PlaneProjection() { CenterOfRotationX = 0.5, CenterOfRotationY = 0.5, CenterOfRotationZ = 0.5 };

        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Card), new PropertyMetadata((double)0, OnXChangedStatic));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Card), new PropertyMetadata((double)0, OnYChangedStatic));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(Card), new PropertyMetadata((double)1, OnScaleChangedStatic));
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Card), new PropertyMetadata((double)0, OnAngleChangedStatic));

        [IndependentlyAnimatable]
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        [IndependentlyAnimatable]
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        [IndependentlyAnimatable]
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        [IndependentlyAnimatable]
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static void OnXChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Card typedSender = (Card)sender;
            typedSender.UpdatePosition(false);
        }

        public static void OnYChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Card typedSender = (Card)sender;
            typedSender.UpdatePosition(false);
        }

        public static void OnScaleChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Card typedSender = (Card)sender;
            typedSender.UpdatePosition(true);
            //typedSender.Rotation.CenterX = typedSender.SourceImage.Width / 2;
            //typedSender.Rotation.CenterY = typedSender.SourceImage.Height / 2;
        }

        public static void OnAngleChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Card typedSender = (Card)sender;
            typedSender.Projection.RotationZ = (double)e.NewValue;
        }

        private void UpdatePosition(bool updateSize)
        {
            Canvas.SetLeft(SourceImage, X - WIDTH * Scale / 2);
            Canvas.SetTop(SourceImage, Y - HEIGHT * Scale / 2);
            if (updateSize)
            {
                SourceImage.Width = WIDTH * Scale;
                SourceImage.Height = HEIGHT * Scale;
            }
        }

        public void FlipTo(bool faceUp, bool noAnimate)
        {
            if (_FaceUp != faceUp)
            {
                Flip(noAnimate);
            }
        }

        private void StopFlip()
        {
            FlipStoryboard.Completed -= FlipStoryboard_Completed;
            if (FaceVisible == FaceUp)
                FlipStoryboard.SkipToFill();
            else
            {
                FlipStoryboard.Seek(TimeSpan.Zero);
            }
            FaceUp = FaceUp;
        }

        private void Flip(bool noAnimate)
        {
            //StopFlip();
            _FaceUp = !FaceUp;

            FlipAnimation.To = 90;
            FlipAnimation.From = 0;
            FlipAnimation.Duration = noAnimate ? TimeSpan.Zero : TimeSpan.FromMilliseconds(150);
            FlipStoryboard.Completed += FlipStoryboard_Completed;
            FlipStoryboard.Begin();
        }

        private void FlipStoryboard_Completed(object sender, object e)
        {
            FaceVisible = FaceUp;
            SetImage(FaceUp ? "Card" + Face.ToInt().ToString() + ".png" : "Back.png");
            FlipStoryboard.Completed -= FlipStoryboard_Completed;
            FlipAnimation.To = 0;
            FlipAnimation.From = -90;
            FlipStoryboard.Begin();
        }
    }
}
