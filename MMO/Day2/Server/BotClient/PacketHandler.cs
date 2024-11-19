
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using NetBase;
using Server.C2SInGame;
using Server;

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
                { PacketType.AttackAck, HandleAttackAck },
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

        private void HandleAttackAck(MemoryStream packetStream, Socket handler)
        {
            Console.WriteLine("Handling AttackAck");

            byte[] packetData = packetStream.ToArray();
            Console.WriteLine($"Packet data length: {packetData.Length} bytes");

            var packet = new PacketBase();
            packet.SetPacketData(packetData);  // 패킷 데이터 설정

            AttackAck attackAck = packet.Read<AttackAck>();
            Console.WriteLine($"AttackAck Results:");

            foreach (var result in attackAck.Results)
            {
                Console.WriteLine($"  Result: {result.Result.ToString()}, ObjectType: {result.TarObjectType.ToString()}, ObjectIndex: {result.TarObjectIndex}, Damage: {result.TarDamage}");
            }
        }
    }
}
