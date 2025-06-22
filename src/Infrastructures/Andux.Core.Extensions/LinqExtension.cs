namespace Andux.Core.Extensions
{
    /// <summary>
    /// LINQ扩展方法
    /// </summary>
    public static class LinqExtension
    {
        #region 集合操作扩展

        /// <summary>
        /// 如果条件为true，则应用指定的Where条件
        /// </summary>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 如果条件为true，则应用指定的Where条件
        /// </summary>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// 如果值不为null或空字符串，则应用Where条件
        /// </summary>
        public static IEnumerable<T> WhereNotNullOrEmpty<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            return source.Where(x => !string.IsNullOrEmpty(selector(x)));
        }

        /// <summary>
        /// 对集合执行指定操作（用于链式调用）
        /// </summary>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// 检查集合是否为null或空
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// 将集合转换为HashSet
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        #endregion

        #region 分页扩展

        /// <summary>
        /// 分页查询
        /// </summary>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 分页查询（返回分页结果对象）
        /// </summary>
        public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Paginate(pageNumber, pageSize).ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };
        }

        public class PagedResult<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        #endregion

        #region 排序扩展

        /// <summary>
        /// 动态排序（根据属性名升序）
        /// </summary>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        /// <summary>
        /// 动态排序（根据属性名降序）
        /// </summary>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static System.Linq.Expressions.Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var property = System.Linq.Expressions.Expression.Property(parameter, propertyName);
            var propAsObject = System.Linq.Expressions.Expression.Convert(property, typeof(object));

            return System.Linq.Expressions.Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }

        #endregion

        #region 条件扩展

        /// <summary>
        /// 如果条件为true，则执行Select转换
        /// </summary>
        public static IEnumerable<TResult> SelectIf<TSource, TResult>(
            this IEnumerable<TSource> source,
            bool condition,
            Func<TSource, TResult> trueSelector,
            Func<TSource, TResult> falseSelector)
        {
            return condition ? source.Select(trueSelector) : source.Select(falseSelector);
        }

        /// <summary>
        /// 对集合中的每个元素执行操作并返回新集合
        /// </summary>
        public static IEnumerable<TResult> Map<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> mapper)
        {
            return source.Select(mapper);
        }

        #endregion

        #region 聚合扩展

        /// <summary>
        /// 计算集合中满足条件的元素百分比
        /// </summary>
        public static double Percentage<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var count = source.Count();
            return count == 0 ? 0 : source.Count(predicate) * 100.0 / count;
        }

        /// <summary>
        /// 计算数值集合的标准差
        /// </summary>
        public static double StdDev<T>(this IEnumerable<T> source, Func<T, double> selector)
        {
            var values = source.Select(selector).ToList();
            var avg = values.Average();
            var sum = values.Sum(d => Math.Pow(d - avg, 2));
            return Math.Sqrt(sum / values.Count);
        }

        #endregion

        #region 集合比较

        /// <summary>
        /// 查找两个集合的交集（基于键选择器）
        /// </summary>
        public static IEnumerable<T> IntersectBy<T, TKey>(
            this IEnumerable<T> first,
            IEnumerable<T> second,
            Func<T, TKey> keySelector)
        {
            return first.Join(second, keySelector, keySelector, (x, y) => x);
        }

        /// <summary>
        /// 查找两个集合的差集（基于键选择器）
        /// </summary>
        public static IEnumerable<T> ExceptBy<T, TKey>(
            this IEnumerable<T> first,
            IEnumerable<T> second,
            Func<T, TKey> keySelector)
        {
            var secondKeys = new HashSet<TKey>(second.Select(keySelector));
            return first.Where(x => !secondKeys.Contains(keySelector(x)));
        }

        #endregion

        #region 空集合处理

        /// <summary>
        /// 如果集合为null则返回空集合
        /// </summary>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 将集合分批处理（每批固定大小）
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        /// <summary>
        /// 对集合中的每个元素执行异步操作（并行度可控）
        /// </summary>
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> action,
            int maxDegreeOfParallelism = 4)
        {
            var tasks = new List<Task>();
            using (var semaphore = new SemaphoreSlim(maxDegreeOfParallelism))
            {
                foreach (var item in source)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await action(item);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
        }

        #endregion
    }
}
