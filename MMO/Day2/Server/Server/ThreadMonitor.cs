using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// ThreadPool 스레드 모니터링을 위한 클래스
/// </summary>
public class ThreadMonitor
{
    private static readonly HashSet<int> monitoredThreadIds = new HashSet<int>();
    private static readonly object lockObject = new object();
    private static readonly ConcurrentDictionary<int, string> threadOrigins = new ConcurrentDictionary<int, string>();

    public static void LogThreadInfo(string origin = "Unknown",
        [CallerFilePath] string sourceFile = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        string threadPoolStatus = Thread.CurrentThread.IsThreadPoolThread ? "ThreadPool" : "Non-ThreadPool";

        lock (lockObject)
        {
            if (monitoredThreadIds.Add(threadId))
            {
                string fileName = Path.GetFileName(sourceFile);

                threadOrigins.TryAdd(threadId, origin + $"[{fileName}/{lineNumber}]");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] New Thread Detected - " +
                                $"ID: {threadId}, Type: {threadPoolStatus}, Origin: {origin}, " +
                                $"File: {fileName}, Line: {lineNumber}");

                // 현재 활성 스레드 상태 출력
                PrintThreadPoolInfo();
            }
        }
    }

    private static void PrintThreadPoolInfo()
    {
        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

        Console.WriteLine($"ThreadPool Status:");
        Console.WriteLine($"Worker Threads: Available = {workerThreads}, Max = {maxWorkerThreads}");
        Console.WriteLine($"IO Completion Threads: Available = {completionPortThreads}, Max = {maxCompletionPortThreads}");
        Console.WriteLine($"Currently Monitored Threads: {monitoredThreadIds.Count}");
        Console.WriteLine("Active Threads:");
        foreach (var threadId in monitoredThreadIds)
        {
            Console.WriteLine($"- Thread ID: {threadId}, Origin: {threadOrigins[threadId]}");
        }
        Console.WriteLine(new string('-', 50));
    }
}

/// <summary>
/// 스레드 모니터링 결과를 파일로 저장하는 확장 기능
/// </summary>
public static class ThreadMonitorExtensions
{
    private static readonly string logFilePath = "thread_monitor.log";

    public static void SaveThreadInfoToFile()
    {
        try
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"[{DateTime.Now}] Thread Monitoring Report");
                ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

                writer.WriteLine($"ThreadPool Configuration:");
                writer.WriteLine($"- Worker Threads: {workerThreads}/{maxWorkerThreads}");
                writer.WriteLine($"- IO Completion Threads: {completionPortThreads}/{maxCompletionPortThreads}");
                writer.WriteLine(new string('-', 50));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving thread info: {ex.Message}");
        }
    }
}