
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO：这个理应也是个接口
/// </summary>
public class TransformNetUpdateTool : MonoBehaviour
{
    private TransformNetMsg transformMsg;
    private Canvas nameCanvas;
    private int netID;
   
    private void Awake()
    {
        netID = GetComponent<INetGameObject>().NetID;
        transformMsg = new TransformNetMsg {GONetID = netID};
    }

    private void OnEnable()
    {
        nameCanvas = GetComponentInChildren<Canvas>(true);
        nameCanvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    //TODO:现在使用最粗暴的方式进行检测是否移动 - 按下移动按键即移动了
    //TODO：实际情况可以使用新输入系统的事件检测或者开启协程检测（刚体）速度是否发生变化
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.position += Vector3.up;
            UpdateNowPos();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.position += Vector3.down;
            UpdateNowPos();
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position += Vector3.left;
            UpdateNowPos();
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position += Vector3.right;
            UpdateNowPos();
        }
    }

    private void UpdateNowPos()
    {
        transformMsg.Posx = transform.position.x;
        transformMsg.PosY = transform.position.y;
        transformMsg.PosZ = transform.position.z;
        
        NetMgr.Instance.BeginSend(transformMsg);
    }
}
