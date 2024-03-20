using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class MummyGoAgent : Agent
{
    public Material FloorMaterials;
    public Material GoodMaterials;
    public Material BadMaterials;
    private Renderer _floorRenderer;

    private Transform _targetTransform;
    private  new Rigidbody _rigidbody;
    
    //초기 설정
    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        _targetTransform = transform.parent.Find("Target");
    }
 
    //에피소드 종료 될때마다 세팅
    public override void OnEpisodeBegin()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.05f, Random.Range(-4f, 4f));
        _targetTransform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.55f, Random.Range(-4f, 4f));

        StartCoroutine(RecoverFloor());
    }

    private IEnumerator RecoverFloor()
    {
        yield return new WaitForSeconds(0.2f);
        _floorRenderer.material = FloorMaterials;
    }
 
    //관찰할 정보를 설정하는 함수
    public override void CollectObservations(VectorSensor sensor)
    {
        // 8개
        sensor.AddObservation(_targetTransform.position); //3개
        sensor.AddObservation(transform.position); //3개
        sensor.AddObservation(_rigidbody.velocity.x); 
        sensor.AddObservation(_rigidbody.velocity.y); 
    }
 
    //에이전트 행동을 설정
    public override void OnActionReceived(ActionBuffers actions)
    {
        var ContinousActions = actions.ContinuousActions;
        Vector3 direction = (Vector3.forward * ContinousActions[0]) + (Vector3.right * ContinousActions[1]);
        direction.Normalize();
        _rigidbody.AddForce(direction * 50f);
        
        AddReward(-0.001f);
    }

    private void OnCollisionEnter(Collision other)
    {
        //WALL, TARGET
        if (other.collider.CompareTag("Wall"))
        {
            _floorRenderer.material = BadMaterials;
            AddReward(-1f);
            EndEpisode();
        }
        if (other.collider.CompareTag("Target"))
        {
            _floorRenderer.material = GoodMaterials;
            AddReward(1f);
            EndEpisode();
        }
    }

    //사용자가 에이전트 행동을 직접 조절 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionOut = actionsOut.ContinuousActions;
        continousActionOut[0] = Input.GetAxis("Vertical");
        continousActionOut[1] = Input.GetAxis("Horizontal");
    }
}


// 내가 한거 
/*
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
    private bool _isCanMove = true;
    
    public override void Initialize()
    {
        _isCanMove = true;
    }
 
    public override void OnEpisodeBegin()
    {
        _floorMesh.material = _floorMat;

        _targetTrm.position = new Vector3(Random.Range(-4f, 4f), 0.55f, Random.Range(-4f, 4f)) + transform.parent.position;
        transform.position = new Vector3(Random.Range(-4f, 4f), 0.55f, Random.Range(-4f, 4f)) + transform.parent.position;
        _isCanMove = true;
    }
 
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(_targetTrm.position - transform.position);
    }
 
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!_isCanMove)
            return;
        
        var continuousActions = actions.ContinuousActions;

        float x = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float z = Mathf.Clamp(continuousActions[1], -1f, 1f);

        transform.position += new Vector3(x, 0, z) * (Time.deltaTime * _speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Wall"))
            StartCoroutine(SetRewardCo(Color.red, -1f));
        else if (other.transform.CompareTag("Target"))
            StartCoroutine(SetRewardCo(Color.blue, 1f));
    }

    private IEnumerator SetRewardCo(Color color, float reward)
    {
        SetReward(reward);
        _isCanMove = false;
        _floorMesh.material = color == Color.blue ? _blueMat : _redMat;
        yield return new WaitForSeconds(0.1f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxisRaw("Horizontal");
        continuousActionOut[1] = Input.GetAxisRaw("Vertical");
    }
}
*/

/*

mlagents-learn "D:\Dev\GitHub\2024_GGM_Study\GameProgramming.AI\1ST\MLAgent_Practice\ml-agents-release_20\config\ppo\BallOnFloor.yaml" --run-id=ballonfloor01 --results-dir="D:\Dev\GitHub\2024_GGM_Study\GameProgramming.AI\1ST\MLAgent_Practice\ml-agents-release_20\rsults"

mlagents-learn "yaml파일 경로" --run-id=모델이름 --results-dir="모델 저장 경로"
*/