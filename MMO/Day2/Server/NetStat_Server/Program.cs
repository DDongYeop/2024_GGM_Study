using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    public class Server
    {
        private Socket _listener;
        private Socket _currentClient;
        private byte[] _backlog;
        private const int BACKLOG_SIZE = 1;
        private bool _isRunning;

        public Server()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _backlog = new byte[BACKLOG_SIZE];
            _isRunning = true;
        }

        public async Task StartAsync()
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Any, 5000);

                Console.WriteLine("\n서버 소켓을 생성하고 로컬 주소에 바인딩합니다.");
                _listener.Bind(endpoint);

                Console.WriteLine("백로그 큐를 초기화합니다. (크기: 1)");
                _listener.Listen(BACKLOG_SIZE);

                Console.WriteLine("\n서버가 시작되었습니다 - LISTENING 상태");
                Console.WriteLine("현재 상태를 확인하려면 다음 명령어를 실행하세요:");
                Console.WriteLine("netstat -nao | findstr :5000");

                while (_isRunning)
                {
                    Console.WriteLine("\n아무 키나 누르면 클라이언트의 연결 요청(SYN 패킷)을 받을 준비를 합니다...");
                    Console.ReadKey(true);

                    Console.WriteLine("클라이언트의 SYN 패킷을 기다리는 중...");

                    IAsyncResult ar = _listener.BeginAccept(null, null);

                    while (!ar.IsCompleted && _isRunning)
                    {
                        await Task.Delay(500);
                    }

                    if (!_isRunning)
                        break;

                    if (ar.IsCompleted)
                    {
                        Console.WriteLine("\n클라이언트로부터 SYN 패킷이 도착했습니다 - SYN_RECEIVED 상태");
                        Console.WriteLine("현재 상태를 확인하려면 다음 명령어를 실행하세요:");
                        Console.WriteLine("netstat -nao | findstr :5000");

                        Console.WriteLine("\n아무 키나 누르면 클라이언트의 연결을 수락합니다 (SYN-ACK 전송)...");
                        Console.ReadKey(true);

                        _currentClient = _listener.EndAccept(ar);
                        _currentClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        _currentClient.LingerState = new LingerOption(true, 0);

                        Console.WriteLine("\n클라이언트 연결이 수락되었습니다 - ESTABLISHED 상태");
                        Console.WriteLine("현재 상태를 확인하려면 다음 명령어를 실행하세요:");
                        Console.WriteLine("netstat -nao | findstr :5000");

                        await HandleClientAsync(_currentClient);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n서버 에러: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(Socket client)
        {
            try
            {
                var buffer = new byte[1024];

                Console.WriteLine("\n아무 키나 누르면 클라이언트로부터 데이터를 수신합니다...");
                Console.ReadKey(true);

                int bytesRead = await Task.Factory.FromAsync(
                    client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                    client.EndReceive);

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"클라이언트로부터 {bytesRead}바이트를 수신했습니다: {receivedData}");

                Console.WriteLine("\n아무 키나 누르면 클라이언트에게 응답을 전송합니다...");
                Console.ReadKey(true);

                var response = Encoding.UTF8.GetBytes("Hello from server!");
                await Task.Factory.FromAsync(
                    client.BeginSend(response, 0, response.Length, SocketFlags.None, null, null),
                    client.EndSend);

                Console.WriteLine($"클라이언트에게 {response.Length}바이트를 전송했습니다.");

                // 클라이언트의 연결 종료 대기
                try
                {
                    while (client.Connected)
                    {
                        try
                        {
                            bytesRead = await Task.Factory.FromAsync(
                                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                                client.EndReceive);

                            if (bytesRead == 0)
                                break;
                        }
                        catch (SocketException)
                        {
                            break;
                        }
                    }
                }
                catch { }

                // 서버 측 소켓도 바로 닫기
                client.LingerState = new LingerOption(false, 0);
                client.Close();
                Console.WriteLine("서버: 연결이 종료되었습니다.");

                Console.WriteLine("\n다음 클라이언트를 기다립니다...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n클라이언트 처리 에러: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (client.Connected)
                    {
                        client.LingerState = new LingerOption(false, 0);
                        client.Close();
                    }
                }
                catch { }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                if (_currentClient != null && _currentClient.Connected)
                {
                    _currentClient.Shutdown(SocketShutdown.Both);
                    _currentClient.Close();
                }
                _listener.Close();
                Console.WriteLine("\n서버가 종료되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n서버 종료 에러: {ex.Message}");
            }
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("TCP 서버 프로그램");
        Console.WriteLine("각 단계별로 아무 키나 누르면 다음 단계로 진행됩니다.");
        Console.WriteLine("상태 확인을 위해 별도의 명령 프롬프트 창을 준비해주세요.");
        Console.WriteLine("프로그램 종료는 Ctrl+C를 누르세요.");
        Console.WriteLine("\n아무 키나 누르면 서버를 시작합니다...");
        Console.ReadKey(true);

        var server = new Server();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n사용자가 Ctrl+C를 눌러 서버를 종료합니다.");
            server.Stop();
        };

        await server.StartAsync();
    }
}