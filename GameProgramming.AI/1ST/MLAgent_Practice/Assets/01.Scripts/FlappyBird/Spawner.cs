using UnityEngine;
using System.Collections.Generic;
public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public float spawnRate = 3f;
    public float minHeight = -1f;
    public float maxHeight = 2f;

    public List<GameObject> pipeList = new List<GameObject>();
    private void Start()
    {
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }

    private void Spawn()
    {
        GameObject pipe = Instantiate(prefab, transform.position, Quaternion.identity,transform);
        pipe.transform.localPosition += Vector3.up * Random.Range(minHeight, maxHeight);
        pipeList.Add(pipe);
    }

    public void ReSetSpawn()
    {
        foreach (var pipe in pipeList)
            Destroy(pipe);
        pipeList = new List<GameObject>();
    }
}
