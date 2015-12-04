using System;
using System.Collections.Generic;
using System.Text;

namespace Three_Item_Match
{
    public struct SetPile
    {
        public const double SCALE_MULTIPLIER = 0.83;

        public SetPile(int card1, int card2, int card3)
        {
            Card1 = card1;
            Card2 = card2;
            Card3 = card3;
            Rand.
        }

        public int Card1 { get; set; }
        public int Card2 { get; set; }
        public int Card3 { get; set; }
        public double Angle1 { get; set; }
        public double Angle2 { get; set; }
        public double Angle3 { get; set; }

        private static Random Rand = new Random();
    }
}
