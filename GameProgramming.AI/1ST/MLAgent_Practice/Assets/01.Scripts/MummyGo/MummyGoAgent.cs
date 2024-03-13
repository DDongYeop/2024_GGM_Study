using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class MummyGoAgent : Agent
{
    // Agent가 Target과 충돌시 바닥이 파랗게 변허고 종료
    // Agent가 Wall과 충돌시 바닥이 빨간색으로 변허고 종료
    // 변한 색은 제시작할때 원래 색으로 복원
    // OnEpisodeBegin Target과 에이전트 위치 랜덤 배치
    
    // MummyGoAgent에게 Wall를 피해 Target으로 가도록 학습 

    [Header("Materials")] 
    [SerializeField] private Material _floorMat;
    [SerializeField] private Material _redMat;
    [SerializeField] private Material _blueMat;

    [Header("Object")] 
    [SerializeField] private Transform _targetTrm;
    
    [Header("Mesh")] 
    [SerializeField] private MeshRenderer _floorMesh;

    [Header("Other")] 
    [SerializeField] private float _speed; 
    
    public override void Initialize()
    {
    }
 
    public override void OnEpisodeBegin()
    {
        _floorMesh.material = _floorMat;

        _targetTrm.position = new Vector3(Random.Range(-4f, 4f), 0.55f, Random.Range(-4f, 4f));
        transform.position = new Vector3(Random.Range(-4f, 4f), 0.55f, Random.Range(-4f, 4f));
    }
 
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(_targetTrm.position - transform.position);
    }
 
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        float x = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float z = Mathf.Clamp(continuousActions[1], -1f, 1f);

        transform.position += new Vector3(x, 0, z) * (Time.deltaTime * _speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Wall"))
        {
            SetReward(-1f);
            _floorMesh.material = _redMat;
            EndEpisode();
        }
        else if (other.transform.CompareTag("Target"))
        {
            SetReward(1);
            _floorMesh.material = _blueMat;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxisRaw("Horizontal");
        continuousActionOut[1] = Input.GetAxisRaw("Vertical");
    }
}

/*

mlagents-learn "D:\Dev\GitHub\2024_GGM_Study\GameProgramming.AI\1ST\MLAgent_Practice\ml-agents-release_20\config\ppo\BallOnFloor.yaml" --run-id=ballonfloor01 --results-dir="D:\Dev\GitHub\2024_GGM_Study\GameProgramming.AI\1ST\MLAgent_Practice\ml-agents-release_20\rsults"

mlagents-learn "yaml파일 경로" --run-id=모델이름 --results-dir="모델 저장 경로 "
*/