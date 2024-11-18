//#define TimeLogger

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Security.Principal;
using System.Linq;
using NetBase;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Server.C2SInGame;
using Server;
using System.Security.AccessControl;
public static class Constants
{
    public const int Name_Length = 64;
    public const int DebugMsg_Length = 1024;
}

public class GameManager : ISingleton<GameManager>
{
    private static readonly TimeSpan NPC_REMOVE_DELAY = TimeSpan.FromSeconds(10); // 5초 후에 NPC 제거
    private const int TIMEOUT_MILLISECONDS = 5000; // 5초, 필요에 따라 조정
    private const int MapSize = 400;
    private static GameManager _instance;
    private System.Diagnostics.Stopwatch aStarStopwatch = new System.Diagnostics.Stopwatch();

    private static SpinLock _lock = new SpinLock();

    private Dictionary<int, CObject> activeObjects = new Dictionary<int, CObject>();
    private Dictionary<int, CObject> activeClientIdPc = new Dictionary<int, CObject>();
    private int nextIndex = 1;
    private ObjectPool<Pc> pcPool;

    // Parameterless constructor
    public GameManager()
    {
        pcPool = new ObjectPool<Pc>(1000, 3000, 50);
    }

    public static GameManager GetGameManager()
    {
        return new GameManager();
    }

