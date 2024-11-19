using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 제네릭 객체 풀 클래스
/// 자주 사용되는 객체를 미리 생성하여 재사용함으로써 메모리 할당/해제 비용을 줄이는 패턴 구현
/// </summary>
/// <typeparam name="T">풀링할 객체 타입. CObject를 상속받고 매개변수 없는 생성자가 있어야 함</typeparam>
public class ObjectPool<T> where T : CObject, new()
{
    /// <summary>
    /// 재사용 가능한 객체들을 저장하는 큐
    /// Queue를 사용하여 FIFO(First In First Out) 방식으로 객체를 관리
    /// </summary>
    private Queue<T> pool = new Queue<T>();

    /// <summary>
    /// 풀이 보유할 수 있는 최대 객체 수
    /// 메모리 사용량을 제한하기 위해 사용
    /// </summary>
    private int maxSize;

    /// <summary>
    /// 풀 확장 시 한 번에 생성할 객체 수
    /// 빈번한 확장을 방지하기 위해 사용
    /// </summary>
    private int growSize;

    /// <summary>
    /// ObjectPool 생성자
    /// </summary>
    /// <param name="initialSize">초기에 생성할 객체 수</param>
    /// <param name="maxSize">풀이 보유할 수 있는 최대 객체 수</param>
    /// <param name="growSize">풀 확장 시 한 번에 생성할 객체 수</param>
    public ObjectPool(int initialSize, int maxSize, int growSize)
    {
        this.maxSize = maxSize;
        this.growSize = growSize;

        // 초기 객체들을 생성하여 풀에 추가
        for (int i = 0; i < initialSize; i++)
        {
            pool.Enqueue(new T());
        }
    }

    /// <summary>
    /// 풀에서 객체를 가져오는 메서드
    /// 풀이 비어있는 경우 자동으로 확장됨
    /// </summary>
    /// <returns>재사용 가능한 객체</returns>
    public T Get()
    {
        // 풀이 비어있으면 확장
        if (pool.Count == 0)
        {
            GrowPool();
        }
        // 큐에서 객체를 꺼내서 반환
        return pool.Dequeue();
    }

    /// <summary>
    /// 사용이 끝난 객체를 풀에 반환하는 메서드
    /// 풀이 최대 크기에 도달하지 않은 경우에만 객체를 다시 풀에 추가
    /// </summary>
    /// <param name="obj">재사용을 위해 반환할 객체</param>
    public void Return(T obj)
    {
        // 풀의 크기가 최대 크기보다 작은 경우에만 객체를 다시 풀에 추가
        if (pool.Count < maxSize)
        {
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 풀의 크기를 증가시키는 private 메서드
    /// maxSize를 초과하지 않는 범위 내에서 growSize만큼 새로운 객체를 생성
    /// </summary>
    private void GrowPool()
    {
        // 실제로 생성할 객체 수 계산 (maxSize를 초과하지 않도록)
        int growCount = Math.Min(growSize, maxSize - pool.Count);

        // 새로운 객체들을 생성하여 풀에 추가
        for (int i = 0; i < growCount; i++)
        {
            pool.Enqueue(new T());
        }

        // 디버깅을 위한 로그 출력
        Console.WriteLine($"Pool for {typeof(T).Name} grew by {growCount}. New size: {pool.Count}");
    }

    /// <summary>
    /// 현재 풀에서 사용 가능한 객체의 수를 반환하는 속성
    /// </summary>
    public int Count => pool.Count;
}