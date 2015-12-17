using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Three_Item_Match
{
    public class GameManager : INotifyPropertyChanged
    {
        private readonly DealArranger Dealer;
        private readonly bool MissOnIncorrect;
        private readonly bool AutodealNoSets;
        private readonly bool EnsureSets;
        private readonly bool PenaltyOnDealWithSets;
        private readonly bool TrainingMode;
        private readonly bool DrawThree;
        private readonly bool InstantDeal;

        public GameManager(DealArranger dealer, bool missOnIncorrect, bool autodealNoSets, bool ensureSets, bool penaltyOnDealWithSets, bool trainingMode, bool drawThree, bool instantDeal)
        {
            Dealer = dealer;
            Dealer.ShuffleDrawPile();
            MissOnIncorrect = missOnIncorrect;
            AutodealNoSets = autodealNoSets;
            EnsureSets = ensureSets;
            PenaltyOnDealWithSets = penaltyOnDealWithSets;
            TrainingMode = trainingMode;
            DrawThree = drawThree;
            InstantDeal = instantDeal;

            Dealer.InstantDeal = InstantDeal;
            Dealer.ShowHighlights = TrainingMode;

            Dealer.HighlightedCardsChanged += Dealer_HighlightedCardsChanged;
            Dealer.SelectionChanged += Dealer_SelectionChanged;
            Dealer.DrawnCardsChanged += Dealer_DrawnCardsChanged;
        }

        private bool IsInGame = false;

        private int NumHints = 0;

        private void Dealer_DrawnCardsChanged(object sender, EventArgs e)
        {
            if (!IsInGame) return;
            if (TrainingMode)
                HighlightPossibleSets();
            else
                Dealer.ShowHighlights = false;
            if (Dealer.DrawnCards.Count < 12)
                DrawCards();
            while (AutodealNoSets && Dealer.ShownSets.Count == 0 && Dealer.DrawPile.Count > 0)
                DrawCards();
        }

        private void Dealer_SelectionChanged(object sender, EventArgs e)
        {
            if (!IsInGame) return;
            if (Dealer.SelectedCards.Count() == 3)
            {
                if (SetHelper.IsASet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]))
                    Dealer.CollectSet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]);
                else if (MissOnIncorrect)
                    Dealer.MissSet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]);
                else
                    Dealer.Deselect(Dealer.SelectedCards.ToArray());
            }
            else if (TrainingMode)
                HighlightPossibleSets();
            else
                Dealer.ShowHighlights = false;
        }

        private void Dealer_HighlightedCardsChanged(object sender, EventArgs e)
        {
        }

        private void HighlightPossibleSets()
        {
            var sets = Dealer.ShownSets.ToList();
            if (Dealer.SelectedCards.Count != 0)
            {
                sets.RemoveAll(itm => !itm.Contains(Dealer.SelectedCards.ToArray()));
            }
            List<int> highlightedCards = new List<int>();
            foreach (var crd in Dealer.DrawnCards)
            {
                if (sets.Any(itm => itm.Item1 == crd || itm.Item2 == crd || itm.Item3 == crd))
                    highlightedCards.Add(crd);
            }
            Dealer.ShowHighlights = true;
            Dealer.Highlight(highlightedCards.ToArray());
        }

        private void DrawCards()
        {
            if (Dealer.DrawPile.Count == 0)
                return;
            if (Dealer.DrawnCards.Count < 12)
                Dealer.DealCards(12 - Dealer.DrawnCards.Count);
            else
                Dealer.DealCards(DrawThree ? 3 : 1);
        }

        public void Start()
        {
            IsInGame = true;
            DrawCards();
        }

        private bool _IsPaused = false;
        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                _IsPaused = value;
                OnPropertiesChanged("IsPaused");
            }
        }

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RequestHint()
        {
            HighlightPossibleSets();
            NumHints++;
        }

        public void CallNoSets()
        {
            if (Dealer.ShownSets.Count == 0)
            {
                DrawCards();
            }
            else if (PenaltyOnDealWithSets)
            {
                Dealer.SuspendRender = true;
                var randSet = Dealer.ShownSets[new Random().Next(Dealer.ShownSets.Count)];
                Dealer.MissSet(randSet.Item1, randSet.Item2, randSet.Item3);
                Dealer.SuspendRender = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
