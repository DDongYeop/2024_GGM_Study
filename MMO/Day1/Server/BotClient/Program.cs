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
    public struct PacketHeader
    {
        public ushort PacketSize;
        public ushort PacketType;

        public const int HeaderSize = 4; // 2 bytes for PacketSize, 2 bytes for PacketType

        public static PacketHeader FromBytes(byte[] buffer)
        {
            PacketHeader header = new PacketHeader
            {
                PacketSize = BitConverter.ToUInt16(buffer, 0),
                PacketType = BitConverter.ToUInt16(buffer, 2)
            };
            return header;
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[HeaderSize];
            BitConverter.GetBytes(PacketSize).CopyTo(buffer, 0);
            BitConverter.GetBytes(PacketType).CopyTo(buffer, 2);
            return buffer;
        }
    }

    public class PacketHandler
    {
        private Dictionary<PacketType, Action<MemoryStream, Socket>> _packetHandlers;
        private PcManager _pcManager;

        public PacketHandler(PcManager pcManager)
        {
            _pcManager = pcManager;
            _packetHandlers = new Dictionary<PacketType, Action<MemoryStream, Socket>>
            {
                { PacketType.ResourceLoadCompleteAck, HandleResourceLoadCompleteAck },
                { PacketType.ZoneUpdateAck, HandleZoneUpdateAck },
                { PacketType.MoveAck, HandleMoveAck },
            };
        }

        public void HandlePacket(MemoryStream packetStream, Socket handler)
        {
            packetStream.Position = 0;
            PacketHeader header = DeserializeHeader(packetStream);

            if (_packetHandlers.TryGetValue((PacketType)header.PacketType, out Action<MemoryStream, Socket> handlerAction))
            {
                byte[] bodyData = new byte[packetStream.Length - PacketHeader.HeaderSize];
                packetStream.Read(bodyData, 0, bodyData.Length);
                MemoryStream bodyStream = new MemoryStream(bodyData);

                Console.WriteLine($"[PACKET] Received packet: Type {header.PacketType}, Size {header.PacketSize}");
                handlerAction(bodyStream, handler);
            }
            else
            {
                Console.WriteLine($"[PACKET] Unknown packet type: {header.PacketType}");
            }
        }

        private PacketHeader DeserializeHeader(MemoryStream stream)
        {
            byte[] headerData = new byte[PacketHeader.HeaderSize];
            stream.Read(headerData, 0, PacketHeader.HeaderSize);

            return new PacketHeader
            {
                PacketSize = BitConverter.ToUInt16(headerData, 0),
                PacketType = BitConverter.ToUInt16(headerData, 2)
            };
        }

        private void HandleResourceLoadCompleteAck(MemoryStream packetStream, Socket handler)
        {
            Console.WriteLine("[PACKET] Handling ResourceLoadCompleteAck");
            byte[] packetData = packetStream.ToArray();
            Console.WriteLine($"[PACKET] Packet data length: {packetData.Length} bytes");
            var packet = new PacketBase();
            packet.SetPacketData(packetData);

            ResourceLoadCompleteAck resourceLoadCompleteAck = new ResourceLoadCompleteAck();
            resourceLoadCompleteAck = packet.Read<ResourceLoadCompleteAck>();

            Console.WriteLine($"[PACKET] ResourceLoadCompleteAck PcIndex:{resourceLoadCompleteAck.PcIndex}");
            _pcManager.SetControlledPcIndex(resourceLoadCompleteAck.PcIndex);
        }

        private void HandleZoneUpdateAck(MemoryStream packetStream, Socket handler)
        {
            Console.WriteLine("[PACKET] Handling ZoneUpdateAck");
            byte[] packetData = packetStream.ToArray();
            Console.WriteLine($"[PACKET] Packet data length: {packetData.Length} bytes");
            var packet = new PacketBase();
            packet.SetPacketData(packetData);

            ZoneUpdateAck zoneUpdate = new ZoneUpdateAck();
            zoneUpdate = packet.Read<ZoneUpdateAck>();

            Console.WriteLine("[PACKET] ZoneUpdateAck Data:");

            // PC Enters
            if (zoneUpdate.PcEnters != null && zoneUpdate.PcEnters.Count > 0)
            {
                Console.WriteLine("[PACKET] PC Enters:");
                foreach (var pc in zoneUpdate.PcEnters)
                {
                    Console.WriteLine($"[PACKET]   PC Index: {pc.Index}");
                    Console.WriteLine($"[PACKET]   Position: X={pc.Pos.X}, Y={pc.Pos.Y}, Z={pc.Pos.Z}");
                    Console.WriteLine($"[PACKET]   Destination: X={pc.Dest.X}, Y={pc.Dest.Y}, Z={pc.Dest.Z}");
                    Console.WriteLine($"[PACKET]   Direction: {pc.Direction}");
                    Console.WriteLine($"[PACKET]   Attack Speed: {pc.AttackSpeed}");
                    Console.WriteLine($"[PACKET]   Move Speed: {pc.MoveSpeed}");
                    Console.WriteLine($"[PACKET]   Casting Speed: {pc.CastingSpeed}");
                    Console.WriteLine($"[PACKET]   HP: {pc.Hp}");
                    Console.WriteLine($"[PACKET]   MP: {pc.Mp}");
                    Console.WriteLine($"[PACKET]   Status Effect: {pc.StatusEffect}");
                    _pcManager.UpdatePc(pc);
                }
            }
            else
            {
                Console.WriteLine("[PACKET] No PC Enters in this update.");
            }

            // Moves
            if (zoneUpdate.Moves != null && zoneUpdate.Moves.Count > 0)
            {
                Console.WriteLine("[PACKET] Moves:");
                foreach (var move in zoneUpdate.Moves)
                {
                    Console.WriteLine($"[PACKET]   Index: {move.Index}");
                    Console.WriteLine($"[PACKET]   Object Type: {move.ObjectType}");
                    Console.WriteLine($"[PACKET]   Position: X={move.Pos.X}, Y={move.Pos.Y}, Z={move.Pos.Z}");
                    Console.WriteLine($"[PACKET]   Destination: X={move.Dest.X}, Y={move.Dest.Y}, Z={move.Dest.Z}");

                    PcInfo pc = _pcManager.GetPc(move.Index);
                    if (pc != null)
                    {
                        pc.Position = new FLocation
                        {
                            X = move.Pos.X,
                            Y = move.Pos.Y,
                            Z = move.Pos.Z
                        };

                        pc.Destination = new FLocation
                        {
                            X = move.Dest.X,
                            Y = move.Dest.Y,
                            Z = move.Dest.Z
                        };

                        _pcManager.UpdatePc(pc);
                    }
                }
            }
            else
            {
                Console.WriteLine("[PACKET] No Moves in this update.");
            }

            // Removes
            if (zoneUpdate.Removes != null && zoneUpdate.Removes.Count > 0)
            {
                Console.WriteLine("[PACKET] Removes:");
                foreach (var remove in zoneUpdate.Removes)
                {
                    Console.WriteLine($"[PACKET]   Index: {remove.Index}");
                    Console.WriteLine($"[PACKET]   Object Type: {remove.ObjectType}");
                }
            }
            else
            {
                Console.WriteLine("[PACKET] No Removes in this update.");
            }
        }

        private void HandleMoveAck(MemoryStream packetStream, Socket handler)
        {
            Console.WriteLine("[PACKET] Handling MoveAck");

            byte[] packetData = packetStream.ToArray();
            Console.WriteLine($"[PACKET] Packet data length: {packetData.Length} bytes");
            var packet = new PacketBase();
            packet.SetPacketData(packetData);
            MoveAck moveAck = new MoveAck();
            moveAck = packet.Read<MoveAck>();
            Console.WriteLine($"[PACKET] MoveAck Result: {moveAck.Result.ToString()}");
        }
    }

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
                Console.WriteLine($"[PACKET] Sent {bytesSent} bytes to server, PacketType: {packetType.ToString()}");
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