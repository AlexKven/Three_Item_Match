using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using static System.Math;
using System.Linq;

namespace Three_Item_Match
{
    public class DealArranger
    {
        private List<int> DrawnCards = new List<int>();
        private List<int> DrawPile = new List<int>();
        private List<Tuple<int, int, int>> CollectedSets = new List<Tuple<int, int, int>>();
        private List<int> SelectedCards = new List<int>();

        private DateTime LastResizeTime = DateTime.Now;

        const double ANIMATION_TIME = 150;
        const double NON_SEL_DEAL_SCALE = 0.9;
        const double NON_SEL_DEAL_OPACITY = 0.6;

        public DealArranger(Card[] cards)
        {
            Cards = cards;
            for (int i = 0; i < 81; i++)
            {
                DrawPile.Add(i);
                Cards[i].SourceImage.Tapped += CardTappedHandlerGenerator(i);
                //Cards[i].SourceImage.PointerReleased += CardClickedHandlerGenerator(i);
                //Cards[i].SetValue(Control.HeightProperty, 225);
                //Cards[i].SetValue(Control.WidthProperty, 150);
                Animations[i, 0] = new DoubleAnimation() { From = null, To = Card.WIDTH * 0.75, Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME), EnableDependentAnimation = true };
                Animations[i, 1] = new DoubleAnimation() { From = null, To = Card.HEIGHT * 0.75, Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME), EnableDependentAnimation = true };
                Animations[i, 2] = new DoubleAnimation() { From = null, To = 0, Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME), EnableDependentAnimation = true };
                Animations[i, 3] = new DoubleAnimation() { From = null, To = 1, Duration = TimeSpan.FromMilliseconds(ANIMATION_TIME), EnableDependentAnimation = true };

                Animations[i, 0].Completed += AnimationCompletedGenerator(i);

                Storyboard.SetTarget(Animations[i, 0], Cards[i]);
                Storyboard.SetTarget(Animations[i, 1], Cards[i]);
                Storyboard.SetTarget(Animations[i, 2], Cards[i]);
                Storyboard.SetTarget(Animations[i, 3], Cards[i]);

                Storyboard.SetTargetProperty(Animations[i, 0], "X");
                Storyboard.SetTargetProperty(Animations[i, 1], "Y");
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

        private TappedEventHandler CardTappedHandlerGenerator(int index)
        {
            return new TappedEventHandler((s, e) => OnCardClicked(index));
        }

        private PointerEventHandler CardClickedHandlerGenerator(int index)
        {
            return new PointerEventHandler((s, e) => OnCardClicked(index));
        }

        private void OnCardClicked(int cardNumber)
        {
            if (SelectedCards.Contains(cardNumber))
                SelectedCards.Remove(cardNumber);
            else
                SelectedCards.Add(cardNumber);
            ArrangeCards(false, TimeSpan.FromMilliseconds(ANIMATION_TIME / 2));
        }

        Storyboard MainStoryboard = new Storyboard();
        DoubleAnimation[,] Animations = new DoubleAnimation[81, 4];
        private bool[] FaceUpCards = new bool[81];

        private double _Width;
        private double _Height;

