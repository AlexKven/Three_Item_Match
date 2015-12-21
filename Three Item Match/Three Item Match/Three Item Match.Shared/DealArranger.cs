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
using System.Collections.ObjectModel;

namespace Three_Item_Match
{
    public class DealArranger
    {
        #region Properties
        private List<int> _DrawnCards = new List<int>();
        private List<int> _DrawPile = new List<int>();
        private List<int> _SelectedCards = new List<int>();
        private List<int> _HighlightedCards = new List<int>();
        private List<SetPile> _CollectedSets = new List<SetPile>();
        private List<SetPile> _MissedSets = new List<SetPile>();
        private Tuple<int, int, int>[] _ShownSets = null;
        private Card[] Cards;
        private FrameworkElement TimeBlock;
        private bool _SuspendRender = false;

        private bool _ShowHighlights = false;

        private bool _InstantDeal = false;

        public Card this[int index]
        {
            get { return Cards[index]; }
        }

        public IReadOnlyList<int> DrawnCards
        {
            get { return new ReadOnlyCollection<int>(_DrawnCards); }
        }

        public IReadOnlyList<int> DrawPile
        {
            get { return new ReadOnlyCollection<int>(_DrawPile); }
        }

        public IReadOnlyList<int> SelectedCards
        {
            get { return new ReadOnlyCollection<int>(_SelectedCards); }
        }

        public IReadOnlyList<int> HighlightedCards
        {
            get { return new ReadOnlyCollection<int>(_HighlightedCards); }
        }

        public IReadOnlyList<SetPile> CollectedSets
        {
            get { return new ReadOnlyCollection<SetPile>(_CollectedSets); }
        }

        public IReadOnlyList<SetPile> MissedSets
        {
            get { return new ReadOnlyCollection<SetPile>(_MissedSets); }
        }

        public bool SuspendRender
        {
            get { return _SuspendRender; }
            set
            {
                _SuspendRender = value;
                if (!SuspendRender)
                    ArrangeCards(true, CurrentAnimationTime, CurrentAnimationTime + CurrentAnimationTime);
            }
        }

        public IReadOnlyList<Tuple<int, int, int>> ShownSets
        {
            get
            {
                if (_ShownSets == null)
                    RefreshSets();
                return new ReadOnlyCollection<Tuple<int, int, int>>(_ShownSets.ToList());
            }
        }

        public bool ShowHighlights
        {
            get { return _ShowHighlights; }
            set
            {
                _ShowHighlights = value;
                ArrangeCards();
            }
        }

        public bool InstantDeal
        {
            get { return _InstantDeal; }
            set { _InstantDeal = value; }
        }

        public TimeSpan CurrentAnimationTime
        {
            get { return InstantDeal ? TimeSpan.Zero : TimeSpan.FromMilliseconds(ANIMATION_TIME); }
        }

        public TimeSpan CurrentCascadeTime
        {
            get { return InstantDeal ? TimeSpan.Zero : TimeSpan.FromMilliseconds(CASC_TIME); }
        }
        #endregion

        #region Public Methods
        public void Select(params int[] cards)
        {
            _SelectedCards.Clear();
            _SelectedCards.AddRange(cards);
            ArrangeCards();
            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());
        }

        public void Highlight(params int[] cards)
        {
            _HighlightedCards.Clear();
            _HighlightedCards.AddRange(cards);
            ArrangeCards();
            if (HighlightedCardsChanged != null)
                HighlightedCardsChanged(this, new EventArgs());
        }

        public void Dehighlight(params int[] cards)
        {
            foreach (var card in cards)
            {
                if (_HighlightedCards.Contains(card))
                    _HighlightedCards.Remove(card);
                ArrangeCards();
                if (HighlightedCardsChanged != null)
                    HighlightedCardsChanged(this, new EventArgs());
            }
        }

        public void Deselect(params int[] cards)
        {
            foreach (var card in cards)
            {
                if (_SelectedCards.Contains(card))
                    _SelectedCards.Remove(card);
                ArrangeCards();
                if (SelectionChanged != null)
                    SelectionChanged(this, new EventArgs());
            }
        }

