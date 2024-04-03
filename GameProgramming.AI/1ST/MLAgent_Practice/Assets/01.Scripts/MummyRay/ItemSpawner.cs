using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _goodItem;
    [SerializeField] private GameObject _badItem;

    [SerializeField] private int _goodItemCnt;
    [SerializeField] private int _badItemCnt;
    
    private List<GameObject> _itemLists = new List<GameObject>();

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        GameObject obj = new GameObject("ItemLists");
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        
        for (int i = 0; i < _goodItemCnt; ++i)
            _itemLists.Add(Instantiate(_goodItem, obj.transform));
        for (int i = 0; i < _badItemCnt; ++i)
            _itemLists.Add(Instantiate(_badItem, obj.transform));
        
        ItemPositionSet();
    }

    public void ItemPositionSet()
    {
        foreach (GameObject item in _itemLists)
        {
            item.SetActive(true);
            item.transform.localPosition = new Vector3(Random.Range(-23.0f, 23.0f), 0.05f, Random.Range(-23.0f, 23.0f));
            item.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0.0f, 360.0f));
        }
    }
}
