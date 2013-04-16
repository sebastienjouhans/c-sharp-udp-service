

namespace Kinrou.io.Udp
{
    public static class UdpHelper
    {
        public static string START_PROTOCOL = "/00/";
        public static string END_PROTOCOL = "/99/";

        public static bool isDataValid(this string data)
        {
            if (data.Contains(UdpHelper.START_PROTOCOL) && data.Contains(UdpHelper.END_PROTOCOL))
            {
                return true;
            }

            return false;
        }

        public static string trimStartAndEnd(string data)
        {
            data = data.Substring(UdpHelper.START_PROTOCOL.Length, data.IndexOf(UdpHelper.END_PROTOCOL) - UdpHelper.END_PROTOCOL.Length);
            return data;
        }
    }
}
