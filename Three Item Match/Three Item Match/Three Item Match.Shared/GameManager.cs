using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Three_Item_Match
{
    public enum IncorrectSetBehavior
    {
        Nothing, Miss, EndGame
    }

    public class GameManager
    {
        private enum RenderTask
        {
            None, HighlightChanged, SelectionChanged, DrawnCardsChanged
        }
        private readonly DealArranger Dealer;
        private IncorrectSetBehavior IncorrectBehavior;
        private bool AutoDeal;
        private bool EnsureSets;
        private bool PenaltyOnDealWithSets;
        private bool TrainingMode;
        private bool DrawThree;
        private bool InstantDeal;

        private TimeSpan _CurrentTime;
        private int _CollectedSets = 0;
        private int _MissedSets = 0;
        private int _NumHints = 0;
        private int _DealsWithSets = 0;

        public TimeSpan CurrentTime
        {
            set { _CurrentTime = value; }
        }

        public int CollectedSets
        {
            get { return _CollectedSets; }
        }

        public int MissedSets
        {
            get { return _MissedSets; }
        }

        public int NumHints
        {
            get { return _NumHints; }
        }

        public int DealsWithSets
        {
            get { return _DealsWithSets; }
        }

        public GameManager(DealArranger dealer)
        {
            Dealer = dealer;
            Dealer.ShuffleDrawPile();

            Dealer.HighlightedCardsChanged += Dealer_HighlightedCardsChanged;
            Dealer.SelectionChanged += Dealer_SelectionChanged;
            Dealer.DrawnCardsChanged += Dealer_DrawnCardsChanged;
        }

        public bool IsInGame { get; private set; }

        private void Dealer_DrawnCardsChanged(object sender, EventArgs e)
        {
            Dealer.RenderBeginTransaction();
            try
            {
                if (!IsInGame) return;
                if (TrainingMode)
                    HighlightPossibleSets();
                else
                    Dealer.ShowHighlights = false;
                if (Dealer.DrawnCards.Count < 12)
                    DrawCards();
                if (AutoDeal && Dealer.ShownSets.Count == 0)
                {
                    if (Dealer.DrawPile.Count > 0)
                        DrawCards();
                    else
                        EndGame();
                }
            }
            finally
            {
                Dealer.RenderCommit();
            }
        }

        private void Dealer_SelectionChanged(object sender, EventArgs e)
        {
            Dealer.RenderBeginTransaction();
            try
            {
                if (!IsInGame) return;
                if (Dealer.SelectedCards.Count() == 3)
                {
                    if (SetHelper.IsASet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]))
                    {
                        Dealer.CollectSet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]);
                        _CollectedSets++;
                    }
                    else if (IncorrectBehavior != IncorrectSetBehavior.Nothing)
                    {
                        _MissedSets++;
                        Dealer.MissSet(Dealer.SelectedCards[0], Dealer.SelectedCards[1], Dealer.SelectedCards[2]);
                        if (IncorrectBehavior == IncorrectSetBehavior.EndGame)
                            EndGame();
                    }
                    else
                        Dealer.Deselect(Dealer.SelectedCards.ToArray());
                }
                else if (TrainingMode)
                    HighlightPossibleSets();
                else
                    Dealer.ShowHighlights = false;
            }
            finally
            {
                Dealer.RenderCommit();
            }
        }

        private void Dealer_HighlightedCardsChanged(object sender, EventArgs e)
        {
        }

        private void HighlightPossibleSets()
        {
            Dealer.RenderBeginTransaction();
            try
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
            finally
            {
                Dealer.RenderCommit();
            }
        }

        private void DrawCards()
        {
            Dealer.RenderBeginTransaction();
            try
            {
                if (Dealer.DrawPile.Count == 0)
                    return;
                if (EnsureSets && !Dealer.EnsureSetNextDraw())
                    EndGame();
                if (Dealer.DrawnCards.Count < 12)
                    Dealer.DrawCards(12 - Dealer.DrawnCards.Count);
                else
                    Dealer.DrawCards(DrawThree ? 3 : 1);
            }
            finally
            {
                Dealer.RenderCommit();
            }
        }

        public void Start()
        {
            IsInGame = true;
            if (Dealer.DrawnCards.Count == 0)
            {
                if (EnsureSets)
                    Dealer.EnsureSetNextDraw();
                DrawCards();
            }
            if (AutoDeal && Dealer.ShownSets.Count == 0 && Dealer.DrawPile.Count > 0)
                DrawCards();
        }

        private bool _IsPaused = false;
        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                _IsPaused = value;
            }
        }

        public void RequestHint()
        {
            HighlightPossibleSets();
            _NumHints++;
        }

        public void CallNoSets()
        {
            if (Dealer.ShownSets.Count == 0)
            {
                if (Dealer.DrawPile.Count == 0)
                    EndGame();
                DrawCards();
            }
            else
            {
                _DealsWithSets++;
                if (PenaltyOnDealWithSets)
                {
                    var randSet = Dealer.ShownSets[new Random().Next(Dealer.ShownSets.Count)];
                    Dealer.MissSet(randSet.Item1, randSet.Item2, randSet.Item3);
                    _MissedSets++;
                    Dealer.Render(false);
                }
            }
        }

        public void EndGame()
        {
            IsInGame = false;
            Func<bool, string> boolToStringFunc = value => value ? "T" : "F";
            Archiver.ExecuteSQL(App.DatabaseConnection, $@"insert into GameArchive(TimePlayed, Game, IncorrectBehavior, AutoDeal, EnsureSets, PenaltyOnDealWithSets, TrainingMode, DrawThree, InstantDeal, DurationSeconds, CollectedSets, MissedSets, Hints, DealsWithSets)
                         values(DateTime('now', 'localtime'), 'Set', '{IncorrectBehavior.ToString()}', '{boolToStringFunc(AutoDeal)}', '{boolToStringFunc(EnsureSets)}', '{boolToStringFunc(PenaltyOnDealWithSets)}', '{boolToStringFunc(TrainingMode)}', '{boolToStringFunc(DrawThree)}', '{boolToStringFunc(InstantDeal)}', {((int)_CurrentTime.TotalSeconds).ToString()}, {CollectedSets.ToString()}, {MissedSets.ToString()}, {NumHints.ToString()}, {DealsWithSets});");
            if (GameEnded != null)
                GameEnded(this, new EventArgs());
        }

        public bool HintEnabled
        {
            get { return !TrainingMode; }
        }

        public bool CallNoSetsEnabled
        {
            get { return !AutoDeal; }
        }

        public event EventHandler GameEnded;

        private void SetProperties(IncorrectSetBehavior incorrectBehavior, bool autoDeal, bool ensureSets, bool penaltyOnDealWithSets, bool trainingMode, bool drawThree, bool instantDeal)
        {
            IncorrectBehavior = incorrectBehavior;
            AutoDeal = autoDeal;
            EnsureSets = ensureSets;
            PenaltyOnDealWithSets = penaltyOnDealWithSets;
            TrainingMode = trainingMode;
            DrawThree = drawThree;
            InstantDeal = instantDeal;

            Dealer.InstantDeal = InstantDeal;
            Dealer.ShowHighlights = TrainingMode;
        }

        public void Reset(IncorrectSetBehavior incorrectBehavior, bool autoDeal, bool ensureSets, bool penaltyOnDealWithSets, bool trainingMode, bool drawThree, bool instantDeal)
        {
            Dealer.Reset();
            Dealer.ShuffleDrawPile();
            _CurrentTime = TimeSpan.Zero;
            _NumHints = 0;
            _CollectedSets = 0;
            _MissedSets = 0;
            _DealsWithSets = 0;
            SetProperties(incorrectBehavior, autoDeal, ensureSets, penaltyOnDealWithSets, trainingMode, drawThree, instantDeal);
        }

        public void GetState(Dictionary<string, object> dict)
        {
            Dealer.GetState(dict);
            dict.Add("AutoDeal", AutoDeal);
            dict.Add("EnsureSets", EnsureSets);
            dict.Add("PenaltyOnDealWithSets", PenaltyOnDealWithSets);
            dict.Add("TrainingMode", TrainingMode);
            dict.Add("DrawThree", DrawThree);
            dict.Add("CurrentTimeSeconds", _CurrentTime.TotalSeconds);
            dict.Add("NumHints", NumHints);
            dict.Add("DealsWithSets", DealsWithSets);
        }

        public void SetSate(Dictionary<string, object> dict)
        {
            Dealer.SetSate(dict);
            AutoDeal = (bool)dict["AutoDeal"];
            EnsureSets = (bool)dict["EnsureSets"];
            PenaltyOnDealWithSets = (bool)dict["PenaltyOnDealWithSets"];
            TrainingMode = (bool)dict["TrainingMode"];
            DrawThree = (bool)dict["DrawThree"];
            CurrentTime = TimeSpan.FromSeconds((double)dict["CurrentTimeSeconds"]);
            _NumHints = (int)dict["NumHints"];
            _DealsWithSets = (int)dict["DealsWithSets"];
            _CollectedSets = Dealer.CollectedSets.Count;
            _MissedSets = Dealer.MissedSets.Count;
        }
    }
}
