using System.Collections.Generic;
using static NetMsgAccessTool;

public class MulCreateNetMsg : INetMsg,INetMsgID
{
    public List<INetGameObject> NetGOList { get; set; } = new List<INetGameObject>();
    
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
        for (int i = 0; i < NetGOList.Count; i++)
        {
            WritingInt(bytes,NetGOList[i].NetID,ref index);
        }
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        for (int i = 0; i < NetGOList.Count; i++)
        {
            NetGOList[i].NetID = ReadingInt(buffer, ref index);
        }
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1008;
    }

    public int GetMsgLength()
    {
        return NetGOList.Count * 4;
    }
}
