using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using Server;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;

public enum CharacterState
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Death
}

public abstract class Character : CObject
{
    [DllImport("kernel32.dll")]

    static extern ulong GetTickCount64();
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public bool IsAlive { get; set; }
    public float AttackSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float CastingSpeed { get; set; }
    public CFLocation Dest { get; set; }
    public bool DashFlag { get; set; }
    private DateTime lastAttackTime;
    public CharacterState currentState;

    private ulong lastMoveTime;
    public int Level { get; set; }

    public float attackRange = 1.5f; // 공격 범위
    
    public Character target;

    private bool isMoving = false;
    public Character()
    {
        lastMoveTime = GetTickCount64();
        IsAlive = true;
        lastAttackTime = DateTime.MinValue;
    }
    private float CalculateDistance(CFLocation pos1, CFLocation pos2)
    {
        return (float)Math.Sqrt(
            Math.Pow(pos2.X - pos1.X, 2) +
            Math.Pow(pos2.Z - pos1.Z, 2));
    }
    public virtual void Move(float x, float y, float z, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        CFLocation newPos = new CFLocation { X = x, Y = y, Z = z };
        if (Pos.X == newPos.X && Pos.Y == newPos.Y && Pos.Z == newPos.Z)
            return;

        float distance = CalculateDistance(Pos, newPos);
        if (distance > 3f)
        {
            Console.WriteLine($"3f 이상 점프! {distance} ({Pos.X}, {Pos.Y}, {Pos.Z})->({newPos.X}, {newPos.Y}, {newPos.Z}) {sourceFilePath} / {sourceLineNumber}");
        }
        // 이전 위치 저장
        CFLocation previousPos = new CFLocation { X = Pos.X, Y = Pos.Y, Z = Pos.Z };
        OnDespawn();
        Pos = new CFLocation { X = x, Y = y, Z = z };
        // 이동 후 브로드캐스트
        GameManager.Instance.BroadcastMove(this);

        Console.WriteLine($"{Name} moved to ({Pos.X}, {Pos.Y}, {Pos.Z})");
        OnSpawn();
    }

    public virtual void MoveTowardsDestination(CFLocation dest, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        GameManager gameManager = GameManager.Instance;
        ulong currentTime = GetTickCount64();
        OnDespawn();

        if (!isMoving)
        {
            lastMoveTime = currentTime;
            isMoving = true;
        }

        // 시간 기반 이동 거리 계산
        float deltaTime = (currentTime - lastMoveTime) / 1000f;
        float moveDistance = MoveSpeed * 1.2f * deltaTime;
        float maxMoveDistance = MoveSpeed * 2.0f * 0.1f;
        moveDistance = Math.Min(moveDistance, maxMoveDistance);

        // 현재 위치에서 목적지까지의 방향과 거리 계산
        float dx = dest.X - Pos.X;
        float dy = dest.Y - Pos.Y;
        float dz = dest.Z - Pos.Z;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance > 0)
        {
            // 실제 이동할 거리 계산 (목적지까지의 거리와 moveDistance 중 작은 값)
            float actualMoveDistance = Math.Min(moveDistance, distance);

            // 방향 벡터 정규화 및 새로운 위치 계산
            float nx = dx / distance;
            float ny = dy / distance;
            float nz = dz / distance;

            float newX = Pos.X + (nx * actualMoveDistance);
            float newY = Pos.Y + (ny * actualMoveDistance);
            float newZ = Pos.Z + (nz * actualMoveDistance);

            // 새 위치로 이동
            Move(newX, newY, newZ, sourceFilePath, sourceLineNumber);
            lastMoveTime = currentTime;
        }
        else
        {
            isMoving = false;
        }

        OnSpawn();
    }

    public EAttackResult TakeDamage(int damage, out int out_damage, out bool KillFlag)
    {
        KillFlag = false;
        // 크리티컬 히트 확률 계산 (10%)
        Random random = new Random();
        double criticalChance = random.NextDouble();
        bool isCriticalHit = criticalChance <= 0.1;

        if (isCriticalHit)
        {
            out_damage = (int)(damage * 1.5f);
            Hp -= out_damage;
        }
        else
        {
            out_damage = damage;
            Hp -= out_damage;
        }
        if (Hp <= 0)
        {
            Hp = 0;
            //OnDeath();
            KillFlag = true;
        }
        //GameManager.Instance.BroadcastFigure(this, EFigureType.HP, Hp);

        if (isCriticalHit)
        {
            return EAttackResult.Critical_hit;
        }
        else
        {
            return EAttackResult.Hit;
        }
    }

    public override void Update()
    {
        base.Update();
        // Character 특화 업데이트 로직
    }


    public virtual void OnSpawn()
    {
        GameManager gameManager = GameManager.Instance;
    }

    public virtual void OnDespawn()
    {
        GameManager gameManager = GameManager.Instance;
    }


}