    public static new GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GameManager();
                    }
                }
            }
            return _instance;
        }
    }

    // Parameterized constructor


    public int GetMapSize()
    {
        return MapSize;
    }

    public void SpawnCharacter(Character character, CFLocation position)
    {
        character.Pos = position;
        character.OnSpawn();
        activeObjects[character.Index] = character;
        Console.WriteLine($"Spawned character {character.Name} at position ({position.X}, {position.Y}, {position.Z})");
    }

    public void DespawnCharacter(Character character)
    {
        character.OnDespawn();
        Console.WriteLine($"Despawned character {character.Name}");
    }
    public T CreateObject<T>(string name) where T : CObject, new()
    {
        T obj;
        if (typeof(T) == typeof(Pc))
            obj = (T)(object)pcPool.Get();
        else
            obj = new T();

        if (obj == null)
            obj = new T();

        obj.Name = name;
        obj.Index = nextIndex++;

        activeObjects[obj.Index] = obj;

        return obj;
    }

    public void RemoveObject(int index)
    {
        if (activeObjects.TryGetValue(index, out CObject obj))
        {
            activeObjects.Remove(index);

            if (obj is Pc pc)
                pcPool.Return(pc);
        }
    }

    public CObject GetObject(int index)
    {
        return activeObjects.TryGetValue(index, out CObject obj) ? obj : null;
    }

    public void UpdateAll(bool logFlag)
    {
        int pcCount = 0;
        int npcCount = 0;
        int itemCount = 0;
        Console.Write(".");
        //lock (activeObjects)
        {
            var objectsToUpdate = new List<CObject>();
            foreach (var obj in activeObjects.Values)
            {
                if (obj != null)
                    objectsToUpdate.Add(obj);
            }

            foreach (var obj in objectsToUpdate)
            {
                if (obj != null)
                {
                    obj.Update();
                    if (logFlag)
                    {
                        if (obj is Pc) pcCount++;
                    }
                }
            }

            if (logFlag)
                Console.WriteLine($"PC count: {pcCount} NPC count: {npcCount} Item count: {itemCount}");
        }

    }

    public void PrintPoolStatus()
    {
        Console.WriteLine($"PC Pool size: {pcPool.Count}");
    }

    // Method to associate a Socket with a Pc object
    public void AssociateClientIdWithPc(int ClientId, Pc pc)
    {
        activeClientIdPc[ClientId] = pc;
    }
    public List<Pc> GetAllPc()
    {
        List<Pc> allPcs = new List<Pc>();

        foreach (var kvp in activeClientIdPc)
        {
            if (kvp.Value is Pc pc)
            {
                allPcs.Add(pc);
            }
        }

        return allPcs;
    }

    // Method to get Pc object by Socket
    public Pc GetPcByClientId(int ClientId)
    {
        return activeClientIdPc.TryGetValue(ClientId, out CObject obj) ? obj as Pc : null;
    }

    public void RemovePcByClientId(int clientId)
    {
        if (activeClientIdPc.TryGetValue(clientId, out CObject obj))
        {
            if (obj is Pc pc)
            {
                // Pc 객체 제거
                RemoveObject(pc.Index);
                activeClientIdPc.Remove(clientId);
                Console.WriteLine($"Removed Pc with index {pc.Index} for client ID {clientId}");
            }
        }
    }

    public Character GetNearestPc(CFLocation position, float range)
    {
        Pc nearestPc = null;
        float nearestDistance = float.MaxValue;

        foreach (var kvp in activeClientIdPc)
        {
            if (kvp.Value is Pc pc)
            {
                float distance = CalculateDistance(position, pc.Pos);
                if (distance <= range && distance < nearestDistance)
                {
                    nearestPc = pc;
                    nearestDistance = distance;
                }
            }
        }

        return nearestPc;
    }
    public Character GetNearestEnemy(Character source, float range)
    {
        // Pc와 Npc 목록을 모두 가져옵니다.
        List<Character> allCharacters = new List<Character>();
        allCharacters.AddRange(GetAllPc());

        return allCharacters
            .Where(c => c != source && c.IsAlive && CalculateDistance(source.Pos, c.Pos) <= range)
            .OrderBy(c => CalculateDistance(source.Pos, c.Pos))
        .FirstOrDefault();
    }

    private float CalculateDistance(CFLocation pos1, CFLocation pos2)
    {
        float dx = pos1.X - pos2.X;
        float dy = pos1.Y - pos2.Y;
        float dz = pos1.Z - pos2.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public void BroadcastMove(Character character)
    {
        var broadcastPacket = new PacketBase();
        ZoneUpdateAck zoneUpdateAck = new ZoneUpdateAck
        {
            PcEnters = new List<PcInfoBr>(),
            Moves = new List<MoveBr>
            {
                new MoveBr
                {
                    Index = character.Index,
                    ObjectType = character is Pc ? EObjectType.Pc : EObjectType.Npc,
                    Pos = new FLocation { X = character.Pos.X, Y = character.Pos.Y, Z = character.Pos.Z },
                    Dest = new FLocation { X = character.Dest.X, Y = character.Dest.Y, Z = character.Dest.Z }
                }
            },
            Removes = new List<RemoveBr>()            
        };

        broadcastPacket.Write(zoneUpdateAck);
        SocketManager.BroadcastToAll(PacketType.ZoneUpdateAck, broadcastPacket.GetPacketData());
    }

    public void BroadcastRemove(Character character)
    {
        var broadcastPacket = new PacketBase();
        ZoneUpdateAck zoneUpdateAck = new ZoneUpdateAck
        {
            PcEnters = new List<PcInfoBr>(),
            Moves = new List<MoveBr>(),
            Removes = new List<RemoveBr>
            {
                new RemoveBr
                {
                    Index = character.Index,
                    ObjectType = character is Pc ? EObjectType.Pc : EObjectType.Npc
                }
            }
        };

        broadcastPacket.Write(zoneUpdateAck);
        SocketManager.BroadcastToAll(PacketType.ZoneUpdateAck, broadcastPacket.GetPacketData());
    }

    private char[] ConvertToCharArray(string message)
    {
        char[] result = new char[Constants.DebugMsg_Length];
        if (message.Length > Constants.DebugMsg_Length)
        {
            message = message.Substring(0, Constants.DebugMsg_Length);
        }
        message.CopyTo(0, result, 0, message.Length);
        return result;
    }

    // 모든 활성 객체의 ID를 반환하는 메서드
    public IEnumerable<int> GetAllActiveObjectIds()
    {
        return activeObjects.Keys;
    }

    // 현재 활성화된 객체 수를 반환하는 메서드 (모니터링용)
    public int GetActiveObjectCount()
    {
        return activeObjects.Count;
    }

    // 옵션: 객체 타입별 카운트를 반환하는 메서드
    public Dictionary<string, int> GetActiveObjectCountByType()
    {
        return activeObjects
            .Values
            .GroupBy(obj => obj.GetType().Name)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            );
    }
}
