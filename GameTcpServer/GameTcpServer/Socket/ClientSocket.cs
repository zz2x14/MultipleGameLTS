using System.Net.Sockets;

public class ClientSocket
{
    private static int BeginIndex = 1;
    public int ClientID { get; private set; }

    private Socket clientSocket;
    private ServerSocket server;

    private bool IsConnected => clientSocket.Connected;

    private byte[] receiveBuffer = new byte[1024 * 1024];
    private int cacheNum = 0;

    public ClientSocket(Socket clientSocket, ServerSocket server)
    {
        ClientID = BeginIndex++;
        this.clientSocket = clientSocket;
        this.server = server;

        clientSocket.BeginReceive(receiveBuffer,cacheNum,receiveBuffer.Length,SocketFlags.None,ReceiveCallback,clientSocket);
    }

    private void ReceiveCallback(IAsyncResult iar)
    {
        try
        {
            if (clientSocket != null && IsConnected)
            {
                int receiveiNum = clientSocket.EndReceive(iar);
             
                HandleMsgData(receiveiNum);

                if (receiveiNum <= 0) return;

                clientSocket.BeginReceive(receiveBuffer, cacheNum, receiveBuffer.Length - cacheNum, SocketFlags.None, ReceiveCallback, clientSocket);
            }
            else
            {
                server!.CloseClient(this);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"接收消息出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
            server.CloseClient(this);
        }
        catch (Exception e)
        {
            Console.WriteLine($"接收消息出现错误(非网络因素),错误信息：{e.Message}");
            server.CloseClient(this);
        }
    }

    public void SendMsg(INetMsg msg)
    {
        try
        {
            if (clientSocket != null && IsConnected)
            {
                var bytes = msg.Writing();
                clientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, clientSocket);
            }
            else
            {
                server.CloseClient(this);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"发送消息出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
            server.CloseClient(this);
        }
        catch (Exception e)
        {
            Console.WriteLine($"发送消息出现错误(非网络因素),错误信息：{e.Message}");
            server.CloseClient(this);
        }
    }
    private void SendCallback(IAsyncResult iar)
    {
        try
        {
            if (clientSocket != null && IsConnected)
                clientSocket.EndSend(iar);
            
        }
        catch (SocketException e)
        {
            Console.WriteLine($"发送消息回调时现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
            server.CloseClient(this);
        }
        catch (Exception e)
        {
            Console.WriteLine($"发送消息回调时错误(非网络因素),错误信息：{e.Message}");
            server.CloseClient(this);
        }
    }

    public void SendMsg(byte[] bytes)
    {
        clientSocket.Send(bytes);
    }

    private void HandleMsgData(int receiveiNum)
    {
        int msgID = 0;
        int startIndex = 0;

        cacheNum += receiveiNum;

        while(clientSocket != null && IsConnected)
        {
            var msgLength = -1;
            if(cacheNum - startIndex >= 8)
            {
                msgID = BitConverter.ToInt32(receiveBuffer,startIndex);
                startIndex += 4;
                msgLength = BitConverter.ToInt32(receiveBuffer, startIndex);
                startIndex += 4;
                //Console.WriteLine(msgID);
            }

            if (msgLength != -1 && cacheNum - startIndex >= msgLength)
            {
                INetMsg message = null;

                message = server.msgHandler.GetNetMsg(msgID,receiveBuffer, startIndex);

                if (message == null)
                {
                    Console.WriteLine("收到未知类型消息：" + msgID);
                    return;
                }

                ThreadPool.QueueUserWorkItem(HandleMsg,(msgID,this,message));
                startIndex += msgLength;

                if (cacheNum == startIndex)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else//分包
            {
                //说明已经读取了ID和消息长度 需要减回长度
                if (msgLength != -1)
                    startIndex -= 8;
                //将剩下没读取完的部分重新放入数组 以便接收新的部分
                 Array.Copy(receiveBuffer,startIndex,receiveBuffer,0,cacheNum - startIndex);
                 
                 cacheNum -= startIndex;
            }

        }
    }

    private void HandleMsg(object o)
    {
        (int msgID,ClientSocket client, INetMsg msg) msgTuple = ((int ,ClientSocket, INetMsg))o;

        server.msgHandler.HandleMsg(msgTuple.msgID, msgTuple.client,msgTuple.msg);
    }

    public void Close()
    {
        if (clientSocket == null || !IsConnected ) return;

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();

        clientSocket = null;
    }
}

 