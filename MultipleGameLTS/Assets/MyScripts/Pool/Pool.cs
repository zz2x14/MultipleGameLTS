using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO:对象池池子size的数量也应该是根据写好的数值动态获取的
//TODO:通过配置表配好的
[System.Serializable]
public class Pool
{
    [SerializeField] private int poolSize;
    [SerializeField] private GameObject prefab;
    public string PoolName => prefab.name;
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    public GameObject Prefab => prefab;
    public int PoolSize => poolSize;
    public int RuntimeSize => poolQueue.Count;
    
    // /// <summary>
    // /// 暂时都通过Resources文件夹读取到
    // /// </summary>
    // /// <param name="fileName">预制体名称</param>
    // public void GetPrefab(string fileName)
    // {
    //     prefab = Resources.Load<GameObject>(fileName);
    // }
    
    //TODO:由服务器发起对象池准备，不应该以某个玩家的行为作为信号 - 服务器才是裁判！
    //TODO,EX:5V5-PVP,10名玩家匹配成功进入游戏后 - 服务器检测到该“房间”已经作好开始本局游戏准备
    //TODO:- 向当前“房间的”10名玩家发出创建游戏对象消息（什么场景需要哪些预制体，是配置好的）-每个客户端收到消息通过对象池准备好当前场景需要的所有游戏对象
    //TODO:游戏对象初始化后加入GM中的物体集合
    public void InitPool(Transform parent,List<int> netIDList)
    {
        if (netIDList.Count == poolSize)
        {
            var list = new List<INetGameObject>();
            
            for (var i = 0; i < poolSize; i++)
            {
                var go = Copy();
                poolQueue.Enqueue(go);
                go.transform.SetParent(parent);

                if (go.TryGetComponent(out INetGameObject netGO))
                {
                    netGO.NetID = netIDList[i];
                    list.Add(netGO);
                }
                else
                {
                    Debug.LogError($"当前物体{go.name}缺乏网络接口");
                }
                
            }
        }
        else
        {
            Debug.LogError($"获取得到的网络ID数量:{netIDList.Count}和当前对象池:{PoolName}初始长度:{poolSize}不相符");
        }
        
    }

    private GameObject Copy()
    {
        if (prefab != null)
        {
            var copy = GameObject.Instantiate(prefab);
            copy.SetActive(false);
            return copy;
        }
        
        Debug.LogError("没有正确读取到预制体");
        return null;
    }

    private GameObject GetAvailableGO()
    {
        GameObject availableGO = null;

        if (poolQueue.Count > 0 && !poolQueue.Peek().activeSelf)
        {
            availableGO = poolQueue.Dequeue();
        }
        else
        {
            availableGO = Copy();
        }

        poolQueue.Enqueue(availableGO);
        
        return availableGO;
    }

    public GameObject GetPreparedGO()
    {
        var prepared = GetAvailableGO();
        prepared.SetActive(true);
        return prepared;
    }
    public GameObject GetPreparedGO(Vector3 pos)
    {
        var prepared = GetAvailableGO();
        prepared.SetActive(true);
        prepared.transform.position = pos;
        return prepared;
    }
    public GameObject GetPreparedGO(Vector3 pos,Quaternion rotation)
    {
        var prepared = GetAvailableGO();
        prepared.SetActive(true);
        prepared.transform.position = pos;
        prepared.transform.rotation = rotation;
        return prepared;
    }
    public GameObject GetPreparedGO(Vector3 pos,Quaternion rotation,Vector3 size)
    {
        var prepared = GetAvailableGO();
        prepared.SetActive(true);
        prepared.transform.position = pos;
        prepared.transform.rotation = rotation;
        prepared.transform.localScale = size;
        return prepared;
    }
}
