using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetMsgLibrary
{
    private readonly Dictionary<Type, int> netMsgIdDic;

    private const int ID_NETMSG_USER = 1001;
    private const int ID_NETMSG_RESPONSE = 1999;

    public NetMsgLibrary()
    {
        netMsgIdDic = new Dictionary<Type, int>();
        
        MatchMsgID();
    }
    
    public int GetNetMsgID(IMessage msg)
    {
        if (netMsgIdDic.ContainsKey(msg.GetType()))
        {
            return netMsgIdDic[msg.GetType()];
        }

        return 0;
    }

    private void MatchMsgID()
    {
        // netMsgIdDic.Add(typeof(UserNetMsg),ID_NETMSG_USER);
        // netMsgIdDic.Add(typeof(ResponseNetMsg),ID_NETMSG_RESPONSE);
    }
}
