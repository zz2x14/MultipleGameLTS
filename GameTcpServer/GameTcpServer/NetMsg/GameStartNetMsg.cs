using static NetMsgAccessTool;

public class GameStartNetMsg : INetMsg,INetMsgID
{
    public int PlayerCount { get; set; }
    public List<int> RoomPlayerNetIDList { get; set; }= new List<int>();

    public void GetRoomPlayerNetID(int netID)
    {
        PlayerCount++;
        RoomPlayerNetIDList.Add(netID);
    }
        
    public byte[] Writing()
    {
        int index = 0;
        byte[] bytes = new byte[GetMsgBytesSizeNum()];
        WritingInt(bytes,GetNetMsgID(),ref index);
        WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes,PlayerCount,ref index);
        for (int i = 0; i < PlayerCount; i++)
        {
            WritingInt(bytes,RoomPlayerNetIDList[i],ref index);
        }
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
        int index = beginIndex;
        PlayerCount = ReadingInt(buffer, ref index);
        for (int i = 0; i < PlayerCount; i++)
        {
            RoomPlayerNetIDList.Add(ReadingInt(buffer,ref index));
        }
        return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1010;
    }

    public int GetMsgLength()
    {
        return 4 + 4 * PlayerCount;
    }
}