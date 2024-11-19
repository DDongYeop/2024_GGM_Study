/*
 * PlayerCharacter에 관한 코드를 작성합니다.
 * 캐릭터의 상태변화, 이동 등 기본적인 동작을 정의합니다.
 * 서버와의 통신을 통해 위치 동기화를 수행합니다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.C2SInGame;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터를 제어하는 메인 클래스
/// </summary>
public class PC : MonoBehaviour
{
    // 플레이어의 고유 인덱스
    public int Index;
    // 현재 플레이어가 로컬 플레이어인지 여부
    public bool MyPC = false;
    // 캐릭터가 이동할 목표 위치
    public Vector3 TargetPosition { get; private set; }
    // 서버로부터 전송받은 PC의 위치 큐
    public Queue<Vector3> TargetPositionQueue = new Queue<Vector3>();

    // 캐릭터의 3D 모델을 포함하는 Transform
    public Transform ModelTransform;
    // 캐릭터 모델의 회전값
    public Vector3 ModelRatation;

    // 캐릭터 애니메이션 컨트롤러
    public PC_Animation PC_Animation;

    // 위치 전송을 위한 코루틴 참조
    private Coroutine mSendPositionCoroutine;
    // 위치 전송 간격을 정의하는 대기 시간 (0.3초)
    private WaitForSeconds mSendWaitTime = new WaitForSeconds(0.3f);

    // 8방향 이동을 위한 방향 벡터 상수
    private Vector3 vector45 = new Vector3(1, 1, 0);
    private Vector3 vector135 = new Vector3(1, -1, 0);
    private Vector3 vector225 = new Vector3(-1, -1, 0);
    private Vector3 vector315 = new Vector3(-1, 1, 0);

    #region Unity 기능 정의

    /// <summary>
    /// 매 프레임마다 캐릭터의 이동을 처리합니다.
    /// 로컬 플레이어와 다른 플레이어의 이동을 구분하여 처리합니다.
    /// </summary>
    private void Update()
    {
        // 로컬 플레이어인 경우에만 직접 이동 처리
        if (MyPC == true)
        {
            // 이동 벡터를 계산하고 위치를 업데이트
            ModelRatation = CalculateMoveVector();
            MoveMyPlayer();
            RotatePlayer();
        }
        else
        {
            // 다른 플레이어의 경우 서버에서 받은 위치로 이동
            MoveOtherPC();
        }
    }

    /// <summary>
    /// WASD 키보드 입력을 기반으로 이동 방향 벡터를 계산합니다.
    /// </summary>
    /// <returns>정규화된 이동 방향 벡터</returns>
    private Vector3 CalculateMoveVector()
    {
        Vector3 moveVector = Vector3.zero;

        // WASD 키 입력에 따라 이동 벡터 설정
        if (Input.GetKey(KeyCode.W)) moveVector += Vector3.up;
        if (Input.GetKey(KeyCode.S)) moveVector += Vector3.down;
        if (Input.GetKey(KeyCode.A)) moveVector += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveVector += Vector3.right;

        return moveVector;
    }

    /// <summary>
    /// 로컬 플레이어의 실제 이동을 처리합니다.
    /// </summary>
    private void MoveMyPlayer()
    {
        transform.position += ModelRatation.normalized * Time.deltaTime;
    }

    /// <summary>
    /// 플레이어의 이동 방향에 따라 적절한 애니메이션을 설정합니다.
    /// 8방향 이동에 따른 애니메이션을 처리합니다.
    /// </summary>
    private void RotatePlayer()
    {
        if (ModelRatation == Vector3.zero)
            PC_Animation.SetAnimation(PC_AnimationStatus.Idle);
        else if (ModelRatation == Vector3.up)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run0);
        else if (ModelRatation == vector45)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run45);
        else if (ModelRatation == Vector3.right)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run90);
        else if (ModelRatation == vector135)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run135);
        else if (ModelRatation == Vector3.down)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run180);
        else if (ModelRatation == vector225)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run225);
        else if (ModelRatation == Vector3.left)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run270);
        else if (ModelRatation == vector315)
            PC_Animation.SetAnimation(PC_AnimationStatus.Run315);
    }

    /// <summary>
    /// 다른 플레이어의 이동을 처리합니다.
    /// 서버로부터 받은 위치 정보를 기반으로 부드러운 이동을 구현합니다.
    /// </summary>
    private void MoveOtherPC()
    {
        // 목표 위치가 없으면 대기 상태로 전환
        if (TargetPositionQueue.Count <= 0)
        {
            PC_Animation.SetAnimation(PC_AnimationStatus.Idle);
            return;
        }

        Vector3 nextDest = TargetPositionQueue.Peek();
        SetDestinationPosition(nextDest);

        Vector3 direction = TargetPosition - transform.position;

        // 목표 지점에 도달했는지 확인
        if (HasReachedDestination(direction))
        {
            TargetPositionQueue.Dequeue();
            return;
        }

        MoveTowardsDestination(direction);
    }

    /// <summary>
    /// 캐릭터의 목표 위치를 설정합니다.
    /// </summary>
    public void SetDestinationPosition(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
    }

    /// <summary>
    /// 목표 위치 큐를 초기화합니다.
    /// </summary>
    public void ResetTargetPositionQueue()
    {
        TargetPositionQueue.Clear();
    }

    /// <summary>
    /// 새로운 목표 위치를 큐에 추가합니다.
    /// </summary>
    public void EnqueueDestinationPosition(Vector3 targetPosition)
    {
        TargetPositionQueue.Enqueue(targetPosition);
    }

    /// <summary>
    /// 캐릭터가 목표 지점에 도달했는지 확인합니다.
    /// </summary>
    private bool HasReachedDestination(Vector3 direction)
    {
        return direction.sqrMagnitude <= 0.1f;
    }

    /// <summary>
    /// 목표 지점을 향해 캐릭터를 이동시킵니다.
    /// </summary>
    private void MoveTowardsDestination(Vector3 direction)
    {
        Vector3 deltaMoveDirection = CalculateMoveDirection(direction);
        UpdateOtherPC_Position(deltaMoveDirection);

        ModelRatation = new Vector3(
            (direction.x).GetNearestOne(),
            (direction.y).GetNearestOne(),
            (direction.z).GetNearestOne()
        );

        Debug.Log($"{ModelRatation}");
        RotatePlayer();
    }

    /// <summary>
    /// 이동 방향과 속도를 계산합니다.
    /// </summary>
    private Vector3 CalculateMoveDirection(Vector3 direction)
    {
        Vector3 deltaMoveDirection = direction.normalized * Time.deltaTime;

        if (direction.sqrMagnitude < deltaMoveDirection.sqrMagnitude)
        {
            deltaMoveDirection = direction;
            TargetPositionQueue.Dequeue();
        }

        return deltaMoveDirection;
    }

    /// <summary>
    /// 다른 플레이어의 위치를 업데이트합니다.
    /// </summary>
    private void UpdateOtherPC_Position(Vector3 deltaMoveDirection)
    {
        transform.position += deltaMoveDirection;
    }
    #endregion

    #region 고유 기능 정의

    /// <summary>
    /// 캐릭터를 초기화하고 위치 전송을 시작합니다.
    /// 애니메이션 초기화와 위치 전송 코루틴을 시작합니다.
    /// </summary>
    public void Initialize()
    {
        PC_Animation.Initialize();
        gameObject.SetActive(true);

        if (MyPC == true)
        {
            mSendPositionCoroutine = StartCoroutine(CoSendPosition());
        }
    }

    /// <summary>
    /// 캐릭터의 위치를 지정된 좌표로 설정합니다.
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 주기적으로 서버에 캐릭터의 위치 정보를 전송하는 코루틴입니다.
    /// 0.3초 간격으로 현재 위치와 회전값을 서버로 전송합니다.
    /// </summary>
    private IEnumerator CoSendPosition()
    {
        while (true)
        {
            yield return mSendWaitTime;

            var playerDestPosition = transform.position;
            var playerRotation = ModelTransform.rotation.y;

            MoveReq moveReq = new MoveReq
            {
                Direction = playerRotation,
                Dest = playerDestPosition.Vector3ToFLocation(),
                DashFlag = false
            };

            Manager.Net.SendMoveReq(moveReq);
        }
    }

    #endregion

    #region 디버깅 기능 정의

#if UNITY_EDITOR
    // 디버그 표시할 최대 위치 개수
    public int MaxDebugPositionCount;
    // 디버그 위치 표시 크기
    public float DebugPositionSize;
    // 전송된 위치들을 저장하는 큐
    private Queue<Vector3> mSendPositionQueue = new Queue<Vector3>();

    /// <summary>
    /// Unity Editor에서 위치 정보를 시각적으로 표시합니다.
    /// 빨간색 구체로 이동 경로를 시각화합니다.
    /// </summary>
    public void OnDrawGizmos()
    {
        while (mSendPositionQueue.Count > MaxDebugPositionCount)
        {
            mSendPositionQueue.Dequeue();
        }

        foreach (var serverPosition in mSendPositionQueue)
        {
            if (MyPC == true)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.blue;
            }

            Gizmos.DrawSphere(serverPosition, DebugPositionSize);
        }
    }
#endif

    /// <summary>
    /// 디버그용 위치 정보를 큐에 추가합니다.
    /// Unity Editor에서만 동작하는 조건부 컴파일 메서드입니다.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void EnqueuePosition(Vector3 position)
    {
        mSendPositionQueue.Enqueue(position);
    }

    #endregion
}