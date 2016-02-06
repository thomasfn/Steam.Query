using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query
{
    internal static class Extensions
    {

        private static readonly IPEndPoint NullEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public static bool IsEmpty(this IPEndPoint ipEndPoint)
        {
            return ipEndPoint.Equals(NullEndPoint);
        }

        public static string GetFilterCollectionString(this IEnumerable<IFilter> filters)
        {
            var filterStrings = new[] { "" }.Concat(filters.Select(x => x.GetFilterString()));
            return string.Join("\\", filterStrings);
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan t)
        {
            await Task.WhenAny(task, Task.Delay(t));

            if (!task.IsCompleted)
                throw new TimeoutException();

            return await task;
        }

    }
}
