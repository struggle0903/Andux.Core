namespace Andux.Core.Extensions
{
    /// <summary>
    /// 日期时间扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        #region 时间戳转换
        /// <summary>
        /// 将日期转换为Unix时间戳（秒）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>Unix时间戳</returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
            => (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
       
        /// <summary>
        /// 将Unix时间戳转换为DateTime
        /// </summary>
        /// <param name="timestamp">Unix时间戳（秒）</param>
        /// <returns>DateTime对象</returns>
        public static DateTime FromUnixTimestamp(this long timestamp)
            => DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
        #endregion

        #region 时间范围获取
        /// <summary>
        /// 获取当天开始时间（00:00:00）
        /// </summary>
        public static DateTime StartOfDay(this DateTime dateTime)
            => dateTime.Date;

        /// <summary>
        /// 获取当天结束时间（23:59:59.999）
        /// </summary>
        public static DateTime EndOfDay(this DateTime dateTime)
            => dateTime.Date.AddDays(1).AddTicks(-1);

        /// <summary>
        /// 获取当月第一天
        /// </summary>
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
            => new DateTime(dateTime.Year, dateTime.Month, 1);

        /// <summary>
        /// 获取当月最后一天
        /// </summary>
        public static DateTime LastDayOfMonth(this DateTime dateTime)
            => dateTime.FirstDayOfMonth().AddMonths(1).AddDays(-1);

        /// <summary>
        /// 获取季度范围
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns>(季度开始日期, 季度结束日期)</returns>
        public static (DateTime start, DateTime end) GetQuarterRange(this DateTime dateTime)
        {
            var quarter = (dateTime.Month - 1) / 3 + 1;
            var startMonth = (quarter - 1) * 3 + 1;
            var startDate = new DateTime(dateTime.Year, startMonth, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            return (startDate, endDate);
        }
        #endregion

        #region 实用计算
        /// <summary>
        /// 计算年龄（根据生日）
        /// </summary>
        /// <param name="birthDate">出生日期</param>
        /// <param name="referenceDate">参考日期（默认为当前日期）</param>
        /// <returns>年龄</returns>
        public static int CalculateAge(this DateTime birthDate, DateTime? referenceDate = null)
        {
            var refDate = referenceDate ?? DateTime.Today;
            var age = refDate.Year - birthDate.Year;
            if (birthDate.Date > refDate.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// 获取工作日（跳过周末）
        /// </summary>
        /// <param name="dateTime">起始日期</param>
        /// <param name="days">要添加的工作日数</param>
        public static DateTime AddBusinessDays(this DateTime dateTime, int days)
        {
            var sign = Math.Sign(days);
            var remainingDays = Math.Abs(days);
            while (remainingDays > 0)
            {
                dateTime = dateTime.AddDays(sign);
                if (dateTime.DayOfWeek != DayOfWeek.Saturday &&
                    dateTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    remainingDays--;
                }
            }
            return dateTime;
        }

        /// <summary>
        /// 检查日期是否在指定范围内
        /// </summary>
        /// <param name="dateTime">要检查的日期</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="inclusive">是否包含边界</param>
        public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate, bool inclusive = true)
            => inclusive
                ? dateTime >= startDate && dateTime <= endDate
                : dateTime > startDate && dateTime < endDate;
        #endregion

        #region 格式化输出
        /// <summary>
        /// 获取星期几的中文描述（如"星期一"）
        /// </summary>
        public static string ToChineseDayOfWeek(this DateTime dateTime)
        {
            string[] chineseDays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            return chineseDays[(int)dateTime.DayOfWeek];
        }

        /// <summary>
        /// 获取星期几的简短中文描述（如"周一"）
        /// </summary>
        public static string ToShortChineseDayOfWeek(this DateTime dateTime)
        {
            string[] chineseDays = { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
            return chineseDays[(int)dateTime.DayOfWeek];
        }

        /// <summary>
        /// 转换为友好时间描述（如"3分钟前"）
        /// </summary>
        public static string ToFriendlyRelativeTime(this DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalSeconds < 60) return $"{span.Seconds}秒前";
            if (span.TotalMinutes < 60) return $"{span.Minutes}分钟前";
            if (span.TotalHours < 24) return $"{span.Hours}小时前";
            if (span.TotalDays < 30) return $"{span.Days}天前";
            if (span.TotalDays < 365) return $"{dateTime:MM-dd}";
            return $"{dateTime:yyyy-MM-dd}";
        }

        /// <summary>
        /// 转换为财务年度季度表示（如2023Q1）
        /// </summary>
        public static string ToFinancialQuarter(this DateTime dateTime)
            => $"{dateTime.Year}Q{(dateTime.Month - 1) / 3 + 1}";

        /// <summary>
        /// 转换为中国农历日期
        /// </summary>
        public static string ToChineseLunarDate(this DateTime dateTime)
        {
            var chineseCalendar = new System.Globalization.ChineseLunisolarCalendar();
            var year = chineseCalendar.GetYear(dateTime);
            var month = chineseCalendar.GetMonth(dateTime);
            var day = chineseCalendar.GetDayOfMonth(dateTime);

            // 处理闰月
            var isLeapMonth = month > 12;
            month = isLeapMonth ? month - 12 : month;

            return $"{year}年{(isLeapMonth ? "闰" : "")}{month}月{day}日";
        }

        /// <summary>
        /// 转换为ISO 8601格式字符串
        /// </summary>
        public static string ToIso8601String(this DateTime dateTime)
            => dateTime.ToString("o");
        #endregion

        #region 集合操作
        /// <summary>
        /// 获取两个日期之间的所有日期
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="includeWeekend">是否包含周末</param>
        public static IEnumerable<DateTime> GetDateRange(
            this DateTime startDate,
            DateTime endDate,
            bool includeWeekend = true)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (includeWeekend ||
                    (date.DayOfWeek != DayOfWeek.Saturday &&
                     date.DayOfWeek != DayOfWeek.Sunday))
                {
                    yield return date;
                }
            }
        }
        #endregion

        #region 判断操作
        /// <summary>
        /// 判断日期是否是今天
        /// </summary>
        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today;
        }

        /// <summary>
        /// 判断日期是否在本周（基于当前系统时间）
        /// </summary>
        public static bool IsThisWeek(this DateTime dateTime)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // 本周第一天（周日）
            var endOfWeek = startOfWeek.AddDays(7); // 下周第一天（周日）
            return dateTime >= startOfWeek && dateTime < endOfWeek;
        }

        /// <summary>
        /// 判断日期是否是今年
        /// </summary>
        public static bool IsThisYear(this DateTime dateTime)
        {
            return dateTime.Year == DateTime.Now.Year;
        }
        #endregion

    }
}
