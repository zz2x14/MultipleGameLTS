using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayerController : MonoBehaviour,INetPlayer
{
	public int NetID { get; set; }

	private Canvas matchGameCanvas;
	
	private void Awake()
	{
		matchGameCanvas = GameObject.Find("Prefab_Cv_MatchGame(Clone)").GetComponent<Canvas>();
	}
	
	private void OnEnable()
	{
		Debug.Log(NetID);
	}
    
	private void Update()
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
}
