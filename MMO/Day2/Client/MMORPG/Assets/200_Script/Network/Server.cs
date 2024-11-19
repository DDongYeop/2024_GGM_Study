using System;
using System.Collections.Generic;
using NetBase;

namespace Server
{
    public interface IPacketSerializable
    {
        void Serialize(PacketBase packet);
        void Deserialize(PacketBase packet);
    }
public static class Constants
{
	public const int Name_Length = 64;
}
public enum EObjectType
{
    None,
    Pc,
    Npc,
    Item,
    Prop,
    Max,
    
}

public enum EMoveResult
{
    None = 0,
    // 성공
    Success,
    // 이동 성공

    // 실패 원인들
    Failed_Obstacle,
    // 장애물로 인한 실패
    Failed_OutOfBounds,
    // 경계를 벗어남
    Failed_NoPath,
    // 경로가 없음
    Failed_Blocked,
    // 다른 유닛에 의해 막힘
    Failed_Terrain,
    // 지형으로 인한 실패 (예: 물/용암)
    Failed_Exhausted,
    // 스태미나/행동력 부족
    Failed_Stunned,
    // 기절 상태
    Failed_Rooted,
    // 속박 상태
    Failed_Frozen,
    // 빙결 상태
    Failed_Feared,
    // 공포 상태
    Failed_Charmed,
    // 매혹 상태
    Failed_Overencumbered,
    // 과도한 무게로 인한 실패
    Failed_NoFlyingAbility,
    // 비행 능력 부족 (공중 이동 시)
    Failed_NoSwimmingAbility,
    // 수영 능력 부족 (수중 이동 시)
    Failed_Cooldown,
    // 이동 능력 쿨다운 중
    Failed_InsufficientResource,
    // 자원 부족 (마나 등)
    Failed_TargetUnreachable,
    // 목표 지점에 도달 불가능
    Failed_InvalidMove,
    // 유효하지 않은 이동

    // 특수 상황
    Partial_Success,
    // 부분적 성공 (일부 거리만 이동)
    Teleported,
    // 텔레포트로 이동 (일반 이동과 다른 경우)
    Pushed,
    // 밀려남 (강제 이동)
    Pulled,
    // 끌려감 (강제 이동)

