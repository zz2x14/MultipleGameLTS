using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetMsgAccessTool : MonoBehaviour
{
    public static void WritingByte(byte[] buffer, byte data, ref int nowIndex)
    {
        buffer[nowIndex] = data;
        nowIndex += 1;
    }
    public static void WritingInt(byte[] buffer, int data, ref int nowIndex)
    {
        BitConverter.GetBytes(data).CopyTo(buffer,nowIndex);
        nowIndex += 4;
    }
    public static void WritingBool(byte[] buffer, bool data, ref int nowIndex)
    {
        BitConverter.GetBytes(data).CopyTo(buffer,nowIndex);
        nowIndex += 1;
    }
    public static void WritingFloat(byte[] buffer, float data, ref int nowIndex)
    {
        BitConverter.GetBytes(data).CopyTo(buffer,nowIndex);
        nowIndex += 4;
    }
    public static void WritingLong(byte[] buffer, long data, ref int nowIndex)
    {
        BitConverter.GetBytes(data).CopyTo(buffer,nowIndex);
        nowIndex += 8;
    }
    public static void WritingShort(byte[] buffer, short data, ref int nowIndex)
    {
        BitConverter.GetBytes(data).CopyTo(buffer,nowIndex);
        nowIndex += 2;
    }
    public static void WritingString(byte[] buffer, string data, ref int nowIndex)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var strLength = bytes.Length;
        WritingInt(buffer,strLength,ref nowIndex);
        bytes.CopyTo(buffer,nowIndex);
        nowIndex += strLength;
    }
    public static void WritingNetMsg(byte[] buffer, INetMsg netMsg, ref int nowIndex)
    {
        netMsg.Writing().CopyTo(buffer,nowIndex);
        nowIndex += netMsg.GetMsgBytesSizeNum();
    }

    public static byte ReadingByte(byte[] buffer,ref int nowIndex)
    {
        var bt = buffer[nowIndex];
        nowIndex += 1;
        return bt;
    }
    public static int ReadingInt(byte[] buffer, ref int nowIndex)
    {
        var i = BitConverter.ToInt32(buffer, nowIndex);
        nowIndex += 4;
        return i;
    }
    public static float ReadingFloat(byte[] buffer, ref int index)
    {
        float f = BitConverter.ToSingle(buffer, index);
        index += 4;
        return f;
    }
    public static short ReadingShort(byte[] buffer, ref int index)
    {
        short s = BitConverter.ToInt16(buffer, index);
        index += 2;
        return s;
    }
    public static bool ReadingBool(byte[] buffer, ref int index)
    {
        bool b = BitConverter.ToBoolean(buffer, index);
        index += 1;
        return b;
    }
    public static string ReadingString(byte[] buffer, ref int index)
    {
        int strLength = ReadingInt(buffer, ref index);
        string str = Encoding.UTF8.GetString(buffer, index, strLength);
        index += strLength;
        return str;
    }
    public static long ReadingLong(byte[] buffer, ref int nowIndex)
    {
        var l = BitConverter.ToInt64(buffer, nowIndex);
        nowIndex += 8;
        return l;
    }
    public static T ReadingNetMsg<T>(byte[] buffer, ref int nowIndex)where T : INetMsg, new()
    {
        T msg = new T();
        msg.Reading(buffer,nowIndex);
        //不用单独加一次消息长度 进入Reading时候会增加
        //nowIndex += msg.GetMsgBytesSizeNum();
        return msg;
    }
}
