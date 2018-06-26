using System.Collections.Generic;

namespace DigitalHealth.StoreAndForward.Core
{
    /// <summary>
    /// Paged list of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// Total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Limit.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="total"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        public PagedList(IList<T> items, int total, int offset, int limit)
        {
            Total = total;
            Offset = offset;
            Limit = limit;

            AddRange(items);
        }
    }
}