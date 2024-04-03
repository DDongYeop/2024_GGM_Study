using System;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class MummyRayAgent : Agent
{
    [SerializeField] private Material _goodMaterial;
    [SerializeField] private Material _badMaterial;
    private Renderer _floorRenderer;
    private Material _originalMaterial;

    [SerializeField] private float _turnSpeed = 100f;
    [SerializeField] private float _moveSpeed = 30f;

    private ItemSpawner _itemSpawner;
    
    //초기 설정
    public override void Initialize()
    {
        _itemSpawner = transform.parent.GetComponent<ItemSpawner>();
        _floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        _originalMaterial = _floorRenderer.material;
    }
 
    //에피소드 종료 될때마다 세팅
    public override void OnEpisodeBegin()
    {
        //item, 미라 위치 랜덤, 미라 회전 랜덤
        _itemSpawner.ItemPositionSet();
        transform.localPosition = new Vector3(Random.Range(-22.0f, 22.0f), 0.5f, Random.Range(-22.0f, 22.0f));
        transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
    }
 
    //에이전트 행동을 설정
    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        Vector3 direction = Vector3.zero;
        Vector3 rotation = Vector3.zero;
        // 0: 앞뒤 움직이기
        switch (discreteActions[0])
        {
            case 0:
                direction = Vector3.zero;
                break;
            case 1:
                direction = transform.forward;
                break;
            case 2:
                direction = -transform.forward;
                break;
        }
        // 1: 회전
        switch (discreteActions[1])
        {
            case 0:
                rotation = Vector3.zero;
                break;
            case 1:
                rotation = Vector3.down;
                break;
            case 2:
                rotation = Vector3.up;
                break;
        }
        
        transform.Rotate(rotation, _turnSpeed * Time.fixedDeltaTime);
        transform.localPosition += direction * (_moveSpeed * Time.fixedDeltaTime);
        
        AddReward(-1 / (float)MaxStep);
    }

    //사용자가 에이전트 행동을 직접 조절 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreateActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
            discreateActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S))
            discreateActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.A))
            discreateActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.D))
            discreateActionsOut[1] = 2;
    }

    private IEnumerator ChangeFloorColor(Material changeMaterial)
    {
        _floorRenderer.material = changeMaterial;
        yield return new WaitForSeconds(0.2f);
        _floorRenderer.material = _originalMaterial;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("GoodItem"))
        {
            other.gameObject.SetActive(false);
            AddReward(1);
            StartCoroutine(ChangeFloorColor(_goodMaterial));
        }

        if (other.transform.CompareTag("BadItem"))
        {
            AddReward(-1);
            EndEpisode();
            StartCoroutine(ChangeFloorColor(_badMaterial));
        }

        if (other.transform.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            EndEpisode();
        }
    }
}
