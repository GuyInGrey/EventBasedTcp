using System.Text;

namespace EventBasedTCP
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string s)
            => Encoding.ASCII.GetBytes(s);

        public static string GetString(this byte[] b)
            => Encoding.ASCII.GetString(b);
    }
}