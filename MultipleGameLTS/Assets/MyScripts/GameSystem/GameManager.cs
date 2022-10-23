using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Google.Protobuf;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState gameState = GameState.Login;
    public GameState GameState
    {
        get => gameState;
        set => gameState = value;
    }

    private Dictionary<int, GameObject> netPlayerGODic = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> netGameObjectDic = new Dictionary<int, GameObject>();

    //TODO:实际情况下会在线玩家需要一个单独的列表
    [SerializeField] private List<INetPlayer> netPlayerList = new List<INetPlayer>();

    //Sign:Resources加载文件不需要后缀 
    public const string PATH_PREFAB_PLAYER = "Prefabs/Prefab_NetPlayer";
    public const string PATH_PREFAB_UI_MATCHGAME = "Prefabs/Prefab_Cv_MatchGame";
    public const string PATH_PREFAB_BULLET = "Prefabs/Prefab_PlayerBullet";
   
	public int MatchingID { get; set; }
   
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
                Destroy(gameObject);
            }
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitScene();
    }

    private void InitScene()
    {
        var netMgrGo = new GameObject("NetMgr")
        {
            transform =
            {
                position = Vector3.zero
            }
        };
        netMgrGo.AddComponent<NetMgr>();

        var senceMgrGo = new GameObject("SceneMgr") 
        {
            transform =
            {
                position = Vector3.zero
            }
        };
        senceMgrGo.AddComponent<SceneMgr>();
    }

    //TODO:进入游戏时还需要携带当前角色信息（网络信息或者是网络数据） 比如外观样式 名称等
    /// <summary>
    /// 生成其他玩家
    /// </summary>
    public void NewNetPlayer(BornNetMsg bornNetMsg)
    {
        if(GameState == GameState.Login) return;
        if(!bornNetMsg.IsNetPlayer) return;
        
        if (!netPlayerGODic.ContainsKey(bornNetMsg.NetID))
        {
            var prefab = Resources.Load<GameObject>(bornNetMsg.PrefabPath);
            var netGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            netGO.AddComponent<NetPlayerController>().NetID = bornNetMsg.NetID;
            netPlayerList.Add(netGO.GetComponent<INetPlayer>());
            
            netPlayerGODic.Add(bornNetMsg.NetID,netGO);
            
            netGO.SetActive(true);
        }
    }
    /// <summary>
    /// 生成当前玩家
    /// </summary>
    public void NewNetPlayer(int selfNetID)
    {
        if (!netPlayerGODic.ContainsKey(selfNetID))
        {
            var prefab = Resources.Load<GameObject>(PATH_PREFAB_PLAYER);
            var netGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            netPlayerGODic.Add(selfNetID,netGO);
            
            netGO.AddComponent<NetPlayerController>().NetID = selfNetID;
            netPlayerList.Add(netGO.GetComponent<INetPlayer>()); 
            netGO.GetComponent<SpriteRenderer>().color = UserUI.Instance.curRoleColor;
            netGO.GetComponentInChildren<TextMeshProUGUI>(true).text = UserUI.Instance.curRoleName;
            netGO.SetActive(true);
        }
    }
    //TODO:实际情况：请求更新时需要携带其它玩家的状态（比如位置信息）- 服务端向玩家搜集位置信息 再整合发送给当前玩家
    public void UpdateOtherNetPlayers(int playerNetID)
    {
        if (!netPlayerGODic.ContainsKey(playerNetID))
        {
            var prefab = Resources.Load<GameObject>(PATH_PREFAB_PLAYER);
            var netGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            netGO.AddComponent<NetPlayerController>().NetID = playerNetID;
            netPlayerList.Add(netGO.GetComponent<INetPlayer>());
            
            netPlayerGODic.Add(playerNetID,netGO);
            
            netGO.SetActive(true);
        }
    }
    /// <summary>
    /// 某些环节、间隔定时地更新其它在线玩家
    /// </summary>
    public void RequireUpdateOtherPlayer()
    {
        RequireUpdateNetMsg requireUpdateNetMsg = new RequireUpdateNetMsg();
        NetMgr.Instance.BeginSend(requireUpdateNetMsg);
    }

    //TODO:更新位置在数据库中的表现 当前场景（sceneID）中有哪些玩家 player的NetID
    public void UpdatePos(int goNetID,float x,float y,float z)
    {
        if (netPlayerGODic.ContainsKey(goNetID))
        {
            var newPos = new Vector3(x, y, z);
            netPlayerGODic[goNetID].transform.position = newPos;
        } 
    }

    public void StartBattle()
    {
        RequestMulNetIDMsg requestMulNetIDMsg = new RequestMulNetIDMsg
        {
            Count = PoolMgr.Instance.GetPoolsTotalCount(PoolMgr.Instance.PlayerBulletPools)
        };

        NetMgr.Instance.BeginSend(requestMulNetIDMsg);
    }

    public void SwitchGameObjectState(SwitchNetMsg switchNetMsg)
    {
        if (netGameObjectDic.ContainsKey(switchNetMsg.GONetID))
        {
            netGameObjectDic[switchNetMsg.GONetID].SetActive(switchNetMsg.DoEnable);
        }
        else
        {
            //TODO:没有则要生成一个新的
            Debug.LogError($"当前客户端没有网络Id为{switchNetMsg.GONetID}的游戏对象");
        }
    }
}

public enum GameState
{
    Login,
    Role,
    Lobby,
    Battle
}
