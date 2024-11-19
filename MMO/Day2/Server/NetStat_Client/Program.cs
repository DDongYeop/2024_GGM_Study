using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    public class Client
    {
        private Socket _socket;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public Client()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _socket.LingerState = new LingerOption(true, 0);
            _isRunning = true;
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            try
            {
                var keyMonitorTask = MonitorKeyPressAsync();

                Console.WriteLine("\n클라이언트 소켓을 생성합니다.");
                Console.WriteLine("프로그램 종료를 위해 'Q'를 누르거나 Ctrl+C를 누르세요.");

                Console.WriteLine("\n아무 키나 누르면 서버에 연결을 시도합니다 (SYN 패킷 전송)...");
                Console.ReadKey(true);

                Console.WriteLine("서버에 연결을 시도합니다 - SYN_SENT 상태");
                Console.WriteLine("현재 상태를 확인하려면 다음 명령어를 실행하세요:");
                Console.WriteLine("netstat -nao | findstr :5000");

                var connectTask = Task.Factory.FromAsync(
                    _socket.BeginConnect("127.0.0.1", 5000, null, null),
                    _socket.EndConnect);

                while (!connectTask.IsCompleted && _isRunning)
                {
                    Console.WriteLine("서버의 SYN-ACK 패킷을 기다리는 중...");
                    await Task.Delay(100);
                }

                if (!_isRunning)
                    return;

                await connectTask;

                Console.WriteLine("\n서버와 연결이 완료되었습니다 - ESTABLISHED 상태");
                Console.WriteLine("현재 상태를 확인하려면 다음 명령어를 실행하세요:");
                Console.WriteLine("netstat -nao | findstr :5000");

                Console.WriteLine("\n아무 키나 누르면 서버로 데이터를 전송합니다...");
                Console.ReadKey(true);

                string message = "Hello from client!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                await Task.Factory.FromAsync(
                    _socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, null),
                    _socket.EndSend);

                Console.WriteLine($"서버로 {data.Length}바이트를 전송했습니다: {message}");

                Console.WriteLine("\n아무 키나 누르면 서버로부터 응답을 수신합니다...");
                Console.ReadKey(true);

                byte[] buffer = new byte[1024];
                int received = await Task.Factory.FromAsync(
                    _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                    _socket.EndReceive);

                string response = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"서버로부터 {received}바이트를 수신했습니다: {response}");

                Console.WriteLine("\n아무 키나 누르면 연결 종료를 시작합니다...");
                Console.ReadKey(true);

                // 클라이언트가 먼저 종료를 시작
                Console.WriteLine("클라이언트가 먼저 종료를 시작합니다...");
                _socket.Shutdown(SocketShutdown.Send);
                Console.WriteLine("클라이언트: 송신 종료 신호를 보냈습니다.");

                // 서버로부터의 응답 대기
                byte[] finalBuffer = new byte[1024];
                Console.WriteLine("클라이언트: 서버의 응답을 기다립니다...");
                while (_socket.Connected)
                {
                    try
                    {
                        int finalReceived = await Task.Factory.FromAsync(
                            _socket.BeginReceive(finalBuffer, 0, finalBuffer.Length, SocketFlags.None, null, null),
                            _socket.EndReceive);

                        if (finalReceived == 0)
                            break;
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }

                _socket.Close();
                Console.WriteLine("클라이언트: 연결이 완전히 종료되었습니다.");

                _cts.Cancel();
                try
                {
                    await keyMonitorTask;
                }
                catch (OperationCanceledException)
                {
                    // 정상적인 취소
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n클라이언트 에러: {ex.Message}");
                try
                {
                    if (_socket.Connected)
                    {
                        _socket.Close();
                    }
                }
                catch { }
            }
            finally
            {
                _cts.Dispose();
            }
        }

        private async Task MonitorKeyPressAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            Console.WriteLine("\n사용자가 'Q'를 눌러 프로그램을 종료합니다.");
                            if (_socket.Connected)
                            {
                                _socket.Shutdown(SocketShutdown.Send);
                                _socket.Close();
                            }
                            break;
                        }
                    }
                    await Task.Delay(100, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소
            }
        }

        public async Task StopAsync()
        {
            if (_socket.Connected)
            {
                // 클라이언트의 StartAsync 메서드에서 종료 부분 수정
                Console.WriteLine("\n아무 키나 누르면 연결 종료를 시작합니다...");
                Console.ReadKey(true);

                try
                {
                    // 1. 남은 데이터를 모두 받음
                    byte[] buffer = new byte[1024];
                    while (_socket.Available > 0)
                    {
                        await Task.Factory.FromAsync(
                            _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                            _socket.EndReceive);
                    }

                    // 2. Linger 옵션을 비활성화하여 TIME_WAIT 방지
                    _socket.LingerState = new LingerOption(false, 0);

                    // 3. 소켓을 직접 닫음 (Shutdown 호출하지 않음)
                    _socket.Close();
                    Console.WriteLine("클라이언트: 연결이 완전히 종료되었습니다.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"종료 중 에러 발생: {ex.Message}");
                    _socket?.Close();
                }
            }

            await Task.Run(() => _cts.Cancel());
        }
    }

    static async Task Main()
    {
        Console.WriteLine("TCP 클라이언트 프로그램");
        Console.WriteLine("각 단계별로 아무 키나 누르면 다음 단계로 진행됩니다.");
        Console.WriteLine("프로그램 종료는 'Q'키 또는 Ctrl+C를 누르세요.");
        Console.WriteLine("상태 확인을 위해 별도의 명령 프롬프트 창을 준비해주세요.");
        Console.WriteLine("\n아무 키나 누르면 클라이언트를 시작합니다...");
        Console.ReadKey(true);

        var client = new Client();

        Console.CancelKeyPress += async (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n사용자가 Ctrl+C를 눌러 프로그램을 종료합니다.");
            await client.StopAsync();
        };

        await client.StartAsync();

        Console.WriteLine("\n아무 키나 누르면 프로그램을 종료합니다...");
        Console.ReadKey(true);
    }
}