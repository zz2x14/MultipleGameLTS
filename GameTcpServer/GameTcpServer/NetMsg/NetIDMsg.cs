using static NetMsgAccessTool;

public class NetIDMsg : INetMsg,INetMsgID
{ 
	public int SelfNetID { get; set; }
	
	public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
	    WritingInt(bytes,SelfNetID,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        SelfNetID = ReadingInt(buffer, ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
	    return 1004;
    }

    public int GetMsgLength()
    {
	    return 4;
    }
}
