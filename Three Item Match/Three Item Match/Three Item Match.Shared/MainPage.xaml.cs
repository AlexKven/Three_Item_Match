using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            for (int i = 0; i < 81; i++)
            {
                Cards[i] = new Card() { Face = CardFace.FromInt(i) };
                MainCanvas.Children.Add(Cards[i]);
            }
            //Dealer = new DealArranger(Cards);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //await Dealer.DealCards(81);
            await HelperFunctions.SaveImage(await Cards[80].GetImage(), "Back");
            for (int i = 80; i >= 0; i--)
            {
                Cards[i].FaceUp = true;
                await HelperFunctions.SaveImage(await Cards[i].GetImage(), "Card" + i.ToString());
                MainCanvas.Children.Remove(Cards[i]);
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (MainCanvas.ActualWidth > 0 && MainCanvas.ActualHeight > 0)
            //{
            //    Dealer.Width = MainCanvas.ActualWidth;
            //    Dealer.Height = MainCanvas.ActualHeight;
            //}
        }
    }
}
