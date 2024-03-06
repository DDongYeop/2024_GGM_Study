using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FloorAgent : Agent
{
    public Transform BallTransform;
    private Rigidbody _ballRigidbody;
    
    //한번만 실행되는 초기화 함수
    public override void Initialize()
    {
        _ballRigidbody = BallTransform.GetComponent<Rigidbody>();
    }

    //에피소드 시작될 때 호출 되는 함수. 에피소드 << 공을 떨어트리지 않을때. Ex)공이 떨어져라 == 에피소드 끝. 즉, 환경 상태를 초기화 
    public override void OnEpisodeBegin()
    {
        //floor위치, 속도 x,z기준으로 회전
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));

        //ball 위치 속도 초기화
        BallTransform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), 1.5f, Random.Range(-1.5f, 1.5f));
        _ballRigidbody.velocity = Vector3.zero;
    }

    //Agent가 계속 추적하는 함수 
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation.x); //1
        sensor.AddObservation(transform.rotation.z); //1
        sensor.AddObservation(BallTransform.position - transform.position); //3
        sensor.AddObservation(_ballRigidbody.velocity); //3
    }

    //에이전트의 행동 결정하는 함수 
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        float x_rotation = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float z_rotation = Mathf.Clamp(continuousActions[1], -1f, 1f);
        
        transform.Rotate(new Vector3(1, 0, 0), x_rotation);
        transform.Rotate(new Vector3(0, 0, 1), z_rotation);

        if (BallTransform.position.y - transform.position.y <= -2f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (Mathf.Abs(BallTransform.position.x - transform.position.x) > 2.5f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (Mathf.Abs(BallTransform.position.z - transform.position.z) > 2.5f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            SetReward(0.1f);
        }
    }

    //사용자가 Agent를 직접 제어하는 함수. (확인용. 필수는 아님.)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = -Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }
}
