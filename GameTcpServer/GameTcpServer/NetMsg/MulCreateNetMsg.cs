using System.Text;
using static NetMsgAccessTool;

public class MulCreateNetMsg : INetMsg,INetMsgID
{
    public int Count { get; set; }
    public List<INetGameObject> NetGOList { get; set; } = new List<INetGameObject>();
    public string PrefabPath { get; set; }

    public void GetSingleID(int index,int singleID)
    {
        if (index < NetGOList.Count)
        {
            NetGOList[index].NetID = singleID;
        }
        else
        {
            Console.WriteLine("获得索引超出列表长度");
        }
    }
    
    public byte[] Writing()
    {
      	int index = 0;
	    byte[] bytes = new byte[GetMsgBytesSizeNum()];
	    WritingInt(bytes,GetNetMsgID(),ref index);
	    WritingInt(bytes,GetMsgLength(),ref index);
        WritingInt(bytes, Count, ref index);
        for (int i = 0; i < Count; i++)
        {
            WritingInt(bytes,NetGOList[i].NetID,ref index);
        }
        WritingString(bytes,PrefabPath,ref index);
        return bytes;
    }

    public int Reading(byte[] buffer, int beginIndex = 0)
    {
       	int index = beginIndex;
        Count = ReadingInt(buffer, ref index);
        for (int i = 0; i < Count; i++)
        {
            NetGOList[i].NetID = ReadingInt(buffer, ref index);
        }
        PrefabPath = ReadingString(buffer, ref index);
	    return index;
    }

    public int GetMsgBytesSizeNum()
    {
        return 4 + 4 + GetMsgLength();
    }

    public int GetNetMsgID()
    {
        return 1007;
    }

    public int GetMsgLength()
    {
        return 4 + 4 * Count + 4 + Encoding.UTF8.GetBytes(PrefabPath).Length;
    }
}
