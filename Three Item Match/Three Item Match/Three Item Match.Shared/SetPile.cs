using System;
using System.Collections.Generic;
using System.Text;

namespace Three_Item_Match
{
    public struct SetPile
    {
        public const double SCALE_MULTIPLIER = 0.75;

        public SetPile(int card1, int card2, int card3, double startAngle)
        {
            Card1 = card1;
            Card2 = card2;
            Card3 = card3;
            Angle1 = startAngle;
            Angle2 = Angle1 + 5;
            Angle3 = Angle2 + 5;
        }

        public int Card1 { get; set; }
        public int Card2 { get; set; }
        public int Card3 { get; set; }
        public double Angle1 { get; set; }
        public double Angle2 { get; set; }
        public double Angle3 { get; set; }
    }
}
