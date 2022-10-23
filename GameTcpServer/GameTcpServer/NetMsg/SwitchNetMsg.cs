using static NetMsgAccessTool;

public class SwitchNetMsg : INetMsg,INetMsgID
{
    public int GONetID { get; set; }
    public bool DoEnable { get; set; } = true;
    
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,GONetID,ref index);
        WritingBool(bytes,DoEnable,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        GONetID = ReadingInt(buffer, ref index);
        DoEnable = ReadingBool(buffer, ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1011;
    }

    public int GetMsgLength()
    {
	    return 4 + 1;
    }
}
