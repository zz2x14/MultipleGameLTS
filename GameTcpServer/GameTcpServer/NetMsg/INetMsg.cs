using System.Collections;
using System.Collections.Generic;

public interface INetMsg
{
    public byte[] Writing();
    public int Reading(byte[] buffer,int beginIndex = 0);
    public int GetMsgBytesSizeNum();
}
