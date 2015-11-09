using System;
using System.Collections.Generic;
using System.Text;

namespace Three_Item_Match
{
    public struct CardFace
    {
        public CardFace(CardNumber number, CardShape shape, CardColor color, CardFill fill)
        {
            Number = number;
            Shape = shape;
            Color = color;
            Fill = fill;
        }

        public CardNumber Number { get; private set; }
        public CardShape Shape { get; private set; }
        public CardColor Color { get; private set; }
        public CardFill Fill { get; private set; }

        public int ToInt()
        {
            return (int)Shape + 3 * (int)Fill + 9 * (int)Color + 27 * (int)Number;
        }

        public static CardFace FromInt(int value)
        {
            int num = value;
            CardShape shape = (CardShape)(num % 3);
            num /= 3;
            CardFill fill = (CardFill)(num % 3);
            num /= 3;
            CardColor color = (CardColor)(num % 3);
            num /= 3;
            CardNumber number = (CardNumber)(num % 3);
            return new CardFace(number, shape, color, fill);
        }
    }

    public enum CardNumber { Single, Double, Triple }

    public enum CardShape { Diamond, Squiggle, Oval }

    public enum CardColor { Red, Green, Purple }

    public enum CardFill { Solid, Shaded, Empty }
}
