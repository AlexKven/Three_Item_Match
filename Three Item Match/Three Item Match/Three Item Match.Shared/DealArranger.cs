using System;
using System.Collections.Generic;
using System.Text;
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
                Animations[i, 0] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(150) };
                Animations[i, 1] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(150) };
                Animations[i, 2] = new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(150) };
                Animations[i, 3] = new DoubleAnimation() { To = 200, Duration = TimeSpan.FromMilliseconds(150) };
                Animations[i, 4] = new DoubleAnimation() { To = 300, Duration = TimeSpan.FromMilliseconds(150) };

                Animations[i, 0].Completed += AnimationCompletedGenerator(i);

                RotateTransform rotation = new RotateTransform() { CenterX = 0.5, CenterY = 0.5 };
                Cards[i].RenderTransform = rotation;

                Storyboard.SetTarget(Animations[i, 0], Cards[i]);
                Storyboard.SetTarget(Animations[i, 1], Cards[i]);
                Storyboard.SetTarget(Animations[i, 2], rotation);
                Storyboard.SetTarget(Animations[i, 3], Cards[i]);
                Storyboard.SetTarget(Animations[i, 4], Cards[i]);

                Storyboard.SetTargetProperty(Animations[i, 0], "(Canvas.Left)");
                Storyboard.SetTargetProperty(Animations[i, 1], "(Canvas.Top)");
                Storyboard.SetTargetProperty(Animations[i, 2], "Angle");
                Storyboard.SetTargetProperty(Animations[i, 3], "Width");
                Storyboard.SetTargetProperty(Animations[i, 4], "Height");

                MainStoryboard.Children.Add(Animations[i, 0]);
                MainStoryboard.Children.Add(Animations[i, 1]);
                MainStoryboard.Children.Add(Animations[i, 2]);
                MainStoryboard.Children.Add(Animations[i, 3]);
                MainStoryboard.Children.Add(Animations[i, 4]);
            }
            MainStoryboard.Begin();
        }

        private EventHandler<object> AnimationCompletedGenerator(int majorIndex)
        {
            return new EventHandler<object>((s, e) =>
            {
                Cards[majorIndex].FlipTo(FaceUpCards[majorIndex]);
            });
        }

        Storyboard MainStoryboard = new Storyboard();
        DoubleAnimation[,] Animations = new DoubleAnimation[81, 5];
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

        public void DealCards(int numCards)
        {
            double width = 200;
            double height = 300;
            for (int i = 0; i < numCards; i++)
            {
                FaceUpCards[i] = true;
                Animations[i, 0].To = (Width - width * numCards) / 2 + width * i;
                Animations[i, 1].To = (Height - height) / 2;
                Animations[i, 2].To = 0;
                Animations[i, 3].To = width;
                Animations[i, 4].To = height;
                Animations[i, 0].BeginTime = TimeSpan.FromMilliseconds(100 * (numCards - i - 1));
                Animations[i, 1].BeginTime = TimeSpan.FromMilliseconds(100 * (numCards - i - 1));
                Animations[i, 2].BeginTime = TimeSpan.FromMilliseconds(100 * (numCards - i - 1));
                Animations[i, 3].BeginTime = TimeSpan.FromMilliseconds(100 * (numCards - i - 1));
                Animations[i, 4].BeginTime = TimeSpan.FromMilliseconds(100 * (numCards - i - 1));
            }
            MainStoryboard.Begin();
        }
    }
}
