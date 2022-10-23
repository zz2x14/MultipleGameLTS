using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using static DatabaseTool;
using static DBContentLibrary;

//TODO:服务器掉了记得处理
public class ServerSocket
{
    private Socket tcpServerSocket;
    public MsgHandler msgHandler { get; private set; }
    
    private const string IP_SERVER_LOCAL = "192.168.31.27";
    private const string IP_SERVER_ALIYUN = "172.24.158.45";
    private const string IP_SERVER_TENCENT = "10.0.12.9";
    private const int PORT_SERVER = 8080;
    private const int PORT_SERVER_ALIYUN = 3389;
    private const int LISTENNUM = 1024;
    
    private Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();

    //TODO:UserID、RoleID也是需要进一步处理的  - 以及玩家在哪个场景（数据库中表现为 sceneID对应玩家）
    //TODO:物体若摧毁或者隐藏则需要移除ID
    
    //ID才是真正唯一的标识，其它都会可能发生改变
    private Dictionary<int,ClientSocket> netPlayerDic = new Dictionary<int,ClientSocket>();
    private Dictionary<int, INetGameObject> netGoDic = new Dictionary<int, INetGameObject>();
    
    //TODO:暂时 使用随机数获取到网络ID，使用ID不同区域范围划分Player和其它GO  - 直接使用随机数赋予网络ID还需考虑溢出
    private const int NETPLAYERID_MIN = 9;
    private const int NETPLAYERID_MAX = 255;
    private const int NETGOID_MIN = 256;
    private const int NETGOID_MAX = 9999;

    private Dictionary<int, MatchingPool> matchingPoolDic = new Dictionary<int, MatchingPool>();

    public ServerSocket()
    {
        tcpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        EndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(IP_SERVER_LOCAL), PORT_SERVER);

