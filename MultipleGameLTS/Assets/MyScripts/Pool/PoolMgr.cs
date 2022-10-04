using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMgr : MonoBehaviour
{
    public static PoolMgr Instance { get; private set; }
    
    [SerializeField] private Pool[] playerBulletPools;
    public Pool[] PlayerBulletPools => playerBulletPools;

    private Dictionary<GameObject, Pool> poolDic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Debug.LogError( gameObject.name + "并不是唯一单例");
                Destroy(gameObject);
            }
        }
        
        DontDestroyOnLoad(gameObject);

        poolDic = new Dictionary<GameObject, Pool>();
    }

    private void CheckPoolRealSize(Pool[] pools)
    {
        foreach (var pool in pools)
        {
            if (pool.RuntimeSize > pool.PoolSize)
            {
                Debug.LogFormat($"{pool.PoolName}Pool originalSize:{pool.PoolSize} is not enough,the runtime size为:{pool.RuntimeSize}");
            }
        }
    }
    
    private void OnDestroy()
    {
        CheckPoolRealSize(playerBulletPools);
    }

    /// <summary>
    /// TODO:通过GM动态创建PM
    /// </summary>
    public void InitPoolMgr()
    {
        
    }
    
    private void InitPools(Pool[] pools,List<int> netIDList)
    {
        foreach (var pool in pools)
        {
            var poolParent = new GameObject("Pool:" + pool.PoolName);
            poolParent.transform.SetParent(transform);
            pool.InitPool(poolParent.transform,netIDList);

            poolDic.Add(pool.Prefab,pool);
        }
    }
    
    //TODO:现在只有一个对象池，对应一个列表没有问题，需要考虑多个对象池时的情况 - 简单粗暴的方法：一个列表对应一个对象池
    public void InitBattlePools(List<int> netIDList)
    {
        InitPools(playerBulletPools,netIDList);
    }
    
    
    //TODO:还需要备用考虑不够增加时的情况
    public int GetPoolsTotalCount(Pool[] pools)
    {
        int total = 0;
        
        foreach (var pool in pools)
        {
            total += pool.PoolSize;
        }
        
        return total;
    }

    public GameObject Release(GameObject prefab)
    {
        return poolDic[prefab].GetPreparedGO();
    }
    public GameObject Release(GameObject prefab,Vector3 pos)
    {
        return poolDic[prefab].GetPreparedGO(pos);
    }
    public GameObject Release(GameObject prefab,Vector3 pos,Quaternion rotation)
    {
        return poolDic[prefab].GetPreparedGO(pos,rotation);
    }
    public GameObject Release(GameObject prefab, Vector3 pos, Quaternion rotation, Vector3 scale)
    {
        return poolDic[prefab].GetPreparedGO(pos, rotation, scale);
    }
}