        public double Width
        {
            get { return _Width; }
            set
            {
                _Width = value;
                ResizeArrange();
            }
        }

        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
                ResizeArrange();
            }
        }

        private Card[] Cards;

        public void DealCards(int numCards)
        {
            //Random rnd = new Random();
            //double scale = 90.0 / 200;
            //for (int i = 0; i < numCards; i++)
            //{
            //    FaceUpCards[i] = true;
            //    Animations[i, 0].To = rnd.Next(100, (int)Width - 100);
            //    Animations[i, 1].To = rnd.Next(100, (int)Height - 100);
            //    if (rnd.NextDouble() < .5)
            //        rnd.NextBytes(new byte[50]);
            //    Animations[i, 2].To = (1 + rnd.NextDouble()) * 360;
            //    Animations[i, 3].To = scale;
            //    Animations[i, 0].BeginTime = TimeSpan.FromMilliseconds(50 * (numCards - i - 1));
            //    Animations[i, 1].BeginTime = TimeSpan.FromMilliseconds(50 * (numCards - i - 1));
            //    Animations[i, 2].BeginTime = TimeSpan.FromMilliseconds(50 * (numCards - i - 1));
            //    Animations[i, 3].BeginTime = TimeSpan.FromMilliseconds(50 * (numCards - i - 1));
            //}
            for (int i = 0; i < numCards; i++)
            {
                DrawnCards.Add(DrawPile[0]);
                DrawPile.RemoveAt(0);
            }
            ArrangeCards(true, TimeSpan.FromMilliseconds(ANIMATION_TIME));
            Animate();
        }

        private void ArrangeCards(bool cascade, TimeSpan animationDuration)
        {
            const double CASC_TIME = 40;
            int numCascades = 0;

            bool pileOnTop = (Height > Width);
            Rect dealRegion;
            Rect pileRegion;
            double pileRegionLength;
            if (pileOnTop)
            {
                pileRegionLength = Min(Height * .2, Width / 3);
            }
            else
            {
                pileRegionLength = Min(Width * .2, Height / 3);
            }
            if (pileOnTop)
            {
                pileRegion = new Rect(0, 0, Width, pileRegionLength);
                dealRegion = new Rect(0, pileRegionLength, Width, Height - pileRegionLength);
            }
            else
            {
                pileRegion = new Rect(0, 0, pileRegionLength, Height);
                dealRegion = new Rect(pileRegionLength, 0, Width - pileRegionLength, Height);
            }

            List<double> columnScales = new List<double>();

            int dealColumns = 1;
            int dealRows = 1;
            double scale = 1;
            for (int i = 1; i <= DrawnCards.Count; i++)
            {
                dealColumns = i;
                dealRows = (int)Ceiling((double)DrawnCards.Count / (double)dealColumns);
                scale = Min(dealRegion.Width / dealColumns * NON_SEL_DEAL_SCALE / Card.WIDTH, dealRegion.Height / dealRows * NON_SEL_DEAL_SCALE / Card.HEIGHT);
                columnScales.Add(scale);
            }

            if (columnScales.Count > 0)
                dealColumns = columnScales.IndexOf(columnScales.Max()) + 1;
            dealRows = (int)Ceiling((double)DrawnCards.Count / (double)dealColumns);
            scale = Min(dealRegion.Width / dealColumns * NON_SEL_DEAL_SCALE / Card.WIDTH, dealRegion.Height / dealRows * NON_SEL_DEAL_SCALE / Card.HEIGHT);

            double cellWidth = dealRegion.Width / dealColumns;
            double cellHeight = dealRegion.Height / dealRows;
            for (int i = 0; i < DrawnCards.Count; i++)
            {
                TimeSpan beginTime = cascade ? TimeSpan.FromMilliseconds(CASC_TIME * numCascades++) : TimeSpan.Zero;
                FaceUpCards[DrawnCards[i]] = true;
                int x = i % dealColumns;
                int y = i / dealColumns;
                Animations[DrawnCards[i], 0].To = dealRegion.X + cellWidth * (x + .5);
                Animations[DrawnCards[i], 1].To = dealRegion.Y + cellHeight * (y + .5);
                Cards[DrawnCards[i]].SourceImage.Opacity = (SelectedCards.Count > 0 && !SelectedCards.Contains(DrawnCards[i])) ? NON_SEL_DEAL_OPACITY : 1;
                Animations[DrawnCards[i], 3].To = (SelectedCards.Count > 0) ? SelectedCards.Contains(DrawnCards[i]) ? scale / NON_SEL_DEAL_SCALE : scale * NON_SEL_DEAL_SCALE : scale;
                Animations[DrawnCards[i], 0].Duration = Animations[DrawnCards[i], 1].Duration = Animations[DrawnCards[i], 3].Duration = animationDuration;
                SetAnimationTime(DrawnCards[i], beginTime);
            }

            double pileScale = pileRegionLength * NON_SEL_DEAL_SCALE / Card.WIDTH;

            for (int i = 0; i < DrawPile.Count; i++)
            {
                Animations[DrawPile[i], 0].To = pileRegionLength / 2;
                Animations[DrawPile[i], 1].To = pileRegionLength / 2;
                Animations[DrawPile[i], 3].To = pileScale;
                Animations[DrawPile[i], 0].Duration = Animations[DrawPile[i], 1].Duration = Animations[DrawPile[i], 3].Duration = animationDuration;
                SetAnimationTime(DrawPile[i], TimeSpan.Zero);
            }
            Animate();
        }

        private async void ResizeArrange()
        {
            DateTime time = LastResizeTime = DateTime.Now;
            await Task.Delay(20);
            if (time == LastResizeTime)
                ArrangeCards(false, TimeSpan.Zero);
        }

        private void Animate()
        {
            MainStoryboard.Begin();
        }

        private void SetAnimationTime(int card, TimeSpan time)
        {
            for (int i = 0; i < 4; i++)
                Animations[card, i].BeginTime = time;
        }

        public void ShuffleDrawPile()
        {
            List<int> newPile = new List<int>();
            newPile.AddRange(DrawPile);
            DrawPile.Clear();
            byte[] buffer = new byte[newPile.Count];
            new Random().NextBytes(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                int index = buffer[i] % newPile.Count;
                Canvas.SetZIndex(Cards[newPile[index]].SourceImage, buffer.Length - i);
                DrawPile.Add(newPile[index]);
                newPile.RemoveAt(index);
            }
        }
    }
}