        public void CollectSet(int card1, int card2, int card3)
        {
            _DrawnCards.Remove(card1);
            _DrawnCards.Remove(card2);
            _DrawnCards.Remove(card3);
            _ShownSets = null;
            _CollectedSets.Add(new SetPile(card1, card2, card3, new Random().NextDouble() * 360));
            Deselect(card1, card2, card3);
            Cards[card1].SourceImage.Opacity = 1;
            Cards[card2].SourceImage.Opacity = 1;
            Cards[card3].SourceImage.Opacity = 1;
            if (DrawnCardsChanged != null) DrawnCardsChanged(this, new EventArgs());
        }

        public void MissSet(int card1, int card2, int card3)
        {
            _DrawnCards.Remove(card1);
            _DrawnCards.Remove(card2);
            _DrawnCards.Remove(card3);
            _ShownSets = null;
            _MissedSets.Add(new SetPile(card1, card2, card3, new Random().NextDouble() * 360));
            Deselect(card1, card2, card3);
            Cards[card1].SourceImage.Opacity = 1;
            Cards[card2].SourceImage.Opacity = 1;
            Cards[card3].SourceImage.Opacity = 1;
            if (DrawnCardsChanged != null) DrawnCardsChanged(this, new EventArgs());
        }
        #endregion

        #region Events
        public event EventHandler SelectionChanged;
        public event EventHandler HighlightedCardsChanged;
        public event EventHandler DrawnCardsChanged;
        #endregion

        private DateTime LastResizeTime = DateTime.Now;

        const double ANIMATION_TIME = 150;
        const double CASC_TIME = 40;
        const double NON_SEL_DEAL_SCALE = 0.9;
        const double NON_SEL_DEAL_OPACITY = 0.6;

