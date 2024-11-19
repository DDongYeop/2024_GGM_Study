using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

class Program
{
    public class TcpConnectionInfo
    {
        public string LocalEndPoint { get; set; }
        public string RemoteEndPoint { get; set; }
        public TcpState State { get; set; }
        public int ProcessId { get; set; }

        public override string ToString()
        {
            return $"TCP {LocalEndPoint,23} {RemoteEndPoint,23} {GetStateString(),-11} {ProcessId,8}";
        }

        private string GetStateString()
        {
            switch (State)
            {
                case TcpState.Listen:
                    return "LISTENING";
                case TcpState.Established:
                    return "ESTABLISHED";
                case TcpState.FinWait1:
                    return "FIN_WAIT_1";
                case TcpState.FinWait2:
                    return "FIN_WAIT_2";
                case TcpState.SynSent:
                    return "SYN_SENT";
                case TcpState.SynReceived:
                    return "SYN_RCVD";
                case TcpState.LastAck:
                    return "LAST_ACK";
                case TcpState.Closing:
                    return "CLOSING";
                case TcpState.TimeWait:
                    return "TIME_WAIT";
                case TcpState.CloseWait:
                    return "CLOSE_WAIT";
                case TcpState.Closed:
                    return "CLOSED";
                default:
                    return State.ToString();
            }
        }

        public string GetLogString()
        {
            if (State == TcpState.Listen)
                return $"TCP {LocalEndPoint} (LISTENING, PID: {ProcessId})";
            else
                return $"TCP {LocalEndPoint} -> {RemoteEndPoint} ({State}, PID: {ProcessId})";
        }
    }

    public class TcpMonitor
    {
        private readonly string _logFilePath;
        private readonly HashSet<int> _targetProcessIds;
        private readonly int _port;
        private bool _isRunning;
        private readonly object _logLock = new object();
        private Dictionary<string, TcpConnectionInfo> _previousConnections = new Dictionary<string, TcpConnectionInfo>();
        private const int HEADER_LINES = 3;
        private const int MAX_DISPLAY_LINES = 5; // 화면에 표시할 최대 연결 수

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public uint State;
            public uint LocalAddr;
            public byte LocalPort1;
            public byte LocalPort2;
            public byte LocalPort3;
            public byte LocalPort4;
            public uint RemoteAddr;
            public byte RemotePort1;
            public byte RemotePort2;
            public byte RemotePort3;
            public byte RemotePort4;
            public int OwningPid;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen,
            bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, uint reserved = 0);

        enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER = 0,
            TCP_TABLE_BASIC_CONNECTIONS = 1,
            TCP_TABLE_BASIC_ALL = 2,
            TCP_TABLE_OWNER_PID_LISTENER = 3,
            TCP_TABLE_OWNER_PID_CONNECTIONS = 4,
            TCP_TABLE_OWNER_PID_ALL = 5
        }

        public TcpMonitor(string logFilePath, int port = 0, params int[] processIds)
        {
            _logFilePath = logFilePath;
            _targetProcessIds = new HashSet<int>(processIds);
            _port = port;
            _isRunning = true;
        }

        private string FormatAddress(IPEndPoint endPoint)
        {
            if (endPoint.Address.Equals(IPAddress.Any))
                return $"0.0.0.0:{endPoint.Port}";
            return $"{endPoint.Address}:{endPoint.Port}";
        }

        private string GetConnectionKey(string localEndPoint, string remoteEndPoint, TcpState state)
        {
            // LISTENING 상태는 고유한 키를 가짐
            if (state == TcpState.Listen)
                return $"LISTEN_{localEndPoint}";

            // 일반 연결은 양방향을 고려한 키 생성
            var endpoints = new[] { localEndPoint, remoteEndPoint }.OrderBy(x => x);
            return string.Join("_", endpoints);
        }

        private string GetStateChangeDescription(TcpConnectionInfo prev, TcpConnectionInfo current)
        {
            if (prev == null && current != null)
            {
                if (current.State == TcpState.Listen)
                    return $"서버 시작: {current.GetLogString()}";
                else
                    return $"새로운 연결: {current.GetLogString()}";
            }
            else if (prev != null && current == null)
            {
                return $"연결 종료: {prev.GetLogString()}";
            }
            else if (prev.State != current.State)
            {
                return $"상태 변경: {prev.GetLogString()} -> {current.State}";
            }

            return null;
        }

        private Dictionary<string, TcpConnectionInfo> GetTcpConnections()
        {
            var tcpConnections = new Dictionary<string, TcpConnectionInfo>();
            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, 2, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                uint result = GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, 2, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
                if (result == 0)
                {
                    int rowCount = Marshal.ReadInt32(tcpTablePtr);
                    IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + 4);

                    for (int i = 0; i < rowCount; i++)
                    {
                        MIB_TCPROW_OWNER_PID row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                        rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)));

                        int localPort = (row.LocalPort1 << 8) + row.LocalPort2;
                        int remotePort = (row.RemotePort1 << 8) + row.RemotePort2;

                        if (_port != 0 && localPort != _port && remotePort != _port)
                            continue;

                        if (_targetProcessIds.Any() && !_targetProcessIds.Contains(row.OwningPid))
                            continue;

                        var localEndPoint = new IPEndPoint(row.LocalAddr, localPort);
                        var remoteEndPoint = new IPEndPoint(row.RemoteAddr, remotePort);
                        var state = (TcpState)row.State;

                        var local = FormatAddress(localEndPoint);
                        var remote = FormatAddress(remoteEndPoint);
                        var key = GetConnectionKey(local, remote, state);

                        if (!tcpConnections.ContainsKey(key))
                        {
                            tcpConnections[key] = new TcpConnectionInfo
                            {
                                LocalEndPoint = local,
                                RemoteEndPoint = remote,
                                State = state,
                                ProcessId = row.OwningPid
                            };
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }

            return tcpConnections;
        }

        private void WriteToLog(List<string> lines)
        {
            try
            {
                lock (_logLock)
                {
                    File.AppendAllLines(_logFilePath, lines);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 작성 오류: {ex.Message}");
            }
        }

        private void ClearLine(int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line);
        }

        public async Task MonitorAsync(int intervalMs = 1)
        {
            Console.Clear();
            Console.WriteLine("TCP 연결 모니터 프로그램");
            Console.WriteLine("Proto  Local Address          Foreign Address        State       PID");
            Console.WriteLine("----------------------------------------------------------------");

            var logMessages = new List<string>();
            var cts = new CancellationTokenSource();

            try
            {
                var keyMonitorTask = Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.C)
                            {
                                logMessages.Clear();
                                int maxLogLines = Console.WindowHeight - HEADER_LINES - 5;
                                for (int i = 0; i < maxLogLines; i++)
                                {
                                    Console.SetCursorPosition(0, HEADER_LINES + 5 + i);
                                    ClearLine(HEADER_LINES + 5 + i);
                                }
                            }
                        }
                        await Task.Delay(50);
                    }
                });

                while (_isRunning)
                {
                    var currentConnections = GetTcpConnections();
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    // 상태 변경 로그 준비
                    var stateChanges = new List<string>();

                    foreach (var current in currentConnections)
                    {
                        if (!_previousConnections.TryGetValue(current.Key, out var previous))
                        {
                            var desc = GetStateChangeDescription(null, current.Value);
                            if (desc != null)
                                stateChanges.Add($"[{timestamp}] {desc}");
                        }
                        else if (previous.State != current.Value.State)
                        {
                            var desc = GetStateChangeDescription(previous, current.Value);
                            if (desc != null)
                                stateChanges.Add($"[{timestamp}] {desc}");
                        }
                    }

                    foreach (var prev in _previousConnections)
                    {
                        if (!currentConnections.ContainsKey(prev.Key))
                        {
                            var desc = GetStateChangeDescription(prev.Value, null);
                            if (desc != null)
                                stateChanges.Add($"[{timestamp}] {desc}");
                        }
                    }

                    // 현재 연결 상태 화면 표시
                    Console.SetCursorPosition(0, HEADER_LINES);
                    var orderedConnections = currentConnections.Values
                        .OrderBy(x => x.State != TcpState.Listen)
                        .ThenBy(x => x.LocalEndPoint)
                        .Take(MAX_DISPLAY_LINES)
                        .ToList();

                    foreach (var connection in orderedConnections)
                    {
                        Console.WriteLine(connection);
                    }

                    // 로그 메시지 추가
                    if (stateChanges.Any())
                    {
                        logMessages.InsertRange(0, stateChanges);
                        logMessages = logMessages.Take(100).ToList(); // 최근 100개의 로그만 유지

                        // 로그 파일에 쓰기
                        await Task.Run(() => WriteToLog(stateChanges));
                    }

                    // 로그 영역 표시
                    const int LOG_START_LINE = HEADER_LINES + MAX_DISPLAY_LINES + 2;
                    Console.SetCursorPosition(0, LOG_START_LINE);
                    Console.WriteLine("\n---------------------------- 연결 상태 변경 로그 ----------------------------");
                    Console.WriteLine("로그 초기화: 'C' 키를 누르세요\n");

                    int maxLogLines = Console.WindowHeight - LOG_START_LINE - 4;
                    for (int i = 0; i < Math.Min(maxLogLines, logMessages.Count); i++)
                    {
                        Console.SetCursorPosition(0, LOG_START_LINE + 3 + i);
                        ClearLine(LOG_START_LINE + 3 + i);
                        Console.WriteLine(logMessages[i]);
                    }

                    _previousConnections = new Dictionary<string, TcpConnectionInfo>(currentConnections);
                    await Task.Delay(intervalMs);
                }

                cts.Cancel();
                await keyMonitorTask;
            }
            finally
            {
                cts.Dispose();
            }
        }


        public void Stop()
        {
            _isRunning = false;
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("TCP 연결 모니터 프로그램");
        Console.WriteLine("상태 확인을 위해 별도의 명령 프롬프트 창을 준비해주세요.");
        Console.WriteLine("프로그램 종료는 Ctrl+C를 누르세요.");
        Console.WriteLine("\n아무 키나 누르면 모니터링을 시작합니다...");
        Console.ReadKey(true);

        var monitor = new TcpMonitor("tcp_monitor.log", 5000);

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n사용자가 Ctrl+C를 눌러 모니터링을 종료합니다.");
            monitor.Stop();
        };

        await monitor.MonitorAsync();
    }
}