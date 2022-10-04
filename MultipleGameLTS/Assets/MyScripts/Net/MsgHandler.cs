using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Google.Protobuf;

public class MsgHandler
{
    //private Dictionary<Type, UnityAction<IMessage>> netMsgHandlerDic;
    private readonly Dictionary<int, Type> msgTypeDic;
    private Dictionary<Type, UnityAction<INetMsg>> netMsgHandlerDic;
    
    private const int ID_NETMSG_QUIT = 777;
    private const int ID_NETMSG_USER = 1001;
    private const int ID_NETMSG_CREATEROLE = 1002;
    private const int ID_NETMSG_BORN = 1003;
    private const int ID_NETMSG_NETID = 1004;
    private const int ID_NETMSG_TF = 1005;
    private const int ID_NETMSG_REQUIRE = 1006;
    private const int ID_NETMSG_MULNETID = 1007;
    private const int ID_NETMSG_MATCHGAME = 1009;
    private const int ID_NETMSG_RESPONSE = 1999;
    
    #region RESPONSEID_USER
    public const int ID_RESPONSE_LOGIN = 1;
    public const int ID_RESPONSE_WRONG = 2;
    public const int ID_RESPONSE_NOTEXISTENT = 3;
    public const int ID_RESPONSE_REPEATEDNUM = 4;
    public const int ID_RESPONSE_REGISTER = 5;
    public const int ID_RESPONSE_REPETEDNAME = 6;
    public const int ID_RESPONSE_ROLE = 7;
    #endregion
    
    #region RESPONSEID_MATCH
    public const int ID_RESPONSE_MATCHING = 8;
    public const int ID_RESPONSE_MATCHSUCCESSFULLY = 9;
    public const int ID_RESPONSE_MATCHFAILED = 10;
    public const int ID_RESPONSE_MATCHOVER = 11;
    #endregion
    
    public MsgHandler()
    {
        msgTypeDic = new Dictionary<int, Type>();
        netMsgHandlerDic = new Dictionary<Type, UnityAction<INetMsg>>();
        
        Register(ID_NETMSG_USER,typeof(UserNetMsg),UserMsgHandler);
        Register(ID_NETMSG_RESPONSE,typeof(ResponseNetMsg),ResponseMsgHandler);
        Register(ID_NETMSG_CREATEROLE,typeof(CreateRoleNetMsg),CreateRoleMsgHandler);
        Register(ID_NETMSG_BORN,typeof(BornNetMsg),BornMsgHandler);
        Register(ID_NETMSG_NETID,typeof(NetIDMsg),NetIDMsgHandler);
        Register(ID_NETMSG_TF,typeof(TransformNetMsg),TransformMsgHandler);
        Register(ID_NETMSG_REQUIRE,typeof(RequireUpdateNetMsg),RequireUpdateMsgHandler);
        Register(ID_NETMSG_MULNETID,typeof(RequestMulNetIDMsg),MulNetIDMsgHandler);
        Register(ID_NETMSG_MATCHGAME,typeof(MatchGameNetMsg),MatchGameMsgHandler);
    }

    private void Register(int msgID,Type msgType,UnityAction<INetMsg> action)
    {
        msgTypeDic.Add(msgID,msgType);
        netMsgHandlerDic.Add(msgType, action);
    }
    
    public INetMsg GetNetMsg(int netMsgId, byte[] bytes, int startIndex)
    {
        if (msgTypeDic.ContainsKey(netMsgId))
        {
            var msg = Activator.CreateInstance(msgTypeDic[netMsgId]) as INetMsg;
            msg.Reading(bytes, startIndex);
            return msg;
        }
        return null;
    }

    public void HandleMsg(INetMsg msg)
    {
        if (netMsgHandlerDic.ContainsKey(msg.GetType()))
        {
            netMsgHandlerDic[msg.GetType()].Invoke(msg);
        }
    }
    
    private void UserMsgHandler(INetMsg msg)
    {
        var userNetMsg = msg as UserNetMsg;
        if (userNetMsg.DoRegister)
        {
            NetMgr.Instance.UserID = userNetMsg.UserID;
            Debug.Log("获得用户ID：" + userNetMsg.UserID);
        }
    }
    
    private void ResponseMsgHandler(INetMsg msg)
    {
        UserUI.Instance.MatchResponseID((msg as ResponseNetMsg).ResponseID);
    }

    private void CreateRoleMsgHandler(INetMsg msg)
    {
        UserUI.Instance.NewRoleID = (msg as CreateRoleNetMsg).RoleID;
        UserUI.Instance.GetRoleInfo();
    }

    private void BornMsgHandler(INetMsg msg)
    {
        var bornMsg = msg as BornNetMsg;
        if (bornMsg.IsNetPlayer)
        {
            
            GameManager.Instance.NewNetPlayer(msg as BornNetMsg);
        }
    }

    private void NetIDMsgHandler(INetMsg msg)
    {
        var netIDMsg = msg as NetIDMsg;
        if (netIDMsg.SelfNetID == 0)
        {
            Debug.LogError("没有正确获得当前角色的网络ID");
            return;
        }
        
        UserUI.Instance.NetID = netIDMsg.SelfNetID;
    }

    private void TransformMsgHandler(INetMsg msg)
    {
        var transformMsg = msg as TransformNetMsg;
        GameManager.Instance.UpdatePos(transformMsg.GONetID,transformMsg.Posx,transformMsg.PosY,transformMsg.PosZ);
    }
    
    private void RequireUpdateMsgHandler(INetMsg msg)
    {
        var require = msg as RequireUpdateNetMsg;
        
        if (require.ListCount > 0)
        {
            for (int i = 0; i < require.ListCount; i++)
            {
                GameManager.Instance.UpdateOtherNetPlayers(require.CurNetPlayerNetIDList[i]);
            }
        }
    }

    private void MulNetIDMsgHandler(INetMsg msg)
    {
        RequestMulNetIDMsg requestMulNetIDMsg = msg as RequestMulNetIDMsg;
        
        PoolMgr.Instance.InitBattlePools(requestMulNetIDMsg.NetGOIDList);
    }

    private void MatchGameMsgHandler(INetMsg msg)
    {
        
    }
    
}