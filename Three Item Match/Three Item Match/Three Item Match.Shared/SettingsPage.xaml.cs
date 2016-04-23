using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Three_Item_Match.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Three_Item_Match.HelperFunctions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Three_Item_Match
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
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
            IsLoading = true;
            var incorrectSetBehavior = SettingsManager.GetSetting<int>("IncorrectSetBehavior", false, 0);
            IncorrectBehaviorBox.SelectedIndex = incorrectSetBehavior;
            (SettingsManager.GetSetting<bool>("AutoDeal", false, false) ? AutoDealButtonTrue : AutoDealButtonFalse).IsChecked = true;
            (SettingsManager.GetSetting<bool>("EnsureSets", false, false) ? EnsureSetsButtonTrue : EnsureSetsButtonFalse).IsChecked = true;
            PenaltyOnDealWithSetsBox.SelectedIndex = SettingsManager.GetSetting<bool>("PenaltyOnDealWithSets", false, false) ? 1 : 0;
            DrawThreeBox.SelectedIndex = SettingsManager.GetSetting<bool>("DrawThree", false, true) ? 1 : 0;
            TrainingModeBox.IsChecked = SettingsManager.GetSetting<bool>("TrainingMode", false, false);
            InstantDealBox.IsChecked = SettingsManager.GetSetting<bool>("InstantDeal", false, false);
            IsLoading = false;
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

            if (!App.ProvideGuiBackButton)
                BackRow.Height = new GridLength(0);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private bool IsLoading;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AutoDealButtonTrue_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("AutoDeal", false, true);
        }

        private void AutoDealButtonFalse_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("AutoDeal", false, false);
        }

        private void EnsureSetsButtonTrue_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("EnsureSets", false, true);
        }

        private void EnsureSetsButtonFalse_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("EnsureSets", false, false);
        }

        private void IncorrectBehaviorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<int>("IncorrectBehavior", false, IncorrectBehaviorBox.SelectedIndex);
        }

        private void PenaltyOnDealWithSetsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("PenaltyOnDealWithSets", false, PenaltyOnDealWithSetsBox.SelectedIndex == 1);
        }

        private void DrawThreeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("DrawThree", false, DrawThreeBox.SelectedIndex == 1);
        }

        private void TrainingModeBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("TrainingMode", false, true);
        }

        private void TrainingModeBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("TrainingMode", false, false);
        }

        private void InstantDealBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("InstantDeal", false, true);
        }

        private void InstantDealBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
                return;
            SettingsManager.SetSetting<bool>("InstantDeal", false, false);
        }
    }
}
