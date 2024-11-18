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
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터를 제어하는 메인 클래스
/// </summary>
public class PC : MonoBehaviour
{
    // 플레이어의 고유 인덱스
    public int Index;
    // 캐릭터의 3D 모델을 포함하는 Transform
    public Transform ModelTransform;
    
    // 최종적으로 계산된 이동 벡터
    private Vector3 mResultMoveVector;
    // 위치 전송을 위한 코루틴 참조
    private Coroutine mSendPositionCoroutine;
    // 위치 전송 간격을 정의하는 대기 시간 (0.3초)
    private WaitForSeconds mSendWaitTime = new WaitForSeconds(0.3f);
    
    #region Unity 기능 정의
    
    /// <summary>
    /// 매 프레임마다 캐릭터의 이동을 처리합니다.
    /// </summary>
    private void Update()
    {
        // 이동 벡터를 계산하고 위치를 업데이트
        mResultMoveVector = CalculateMoveVector();
        transform.position += mResultMoveVector * Time.deltaTime;
    }

    /// <summary>
    /// 키보드 입력을 기반으로 이동 방향 벡터를 계산합니다.
    /// </summary>
    /// <returns>정규화된 이동 방향 벡터</returns>
    private Vector3 CalculateMoveVector()
    {
        Vector3 moveVector = Vector3.zero;
        
        // WASD 키 입력에 따라 이동 벡터 설정
        if (Input.GetKey(KeyCode.W) == true)
        {
            moveVector += Vector3.up;
        }
        if (Input.GetKey(KeyCode.S) == true)
        {
            moveVector += Vector3.down;
        }
        if (Input.GetKey(KeyCode.A) == true)
        {
            moveVector += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) == true)
        {
            moveVector += Vector3.right;
        }

        // 대각선 이동 시 속도가 빨라지는 것을 방지하기 위해 정규화
        //return moveVector;
        //TODO: 1
        return moveVector.normalized;
    }

    #endregion

    #region 고유 기능 정의

    /// <summary>
    /// 캐릭터를 초기화하고 위치 전송을 시작합니다.
    /// </summary>
    public void Initialize()
    {
        gameObject.SetActive(true);
        
        // 서버로 위치 전송을 시작하는 코루틴 실행
        //TODO: 3
        mSendPositionCoroutine = StartCoroutine(CoSendPosition());
    }

    /// <summary>
    /// 캐릭터의 위치를 지정된 좌표로 설정합니다.
    /// </summary>
    /// <param name="position">설정할 위치 벡터</param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 주기적으로 서버에 캐릭터의 위치 정보를 전송하는 코루틴
    /// </summary>
    private IEnumerator CoSendPosition()
    {
        while (true)
        {
            // 설정된 대기 시간만큼 대기
            yield return mSendWaitTime;
            
            // 현재 위치와 회전값 획득
            var playerDestPosition = transform.position;
            var playerRotation = ModelTransform.rotation.y;

            // 서버로 전송할 이동 요청 패킷 생성
            MoveReq moveReq = new MoveReq 
            { 
                Direction = playerRotation, 
                Dest = playerDestPosition.Vector3ToFLocation(), 
                DashFlag = false 
            };
            
            // 서버로 이동 요청 전송
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
    /// </summary>
    public void OnDrawGizmos()
    {
        //TODO: 4
        return;

        // 최대 개수를 초과하는 위치 정보 제거
        while (mSendPositionQueue.Count > MaxDebugPositionCount)
        {
            mSendPositionQueue.Dequeue();
        }
        
        // 저장된 모든 위치에 빨간 구체로 표시
        foreach (var serverPosition in mSendPositionQueue)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(serverPosition, DebugPositionSize);
        }
    }
#endif

    /// <summary>
    /// 디버그용 위치 정보를 큐에 추가합니다.
    /// UNITY_EDITOR 조건부 컴파일 지시문을 사용합니다.
    /// </summary>
    /// <param name="position">추가할 위치 벡터</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void EnqueuePosition(Vector3 position)
    {
        mSendPositionQueue.Enqueue(position);
    }

    #endregion
}