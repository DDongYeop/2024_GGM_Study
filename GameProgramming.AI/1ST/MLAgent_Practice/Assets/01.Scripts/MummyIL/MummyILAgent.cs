using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MummyILAgent : Agent
{
    [SerializeField] private float _moveSpeed = 10;
    [SerializeField] private float _turnSpeecd = 50;

    private ColorTarget _colorTarget;
    private Rigidbody _rigidbody;
    private Vector3 _originPos;

    [SerializeField] private Material _goodMat; 
    [SerializeField] private Material _badMat;
    private Material _originMat;
    private Renderer _floorRenderer;
    
    public override void Initialize()
    {
        _colorTarget = transform.parent.GetComponent<ColorTarget>();
        _rigidbody = GetComponent<Rigidbody>();
        _originPos = transform.localPosition;
        _floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        _originMat = _floorRenderer.material;
    }
    public override void OnEpisodeBegin()
    {
        _colorTarget.TargetingColor();
        _rigidbody.velocity = _rigidbody.angularVelocity = Vector3.zero;
        transform.localPosition = _originPos;
        transform.localRotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //센서 사용 안 해서 건너 뛴다
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        
        Vector3 direction = Vector3.zero;
        Vector3 rotationAxis = Vector3.zero;
 
        switch (discreteActions[0])
        {
            case 1:
                direction = transform.forward;
                break;
            case 2:
                direction = -transform.forward;
                return;
        }

        switch (discreteActions[1])
        {
            case 1:
                rotationAxis = Vector3.down;
                break;
            case 2:
                rotationAxis = Vector3.up;
                break;
        }

        _rigidbody.MovePosition(transform.position + direction * (_moveSpeed * Time.fixedDeltaTime));
        transform.Rotate(rotationAxis, _turnSpeecd * Time.fixedDeltaTime);
        
        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.A))
            discreteActions[1] = 1;
        if (Input.GetKey(KeyCode.D))
            discreteActions[1] = 2;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag(_colorTarget.TargetColor.ToString()))
        {
            AddReward(1);
            EndEpisode();
        }
        else if (other.transform.CompareTag("Hint")) { }
        else if (other.transform.CompareTag("Floor")) { }
        else if (other.transform.CompareTag("Wall"))
        {
            StartCoroutine(ChangeFloorColor(_badMat));
            AddReward(-1);
            EndEpisode();
        }
    }

    private IEnumerator ChangeFloorColor(Material changeMat)
    {
        _floorRenderer.material = changeMat;
        yield return new WaitForSeconds(.2f);
        _floorRenderer.material = _originMat;
    }
}
