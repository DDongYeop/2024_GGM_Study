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
        //if (DashFlag == true)
        {
            Move(dest.X, dest.Y, dest.Z, sourceFilePath, sourceLineNumber);
            if (!isMoving)
            {
                lastMoveTime = currentTime;
                isMoving = true;
            }
        }
        //else
        //{
        //    COURSE direction = (COURSE)gameManager.FindPath(Pos, dest, this);

        //    if (direction != COURSE.NOWAY)
        //    {
        //        if (!isMoving)
        //        {
        //            lastMoveTime = currentTime;
        //            isMoving = true;
        //        }

        //        float deltaTime = (currentTime - lastMoveTime) / 1000f;
        //        float moveDistance = MoveSpeed * 1.2f * deltaTime;
        //        float maxMoveDistance = MoveSpeed * 2.0f * 0.1f;
        //        moveDistance = Math.Min(moveDistance, maxMoveDistance);

        //        CFLocation newPos = CalculateNewPosition(Pos, direction, moveDistance);

        //        // 충돌 검사
        //        if (!IsPositionOccupied(newPos))
        //        {
        //            Move(newPos.X, newPos.Y, newPos.Z, sourceFilePath, sourceLineNumber);
        //            lastMoveTime = currentTime;
        //        }
        //        else
        //        {
        //            // 충돌 발생 시 대체 경로 탐색
        //            COURSE alternativeDirection = FindAlternativeDirection(direction);
        //            if (alternativeDirection != COURSE.NOWAY)
        //            {
        //                newPos = CalculateNewPosition(Pos, alternativeDirection, moveDistance);
        //                if (!IsPositionOccupied(newPos))
        //                {
        //                    Move(newPos.X, newPos.Y, newPos.Z, sourceFilePath, sourceLineNumber);
        //                    lastMoveTime = currentTime;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        isMoving = false;
        //    }
        //}
        OnSpawn();
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