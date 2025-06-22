namespace Andux.Core.Extensions
{
    /// <summary>
    /// 集合类型扩展方法
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 将集合转换为分隔符连接的字符串
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">要转换的集合</param>
        /// <param name="separator">分隔符（默认为逗号）</param>
        /// <returns>连接后的字符串</returns>
        public static string JoinToString<T>(this IEnumerable<T> source, string separator = ",")
            => source == null ? string.Empty : string.Join(separator, source);

        /// <summary>
        /// 对集合中的每个元素执行指定操作
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">要处理的集合</param>
        /// <param name="action">要执行的操作</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null || action == null) return;
            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// 从集合中随机获取一个元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>随机元素，如果集合为空返回 default(T)</returns>
        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source.IsNullOrEmpty()) return default;
            var list = source.ToList();
            return list[new Random().Next(0, list.Count)];
        }

        /// <summary>
        /// 从集合中随机获取指定数量的元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="count">要获取的元素数量</param>
        /// <returns>随机元素集合</returns>
        public static IEnumerable<T> RandomTake<T>(this IEnumerable<T> source, int count)
        {
            if (source.IsNullOrEmpty() || count <= 0) yield break;

            var list = source.ToList();
            var random = new Random();
            for (int i = 0; i < Math.Min(count, list.Count); i++)
            {
                int index = random.Next(0, list.Count);
                yield return list[index];
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// 将集合分页处理
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>分页后的子集合</returns>
        public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int page, int pageSize)
        {
            if (source == null) return Enumerable.Empty<T>();
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 检查集合是否包含任何指定元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="items">要检查的元素集合</param>
        /// <returns>如果包含任意一个元素返回 true，否则返回 false</returns>
        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items)
            => source != null && items != null && items.Any(source.Contains);

        /// <summary>
        /// 检查集合是否包含所有指定元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="items">要检查的元素集合</param>
        /// <returns>如果包含所有元素返回 true，否则返回 false</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> items)
            => source != null && items != null && items.All(source.Contains);

        /// <summary>
        /// 将集合转换为 HashSet
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>HashSet 实例</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
            => new HashSet<T>(source);

        /// <summary>
        /// 根据指定条件去重
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TKey">去重依据的属性类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">去重依据的属性选择器</param>
        /// <returns>去重后的集合</returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// 将集合转换为只读集合
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>只读集合</returns>
        public static IReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> source)
            => source.ToList().AsReadOnly();

        /// <summary>
        /// 向字典安全添加键值对（如果键已存在则更新）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">目标字典</param>
        /// <param name="key">要添加的键</param>
        /// <param name="value">要添加的值</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// 获取字典中的值，如果键不存在返回默认值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">源字典</param>
        /// <param name="key">要查找的键</param>
        /// <param name="defaultValue">默认值（可选）</param>
        /// <returns>找到的值或默认值</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
            => dictionary.TryGetValue(key, out var value) ? value : defaultValue;

        /// <summary>
        /// 将两个集合合并为一个新集合
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <returns>合并后的新集合</returns>
        public static IEnumerable<T> UnionWith<T>(this IEnumerable<T> first, IEnumerable<T> second)
            => first.Union(second);

        /// <summary>
        /// 将元素添加到集合的开头
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="item">要添加的元素</param>
        /// <returns>新集合</returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
            => new[] { item }.Concat(source);

        /// <summary>
        /// 将元素添加到集合的末尾
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="item">要添加的元素</param>
        /// <returns>新集合</returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
            => source.Concat(new[] { item });

        /// <summary>
        /// 将集合分批处理
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">每批大小</param>
        /// <returns>分批后的集合</returns>
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
        /// 计算集合的加权平均值
        /// </summary>
        /// <param name="source">包含值的集合</param>
        /// <param name="weights">包含权重的集合</param>
        /// <returns>加权平均值</returns>
        public static double WeightedAverage(this IEnumerable<double> source, IEnumerable<double> weights)
        {
            var values = source.ToArray();
            var weightArray = weights.ToArray();

            if (values.Length != weightArray.Length)
                throw new ArgumentException("值和权重的数量必须相同");

            double sum = 0;
            double weightSum = 0;

            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i] * weightArray[i];
                weightSum += weightArray[i];
            }

            return sum / weightSum;
        }

        /// <summary>
        /// 获取两个集合的差异项（存在于source但不存在于target的项）
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="target">对比集合</param>
        /// <returns>差异项集合</returns>
        public static IEnumerable<T> ExceptBy<T, TKey>(
            this IEnumerable<T> source,
            IEnumerable<T> target,
            Func<T, TKey> keySelector)
        {
            var targetKeys = new HashSet<TKey>(target.Select(keySelector));
            return source.Where(item => !targetKeys.Contains(keySelector(item)));
        }

        /// <summary>
        /// 将集合转换为字典（自动处理重复键）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">键选择器</param>
        /// <param name="valueSelector">值选择器</param>
        /// <param name="duplicateHandler">重复键处理回调（可选）</param>
        /// <returns>生成的字典</returns>
        public static Dictionary<TKey, TValue> ToDictionarySafe<T, TKey, TValue>(
            this IEnumerable<T> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector,
            Action<TKey, TValue, TValue> duplicateHandler = null)
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                var value = valueSelector(item);
                if (dict.TryGetValue(key, out var existingValue))
                {
                    duplicateHandler?.Invoke(key, existingValue, value);
                }
                else
                {
                    dict.Add(key, value);
                }
            }
            return dict;
        }

        /// <summary>
        /// 按条件分组后转换为字典（一对多关系）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <typeparam name="TKey">分组键类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">分组依据</param>
        /// <returns>分组字典</returns>
        public static Dictionary<TKey, List<T>> GroupToDictionary<T, TKey>(
            this IEnumerable<T> source,
            Func<T, TKey> keySelector)
        {
            return source
                .GroupBy(keySelector)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// 线程安全地添加元素到集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">目标集合</param>
        /// <param name="item">要添加的元素</param>
        public static void AddLocked<T>(this ICollection<T> collection, T item)
        {
            lock (collection)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// 递归展开树形结构
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="source">根节点集合</param>
        /// <param name="childrenSelector">子节点选择器</param>
        /// <returns>平铺后的所有节点</returns>
        public static IEnumerable<T> FlattenTree<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> childrenSelector)
        {
            foreach (var item in source)
            {
                yield return item;
                foreach (var child in childrenSelector(item).FlattenTree(childrenSelector))
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// 批量移除满足条件的元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">目标集合</param>
        /// <param name="predicate">移除条件</param>
        /// <returns>被移除的元素数量</returns>
        public static int RemoveAll<T>(
            this ICollection<T> collection,
            Func<T, bool> predicate)
        {
            var removedItems = collection.Where(predicate).ToList();
            foreach (var item in removedItems)
            {
                collection.Remove(item);
            }
            return removedItems.Count;
        }

        /// <summary>
        /// 若集合为null则返回空集合
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>非空集合</returns>
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
            => source ?? Enumerable.Empty<T>();

        /// <summary>
        /// 获取元素在集合中的索引
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>索引值（未找到返回-1）</returns>
        public static int IndexOf<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item)) return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// 深度合并两个字典（嵌套字典递归合并）
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="target">目标字典</param>
        /// <param name="source">源字典</param>
        public static void DeepMerge<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source)
        {
            foreach (var pair in source)
            {
                if (target.TryGetValue(pair.Key, out var existingValue) &&
                    existingValue is IDictionary<TKey, TValue> existingDict &&
                    pair.Value is IDictionary<TKey, TValue> newDict)
                {
                    existingDict.DeepMerge(newDict);
                }
                else
                {
                    target[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// 验证集合是否满足指定条件（支持自定义错误消息）
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">验证条件</param>
        /// <param name="errorMessage">错误消息模板</param>
        /// <exception cref="ArgumentException">验证失败时抛出</exception>
        public static void Validate<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate,
            string errorMessage = "集合包含无效项")
        {
            var invalidItems = source.Where(x => !predicate(x)).ToList();
            if (invalidItems.Any())
            {
                throw new ArgumentException(
                    $"{errorMessage}。无效项：{string.Join(",", invalidItems.Take(3))}");
            }
        }

    }
}
