using static NetMsgAccessTool;

public class MatchGameNetMsg : INetMsg,INetMsgID
{
    public int PlayerNetID { get; set; }
    public int MatchingPoolID { get; set; }
    public bool DoMatch { get; set; } = true;
	
    public byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetMsgBytesSizeNum()];
        WritingInt(bytes,GetNetMsgID(),ref index);
        WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,PlayerNetID,ref index);
        WritingInt(bytes,MatchingPoolID,ref index);
        WritingBool(bytes,DoMatch,ref index);
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
        int index = beginIndex;
        PlayerNetID = ReadingInt(buffer,ref index);
        MatchingPoolID = ReadingInt(buffer,ref index);
        DoMatch = ReadingBool(buffer,ref index);
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
        return 4 + 4 +  1;
    }
}