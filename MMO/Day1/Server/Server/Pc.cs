using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Server;


// Pc 클래스
public class Pc : Character
{
    public int Experience { get; set; }
    public int ExperienceToNextLevel { get; set; }

    public bool AutoPlayEnabled { get; set; }

    public Pc() : base()
    {
        Init();
    }
    public void Init()
    {
        Level = 1;
        Name = "Player_" + Index;
        Experience = 0;
        ExperienceToNextLevel = 100;
        Attack = 10;
        Defense = 5;
        MaxHp = 300;
        Hp = MaxHp;
        MaxMp = 100;
        Mp = MaxMp;
        AttackSpeed = 1.0f;
        MoveSpeed = 3.0f;
        CastingSpeed = 1.0f;
        AutoPlayEnabled = false;
    }

    public override void Update()
    {
        base.Update();
        if (!AutoPlayEnabled && (Pos.X != Dest.X || Pos.Y != Dest.Y || Pos.Z != Dest.Z))
        {
            MoveTowardsDestination(Dest);
        }
    }
}


