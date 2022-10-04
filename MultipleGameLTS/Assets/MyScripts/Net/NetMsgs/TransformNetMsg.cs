using static NetMsgAccessTool;

//TODO:或许位置数据不是单独作为一个网络消息发送 而是同步玩家状态时的被包裹信息
public class TransformNetMsg : INetMsg,INetMsgID
{
	public int GONetID { get; set; }
	public float Posx { get; set; }
	public float PosY { get; set; }
	public float PosZ { get; set; }
	
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
	    WritingInt(bytes,GONetID,ref index);
	    WritingFloat(bytes,Posx,ref index);
	    WritingFloat(bytes,PosY,ref index);
	    WritingFloat(bytes,PosZ,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        GONetID = ReadingInt(buffer, ref index);
        Posx = ReadingFloat(buffer, ref index);
        PosY = ReadingFloat(buffer, ref index);
        PosZ = ReadingFloat(buffer, ref index);
        return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
	    return 1005;
    }

    public int GetMsgLength()
    {
	    return 4 + 4 + 4 + 4;
    }
}
