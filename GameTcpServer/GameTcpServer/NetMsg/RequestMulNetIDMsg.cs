using static NetMsgAccessTool;

public class RequestMulNetIDMsg : INetMsg,INetMsgID
{
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
        for (int i = 0; i < Count; i++)
        {
            WritingInt(bytes,NetGOIDList[i],ref index);
        }
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        Count = ReadingInt(buffer, ref index);
        if (Count == NetGOIDList.Count)
        {
            for (int i = 0; i < Count; i++)
            {
                NetGOIDList.Add(ReadingInt(buffer,ref index));
            }
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
