using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 각 오브젝트의 위치를 스폰해주는 작업 진행

public class PenguinArea : MonoBehaviour
{
    [SerializeField] private PenguinAgent _penguinAgent;
    [SerializeField] private GameObject _babyPenguin;
    [SerializeField] private Fish _fishPrefabs;
    private List<GameObject> _fishList = new List<GameObject>();

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
            ResetArea();
    }

    //각도와 거리를 이용해서 랜덤으로 스폰 
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float randomAngle;
        float randomRadius;

        randomAngle = Random.Range(minAngle, maxAngle);
        randomRadius = Random.Range(minRadius, maxRadius);

        return center + Quaternion.Euler(0, randomAngle, randomRadius) * Vector3.forward * randomRadius;
    }

    //엄마팽귄생성
    private void PlacePenguin()
    {
        Rigidbody rb = _penguinAgent.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        _penguinAgent.transform.position = ChooseRandomPosition(transform.parent.position, 0, 360, 0, 9f) + Vector3.up * .5f;
        _penguinAgent.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    private void PlaceBabyPenguin()
    {
        Rigidbody rb = _babyPenguin.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        _babyPenguin.transform.position = ChooseRandomPosition(transform.parent.position, -45, 45, 4, 9f) + Vector3.up * .5f;
        _babyPenguin.transform.rotation = Quaternion.Euler(0, Random.Range(0, 180), 0);
    }

    private void SpawnFish(int cnt)
    {
        for (int i = 0; i < cnt; ++i)
        {
            GameObject fishObject = Instantiate(_fishPrefabs).gameObject;
            fishObject.transform.SetParent(transform);
            fishObject.transform.position = ChooseRandomPosition(transform.parent.position, 100, 260, 2, 9f) + Vector3.up * .5f;
            fishObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            _fishList.Add(fishObject);
        }
    }

    private void RemoveAllFish()
    {
        if (_fishList != null)
        {
            foreach (var fish in _fishList)
                Destroy(fish);
        }
        
        _fishList = new List<GameObject>();
    }

    public void ResetArea()
    {
        RemoveAllFish();
        PlacePenguin();
        PlaceBabyPenguin();
        SpawnFish(5);
    }
}
