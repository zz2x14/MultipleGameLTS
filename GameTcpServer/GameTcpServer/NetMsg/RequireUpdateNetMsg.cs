using static NetMsgAccessTool;

//TODO:或者也该跟着其它信息一起发出
public class RequireUpdateNetMsg : INetMsg,INetMsgID
{
    public int ListCount { get; set; }
    public List<int> CurNetPlayerNetIDList = new List<int>();

    //TODO:更能体现出netID的唯一性
    public void UpdateNetPlayerList(int playerNetID)
    {
        CurNetPlayerNetIDList.Add(playerNetID);
    }
    
    public byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetMsgBytesSizeNum()];
        WritingInt(bytes,GetNetMsgID(),ref index);
        WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,ListCount,ref index);
        for (int i = 0; i < ListCount; i++)
        {
            WritingInt(bytes,CurNetPlayerNetIDList[i],ref index);
        }
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
        int index = beginIndex;
        ListCount = ReadingInt(buffer, ref index);
        for (int i = 0; i < ListCount; i++)
        {
            CurNetPlayerNetIDList.Add(ReadingInt(buffer,ref index));;
        }
        return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1006;
    }

    public int GetMsgLength()
    {
        return 4 + 4 * CurNetPlayerNetIDList.Count;
    }
}
