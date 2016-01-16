using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Query
{
    internal static class Extensions
    {

        private static readonly IPEndPoint NullEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public static bool IsEmpty(this IPEndPoint ipEndPoint)
        {
            return ipEndPoint.Equals(NullEndPoint);
        }

    }
}
