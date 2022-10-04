using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//子弹每次发送时需要记录发送人 - netID 传递信息
[RequireComponent(typeof(Rigidbody2D))]
public abstract class Bullet : MonoBehaviour,INetGameObject
{
    public int NetID { get; set; }
    
    [SerializeField] protected float speed;
    [SerializeField] protected float lifeCycle;

    public float LifeCycle => lifeCycle;
    public float Speed => speed;

    private Rigidbody2D rb;

    private WaitForSeconds LifeCycleWFS;
    
    public int MyNetID { get; set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        LifeCycleWFS = new WaitForSeconds(lifeCycle);
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(nameof(NonActiveCor));
        StartCoroutine(MoveCor(GetMoveDir()));
    }

    //不应该在让客户端主动通过对象池创建备用对象 而是服务器发送命令 主动创建
    // private void SendBornMsg()
    // {
    //     BornNetMsg bornNetMsg = new BornNetMsg
    //     {
    //         DoDisable = false
    //     };
    //     
    //     NetMgr.Instance.BeginSend(bornNetMsg);
    // }

    IEnumerator MoveCor(Vector3 dir)
    {
        while (gameObject.activeSelf)
        {
            rb.velocity = dir * speed * Time.deltaTime;
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
        if (col.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
}
