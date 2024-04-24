using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PenguinAgent : Agent
{
    //엄마 앞으로 앞으로 움직, 움직X
    //왼, 오, 회전 X

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 180;
    //물고기 전달 했을 때 아기 위에 하트
    [SerializeField] private GameObject _heartPrefab;

    private PenguinArea _penguinArea;
    private Rigidbody _rigidbody;
    private GameObject _babyPenguin;
    //엄마 펭귄이 물고기 한마리씩 가져다 줄 수 잇게
    private bool _isFull;

    public override void Initialize()
    {
        _penguinArea = transform.parent.Find("PenguinArea").GetComponent<PenguinArea>();
        _babyPenguin = _penguinArea.BabyPenguin;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        _isFull = false;
        _penguinArea.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Fish -> Spawn
        sensor.AddObservation(Vector3.Distance(_babyPenguin.transform.position, transform.position));
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(_isFull);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var DiscreteActions = actions.DiscreteActions;
        float forwardAmount = DiscreteActions[0];
        float turnAmount = 0;
        if (DiscreteActions[1] == 1)
            turnAmount = -1f;
        else if (DiscreteActions[1] == 2)
            turnAmount = 1f;
        
        _rigidbody.MovePosition(transform.position + transform.forward * (forwardAmount * _moveSpeed * Time.fixedDeltaTime));
        transform.Rotate(Vector3.up * (_turnSpeed * turnAmount * Time.fixedDeltaTime));
        AddReward(-1.0f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var DiscreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
            DiscreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.A))
            DiscreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.D))
            DiscreteActionsOut[1] = 2;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Baby")) //아가
        {
            RegurgitateFish();
        }
        else if (other.transform.CompareTag("Fish")) //물고기
        {
            EatFish(other.gameObject);
        }
    }

    private void EatFish(GameObject fishObject)
    {
        if(_isFull)
            return;
        _isFull = true;
        _penguinArea.RemoveFishList(fishObject);
        AddReward(1);
    }

    private void RegurgitateFish()
    {
        if (!_isFull)
            return;

        _isFull = false;
        GameObject heart = Instantiate(_heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.localPosition = _babyPenguin.transform.localPosition + Vector3.up;
        Destroy(heart, 4f);
        AddReward(1);

        if (_penguinArea.RemainingFish <= 0) //물고기 다 머금
            EndEpisode();
    }
}
