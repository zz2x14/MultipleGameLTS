using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//子弹每次发送时需要记录发送人 - netID 传递信息
[RequireComponent(typeof(Rigidbody2D))]
public abstract class Bullet : MonoBehaviour,INetGameObject
{
    public int NetID { get; set; }
    
    public SwitchNetMsg SwitchNetMsg { get; set; }

    [SerializeField] protected float speed;
    [SerializeField] protected float lifeCycle;

    public float LifeCycle => lifeCycle;
    public float Speed => speed;

    private Rigidbody2D rb;

    private WaitForSeconds LifeCycleWFS;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        LifeCycleWFS = new WaitForSeconds(lifeCycle);

        SwitchNetMsg = new SwitchNetMsg {GONetID = NetID};
    }

    protected virtual void OnEnable()
    {
        NetMgr.Instance.BeginSend(SwitchNetMsg);
        
        StartCoroutine(nameof(NonActiveCor));
        StartCoroutine(MoveCor(GetMoveDir()));
    }
    

    IEnumerator MoveCor(Vector3 dir)
    {
        while (gameObject.activeSelf)
        {
            rb.velocity = dir * speed;
            yield return null;
        }
    }

    //后续直接使用自动摧毁工具脚本
    IEnumerator NonActiveCor()
    {
        yield return LifeCycleWFS;
        gameObject.SetActive(false);
    }

    protected Vector2 GetMoveDir()
    {
        return Vector2.right;
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // if (col.gameObject.CompareTag("Enemy"))
        // {
        //     gameObject.SetActive(false);
        // }
    }

    private void OnDisable()
    {
        SwitchNetMsg.DoEnable = false;
        NetMgr.Instance.BeginSend(SwitchNetMsg);
        
        StopAllCoroutines();
    }
    
}
