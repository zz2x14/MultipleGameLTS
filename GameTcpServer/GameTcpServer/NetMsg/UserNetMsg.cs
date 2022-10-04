using System.Text;
using static NetMsgAccessTool;

public class UserNetMsg : INetMsg,INetMsgID
{
    public int UserID { get; set; }
    public bool DoRegister { get; set; } = false;
    public string AccountNum { get; set; }
    public string Password { get; set; }
    
    
    public byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetMsgBytesSizeNum()];
        WritingInt(bytes,GetNetMsgID(),ref index);
        WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,UserID,ref index);
        WritingBool(bytes,DoRegister,ref index);
        WritingString(bytes,AccountNum,ref index);
        WritingString(bytes,Password,ref index);
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
        int index = beginIndex;
        UserID = ReadingInt(buffer, ref index);
        DoRegister = ReadingBool(buffer, ref index);
        AccountNum = ReadingString(buffer, ref index);
        Password = ReadingString(buffer, ref index);
        return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1001;
    }

    public int GetMsgLength()
    {
        return 4 + 1 + 4 + Encoding.UTF8.GetBytes(AccountNum).Length + 4 + Encoding.UTF8.GetBytes(Password).Length;
    }
}