        public DealArranger(Card[] cards, FrameworkElement timeBlock)
        {
            Cards = cards;
            TimeBlock = timeBlock;
            for (int i = 0; i < 81; i++)
            {
                _DrawPile.Add(i);
                Cards[i].SourceImage.Tapped += CardTappedHandlerGenerator(i);
                //Cards[i].SourceImage.PointerReleased += CardClickedHandlerGenerator(i);
                //Cards[i].SetValue(Control.HeightProperty, 225);
                //Cards[i].SetValue(Control.WidthProperty, 150);
                Animations[i, 0] = new DoubleAnimation() { From = null, To = Card.WIDTH * 0.75, Duration = CurrentAnimationTime, EnableDependentAnimation = true };
                Animations[i, 1] = new DoubleAnimation() { From = null, To = Card.HEIGHT * 0.75, Duration = CurrentAnimationTime, EnableDependentAnimation = true };
                Animations[i, 2] = new DoubleAnimation() { From = null, To = 0, Duration = CurrentAnimationTime, EnableDependentAnimation = true };
                Animations[i, 3] = new DoubleAnimation() { From = null, To = 1, Duration = CurrentAnimationTime, EnableDependentAnimation = true };

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
                Cards[majorIndex].FlipTo(FaceUpCards[majorIndex], InstantDeal);
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
            if (!DrawnCards.Contains(cardNumber))
                return;
            List<int> selection = SelectedCards.ToList();
            if (SelectedCards.Contains(cardNumber))
                selection.Remove(cardNumber);
            else
                selection.Add(cardNumber);
            Select(selection.ToArray());
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

        private void RefreshSets()
        {
            _ShownSets = SetHelper.FindSets(DrawnCards.ToArray());
        }

        public void DrawCards(int numCards)
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
            if (numCards > DrawPile.Count)
                numCards = DrawPile.Count;
            for (int i = 0; i < numCards; i++)
            {
                _DrawnCards.Add(_DrawPile[0]);
                _DrawPile.RemoveAt(0);
            }
            _ShownSets = null;
            if (DrawnCardsChanged != null) DrawnCardsChanged(this, new EventArgs());
            ArrangeCards(true, CurrentAnimationTime, CurrentAnimationTime + CurrentAnimationTime);
            Animate();
        }

        private void ArrangeCards()
        {
            ArrangeCards(false, CurrentAnimationTime, CurrentAnimationTime  + CurrentAnimationTime);
        }

        private void ArrangeCards(bool cascade, TimeSpan moveDuration, TimeSpan collectDuration)
        {
            if (SuspendRender)
                return;
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
            for (int i = 1; i <= _DrawnCards.Count; i++)
            {
                dealColumns = i;
                dealRows = (int)Ceiling((double)_DrawnCards.Count / (double)dealColumns);
                scale = Min(dealRegion.Width / dealColumns * NON_SEL_DEAL_SCALE / Card.WIDTH, dealRegion.Height / dealRows * NON_SEL_DEAL_SCALE / Card.HEIGHT);
                columnScales.Add(scale);
            }

            if (columnScales.Count > 0)
                dealColumns = columnScales.IndexOf(columnScales.Max()) + 1;
            dealRows = (int)Ceiling((double)_DrawnCards.Count / (double)dealColumns);
            scale = Min(dealRegion.Width / dealColumns * NON_SEL_DEAL_SCALE / Card.WIDTH, dealRegion.Height / dealRows * NON_SEL_DEAL_SCALE / Card.HEIGHT);

            double cellWidth = dealRegion.Width / dealColumns;
            double cellHeight = dealRegion.Height / dealRows;
            TimeSpan beginTime = TimeSpan.Zero;
            for (int i = 0; i < _DrawnCards.Count; i++)
            {
                FaceUpCards[_DrawnCards[i]] = true;
                int x = i % dealColumns;
                int y = i / dealColumns;
                if (!SetAnimationProperties(_DrawnCards[i], dealRegion.X + cellWidth * (x + .5), dealRegion.Y + cellHeight * (y + .5), null, (_SelectedCards.Count > 0) ? _SelectedCards.Contains(_DrawnCards[i]) ? scale / NON_SEL_DEAL_SCALE : scale * NON_SEL_DEAL_SCALE : scale, moveDuration, beginTime) && cascade)
                    beginTime += CurrentCascadeTime;
                if (ShowHighlights)
                    Cards[_DrawnCards[i]].SourceImage.Opacity = HighlightedCards.Contains(_DrawnCards[i]) || SelectedCards.Contains(_DrawnCards[i]) ? 1 : NON_SEL_DEAL_OPACITY;
                else
                    Cards[_DrawnCards[i]].SourceImage.Opacity = (SelectedCards.Count > 0 && !SelectedCards.Contains(_DrawnCards[i])) ? NON_SEL_DEAL_OPACITY : 1;
            }

            double pileScale = pileRegionLength * NON_SEL_DEAL_SCALE / Card.WIDTH;

            Canvas.SetLeft(TimeBlock, (pileRegionLength - TimeBlock.ActualWidth) / 2);
            Canvas.SetTop(TimeBlock, (pileRegionLength - TimeBlock.ActualHeight) / 2);

            for (int i = 0; i < _DrawPile.Count; i++)
            {
                Cards[_DrawPile[i]].SourceImage.Opacity = 1;
                SetAnimationProperties(_DrawPile[i], pileRegionLength / 2, pileRegionLength / 2 /* + TimeBlock.ActualHeight*/, null, pileScale, TimeSpan.Zero, TimeSpan.Zero);
            }

            double pileX = pileOnTop ? Width / 2 : pileRegionLength / 2;
            double pileY = pileOnTop ? pileRegionLength / 2 : Height / 2;

            for (int i = 0; i < _CollectedSets.Count; i++)
            {
                bool casc = cascade;
                casc &= SetAnimationProperties(_CollectedSets[i].Card1, pileX, pileY, _CollectedSets[i].Angle1, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
                casc &= SetAnimationProperties(_CollectedSets[i].Card2, pileX, pileY, _CollectedSets[i].Angle2, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
                casc &= SetAnimationProperties(_CollectedSets[i].Card3, pileX, pileY, _CollectedSets[i].Angle3, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
            }

            pileX = pileOnTop ? Width - pileRegionLength / 2 : pileRegionLength / 2;
            pileY = pileOnTop ? pileRegionLength / 2 : Height - pileRegionLength / 2;

            for (int i = 0; i < _MissedSets.Count; i++)
            {
                bool casc = cascade;
                casc &= SetAnimationProperties(_MissedSets[i].Card1, pileX, pileY, _MissedSets[i].Angle1, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
                casc &= SetAnimationProperties(_MissedSets[i].Card2, pileX, pileY, _MissedSets[i].Angle2, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
                casc &= SetAnimationProperties(_MissedSets[i].Card3, pileX, pileY, _MissedSets[i].Angle3, pileScale * SetPile.SCALE_MULTIPLIER, collectDuration, TimeSpan.Zero);
            }
            Animate();
        }

        private async void ResizeArrange()
        {
            DateTime time = LastResizeTime = DateTime.Now;
            await Task.Delay(20);
            if (time == LastResizeTime)
                ArrangeCards(false, TimeSpan.Zero, TimeSpan.Zero);
        }

        private void Animate()
        {
            MainStoryboard.Begin();
        }

        private bool SetAnimationProperties(int card, double? x, double? y, double? angle, double? scale, TimeSpan duration, TimeSpan beginTime)
        {
            var ani1 = Animations[card, 0];
            var ani2 = Animations[card, 1];
            var ani3 = Animations[card, 2];
            var ani4 = Animations[card, 3];
            //Cards[card].SourceImage.Opacity = 1;
            if ((!x.HasValue || Cards[card].X.VisuallyApproximate(x.Value)) && (!y.HasValue || Cards[card].Y.VisuallyApproximate(y.Value)) && (!angle.HasValue || Cards[card].Angle.VisuallyApproximate(angle.Value)) && (!scale.HasValue || Cards[card].Scale.VisuallyApproximate(scale.Value)))
                return true;
            ani1.BeginTime = beginTime;
            ani1.Duration = duration;
            ani1.To = x;
            ani2.BeginTime = beginTime;
            ani2.Duration = duration;
            ani2.To = y;
            ani3.BeginTime = beginTime;
            ani3.Duration = duration;
            ani3.To = angle;
            ani4.BeginTime = beginTime;
            ani4.Duration = duration;
            ani4.To = scale;

            return false;
        }

        public void ShuffleDrawPile()
        {
            List<int> newPile = new List<int>();
            newPile.AddRange(_DrawPile);
            _DrawPile.Clear();
            byte[] buffer = new byte[newPile.Count];
            new Random().NextBytes(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                int index = buffer[i] % newPile.Count;
                Canvas.SetZIndex(Cards[newPile[index]].SourceImage, i);
                _DrawPile.Add(newPile[index]);
                newPile.RemoveAt(index);
            }
        }

        public bool EnsureSetNextDraw()
        {
            int drawCount = DrawnCards.Count >= 12 ? 1 : 12 - DrawnCards.Count;
            Stack<int> checkStack = new Stack<int>(DrawnCards);
            if (DrawPile.Count == 0)
                return false;
            int index = 0;
            while (index < DrawnCards.Count)
            {
                if (index + drawCount > DrawnCards.Count)
                    drawCount = DrawnCards.Count - index;
                for (int i = 0; i < drawCount; i++)
                    checkStack.Push(DrawPile[index + i]);
                if (SetHelper.FindSets(checkStack.ToArray()).Length > 0)
                {
                    if (index == 0)
                        return true;
                    int tempCard;
                    for (int i = 0; i < drawCount; i++)
                    {
                        tempCard = DrawPile[i];
                        _DrawPile[i] = DrawPile[index + i];
                        _DrawPile[index + i] = tempCard;
                        Canvas.SetZIndex(Cards[DrawPile[index + i]].SourceImage, index + i);
                        Canvas.SetZIndex(Cards[DrawPile[i]].SourceImage, i);
                    }
                    return true;
                }
                index += drawCount;
            }
            return false;
        }
    }
}
