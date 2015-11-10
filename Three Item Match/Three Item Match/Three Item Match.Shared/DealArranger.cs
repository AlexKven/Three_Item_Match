using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Three_Item_Match
{
    public class DealArranger
    {
        public DealArranger(Card[] cards)
        {
            Cards = cards;
            for (int i = 0; i < 81; i++)
            {
                //Cards[i].SetValue(Control.HeightProperty, 225);
                //Cards[i].SetValue(Control.WidthProperty, 150);
                Animations[i, 0] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(550) };
                Animations[i, 1] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(550) };
                Animations[i, 2] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(550) };
                Animations[i, 3] = new DoubleAnimation() { To = 1, Duration = TimeSpan.FromMilliseconds(550), EnableDependentAnimation = true };

                Animations[i, 0].Completed += AnimationCompletedGenerator(i);

                RotateTransform rotation = new RotateTransform() { CenterX = 0.5, CenterY = 0.5 };
                Cards[i].RenderTransform = rotation;

                Storyboard.SetTarget(Animations[i, 0], Cards[i]);
                Storyboard.SetTarget(Animations[i, 1], Cards[i]);
                Storyboard.SetTarget(Animations[i, 2], rotation);
                Storyboard.SetTarget(Animations[i, 3], Cards[i]);

                Storyboard.SetTargetProperty(Animations[i, 0], "(Canvas.Left)");
                Storyboard.SetTargetProperty(Animations[i, 1], "(Canvas.Top)");
                Storyboard.SetTargetProperty(Animations[i, 2], "Angle");
                Storyboard.SetTargetProperty(Animations[i, 3], "Scale");

                MainStoryboard.Children.Add(Animations[i, 0]);
                MainStoryboard.Children.Add(Animations[i, 1]);
                MainStoryboard.Children.Add(Animations[i, 2]);
                MainStoryboard.Children.Add(Animations[i, 3]);
            }
            MainStoryboard.Begin();

            //for (int i = 0; i < 31; i++)
            //    Cards[i].Visibility = Visibility.Collapsed;
            //for (int i = 50; i < 81; i++)
            //    Cards[i].Visibility = Visibility.Collapsed;
        }

        private EventHandler<object> AnimationCompletedGenerator(int majorIndex)
        {
            return new EventHandler<object>((s, e) =>
            {
                Cards[majorIndex].FlipTo(FaceUpCards[majorIndex]);
            });
        }

        Storyboard MainStoryboard = new Storyboard();
        DoubleAnimation[,] Animations = new DoubleAnimation[81, 4];
        private bool[] FaceUpCards = new bool[81];

        private double _Width;
        private double _Height;

        public double Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public double Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        private Card[] Cards;

        public async Task DealCards(int numCards)
        {
            double scale = 90.0 / 200;
            for (int i = 0; i < numCards; i++)
            {
                await Cards[i].CacheImage();
                double x = 30.0 * (1 + (81 - i) / 11.0) * Math.Cos(i / 27.0 * 2 * Math.PI);
                double y = 30.0 * (1 + (81 - i) / 11.0) * Math.Sin(i / 27.0 * 2 * Math.PI);
                FaceUpCards[i] = true;
                Animations[i, 0].To = (Width) / 2.0 + x;
                Animations[i, 1].To = (Height) / 2.0 + y - 50;
                Animations[i, 2].To = (i * 360.0 / 27.0) % 360.0;
                Animations[i, 3].To = scale;
                Animations[i, 0].BeginTime = TimeSpan.FromMilliseconds(120 * (numCards - i - 1));
                Animations[i, 1].BeginTime = TimeSpan.FromMilliseconds(120 * (numCards - i - 1));
                Animations[i, 2].BeginTime = TimeSpan.FromMilliseconds(120 * (numCards - i - 1));
                Animations[i, 3].BeginTime = TimeSpan.FromMilliseconds(120 * (numCards - i - 1));
            }
            MainStoryboard.Begin();
        }
    }
}
