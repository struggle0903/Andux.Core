using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Andux.Core.Helper.Monitor
{
    /// <summary>
    /// 性能监控帮助类（跨平台版）
    /// </summary>
    public static class PerformanceMonitor
    {
        #region 计时相关功能

        /// <summary>
        /// 测量代码执行时间
        /// </summary>
        public static long MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 测量异步代码执行时间
        /// </summary>
        public static async Task<long> MeasureExecutionTimeAsync(Func<Task> asyncAction)
        {
            var stopwatch = Stopwatch.StartNew();
            await asyncAction();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 代码块计时器
        /// </summary>
        public sealed class CodeTimer : IDisposable
        {
            private readonly string _blockName;
            private readonly Stopwatch _stopwatch;
            private readonly long _startMemory;

            public CodeTimer(string blockName)
            {
                _blockName = blockName;
                _stopwatch = Stopwatch.StartNew();
                _startMemory = GC.GetTotalMemory(false);
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                var memoryUsed = GC.GetTotalMemory(false) - _startMemory;

                Console.WriteLine($"[Perf] {_blockName} - " +
                    $"Time: {_stopwatch.Elapsed.TotalMilliseconds:0.###} ms, " +
                    $"Memory: {FormatBytes(memoryUsed)}");
            }
        }

        public static CodeTimer TimeBlock(string blockName) => new CodeTimer(blockName);

        #endregion

        #region 内存监控

        /// <summary>
        /// 内存信息结构
        /// </summary>
        public struct MemoryInfo
        {
            /// <summary>
            /// 工作集内存（Working Set）
            /// 表示进程当前占用的物理内存量（单位：字节）
            /// 这是操作系统分配给进程的实际物理内存
            /// </summary>
            public long WorkingSet { get; set; }

            /// <summary>
            /// 私有内存（Private Memory）
            /// 表示进程独占的、不能被其他进程共享的内存（单位：字节）
            /// 包含堆内存、栈内存等私有分配
            /// </summary>
            public long PrivateMemory { get; set; }

            /// <summary>
            /// 虚拟内存（Virtual Memory）
            /// 表示进程使用的虚拟内存总量（单位：字节）
            /// 包括物理内存和交换文件/分页文件中的内存
            /// </summary>
            public long VirtualMemory { get; set; }

            /// <summary>
            /// .NET 垃圾回收器（GC）管理的内存
            /// 表示当前由 CLR 垃圾回收器管理的内存总量（单位：字节）
            /// 这是托管堆的内存使用量
            /// </summary>
            public long GCMemory { get; set; }

            /// <summary>
            /// 格式化内存信息为易读字符串
            /// </summary>
            /// <returns>格式化的内存信息字符串</returns>
            public override string ToString() =>
                $"工作集内存: {PerformanceMonitor.FormatBytes(WorkingSet)}, " +
                $"私有内存: {PerformanceMonitor.FormatBytes(PrivateMemory)}, " +
                $"虚拟内存: {PerformanceMonitor.FormatBytes(VirtualMemory)}, " +
                $"GC管理内存: {PerformanceMonitor.FormatBytes(GCMemory)}";
        }

        /// <summary>
        /// 获取内存使用情况
        /// </summary>
        public static MemoryInfo GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            return new MemoryInfo
            {
                WorkingSet = process.WorkingSet64,
                PrivateMemory = process.PrivateMemorySize64,
                VirtualMemory = process.VirtualMemorySize64,
                GCMemory = GC.GetTotalMemory(false)
            };
        }

        #endregion

        #region GC统计

        /// <summary>
        /// GC信息结构
        /// </summary>
        public struct GCInfo
        {
            public int Gen0Collections { get; set; }
            public int Gen1Collections { get; set; }
            public int Gen2Collections { get; set; }
            public long TotalMemory { get; set; }

            public override string ToString() =>
                $"Gen0: {Gen0Collections}, " +
                $"Gen1: {Gen1Collections}, " +
                $"Gen2: {Gen2Collections}, " +
                $"Memory: {FormatBytes(TotalMemory)}";
        }

        /// <summary>
        /// 获取GC统计信息
        /// </summary>
        public static GCInfo GetGCStats() => new GCInfo
        {
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalMemory = GC.GetTotalMemory(false)
        };

        #endregion

        #region CPU监控
        public static async Task<double> GetCpuUsageAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                return await GetCpuUsageWindowsAsync();
            }
            else if (OperatingSystem.IsLinux())
            {
                return await GetCpuUsageLinuxAsync();
            }
            else if (OperatingSystem.IsMacOS())
            {
                return await GetCpuUsageMacAsync();
            }
            return -1;
        }

        /// <summary>
        /// 获取 win 系统cpu使用率
        /// </summary>
        /// <returns></returns>
        private static async Task<double> GetCpuUsageWindowsAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "cpu get loadpercentage /value",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            var match = Regex.Match(output, @"LoadPercentage=(\d+)");
            return match.Success ? double.Parse(match.Groups[1].Value) : -1;
        }

        /// <summary>
        /// 获取 Linux 系统cpu使用率
        /// </summary>
        /// <returns></returns>
        private static async Task<double> GetCpuUsageLinuxAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = "-c \"top -bn1 | grep '%Cpu'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            var match = Regex.Match(output, @"(\d+\.\d+)\s+id");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var idle))
                return 100 - idle;
            return -1;
        }

        /// <summary>
        /// 获取 Mac 系统cpu使用率
        /// </summary>
        /// <returns></returns>
        private static async Task<double> GetCpuUsageMacAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = "-c \"ps -A -o %cpu | awk '{s+=$1} END {print s}'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            return double.TryParse(output.Trim(), out var usage) ? usage : -1;
        }
        #endregion

        #region 辅助方法

        /// <summary>
        /// 格式化字节大小
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}
