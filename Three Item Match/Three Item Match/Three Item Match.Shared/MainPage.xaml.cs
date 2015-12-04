using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Three_Item_Match
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Card[] Cards = new Card[81];
        private DealArranger Dealer;

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_UWP
            FullScreenButton.Visibility = Visibility.Visible;
#endif

            for (int i = 0; i < 81; i++)
            {
                Image image = new Image();
                Cards[i] = new Card(image, CardFace.FromInt(i));
                MainCanvas.Children.Add(image);
            }
            Dealer = new DealArranger(Cards);
            Dealer.ShuffleDrawPile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dealer.DealCards(1);
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
            Dealer.DealCards(12);
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS_UWP
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
#endif
        }
    }
}
