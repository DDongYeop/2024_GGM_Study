using NetBase;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.C2SInGame;
using Server;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Process
{
    public class Person
    {
        public int Id { get; set; }
        public int Id2 { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Id2: {Id2}, Name: {Name}";
        }
    }
    public interface IKeyComparer<T, U> : IComparer<U>
    {
        T GetKey(U obj);
    }

    class PersonIdComparer : IKeyComparer<int, Person>
    {
        public int Compare(Person x, Person y)
        {
            return x.Id.CompareTo(y.Id);
        }
        public int GetKey(Person x)
        {
            return x.Id;
        }
    }

    class PersonId2Comparer : IKeyComparer<int, Person>
    {
        public int Compare(Person x, Person y)
        {
            return x.Id2.CompareTo(y.Id2);
        }
        public int GetKey(Person x)
        {
            return x.Id2;
        }
    }
    public class IntKeyComparer : IKeyComparer<int, Person>
    {
        public int Compare(Person x, Person y)
        {
            return x.Id.CompareTo(y.Id);
        }

        public int GetKey(Person value)
        {
            return value.Id;
        }
    }

    // Packet Handler
    public class PacketHandler
    {
        int updateCount = 0;
        public static long GetTickCount64()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }
        private void LogError(string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            string fileName = System.IO.Path.GetFileName(file);
            Console.WriteLine($"Error: {message} (in {fileName}, line {line})");
        }

        private Dictionary<PacketType, Action<MemoryStream, Socket, int, long>> _packetHandlers;
        public PacketHandler(Action<PacketType, long> updateStatsMethod)
        {
            this.updatePacketStats = updateStatsMethod;
            _packetHandlers = new Dictionary<PacketType, Action<MemoryStream, Socket, int, long>>
            {
                { PacketType.HeartbeatReq, HandleHeartbeatReq },
                { PacketType.ResourceLoadCompleteReq, HandleResourceLoadCompleteReq },
                { PacketType.MoveReq, HandleMoveReq }
            };

            foreach (PacketType packetType in Enum.GetValues(typeof(PacketType)))
            {
                Program.packetStats[packetType] = new Program.PacketStats();
            }
        }
        private Action<PacketType, long> updatePacketStats;

        public void HandlePacket(MemoryStream packetStream, Socket handler, int ClientId, long PacketIndex)
        {
            packetStream.Position = 0;
            PacketHeader header = DeserializeHeader(packetStream);
            PacketType packetType = (PacketType)header.Type;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (_packetHandlers.TryGetValue(packetType, out Action<MemoryStream, Socket, int, long> handlerAction))
            {
                byte[] bodyData = new byte[packetStream.Length - PacketHeader.HeaderSize];
                packetStream.Read(bodyData, 0, bodyData.Length);
                MemoryStream bodyStream = new MemoryStream(bodyData);

                handlerAction(bodyStream, handler, ClientId, PacketIndex);
            }
            else
            {
                Console.WriteLine($"Unknown packet type: {packetType}");
            }

            stopwatch.Stop();
            Program.UpdatePacketStats(packetType, stopwatch.ElapsedMilliseconds);
        }

        public delegate void UpdateStatsDelegate(PacketType packetType, long processingTime);
        public UpdateStatsDelegate UpdatePacketStats { get; set; }

        private PacketHeader DeserializeHeader(MemoryStream stream)
        {
            byte[] sizeBytes = new byte[2];
            byte[] typeBytes = new byte[2];
            stream.Read(sizeBytes, 0, 2);
            stream.Read(typeBytes, 0, 2);

            return new PacketHeader
            {
                Size = BitConverter.ToUInt16(sizeBytes, 0),
                Type = (ushort)(PacketType)BitConverter.ToUInt16(typeBytes, 0)
            };
        }

        public void HandleUpdate()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            bool logFlag = false;

            if (updateCount % 50 == 0)
            {
                Console.WriteLine($"현재 스레드 ID: {threadId} Handling HandleUpdate");
                logFlag = true;
            }
            GameManager.Instance.UpdateAll(logFlag);

            if (updateCount > 50)
                updateCount = 0;
            updateCount++;
            stopwatch.Stop();
            Program.UpdatePacketStats(PacketType.None, stopwatch.ElapsedMilliseconds);
        }

        private void HandleHeartbeatReq(MemoryStream packetStream, Socket handler, int ClientId, long PacketIndex)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"현재 스레드 ID: {threadId} Handling HeartbeatReq [{PacketIndex}]");
            // HeartbeatReq 패킷에는 추가 데이터가 없으므로 별도의 파싱 불필요
        }

        private void SendAllCharacterInfo(Socket handler)
        {
            GameManager gameManager = GameManager.Instance;
            var broadcastPacket = new PacketBase();
            ZoneUpdateAck zoneUpdateAck = new ZoneUpdateAck
            {
                PcEnters = new List<PcInfoBr>(),
                Moves = new List<MoveBr>(),
                Removes = new List<RemoveBr>(),
            };

            // PC 정보 추가
            List<Pc> pcs = gameManager.GetAllPc();
            if (pcs != null)
            {
                foreach (Pc pc in pcs)
                {
                    PcInfoBr pcInfoBr = new PcInfoBr
                    {
                        Index = pc.Index,
                        Pos = new FLocation { X = pc.Pos.X, Y = pc.Pos.Y, Z = pc.Pos.Z },
                        Dest = new FLocation { X = pc.Dest.X, Y = pc.Dest.Y, Z = pc.Dest.Z },
                        Direction = pc.Direction,
                        AttackSpeed = pc.AttackSpeed,
                        MoveSpeed = pc.MoveSpeed,
                        CastingSpeed = pc.CastingSpeed,
                        Hp = pc.Hp,
                        Mp = pc.Mp,
                        StatusEffect = 12345
                    };
                    zoneUpdateAck.PcEnters.Add(pcInfoBr);
                }
            }

            broadcastPacket.Write(zoneUpdateAck);
            Program.SendPacket(handler, PacketType.ZoneUpdateAck, broadcastPacket.GetPacketData());
        }

        private void HandleResourceLoadCompleteReq(MemoryStream packetStream, Socket handler, int ClientId, long PacketIndex)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"현재 스레드 ID: {threadId} Handling ResourceLoadCompleteReq [{PacketIndex}]");
            // ResourceLoadCompleteReq 패킷에는 추가 데이터가 없으므로 별도의 파싱 불필요

            GameManager gameManager = GameManager.Instance;

            // 기존 Pc, Npc 정보를 해당 접속자에게 전송
            SendAllCharacterInfo(handler);

            Pc pc = gameManager.CreateObject<Pc>("Player");
            pc.Init();
            pc.Pos = new CFLocation()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            Console.WriteLine($"CreateObject Pc {pc.Index}");

            // SpawnCharacter 메서드 호출
            gameManager.SpawnCharacter(pc, pc.Pos);

            gameManager.AssociateClientIdWithPc(ClientId, pc);
            gameManager.PrintPoolStatus();

            var packet = new PacketBase();
            ResourceLoadCompleteAck resourceLoadCompleteAck = new ResourceLoadCompleteAck
            {
                PcIndex = pc.Index
            };
            packet.Write(resourceLoadCompleteAck);
            Program.SendPacket(handler, PacketType.ResourceLoadCompleteAck, packet.GetPacketData());

            var broadcastPacket = new PacketBase();
            ZoneUpdateAck zoneUpdateAck = new ZoneUpdateAck
            {
                PcEnters = new List<PcInfoBr>(),
                Moves = new List<MoveBr>(),
                Removes = new List<RemoveBr>(),
            };

            // 새로 접속한 PC 정보를 브로드 캐스팅 시킴
            PcInfoBr pcInfoBr = new PcInfoBr
            {
                Index = pc.Index,
                Name = new char[Constants.Name_Length],
                //RaceType = ERaceType.Human,
                Pos = new FLocation { X = pc.Pos.X, Y = pc.Pos.Y, Z = pc.Pos.Z },
                Dest = new FLocation { X = pc.Dest.X, Y = pc.Dest.Y, Z = pc.Dest.Z },
                Direction = pc.Direction,
                AttackSpeed = pc.AttackSpeed,
                MoveSpeed = pc.MoveSpeed,
                CastingSpeed = pc.CastingSpeed,
                Hp = pc.Hp,
                Mp = pc.Mp,
                StatusEffect = 12345
            };
            // Name 배열을 null 문자로 초기화
            Array.Clear(pcInfoBr.Name, 0, Constants.Name_Length);

            // pc.Name을 char 배열로 변환하여 할당
            if (pc.Name != null)
            {
                int nameLength = Math.Min(pc.Name.Length, Constants.Name_Length);
                pc.Name.CopyTo(0, pcInfoBr.Name, 0, nameLength);
            }

            zoneUpdateAck.PcEnters.Add(pcInfoBr);
            broadcastPacket.Write(zoneUpdateAck);
            SocketManager.BroadcastToAll(PacketType.ZoneUpdateAck, broadcastPacket.GetPacketData());
        }

        private void HandleMoveReq(MemoryStream packetStream, Socket handler, int ClientId, long PacketIndex)
        {
            long ticks = GetTickCount64();
            GameManager gameManager = GameManager.Instance;
            var pc = gameManager.GetPcByClientId(ClientId);
            if (pc == null)
                return;

            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"현재 스레드 ID: {threadId} Handling MoveReq [{PacketIndex}]");
            var packet = new PacketBase();

            byte[] packetData = packetStream.ToArray();
            Console.WriteLine($"Packet data length: {packetData.Length} bytes");
            packet.SetPacketData(packetData);  // 패킷 데이터 설정

            MoveReq moveReq = new MoveReq();
            moveReq = packet.Read<MoveReq>();  // ref 키워드 사용
            Console.WriteLine($"Move request: Direction: {moveReq.Direction}, Destination: ({moveReq.Dest.X}, {moveReq.Dest.Y}, {moveReq.Dest.Z})");

            pc.Dest = new CFLocation { X = moveReq.Dest.X, Y = moveReq.Dest.Y, Z = moveReq.Dest.Z };
            pc.DashFlag = moveReq.DashFlag;
            pc.Direction = moveReq.Direction;

            MoveAck moveAck = new MoveAck();
            moveAck.Result = EMoveResult.Success;

            {
                // 길찾기 성공
                pc.Dest = new CFLocation { X = moveReq.Dest.X, Y = moveReq.Dest.Y, Z = moveReq.Dest.Z };
                pc.Direction = moveReq.Direction;
                pc.DashFlag = moveReq.DashFlag;
                moveAck.Result = EMoveResult.Success;
                var responsePacket = new PacketBase();
                responsePacket.Write(moveAck);
                Program.SendPacket(handler, PacketType.MoveAck, responsePacket.GetPacketData());
            }

            // 모든 클라이언트에게 이동 정보 브로드캐스트
            var broadcastPacket = new PacketBase();
            ZoneUpdateAck zoneUpdateAck = new ZoneUpdateAck
            {
                PcEnters = new List<PcInfoBr> { },
                Moves = null,
                Removes = new List<RemoveBr> { },
            };
            zoneUpdateAck.Moves = new List<MoveBr>();

            List<Pc> pcs = gameManager.GetAllPc();
            if (pcs != null)
            {
                foreach (Pc _pc in pcs)
                {
                    MoveBr moveBr = new MoveBr
                    {
                        Index = _pc.Index, // Pc의 고유 ID를 Index로 사용
                        ObjectType = EObjectType.Pc, // Pc는 Player 타입으로 가정
                        Pos = new FLocation
                        {
                            X = _pc.Pos.X,
                            Y = _pc.Pos.Y,
                            Z = _pc.Pos.Z
                        },
                        Dest = new FLocation
                        {
                            X = _pc.Dest.X,
                            Y = _pc.Dest.Y,
                            Z = _pc.Dest.Z
                        }
                    };

                    zoneUpdateAck.Moves.Add(moveBr);
                }
            }
            broadcastPacket.Write(zoneUpdateAck);
            SocketManager.BroadcastToAll(PacketType.ZoneUpdateAck, broadcastPacket.GetPacketData());

        }


    }
}
