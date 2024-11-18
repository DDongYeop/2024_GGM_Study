using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

/// <summary>
/// 스핀락(SpinLock) 구현 클래스
/// 멀티스레드 환경에서 공유 자원에 대한 동기화를 제공하는 저수준 잠금 메커니즘
/// 다른 스레드가 잠금을 해제할 때까지 계속 시도하는 방식으로 동작
/// </summary>
public class SpinLock
{
    /// <summary>
    /// 잠금 상태를 나타내는 플래그
    /// 0: 잠금 해제 상태
    /// 1: 잠금 상태
    /// volatile 키워드로 여러 스레드 간 가시성 보장
    /// </summary>
    private volatile int m_lock_flag;

    /// <summary>
    /// 잠금 획득 시도 횟수를 추적하는 카운터
    /// 데드락이나 과도한 경합 상태 감지에 사용
    /// </summary>
    private int m_iLockCount;

    /// <summary>
    /// 현재 잠금을 소유한 소스 파일 이름 저장
    /// 디버깅 및 문제 추적을 위해 사용
    /// </summary>
    private string m_pcsLockFileName;

    /// <summary>
    /// 현재 잠금을 획득한 소스 코드의 라인 번호
    /// 디버깅 및 문제 추적을 위해 사용
    /// </summary>
    private int m_iLockLine;

    /// <summary>
    /// SpinLock 클래스의 생성자
    /// 초기 상태를 잠금 해제 상태로 설정
    /// </summary>
    public SpinLock()
    {
        // 잠금 플래그를 0(잠금 해제)으로 초기화
        Interlocked.Exchange(ref m_lock_flag, 0);
        // 잠금 시도 카운터 초기화
        m_iLockCount = 0;
    }

    /// <summary>
    /// 잠금을 획득하는 메서드
    /// </summary>
    /// <param name="sourceFilePath">잠금을 요청한 소스 파일 경로 (컴파일러가 자동 주입)</param>
    /// <param name="sourceLineNumber">잠금을 요청한 소스 코드 라인 번호 (컴파일러가 자동 주입)</param>
    public void Lock([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        for (; ; )
        {
            // 잠금 시도 횟수가 임계값을 초과하면 경고 메시지 출력
            if (m_iLockCount > 1000000)
            {
                Console.WriteLine($"SpinLock::Lock Loop Cnt={m_iLockCount} [{m_pcsLockFileName}/{m_iLockLine}][{Path.GetFileName(sourceFilePath)}/{sourceLineNumber}]");
                m_iLockCount = 0;
            }

            // CAS(Compare-And-Swap) 연산으로 잠금 획득 시도
            // m_lock_flag가 0이면 1로 변경하고 이전 값(0) 반환
            if (Interlocked.CompareExchange(ref m_lock_flag, 1, 0) == 0)
            {
                break;  // 잠금 획득 성공
            }
            else
            {
                // 잠금 획득 실패 시 카운터 증가
                m_iLockCount++;
                // 다른 스레드에게 CPU 시간을 양보
                Thread.Yield();
            }
        }

        // 잠금 획득 후 상태 정보 업데이트
        m_iLockCount = 0;
        m_pcsLockFileName = sourceFilePath;
        m_iLockLine = sourceLineNumber;
    }

    /// <summary>
    /// 잠금을 해제하는 메서드
    /// m_lock_flag를 0으로 설정하여 다른 스레드가 잠금을 획득할 수 있게 함
    /// </summary>
    public void Unlock()
    {
        Interlocked.Exchange(ref m_lock_flag, 0);
    }
}