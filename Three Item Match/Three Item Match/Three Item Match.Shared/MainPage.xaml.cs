﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Three_Item_Match.Common;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Three_Item_Match
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Navigation Friendliness
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();


        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                Manager.SetSate(e.PageState);
                if ((bool)e.PageState["GameOver"])
                    ShowGameOver();
                else
                    Manager.Start();
                CurrentTime = TimeSpan.FromSeconds((double)e.PageState["CurrentTimeSeconds"]);
            }
            SetCurrentScreen();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            Manager.GetState(e.PageState);
            e.PageState.Add("GameOver", !Manager.IsInGame);
        }

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        Card[] Cards = new Card[81];
        private DealArranger Dealer;
        private GameManager Manager;
        private Control AppBar;
        private DispatcherTimer CounterTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        private TimeSpan CurrentTime = TimeSpan.Zero;
        bool ConcludedGame = false;

        public MainPage()
        {
            this.InitializeComponent();
            
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            CounterTimer.Tick += (e, s) => IncrementTimer();
            CounterTimer.Start();

            SetPlatformSpecificUI();

            for (int i = 0; i < 81; i++)
            {
                Image image = new Image();
                Cards[i] = new Card(image, CardFace.FromInt(i));
                MainCanvas.Children.Add(image);
            }
            Dealer = new DealArranger(Cards, TimeBlockGrid);
            Manager = new GameManager(Dealer);
            Manager.GameEnded += Manager_GameEnded;

            SetCurrentScreen();
        }

        private void Manager_GameEnded(object sender, EventArgs e)
        {
            SetCurrentScreen();
        }

        private void IncrementTimer()
        {
            IncrementTimer(TimeSpan.FromSeconds(1));
        }

        private void IncrementTimer(TimeSpan time)
        {
            if (!Manager.IsInGame)
                return;
            CurrentTime += time;
            Manager.CurrentTime = CurrentTime;
            TimeBlock.Text = CurrentTime.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dealer.DrawCards(1);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainCanvas.ActualWidth > 0 && MainCanvas.ActualHeight > 0)
            {
                Dealer.Width = MainCanvas.ActualWidth;
                Dealer.Height = MainCanvas.ActualHeight;
            }
        }

        private void Button12_Click(object sender, RoutedEventArgs e)
        {
            Dealer.DrawCards(12);
        }

        private void Pause()
        {
            Manager.IsPaused = true;
            SetCurrentScreen();
            CounterTimer.Stop();
        }

        private void Unpause()
        {
            Manager.IsPaused = false;
            SetCurrentScreen();
            IncrementTimer();
            CounterTimer.Start();
        }

        private bool IsButtonEnabled(int i) => i == 0 ? Manager.CallNoSetsEnabled : i == 2 ? Manager.HintEnabled : true;

        private void SetPlatformSpecificUI()
        {
            Color pageBG = (Color)App.Current.Resources["BGColor"];
            Color appbarBG = Color.FromArgb(255, (byte)((double)pageBG.R * 13 / 9), (byte)((double)pageBG.G * 13 / 9), (byte)((double)pageBG.B * 13 / 9));
            List<ICommandBarElement> buttons = new List<ICommandBarElement>()
            {
                new AppBarButton() { Label = "No Sets?", Icon = new BitmapIcon() {UriSource = new Uri("ms-appx:///Assets/NoSetsIcon.png") } },
                new AppBarToggleButton() { Label = "Pause", Icon = new SymbolIcon(Symbol.Pause) },
                new AppBarButton() { Label = "Hint", Icon = new BitmapIcon() {UriSource = new Uri("ms-appx:///Assets/HintIcon.png") } },
                new AppBarButton() { Label = "New Game", Icon = new SymbolIcon(Symbol.Refresh) },
                new AppBarButton() { Label = "Settings", Icon = new SymbolIcon(Symbol.Setting) },
                new AppBarButton() { Label = "Help", Icon = new SymbolIcon(Symbol.Help) },
                new AppBarButton() { Label = "Statistics", Icon = new SymbolIcon(Symbol.List) }
            };

            ((AppBarButton)buttons[0]).Click += (s, e) => Manager.CallNoSets();
            ((AppBarToggleButton)buttons[1]).Checked += (s, e) => Pause();
            ((AppBarToggleButton)buttons[1]).Unchecked += (s, e) => Unpause();
            ((AppBarButton)buttons[2]).Click += (s, e) => Manager.RequestHint();
            ((AppBarButton)buttons[3]).Click += (s, e) => VerifyNewGame();
            ((AppBarButton)buttons[4]).Click += (s, e) => Frame.Navigate(typeof(SettingsPage));
            ((AppBarButton)buttons[6]).Click += (s, e) => Frame.Navigate(typeof(StatsPage));
#if WINDOWS_UWP
            buttons.Insert(4, new AppBarToggleButton() { Label = "Fullscreen", Icon = new SymbolIcon(Symbol.FullScreen) });
            ((AppBarToggleButton)buttons[4]).Checked += FullscreenButton_Checked;
            ((AppBarToggleButton)buttons[4]).Unchecked += FullscreenButton_Unchecked;
            ((AppBarToggleButton)buttons[4]).SetBinding(AppBarToggleButton.IsCheckedProperty, new Binding() { Source = Manager, Path = new PropertyPath("IsPaused"), Mode = BindingMode.TwoWay });
#endif
#if WINDOWS_APP
            MainGrid.RowDefinitions[1].Height = GridLength.Auto;
            Rectangle background = new Rectangle() { Fill = new SolidColorBrush(appbarBG) };
            Grid.SetRow(background, 1);
            MainGrid.Children.Add(background);
            ScrollViewer controlViewer = new ScrollViewer() { VerticalScrollMode = ScrollMode.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden, HorizontalScrollMode = ScrollMode.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch };
            AppBar = controlViewer;
            StackPanel controlPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            Grid.SetRow(controlViewer, 1);
            controlViewer.Content = controlPanel;
            MainGrid.Children.Add(controlViewer);
            foreach (var btn in buttons)
            {
                controlPanel.Children.Add((UIElement)btn);
            }
#else
            CommandBar bar = new CommandBar() { Background = new SolidColorBrush(appbarBG) };
            AppBar = bar;
            BottomAppBar = bar;
            foreach (var btn in buttons)
                bar.PrimaryCommands.Add(btn);
            bar.SizeChanged += (s, e) =>
            {
#if WINDOWS_PHONE_APP
                double hiddenButtons = buttons.Count - 4;
#else
                double sizeDiscrepancy = 48 + 68 * buttons.Count - e.NewSize.Width;
                if (sizeDiscrepancy < 0)
                    sizeDiscrepancy = 0;
                int hiddenButtons = (int)Math.Ceiling(sizeDiscrepancy / 68);
#endif
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons.Count - i <= hiddenButtons)
                    {
                        if (bar.PrimaryCommands.Contains(buttons[i]) && !bar.SecondaryCommands.Contains(buttons[i]))
                        {
                            bar.PrimaryCommands.Remove(buttons[i]);
                            bar.SecondaryCommands.Add(buttons[i]);
                        }
                        ((ButtonBase)buttons[i]).IsEnabled = IsButtonEnabled(i);
                    }
                    else
                    {
                        if (!bar.PrimaryCommands.Contains(buttons[i]) && bar.SecondaryCommands.Contains(buttons[i]))
                        {
                            bar.SecondaryCommands.Remove(buttons[i]);
                            bar.PrimaryCommands.Add(buttons[i]);
                        }
                        ((ButtonBase)buttons[i]).IsEnabled = IsButtonEnabled(i);
                    }
                }
            };
