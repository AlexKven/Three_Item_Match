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
using static System.Math;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Three_Item_Match
{
    public sealed partial class ScaleDownControl : ContentControl
    {
        public ScaleDownControl()
        {
            this.InitializeComponent();
            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        Grid MainGrid;
        ScaleTransform Scale;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MainGrid = HelperFunctions.FindControl<Grid>(this, "MainGrid");
            Scale = (ScaleTransform)MainGrid.RenderTransform;
        }

        private double _ContentMinWidth;
        private double _ContentMinHeight;

        public double ContentMinWidth
        {
            get { return _ContentMinWidth; }
            set
            {
                _ContentMinWidth = value;
                RefreshScale();
            }
        }
        public double ContentMinHeight
        {
            get { return _ContentMinHeight; }
            set
            {
                _ContentMinHeight = value;
                RefreshScale();
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshScale();
        }

        private void RefreshScale()
        {
            if (MainGrid == null) return;
            if (MainGrid.ActualWidth < 1 || MainGrid.ActualHeight < 1)
                return;
            if (ContentMinWidth > MainGrid.ActualWidth || ContentMinHeight > MainGrid.ActualHeight)
            {
                Scale.ScaleX = Scale.ScaleY = Min(MainGrid.ActualHeight / ContentMinHeight, MainGrid.ActualWidth / ContentMinWidth);
                //MainGrid.Width = ActualWidth / Scale.ScaleX;
                //MainGrid.Height = ActualHeight / Scale.ScaleY;
            }
            else
            {
                Scale.ScaleX = Scale.ScaleY = 1;
            }
        }
    }
}
