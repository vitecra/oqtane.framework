namespace Oqtane.Modules.Visualization3D.Models
{
    /// <summary>
    /// Represents a color with RGBA components
    /// </summary>
    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public double A { get; set; }

        public Color() : this(0, 0, 0, 1.0) { }

        public Color(int r, int g, int b, double a = 1.0)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Red => new Color(255, 0, 0);
        public static Color Green => new Color(0, 255, 0);
        public static Color Blue => new Color(0, 0, 255);
        public static Color White => new Color(255, 255, 255);
        public static Color Black => new Color(0, 0, 0);
        public static Color Yellow => new Color(255, 255, 0);
        public static Color Cyan => new Color(0, 255, 255);
        public static Color Magenta => new Color(255, 0, 255);
        public static Color Gray => new Color(128, 128, 128);

        public string ToRgba()
        {
            return $"rgba({R},{G},{B},{A})";
        }

        public string ToHex()
        {
            return $"#{R:X2}{G:X2}{B:X2}";
        }
    }
}
