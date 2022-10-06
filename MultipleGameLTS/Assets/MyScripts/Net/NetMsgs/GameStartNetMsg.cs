using System.Collections.Generic;
using UnityEngine;
using static NetMsgAccessTool;

//TODO:还需要其它信息 地图ID、模式ID等相关 - 这些内容可以作为单纯的一个data类（ex:GameMap）包裹在消息中发送
public class GameStartNetMsg : INetMsg,INetMsgID
{
    //用来存储同一房间内玩家的网络ID
    public int PlayerCount { get; set; }
    public List<int> RoomPlayerNetIDList { get; set; }= new List<int>();

    public void GetRoomPlayerNetID(int netID)
    {
        PlayerCount++;
        RoomPlayerNetIDList.Add(netID);
    }
        
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,PlayerCount,ref index);
        for (int i = 0; i < PlayerCount; i++)
        {
            WritingInt(bytes,RoomPlayerNetIDList[i],ref index);
        }
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        PlayerCount = ReadingInt(buffer, ref index);
        for (int i = 0; i < PlayerCount; i++)
        {
            RoomPlayerNetIDList.Add(ReadingInt(buffer,ref index));
        }
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1010;
    }

    public int GetMsgLength()
    {
        return 4 + 4 * PlayerCount;
    }
}