    Max
}

public enum PacketType
{
    None,
    HeartbeatReq,
    ResourceLoadCompleteReq,
    ResourceLoadCompleteAck,
    ZoneUpdateAck,
    MoveReq,
    MoveAck,
    Max,
}


public struct FLocation : IPacketSerializable
{
    public float X;
    public float Y;
    public float Z;
    public FLocation(
        float x,
        float y,
        float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    public static FLocation Default => new FLocation
    {
        X = 0f,
        Y = 0f,
        Z = 0f
    };
    public void Serialize(NetBase.PacketBase PacketBase)
    {
        PacketBase.Write(X);
        PacketBase.Write(Y);
        PacketBase.Write(Z);
    }
    public void Deserialize(NetBase.PacketBase PacketBase)
    {
        X = PacketBase.Read<float>();
        Y = PacketBase.Read<float>();
        Z = PacketBase.Read<float>();
    }
}

public struct PcInfoBr : IPacketSerializable
{
    public int Index;
    public char[] Name;
    public FLocation Pos;
    public FLocation Dest;
    public float Direction;
    public float AttackSpeed;
    public float MoveSpeed;
    public float CastingSpeed;
    public int Hp;
    public int Mp;
    public int StatusEffect;
    public PcInfoBr(
        int index,
        char[] name,
        FLocation pos,
        FLocation dest,
        float direction,
        float attackspeed,
        float movespeed,
        float castingspeed,
        int hp,
        int mp,
        int statuseffect)
    {
        this.Index = index;
        this.Name = new char[Constants.Name_Length];
        Array.Copy(name, this.Name, Constants.Name_Length);
        this.Pos = pos;
        this.Dest = dest;
        this.Direction = direction;
        this.AttackSpeed = attackspeed;
        this.MoveSpeed = movespeed;
        this.CastingSpeed = castingspeed;
        this.Hp = hp;
        this.Mp = mp;
        this.StatusEffect = statuseffect;
    }
    public static PcInfoBr Default => new PcInfoBr
    {
        Index = 0,
        Name = new char[Constants.Name_Length],
        Pos = default,
        Dest = default,
        Direction = 0f,
        AttackSpeed = 0f,
        MoveSpeed = 0f,
        CastingSpeed = 0f,
        Hp = 0,
        Mp = 0,
        StatusEffect = 0
    };
    public void Serialize(NetBase.PacketBase PacketBase)
    {
        PacketBase.Write(Index);
        foreach (var item in Name)
        {
            PacketBase.Write(item);
        }
        PacketBase.Write(Pos);
        PacketBase.Write(Dest);
        PacketBase.Write(Direction);
        PacketBase.Write(AttackSpeed);
        PacketBase.Write(MoveSpeed);
        PacketBase.Write(CastingSpeed);
        PacketBase.Write(Hp);
        PacketBase.Write(Mp);
        PacketBase.Write(StatusEffect);
    }
    public void Deserialize(NetBase.PacketBase PacketBase)
    {
        Index = PacketBase.Read<int>();
        for (int i = 0; i < Constants.Name_Length; i++)
        {
            Name[i] = PacketBase.Read<char>();
        }
        Pos = PacketBase.Read<FLocation>();
        Dest = PacketBase.Read<FLocation>();
        Direction = PacketBase.Read<float>();
        AttackSpeed = PacketBase.Read<float>();
        MoveSpeed = PacketBase.Read<float>();
        CastingSpeed = PacketBase.Read<float>();
        Hp = PacketBase.Read<int>();
        Mp = PacketBase.Read<int>();
        StatusEffect = PacketBase.Read<int>();
    }
}

public struct RemoveBr : IPacketSerializable
{
    public int Index;
    public EObjectType ObjectType;
    public RemoveBr(
        int index,
        EObjectType objecttype)
    {
        this.Index = index;
        this.ObjectType = objecttype;
    }
    public static RemoveBr Default => new RemoveBr
    {
        Index = 0,
        ObjectType = default
    };
    public void Serialize(NetBase.PacketBase PacketBase)
    {
        PacketBase.Write(Index);
        PacketBase.Write(ObjectType);
    }
    public void Deserialize(NetBase.PacketBase PacketBase)
    {
        Index = PacketBase.Read<int>();
        ObjectType = PacketBase.Read<EObjectType>();
    }
}

public struct MoveBr : IPacketSerializable
{
    public int Index;
    public EObjectType ObjectType;
    public FLocation Pos;
    public FLocation Dest;
    public MoveBr(
        int index,
        EObjectType objecttype,
        FLocation pos,
        FLocation dest)
    {
        this.Index = index;
        this.ObjectType = objecttype;
        this.Pos = pos;
        this.Dest = dest;
    }
    public static MoveBr Default => new MoveBr
    {
        Index = 0,
        ObjectType = default,
        Pos = default,
        Dest = default
    };
    public void Serialize(NetBase.PacketBase PacketBase)
    {
        PacketBase.Write(Index);
        PacketBase.Write(ObjectType);
        PacketBase.Write(Pos);
        PacketBase.Write(Dest);
    }
    public void Deserialize(NetBase.PacketBase PacketBase)
    {
        Index = PacketBase.Read<int>();
        ObjectType = PacketBase.Read<EObjectType>();
        Pos = PacketBase.Read<FLocation>();
        Dest = PacketBase.Read<FLocation>();
    }
}

namespace C2SInGame
{
    public struct HeartbeatReq
    {
    }
    public struct ResourceLoadCompleteReq
    {
    }
    public struct ResourceLoadCompleteAck
    {
        public int PcIndex;
        public void Serialize(NetBase.PacketBase PacketBase)
        {
            PacketBase.Write(PcIndex);
        }
        public void Deserialize(NetBase.PacketBase PacketBase)
        {
            PcIndex = PacketBase.Read<int>();
        }
    }
    public struct ZoneUpdateAck
    {
        public List<PcInfoBr> PcEnters;
        public List<MoveBr> Moves;
        public List<RemoveBr> Removes;
        public void Serialize(NetBase.PacketBase PacketBase)
        {
            PacketBase.Write(PcEnters);
            PacketBase.Write(Moves);
            PacketBase.Write(Removes);
        }
        public void Deserialize(NetBase.PacketBase PacketBase)
        {
            PcEnters = PacketBase.Read<List<PcInfoBr>>();
            Moves = PacketBase.Read<List<MoveBr>>();
            Removes = PacketBase.Read<List<RemoveBr>>();
        }
    }
    public struct MoveReq
    {
        public float Direction;
        public FLocation Dest;
        public bool DashFlag;
        public void Serialize(NetBase.PacketBase PacketBase)
        {
            PacketBase.Write(Direction);
            PacketBase.Write(Dest);
            PacketBase.Write(DashFlag);
        }
        public void Deserialize(NetBase.PacketBase PacketBase)
        {
            Direction = PacketBase.Read<float>();
            Dest = PacketBase.Read<FLocation>();
            DashFlag = PacketBase.Read<bool>();
        }
    }
    public struct MoveAck
    {
        public EMoveResult Result;
        public void Serialize(NetBase.PacketBase PacketBase)
        {
            PacketBase.Write(Result);
        }
        public void Deserialize(NetBase.PacketBase PacketBase)
        {
            Result = PacketBase.Read<EMoveResult>();
        }
    }
}

}
