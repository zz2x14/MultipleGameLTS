using System.Text;
using static NetMsgAccessTool;

public class BornNetMsg : INetMsg,INetMsgID
{
	public int NetID { get; set; }
	public string PrefabPath { get; set; }
	public bool IsNetPlayer { get; set; }
	public bool DoDisable { get; set; }
	
   public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
	    WritingInt(bytes,NetID,ref index);
	    WritingString(bytes,PrefabPath,ref index);
	    WritingBool(bytes,IsNetPlayer,ref index);
	    WritingBool(bytes,DoDisable,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        NetID = ReadingInt(buffer,ref index);
        PrefabPath = ReadingString(buffer,ref index);
        IsNetPlayer = ReadingBool(buffer,ref index);
        DoDisable = ReadingBool(buffer, ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
	    return 1003;
    }

    public int GetMsgLength()
    {
	    return 4 + 4 + Encoding.UTF8.GetBytes(PrefabPath).Length + 1 + 1;
    }
}