#endif
            }

        private void FullscreenButton_Unchecked(object sender, RoutedEventArgs e)
        {
#if WINDOWS_UWP
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
#endif
        }
        
        private void FullscreenButton_Checked(object sender, RoutedEventArgs e)
        {
#if WINDOWS_UWP
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
#endif
        }

        private void NewGame()
        {
            Manager.Reset((IncorrectSetBehavior)SettingsManager.GetSetting<int>("IncorrectBehavior", false, 0),
                            SettingsManager.GetSetting<bool>("AutoDeal", false, false),
                            SettingsManager.GetSetting<bool>("EnsureSets", false, false),
                            SettingsManager.GetSetting<bool>("PenaltyOnDealWithSets", false, false),
                            SettingsManager.GetSetting<bool>("TrainingMode", false, false),
                            SettingsManager.GetSetting<bool>("DrawThree", false, true),
                            SettingsManager.GetSetting<bool>("InstantDeal", false, false));
            CurrentTime = TimeSpan.Zero;
            Manager.Start();
            SetCurrentScreen();
        }

        private async void VerifyNewGame()
        {
            if (Manager.IsInGame)
            {
                bool result = false;
                MessageDialog verifyDialog = new MessageDialog("There is a game in progress. Starting a new game will end this one and record its progress in the database.", "End Current Game?");
                verifyDialog.Commands.Add(new UICommand() { Label = "Finish Current Game" });
                verifyDialog.Commands.Add(new UICommand() { Invoked = (e) => result = true, Label = "Start New Game" });
                await verifyDialog.ShowAsync();
                if (result)
                    NewGame();
            }
            else
                NewGame();
        }

        private void ShowGameOver()
        {

        }

        private void SetCurrentScreen()
        {
            if (Manager.IsInGame)
            {
                MainCanvas.Visibility = Visibility.Visible;
                WelcomeGrid.Visibility = Visibility.Collapsed;
                GameOverGrid.Visibility = Visibility.Collapsed;
                PauseGrid.Visibility = Manager.IsPaused ? Visibility.Visible : Visibility.Collapsed;
                foreach (var crd in Dealer.DrawnCards)
                    Cards[crd].SourceImage.Visibility = Manager.IsPaused ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                if (ConcludedGame)
                {
                    MainCanvas.Visibility = Visibility.Collapsed;
                    GameOverGrid.Visibility = Visibility.Visible;
                    WelcomeGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainCanvas.Visibility = Visibility.Collapsed;
                    GameOverGrid.Visibility = Visibility.Collapsed;
                    WelcomeGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void GameOverGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }
    }
}
