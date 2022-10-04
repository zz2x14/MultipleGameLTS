using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static NetMsgAccessTool;

/// <summary>
/// 用来为游戏对象获得批量网络ID的消息
/// </summary>
public class RequestMulNetIDMsg : INetMsg,INetMsgID
{
    //TODO:添加对象池ID - 自动匹配初始化哪个池子
    public int Count { get; set; }
    public List<int> NetGOIDList { get;  set; } = new List<int>();

    public void GetSingleNetID(int singleID)
    {
        NetGOIDList.Add(singleID);
    }
    
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes, Count, ref index);
        if (Count == NetGOIDList.Count)
        {
            for (int i = 0; i < Count; i++)
            {
                WritingInt(bytes,NetGOIDList[i],ref index);
            } 
        }
        return bytes;
    }

    //Sign:List等只是一个容器，本身是空的，接收到网络消息后想要获取到多个内容，将它们转载到容器中！！！
    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        Count = ReadingInt(buffer, ref index);
        for (int i = 0; i < Count; i++)
        {
            NetGOIDList.Add(ReadingInt(buffer, ref index));
        }
        return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1007;
    }

    public int GetMsgLength()
    {
        return 4 + 4 * Count;
    }
}
