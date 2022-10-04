using System.Runtime.CompilerServices;
using System.Text;
using static NetMsgAccessTool;

public class CreateRoleNetMsg : INetMsg,INetMsgID
{
	public int UserID { get; set; }
	public int RoleID { get; set; }
	public string RoleName { get; set; }
	
	public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
	    WritingInt(bytes,UserID,ref index);
	    WritingInt(bytes,RoleID,ref index);
	    WritingString(bytes,RoleName,ref index);
	    return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        UserID = ReadingInt(buffer, ref index);
        RoleID = ReadingInt(buffer, ref index);
        RoleName = ReadingString(buffer, ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
	    return 1002;
    }

    public int GetMsgLength()
    {
	    return 4 + 4 + 4 + Encoding.UTF8.GetBytes(RoleName).Length;
    }
}
