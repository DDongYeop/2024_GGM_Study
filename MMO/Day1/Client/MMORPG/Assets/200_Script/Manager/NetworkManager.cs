using System;
using System.Collections;
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
using UnityEngine;

public struct PacketHeader
{
    public ushort PacketSize;
    public ushort PacketType;

    public const int HeaderSize = 4; // 2 bytes for PacketSize, 2 bytes for PacketType

    public static PacketHeader FromBytes(byte[] buffer)
    {
        PacketHeader header = new PacketHeader { PacketSize = BitConverter.ToUInt16(buffer, 0), PacketType = BitConverter.ToUInt16(buffer, 2) };
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

    public PacketHandler()
    {
        _packetHandlers = new Dictionary<PacketType, Action<MemoryStream, Socket>>
        {
            { PacketType.ResourceLoadCompleteAck, HandleResourceLoadCompleteAck }, { PacketType.ZoneUpdateAck, HandleZoneUpdateAck }, { PacketType.MoveAck, HandleMoveAck },
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

        return new PacketHeader { PacketSize = BitConverter.ToUInt16(headerData, 0), PacketType = BitConverter.ToUInt16(headerData, 2) };
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

        Manager.Data.MyPC_Index = resourceLoadCompleteAck.PcIndex;
        Console.WriteLine($"[PACKET] ResourceLoadCompleteAck PcIndex:{resourceLoadCompleteAck.PcIndex}");
    }

    private void HandleZoneUpdateAck(MemoryStream packetStream, Socket handler)
    {
        byte[] packetData = packetStream.ToArray();
        var packet = new PacketBase();
        packet.SetPacketData(packetData);

        ZoneUpdateAck zoneUpdate = new ZoneUpdateAck();
        zoneUpdate = packet.Read<ZoneUpdateAck>();
        
        // PC Enters
        if (zoneUpdate.PcEnters != null && zoneUpdate.PcEnters.Count > 0)
        {
            foreach (var pc in zoneUpdate.PcEnters)
            {
                if (pc.Index == Manager.Data.MyPC_Index)
                {
                    Manager.Char.CreateMyPC(pc);
                }
            }
        }
        else
        {
            Console.WriteLine("[PACKET] No PC Enters in this update.");
        }

        // Moves
        if (zoneUpdate.Moves != null && zoneUpdate.Moves.Count > 0)
        {
            foreach (var move in zoneUpdate.Moves)
            {
                if (move.Index == Manager.Data.MyPC_Index && Manager.Char.MyPC != null)
                {
                    Manager.Char.MyPC.EnqueuePosition(move.Pos.FLocationToVector3());
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
        byte[] packetData = packetStream.ToArray();
        var packet = new PacketBase();
        packet.SetPacketData(packetData);
        MoveAck moveAck = new MoveAck();
        moveAck = packet.Read<MoveAck>();
    }
}

public class NetworkManager : MonoBehaviour
{
    private bool _running = true;
    private List<byte> _receivedDataBuffer = new List<byte>();
    private PacketHandler _packetHandler;
    private Socket sender;

    private void ProcessReceivedData(Socket socket, byte[] receivedData, int bytesReceived)
    {
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

    public void Initialize()
    {
        _packetHandler = new PacketHandler();
        StartCoroutine(CoInitializeServer());
    }
    
    private IEnumerator CoInitializeServer()
    {
        var serverInfo = SetServerEnterInfo();

        IPAddress ipAddress = IPAddress.Parse(serverInfo.ServerIP);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, serverInfo.ServerPort);

        using (sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            sender.Connect(remoteEP);
            Console.WriteLine($"[PACKET] Socket connected to {sender.RemoteEndPoint.ToString()}");

            sender.NoDelay = true;
            sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            sender.Blocking = false;

            var responsePacket = new PacketBase();
            ResourceLoadCompleteReq resourceLoadCompleteReq = new ResourceLoadCompleteReq();
            responsePacket.Write(resourceLoadCompleteReq);
            SendPacket(PacketType.ResourceLoadCompleteReq, responsePacket.GetPacketData());

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

                        yield return null;
                    }
                    else
                    {
                        dataReceived = false;
                    }
                } while (dataReceived);

                if (_running && !dataReceived)
                {
                    Thread.Sleep(10);
                    yield return new WaitForSeconds(0.1f);
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


    private void OnApplicationQuit()
    {
        _running = false;
    }

    public void SendMoveReq(MoveReq playerMoveReq)
    {
        var responsePacket = new PacketBase();
        responsePacket.Write(playerMoveReq);
        SendPacket(PacketType.MoveReq, responsePacket.GetPacketData());
    }

    private void SendPacket(PacketType packetType, byte[] data)
    {
        ushort packetSize = (ushort)(PacketHeader.HeaderSize + data.Length);
        PacketHeader header = new PacketHeader { PacketSize = packetSize, PacketType = (ushort)packetType };
        byte[] headerBytes = header.ToBytes();

        byte[] packet = new byte[packetSize];
        headerBytes.CopyTo(packet, 0);
        data.CopyTo(packet, PacketHeader.HeaderSize);

        int bytesSent = sender.Send(packet);
    }

#if UNITY_EDITOR
    private Dictionary<string, string> mConfig = new Dictionary<string, string>();
    
    private (string ServerIP, int ServerPort) SetServerEnterInfo()
    {
        var configFilePath = "Assets/Editor/Debug/config.txt";
        if (File.Exists(configFilePath))
        {
            foreach (string line in File.ReadAllLines(configFilePath))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    mConfig[parts[0].Trim()] = parts[1].Trim();
                }
            }

            Debug.Log($"Config file loaded successfully: {configFilePath}");

            return (mConfig["ServerIP"], int.Parse(mConfig["ServerPort"]));
            
        }
        else
        {
            Debug.LogError($"Config file not found: {configFilePath}");

            return (String.Empty, -1);
        }
    }
#endif
}