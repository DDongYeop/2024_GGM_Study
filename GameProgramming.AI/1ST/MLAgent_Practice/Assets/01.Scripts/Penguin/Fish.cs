using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour
{
    private float _fishSpeed = 0; //헤엄치는 속도
    private float _nextActionTime = -1f; //다음 위치 전까지 헤엄치는 시간
    private Vector3 _targetPos; //물고기 헤엄쳐서 갈 장소

    private void FixedUpdate()
    {
        Swim();
    }

    private void Swim()
    {
        //next~ 움직이고, 
        if (Time.fixedTime >= _nextActionTime)
        {
            //다음 위치 다겟, 시간
            _fishSpeed = Random.Range(.1f, .8f);
            _targetPos = PenguinArea.ChooseRandomPosition(transform.parent.position, 100, 260, 2, 9f);
            transform.localRotation = Quaternion.LookRotation(_targetPos - transform.position);

            float timeToTarget = Vector3.Distance(transform.position, _targetPos) / _fishSpeed;
            _nextActionTime = Time.fixedTime + timeToTarget;
        }
        else
        {
            //아직 헤엄치는 중
            Vector3 moveVector = _fishSpeed * transform.forward * Time.fixedDeltaTime;
            if (moveVector.magnitude < Vector3.Distance(transform.position, _targetPos))
                transform.position += moveVector;
            else
                transform.localPosition = _targetPos;
        }
    }
}
