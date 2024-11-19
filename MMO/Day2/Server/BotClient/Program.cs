using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NetBase;
using Server;
using Server.C2SInGame;

namespace BotClient
{


    internal class Program
    {
        private static bool _running = true;
        private static readonly int _heartbeatInterval = 5000;
        private static Timer _heartbeatTimer;
        private static Timer _moveReqTimer;
        private static PcManager _pcManager;
        private static List<byte> _receivedDataBuffer = new List<byte>();
        private static PacketHandler _packetHandler;

        private static void ProcessReceivedData(Socket socket, byte[] receivedData, int bytesReceived)
        {
            Console.WriteLine($"[PACKET] Received {bytesReceived} bytes of data");
            _receivedDataBuffer.AddRange(receivedData.Take(bytesReceived));

            while (_receivedDataBuffer.Count >= PacketHeader.HeaderSize)
            {
                byte[] headerBytes = _receivedDataBuffer.Take(PacketHeader.HeaderSize).ToArray();
                PacketHeader receivedHeader = PacketHeader.FromBytes(headerBytes);

                if (_receivedDataBuffer.Count >= receivedHeader.PacketSize)
                {
                    byte[] packetData = _receivedDataBuffer.Take(receivedHeader.PacketSize).ToArray();

                    using (MemoryStream packetStream = new MemoryStream(packetData))
                    {
                        _packetHandler.HandlePacket(packetStream, socket);
                    }

                    _receivedDataBuffer.RemoveRange(0, receivedHeader.PacketSize);
                }
                else
                {
                    break;
                }
            }
        }

        private static void InitializeManagers()
        {
            _pcManager = new PcManager();
            _packetHandler = new PacketHandler(_pcManager);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("[SYSTEM] Process start..");

            InitializeManagers();

            ConfigManager configManager = ConfigManager.Instance();
            string server = configManager.GetValue("ServerIP", "172.30.1.9");
            int port = configManager.GetIntValue("ServerPort", 11000);

            Console.WriteLine($"[SYSTEM] Server IP: {server}, Port: {port}");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            try
            {
                IPAddress ipAddress = IPAddress.Parse(server);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                using (Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    sender.Connect(remoteEP);
                    Console.WriteLine($"[PACKET] Socket connected to {sender.RemoteEndPoint.ToString()}");

                    sender.NoDelay = true;
                    sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                    sender.Blocking = false;

                    var responsePacket = new PacketBase();
                    ResourceLoadCompleteReq resourceLoadCompleteReq = new ResourceLoadCompleteReq();
                    responsePacket.Write(resourceLoadCompleteReq);
                    SendPacket(sender, PacketType.ResourceLoadCompleteReq, responsePacket.GetPacketData());

                    int SendEscapeMoveCount = 0;

                    byte[] buffer = new byte[1024 * 16];
                    while (_running)
                    {
                        bool dataReceived = false;
                        do
                        {
                            if (sender.Poll(0, SelectMode.SelectRead))
                            {
                                int bytesReceived = sender.Receive(buffer);
                                if (bytesReceived > 0)
                                {
                                    ProcessReceivedData(sender, buffer, bytesReceived);
                                    dataReceived = true;
                                }
                                else
                                {
                                    _running = false;
                                    break;
                                }
                            }
                            else
                            {
                                dataReceived = false;
                            }
                        } while (dataReceived);

                        if (_running && !dataReceived)
                        {
                            Thread.Sleep(10);
                            if (SendEscapeMoveCount % 10 == 0)
                            {
                                SendEscapeMoveCount = 0;
                            }
                            SendEscapeMoveCount++;
                        }
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Exception: {e.ToString()}");
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }


        private static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            _running = false;
            args.Cancel = true;
            _heartbeatTimer?.Dispose();
            _moveReqTimer?.Dispose();
            Console.WriteLine("[SYSTEM] Exiting...");
        }

        private static void SendMoveReq(object state)
        {
            try
            {
                Socket socket = (Socket)state;

                var controlledPc = _pcManager.GetControlledPc();
                if (controlledPc == null)
                {
                    Console.WriteLine("[PACKET] No controlled PC available. Cannot send MoveReq.");
                    return;
                }

                var responsePacket = new PacketBase();
                MoveReq moveReq = new MoveReq
                {
                    Direction = controlledPc.Direction,
                    Dest = new FLocation
                    {
                        X = controlledPc.Position.X + 1f,
                        Y = controlledPc.Position.Y,
                        Z = controlledPc.Position.Z + 1f
                    },
                    DashFlag = false
                };
                responsePacket.Write(moveReq);
                SendPacket(socket, PacketType.MoveReq, responsePacket.GetPacketData());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sending MoveReq: {ex.Message}");
            }
        }

        private static void StartMoveReq(Socket socket)
        {
            _moveReqTimer = new Timer(new TimerCallback(SendMoveReq), socket, 0, 10000);
        }

        private static void SendHeartbeat(object state)
        {
            try
            {
                Socket socket = (Socket)state;
                SendPacket(socket, PacketType.HeartbeatReq, new byte[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sending Heartbeat: {ex.Message}");
            }
        }

        public static void SendPacket(Socket socket, PacketType packetType, byte[] data)
        {
            try
            {
                ushort packetSize = (ushort)(PacketHeader.HeaderSize + data.Length);
                PacketHeader header = new PacketHeader { PacketSize = packetSize, PacketType = (ushort)packetType };
                byte[] headerBytes = header.ToBytes();

                byte[] packet = new byte[packetSize];
                headerBytes.CopyTo(packet, 0);
                data.CopyTo(packet, PacketHeader.HeaderSize);

                int bytesSent = socket.Send(packet);
                Console.WriteLine($"[PACKET] Send {bytesSent} bytes to server, PacketType: {packetType.ToString()}");
                Console.WriteLine("[PACKET] Packet content:");
                Console.WriteLine("[PACKET] Header: " + BitConverter.ToString(headerBytes));
                Console.WriteLine("[PACKET] Data: " + BitConverter.ToString(data));
                Console.WriteLine("[PACKET] Full packet: " + BitConverter.ToString(packet));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sending packet: {ex.Message}");
            }
        }
    }
}