        try
        {
            tcpServerSocket.Bind(ipEndPoint);

            tcpServerSocket.Listen(LISTENNUM);

            SelectDatabase("mygamedb");

            msgHandler = new MsgHandler(this);

            MatchingPool preparedPool = new MatchingPool(this);
            matchingPoolDic.Add(preparedPool.CurPoolID,preparedPool);

            ThreadPool.QueueUserWorkItem(OperateMatchingPool, null);
            
            tcpServerSocket.BeginAccept(AcceptCallback, tcpServerSocket);

            Console.WriteLine("服务器开启成功");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"连接服务器出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"连接服务器出现错误(非网络因素),错误信息：{e.Message}");
        }
    }

    private void AcceptCallback(IAsyncResult iar)
    {
        try
        {
            Socket clientSocket = tcpServerSocket.EndAccept(iar);
            ClientSocket client = new ClientSocket(clientSocket, this);
            Console.WriteLine($"{client.ClientID}客户端接入成功" + DateTime.Now);

            lock (clientDic)
            {
                clientDic.Add(client.ClientID, client);
            }

            tcpServerSocket.BeginAccept(AcceptCallback, tcpServerSocket);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"确认连接服务器出现错误,错误码：{e.ErrorCode},错误信息：{e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"确认连接服务器出现错误(非网络因素),错误信息：{e.Message}");
        }
    }
    
    public void BroadCast(INetMsg msg)
    {
        lock (clientDic)
        {
            if (clientDic.Count == 0) return;

            foreach (var client in clientDic.Values)
            {
                client.SendMsg(msg);
            }
        }
    }
    /// <summary>
    /// 将目标客户端排除在外发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="msgID">目标客户端ID</param>
    public void BroadCast(INetMsg msg,int msgID)
    {
        lock (clientDic)
        {
            if(clientDic.Count == 0) return;

            foreach (var id in clientDic.Keys)
            {
                if (id != msgID)
                {
                    clientDic[id].SendMsg(msg);
                }
            }
        }
       
    }
    
    public void BroadCast(byte[] bytes)
    {
        lock (clientDic)
        {
            if (clientDic.Count == 0) return;

            foreach (var client in clientDic.Values)
            {
                client.SendMsg(bytes);
            }
        }
    }

    public void SendMsgToOne(int clientID,INetMsg msg)
    {
        lock (clientDic)
        {
            if (clientDic.ContainsKey(clientID))
            {
                clientDic[clientID].SendMsg(msg);
            }
        }
    }
   
    public void CloseClient(ClientSocket client)
    {
        lock (clientDic)
        {
            if(clientDic.Count == 0) return ;
            client.Close();

            if (clientDic.ContainsKey(client.ClientID))
            {
                Console.WriteLine($"客户端{client.ClientID}主动断开连接" + DateTime.Now);
                clientDic.Remove(client.ClientID);
                
                lock (netPlayerDic)
                {
                    netPlayerDic.Remove(client.ClientID);
                }
            }
        }
    }

    public ResponseNetMsg MatchPlayerAccountNumInfo(ref UserNetMsg? msg)
    {
        ResponseNetMsg response = new ResponseNetMsg();

        if (msg!.DoRegister)
        {
            if (CheckSthUQDoExist(msg.AccountNum, DBLIST_ANUM, DBTABLE_USER))
            {
                response.ResponseID = MsgHandler.ID_RESPONSE_REPEATEDNUM;
            }
            else
            {
                var dbValueList = new List<string>
                {
                    DBLIST_ANUM,
                    DBLIST_PW
                };
                var contentList = new List<string>
                {
                    msg.AccountNum,
                    msg.Password
                };
                InsertInfo(DBTABLE_USER,dbValueList,contentList);

                var tuple = GetUserIDFromDB(msg.AccountNum, msg.Password);
                CloseDataReader(tuple.reader);
                msg.UserID = tuple.usrID;

                response.ResponseID = MsgHandler.ID_RESPONSE_REGISTER;
            }
        }
        else
        {
            if (!CheckSthUQDoExist(msg.AccountNum, DBLIST_ANUM, DBTABLE_USER))
            {
                response.ResponseID = MsgHandler.ID_RESPONSE_NOTEXISTENT;
            }
            else
            {
                response.ResponseID = CheckUserPasswordCorrect(msg.AccountNum, msg.Password) ? MsgHandler.ID_RESPONSE_LOGIN : MsgHandler.ID_RESPONSE_WRONG;
            }
            
        }
        
        return response;  
    }

    public ResponseNetMsg MatchPlayerRoleInfo(ref CreateRoleNetMsg createRoleNetMsg)
    {
        ResponseNetMsg response = new ResponseNetMsg();

        if (CheckSthUQDoExist(createRoleNetMsg.RoleName, DBLIST_ROLENAME, DBTABLE_ROLE))
        {
            response.ResponseID = MsgHandler.ID_RESPONSE_REPETEDNAME;
        }
        else
        {
            InsertRoleInfo(createRoleNetMsg.UserID,createRoleNetMsg.RoleName);
            
            var tuple = GetRoleIDFromDB(createRoleNetMsg.RoleName);
            CloseDataReader(tuple.reader);
            createRoleNetMsg.RoleID = tuple.roleID;
            
            response.ResponseID = MsgHandler.ID_RESPONSE_ROLE;
        }

        return response;
    }

    /// <summary>
    /// 随机生成角色网络ID
    /// </summary>
    public void GiveNetID(ClientSocket client,ref BornNetMsg bornNetMsg)
    {
        var random = new Random();

        lock (netPlayerDic)
        {
            var randomNetID = NETPLAYERID_MAX;
            
            do
            { 
                randomNetID = random.Next(NETPLAYERID_MIN, NETPLAYERID_MAX);
                if (!netPlayerDic.ContainsKey(randomNetID))
                {
                    bornNetMsg.NetID = randomNetID;
                    
                    netPlayerDic.Add(randomNetID,client);
                }

            } while (!netPlayerDic.ContainsKey(randomNetID));
        }
       
    }
    /// <summary>
    /// 随机生成游戏物体网络ID
    /// </summary>
    public void GiveNetID(RequestMulNetIDMsg requestMulNetIdMsg)
    {
        var random = new Random();

        lock (netGoDic)
        {
            for (int i = 0; i < requestMulNetIdMsg.Count; i++)
            {
                var randomNetID = NETGOID_MAX;

                while (!netGoDic.ContainsKey(randomNetID))
                {
                    randomNetID = random.Next(NETGOID_MIN, NETGOID_MAX);
                    netGoDic.Add(randomNetID,null);

                    requestMulNetIdMsg.GetSingleNetID(randomNetID);
                }
            }     
        }
    }

    public void UpdateNetPlayer(int requireClientID,ref RequireUpdateNetMsg requireUpdatemsg)
    {
        lock (netPlayerDic)
        {
            if(netPlayerDic.Count <= 1) return;
            
            //LINQ的效率是低于foreach、for循环的，频繁的网络消息或许应该减少使用
            foreach (var clientID in netPlayerDic.Keys )
            {
                if (clientID != requireClientID)
                {
                    requireUpdatemsg.ListCount += 1;
                    requireUpdatemsg.UpdateNetPlayerList(clientID);
                }
            }
        }
    }

    //TODO:应该将匹配池和匹配池的玩家放在数据库中
    public void JoinAvailableMatchingPool(ref MatchGameNetMsg matchGameNetMsg)
    {
        if (netPlayerDic.ContainsKey(matchGameNetMsg.PlayerNetID))
        {
            if (matchingPoolDic.Count == 0)
            {
                MatchingPool newPool = new MatchingPool(this);
                newPool.Join(matchGameNetMsg.PlayerNetID,netPlayerDic[matchGameNetMsg.PlayerNetID]);

                matchGameNetMsg.MatchingPoolID = newPool.CurPoolID;
            }
            else
            {
                foreach (var matchingPool in matchingPoolDic.Values.Where(matchingPool => matchingPool.PoolState is MatchingPoolState.Wating or MatchingPoolState.Empty))
                {
                    matchingPool.Join(matchGameNetMsg.PlayerNetID,netPlayerDic[matchGameNetMsg.PlayerNetID]);
                    
                    matchGameNetMsg.MatchingPoolID = matchingPool.CurPoolID;
                    break;
                }
            }
            
        }
        else
        {
            //TODO：玩家没有在服务器的在线玩家列表中 将玩家添加到列表中 取消玩家匹配请求，发出网络消息 让玩家再次发起匹配请求
            //几乎不会出现这种情况，但是还是要考虑
        }
        
    }

    public void LeaveMatchingPool(MatchGameNetMsg matchGameNetMsg)
    {
        if (matchingPoolDic.ContainsKey(matchGameNetMsg.MatchingPoolID))
        {
            matchingPoolDic[matchGameNetMsg.MatchingPoolID].Leave(matchGameNetMsg.PlayerNetID);
        }
        else
        {
            //TODO:匹配池ID不合理
            //几乎不会出现这种情况，但是还是要考虑
        }
    }

    public void OperateMatchingPool(object o)
    {
        while (tcpServerSocket.Connected)
        {
            //检测满了的匹配池是否状态错误 TODO:还应检测waiting状态
            foreach (var matchingPool in matchingPoolDic.Values.Where(matchingPool => matchingPool.CurPlayerCount == matchingPool.TargetPlayerCount 
                         &&  matchingPool.PoolState != MatchingPoolState.Full))
            {
                matchingPool.PoolState = MatchingPoolState.Full;
            }
            
            //TODO：真实至少两个玩家在线才能匹配，为了测试改为一个
            
            if (clientDic.Count > 0 && netPlayerDic.Count > 0 && matchingPoolDic.Count > 0)
            {
                foreach (var fullPool in matchingPoolDic.Values.Where(fullPool => fullPool.PoolState == MatchingPoolState.Full))
                {
                    fullPool.MatchOver();
                }
            }
            
        }
    }
    

}


