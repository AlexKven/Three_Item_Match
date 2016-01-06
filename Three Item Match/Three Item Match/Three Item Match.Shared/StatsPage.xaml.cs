using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Three_Item_Match.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Three_Item_Match
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StatsPage : Page
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

        SQLiteConnection connection = Archiver.GetConnection();

        public StatsPage()
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

        public void RunQuery(string query)
        {
            DataGrid.RowDefinitions.Clear();
            DataGrid.ColumnDefinitions.Clear();
            DataGrid.Children.Clear();

            string[] columns;
            int numRows;
            var results = Archiver.ExecuteSQL(connection, query, out columns, out numRows);

            DataGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            if (columns.Length == 0)
                return;

            for (int i = 0; i < columns.Length; i++)
            {
                DataGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                TextBlock propBlock = new TextBlock() { Text = columns[i], Margin = new Thickness(3, 2, 20, 3), MinWidth = 70 };
                Grid.SetColumn(propBlock, i);
                DataGrid.Children.Add(propBlock);
            }

            Rectangle divider = new Rectangle() { Fill = new SolidColorBrush(Colors.White), Height = 1, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Stretch };
            Grid.SetColumnSpan(divider, columns.Length);
            DataGrid.Children.Add(divider);


            object item = null;

            for (int h = 0; h < numRows; h++)
            {
                DataGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                for (int i = 0; i < columns.Length; i++)
                {
                    item = results[i, h];
                    if (item != null)
                    {
                        TextBlock itemBlock = new TextBlock() { Text = item.ToString(), Margin = new Thickness(2, 0, 30, 0) };
                        Grid.SetRow(itemBlock, h + 1);
                        Grid.SetColumn(itemBlock, i);
                        DataGrid.Children.Add(itemBlock);
                    }
                }
            }
        }

        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunQuery(QueryBox.Text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                QueryBox.BorderBrush = new SolidColorBrush(Colors.Red);
                await System.Threading.Tasks.Task.Delay(500);
                QueryBox.SetValue(TextBox.BorderBrushProperty, DependencyProperty.UnsetValue);
            }
        }
    }
}
