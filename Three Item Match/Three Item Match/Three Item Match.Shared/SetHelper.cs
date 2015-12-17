using System;
using System.Collections.Generic;
using System.Text;
using static Three_Item_Match.CardColor;
using static Three_Item_Match.CardNumber;
using static Three_Item_Match.CardShape;
using static Three_Item_Match.CardFill;
using System.Linq;

namespace Three_Item_Match
{
    public static class SetHelper
    {
        private static CardColor[] Colors = new CardColor[] { Red, Green, Purple };
        private static CardNumber[] Numbers = new CardNumber[] { One, Two, Three };
        private static CardShape[] Shapes = new CardShape[] { Diamond, Oval, Squiggle };
        private static CardFill[] Fills = new CardFill[] { Solid, Shaded, Empty };

        public static bool IsASet(int card1, int card2, int card3)
        {
            return CompleteSet(card1, card2) == card3;
        }

        public static int CompleteSet(int card1, int card2)
        {
            CardFace face1 = CardFace.FromInt(card1);
            CardFace face2 = CardFace.FromInt(card2);

            CardColor color = (face1.Color == face2.Color) ? face1.Color : Colors.First(itm => face1.Color != itm && face2.Color != itm);
            CardNumber number = (face1.Number == face2.Number) ? face1.Number : Numbers.First(itm => face1.Number != itm && face2.Number != itm);
            CardShape shape = (face1.Shape == face2.Shape) ? face1.Shape : Shapes.First(itm => face1.Shape != itm && face2.Shape != itm);
            CardFill fill = (face1.Fill == face2.Fill) ? face1.Fill : Fills.First(itm => face1.Fill != itm && face2.Fill != itm);

            return new CardFace(number, shape, color, fill).ToInt();
        }

        public static Tuple<int, int, int>[] FindSets(params int[] cards)
        {
            if (cards.Length < 3) return new Tuple<int, int, int>[0];
            List<Tuple<int, int, int>> result = new List<Tuple<int, int, int>>();
            for (int i = 0; i < cards.Length; i++)
            {
                for (int j = i + 1; j < cards.Length; j++)
                {
                    int completion = SetHelper.CompleteSet(cards[i], cards[j]);
                    if (cards.Contains(completion))
                    {
                        SortedSet<int> sorter = new SortedSet<int>();
                        sorter.Add(cards[i]);
                        sorter.Add(cards[j]);
                        sorter.Add(completion);
                        Tuple<int, int, int> set = new Tuple<int, int, int>(sorter.ToArray()[0], sorter.ToArray()[1], sorter.ToArray()[2]);
                        if (!result.Contains(set))
                            result.Add(set);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
