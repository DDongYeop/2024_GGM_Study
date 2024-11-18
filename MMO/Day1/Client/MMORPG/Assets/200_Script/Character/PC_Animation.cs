using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_Animation : MonoBehaviour
{
    private Animator PC_animator;
    private Dictionary<PC_AnimationStatus, string> PC_AnimationDic = new Dictionary<PC_AnimationStatus, string>()
    {
        {PC_AnimationStatus.Idle, "Idle180" },
        {PC_AnimationStatus.Run0, "Run.Run0" },
        {PC_AnimationStatus.Run45, "Run.Run45" },
        {PC_AnimationStatus.Run90, "Run.Run90" },
        {PC_AnimationStatus.Run135, "Run.Run135" },
        {PC_AnimationStatus.Run180, "Run.Run180" },
        {PC_AnimationStatus.Run225, "Run.Run225" },
        {PC_AnimationStatus.Run270, "Run.Run270" },
        {PC_AnimationStatus.Run315, "Run.Run315" },
    };

    public void Initialize()
    {
        PC_animator = GetComponent<Animator>();
    }

    public void SetAnimation(PC_AnimationStatus playerStatus)
    {
        PC_animator.Play(PC_AnimationDic[playerStatus]);
    }
}
