using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class SceneMgr : MonoBehaviour
{
    public static SceneMgr Instance { get; private set; }

    private const int ID_SCENE_LOBBY = 1;

    private const float TIME_AFTERLOAD = 0.1f;
    private WaitForSeconds afterSceneLoadWFS;
    
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
        afterSceneLoadWFS = new WaitForSeconds(TIME_AFTERLOAD);
    }

    public void LoadLobbyScene(int selfNetID)
    {
        StartCoroutine(LoadLobbySceneCor(selfNetID));
    }

    IEnumerator LoadLobbySceneCor(int selfNetID)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(ID_SCENE_LOBBY);
        async.allowSceneActivation = false;

        yield return new WaitUntil(() => async.progress == 0.9f);
        async.allowSceneActivation = true;
        
        //Debug.Log(async.isDone); -false

        //加载完场景后停留等待短暂的时间继续执行操作
        yield return afterSceneLoadWFS;

        Instantiate(Resources.Load<GameObject>(GameManager.PATH_PREFAB_UI_MATCHGAME));
        
        GameManager.Instance.NewNetPlayer(selfNetID);
        
        //假设此时加载前往战斗场景 - 对象池生成子弹 - 发送消息
        GameManager.Instance.StartBattle();
        
        GameManager.Instance.GameState = GameState.Lobby;
        
        yield return afterSceneLoadWFS;
        GameManager.Instance.RequireUpdateOtherPlayer();

        yield return null;
    }

    
}
