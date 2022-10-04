using static NetMsgAccessTool;

public class MatchGameNetMsg : INetMsg,INetMsgID
{
	public int NetID{get;set;}
	
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
	    WritingInt(bytes,NetID,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
	    int index = beginIndex;
	    NetID = ReadingInt(buffer,ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
	    return 1009;
    }

    public int GetMsgLength()
    {
	    return 4;
    }
}
