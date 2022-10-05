//TODO:挂起等待流程：有玩家加入后，开始挂起等待匹配满员 - 成功匹配，满员，该池子内变为开始游戏状态 - 发送消息给队列玩家，开始游戏
                                          //- 匹配等待60S，若60S没有满员，匹配失败，关闭该匹配池 - 发送消息给队列玩家，匹配失败，请重新匹配 
//TODO:交给客户端计算这60S，一个玩家60S没有匹配到满员，则自动关闭匹配，若匹配池为空，则关闭该匹配池
//TODO:通过效应消息发送匹配状态
//玩家开始匹配实际上就是进入到一个房间中，表现出的形式不同 TODO：①将匹配池和房间分开 ②将房间和匹配池整合

public class MatchingPool
{
    private ServerSocket server;
    
    private static int PoolID;
    public int CurPoolID { get; private set; } = 0;
    
    private const int MAXPLAYERCOUNT = 10;
    private int curPlayerCount;
    private int targetPlayerCount;
    
    public Dictionary<int,ClientSocket> PoolPlayerDic { get; private set; }

    public MatchingPoolState PoolState { get; set; } = MatchingPoolState.Empty;

    public MatchingPool(ServerSocket serverSocket)
    {
        server = serverSocket;

        PoolPlayerDic = new Dictionary<int, ClientSocket>();
        
        CurPoolID = ++PoolID;
        
        curPlayerCount = 0;
        targetPlayerCount = MAXPLAYERCOUNT;
        Console.WriteLine($"创建{CurPoolID}号匹配池,匹配目标数量为{targetPlayerCount}");
    }

    public void Join(int playerID,ClientSocket client)
    {
        if (!PoolPlayerDic.ContainsKey(playerID))
        {
            if (curPlayerCount == targetPlayerCount)
            {
                PoolState = MatchingPoolState.Full;
                Console.WriteLine($"{CurPoolID}号匹配池已满，无法继续加入玩家");
                return;
            }
        
            curPlayerCount = Math.Min(++curPlayerCount, targetPlayerCount);
            if (curPlayerCount > 0)
            {
                PoolState = MatchingPoolState.Wating;
            }
            else if (curPlayerCount == targetPlayerCount)
            {
                PoolState = MatchingPoolState.Full;
            }
            
            PoolPlayerDic.Add(playerID,client);
            Console.WriteLine($"{client.ClientID}客户端{playerID}号玩家加入到{CurPoolID}号匹配池中,当前池中有{curPlayerCount}名玩家");
        }
        else
        {
            Console.WriteLine($"{client.ClientID}客户端{playerID}号玩家已经在{CurPoolID}号匹配池中");
        }
       
    }

    //TODO:玩家退出游戏时若在匹配池中需要从中移除
    public void Leave(int playerID)
    {
        if(!PoolPlayerDic.ContainsKey(playerID) || PoolPlayerDic.Count == 0 ) return;
        
        curPlayerCount = Math.Max(--curPlayerCount, 0);
        PoolState = curPlayerCount switch
        {
            < 10 and > 0 => MatchingPoolState.Wating,
            0 => MatchingPoolState.Empty
        };

        Console.WriteLine($"{PoolPlayerDic[playerID].ClientID}客户端{playerID}号玩家离开了{CurPoolID}号匹配池,当前池中有{curPlayerCount}名玩家");
        PoolPlayerDic.Remove(playerID);
        

        if (curPlayerCount == 0)
        {
            ClearPool();
        }
    }

    //TODO:空池子让它仍然存在
    public void ClearPool()
    {
        //PoolID = Math.Max(PoolID--, 0);
        PoolPlayerDic.Clear();
        Console.WriteLine($"{CurPoolID}号匹配池清空");
    }
    
}

public enum MatchingPoolState
{
    Empty,
    Wating,
    Full
}