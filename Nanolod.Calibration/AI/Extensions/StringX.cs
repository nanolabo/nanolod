using System.Linq;

namespace Nanolod.Calibration.Extensions
{
    public class StringX
    {
        public static string RandomLetters(int length)
        {
            return string.Join(string.Empty, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList().Shuffle().Take(length));
        }

        public static string Random(int length)
        {
            return string.Join(string.Empty, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToList().Shuffle().Take(length));
        }
    }
}
