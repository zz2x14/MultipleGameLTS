using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayerController : MonoBehaviour,INetPlayer,IUpdatePosToNet
{
	public int NetID { get; set; }
	public SwitchNetMsg SwitchNetMsg { get; set; }

	private Transform shootPoint;

	private Canvas matchGameCanvas;
	private Canvas nameCanvas;
	
	private GameObject playerBulletPrefab;
	
	public TransformNetMsg TransformNetMsg { get; set; }

	private void Awake()
	{
		matchGameCanvas = GameObject.Find("Prefab_Cv_MatchGame(Clone)").GetComponent<Canvas>();
		nameCanvas = GetComponentInChildren<Canvas>(true);
		nameCanvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

		playerBulletPrefab = Resources.Load<GameObject>(GameManager.PATH_PREFAB_BULLET);

		shootPoint = transform.GetChild(0).transform;
		
		TransformNetMsg = new TransformNetMsg {GONetID = NetID};
		SwitchNetMsg = new SwitchNetMsg {GONetID = NetID};
	}
	
	private void OnEnable()
	{
		Debug.Log("网络ID为:" + NetID);
	}
    
	private void Update()
	{
		Move();
		
		Shoot();
		
		MatchNewGame();
	}

	private void MatchNewGame()
	{
		if(Input.GetKeyDown(KeyCode.LeftControl))
		{
			MatchGameNetMsg matchGameMsg = new MatchGameNetMsg
			{
				PlayerNetID = NetID
			};
			NetMgr.Instance.BeginSend(matchGameMsg);
			
			matchGameCanvas.enabled = true;
		}
		
		if(matchGameCanvas.enabled)
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				MatchGameNetMsg matchGameMsg = new MatchGameNetMsg
				{
					PlayerNetID = NetID,
					MatchingPoolID = GameManager.Instance.MatchingID,
					DoMatch = false
				};
				NetMgr.Instance.BeginSend(matchGameMsg);
				
				matchGameCanvas.enabled = false;			
			}
		}
	}

	private void Shoot()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			PoolMgr.Instance.Release(playerBulletPrefab, shootPoint.position);
		}
	}

	//TODO:现在使用最粗暴的方式进行检测是否移动 - 按下移动按键即移动了
	//TODO：实际情况可以使用新输入系统的事件检测或者开启协程检测（刚体）速度是否发生变化
	private void Move()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			transform.position += Vector3.up;
			UpdatePos();
		}
        
		if (Input.GetKeyDown(KeyCode.S))
		{
			transform.position += Vector3.down;
			UpdatePos();
		}
        
		if (Input.GetKeyDown(KeyCode.A))
		{
			transform.position += Vector3.left;
			UpdatePos();
		}
        
		if (Input.GetKeyDown(KeyCode.D))
		{
			transform.position += Vector3.right;
			UpdatePos();
		}
	}
	
	public void UpdatePos()
	{
		TransformNetMsg.Posx = transform.position.x;
		TransformNetMsg.PosY = transform.position.y;
		TransformNetMsg.PosZ = transform.position.z;
		
		NetMgr.Instance.BeginSend(TransformNetMsg);
	}
}
