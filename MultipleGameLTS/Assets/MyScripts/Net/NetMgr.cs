using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Google.Protobuf;
using static ProtoNetTool;

public class NetMgr : MonoBehaviour
{
    public static NetMgr Instance { get; private set; }

    private Socket localClientSocket;
    
    private const string IP_SERVER_Local = "192.168.31.27";
    private const string TP_SERVER_ALIYUN = "116.62.240.242";
    private const int PORT_SERVER = 8080;
    private const int PORT_SERVER_ALIYUN = 3389;
    
    //public bool IsConnected => localClientSocket.Connected;
    
    private Queue<INetMsg> msgQueue = new Queue<INetMsg>();
    private byte[] receiveBuffer = new byte[1024 * 1024];
    private int cacheNum = 0;

    private MsgHandler msgHandler;

    public int UserID { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        DontDestroyOnLoad(gameObject);
        
        BeginConnect();

        StartCoroutine(nameof(HandleMsgCor));
    }

    public void BeginConnect()
    {
        if(localClientSocket is {Connected:true}) return;
        
        localClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint remote = new IPEndPoint(IPAddress.Parse(IP_SERVER_Local), PORT_SERVER);

        localClientSocket.BeginConnect(remote, ConnectCallback, localClientSocket);
    }

    private void ConnectCallback(IAsyncResult iar)
    {
        try
        {
            localClientSocket.EndConnect(iar);

            if (localClientSocket is {Connected:true})
            {
                Debug.Log("连接服务器成功" + localClientSocket.RemoteEndPoint);

                msgHandler = new MsgHandler();
                //netMsgLibrary = new NetMsgLibrary();

                localClientSocket.BeginReceive(receiveBuffer, cacheNum, receiveBuffer.Length - cacheNum,
                    SocketFlags.None, ReceiveCallback, localClientSocket);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"连接服务器出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"连接服务器出现错误(非网络因素),错误信息：{e.Message}");
        }
    }

    public void BeginSend(INetMsg msg)
    {
        try
        {
            if (localClientSocket is {Connected: true})
            {
                var bytes = msg.Writing();
                localClientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, localClientSocket);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"发送消息出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"发送消息出现错误(非网络因素),错误信息：{e.Message}");
        }
    }

    private void SendCallback(IAsyncResult iar)
    {
        try
        {
            localClientSocket.EndSend(iar);
            
            //Debug.Log("发送消息成功");
        }
        catch (SocketException e)
        {
            Debug.LogError($"发送消息回调出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"发送消息回调出现错误(非网络因素),错误信息：{e.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult iar)
    {
        try
        {
            int receiveNum = localClientSocket.EndReceive(iar);

            if (localClientSocket is {Connected:true})
            {
                HandleMsgData(receiveNum);
                
                if(receiveNum <= 0) return;
                
                localClientSocket.BeginReceive(receiveBuffer, cacheNum, receiveBuffer.Length - cacheNum,
                    SocketFlags.None, ReceiveCallback, localClientSocket);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"接收消息出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"接收消息出现错误(非网络因素),错误信息：{e.Message}");
        }
    }

    private void HandleMsgData(int receiveNum)
    {
        int msgID = 0;
        int startIndex = 0;

        cacheNum += receiveNum;
        
        while (localClientSocket is {Connected:true})
        {
            var msgLength = -1;
            if (cacheNum - startIndex >= 8)
            {
                msgID = BitConverter.ToInt32(receiveBuffer, startIndex);
                startIndex += 4;
                msgLength = BitConverter.ToInt32(receiveBuffer, startIndex);
                startIndex += 4;
            }
        
            if (msgLength != -1 && cacheNum - startIndex >= msgLength)
            {
                INetMsg msg = null;
                msg = msgHandler.GetNetMsg(msgID, receiveBuffer, startIndex);

                /*if (msg.GetType() == typeof(RequestMulNetIDMsg))
                {
                    Debug.Log((msg as RequestMulNetIDMsg).NetGOIDList.Count);
                }*/

                if (msg == null)
                {
                    Debug.LogWarning("收到未知类型消息：" + msgID);
                    return;
                }
        
                msgQueue.Enqueue(msg);
                startIndex += msgLength;
        
                if (startIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    startIndex -= 8;
                
                Array.Copy(receiveBuffer,startIndex,receiveBuffer,0,cacheNum - startIndex);
                cacheNum -= startIndex;
            }
        }
    }

    IEnumerator HandleMsgCor()
    {
        yield return new WaitUntil(() => localClientSocket is {Connected:true});
        
        while ( localClientSocket is {Connected:true})
        {
            if (msgQueue.Count > 0)
            {
                msgHandler.HandleMsg(msgQueue.Dequeue());
            }

            yield return null;
        }
    }

    public void Disconnect()
    {
        if(localClientSocket is not {Connected: true}) return;

        QuitNetMsg quitNetMsg = new QuitNetMsg();
        localClientSocket.Send(quitNetMsg.Writing());
        
        localClientSocket.Shutdown(SocketShutdown.Both);
        localClientSocket.Disconnect(false);
        localClientSocket.Close();

        localClientSocket = null;
        
        msgQueue.Clear();
    }
    
    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
