//TODO:挂起等待流程：有玩家加入后，开始挂起等待匹配满员 - 成功匹配，满员，该池子内变为开始游戏状态 - 发送消息给队列玩家，开始游戏
//TODO:                                            - 匹配等待60S，若60S没有满员，匹配失败，关闭该匹配池 - 发送消息给队列玩家，匹配失败，请重新匹配
//TODO:通过效应消息发送匹配状态

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
        Console.WriteLine($"创建{CurPoolID}号匹配池");
        
        curPlayerCount = 0;
        targetPlayerCount = MAXPLAYERCOUNT;
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
        
            curPlayerCount = Math.Min(curPlayerCount++, targetPlayerCount);
            if (curPlayerCount > 0)
            {
                PoolState = MatchingPoolState.Wating;
            }
            else if (curPlayerCount == targetPlayerCount)
            {
                PoolState = MatchingPoolState.Full;
            }
            
            PoolPlayerDic.Add(playerID,client);
            Console.WriteLine($"{client.ClientID}客户端加入到{CurPoolID}号匹配池中");
        }
        else
        {
            Console.WriteLine($"{client.ClientID}客户端已经在{CurPoolID}号匹配池中");
        }
       
    }

    public void Leave(int playerID)
    {
        if(!PoolPlayerDic.ContainsKey(playerID) || PoolPlayerDic.Count == 0 ) return;
        
        curPlayerCount = Math.Max(curPlayerCount--, 0);
        PoolState = curPlayerCount switch
        {
            < 10 and > 0 => MatchingPoolState.Wating,
            0 => MatchingPoolState.Empty
        };

        PoolPlayerDic.Remove(playerID);
        Console.WriteLine($"{PoolPlayerDic[playerID].ClientID}客户端离开了{CurPoolID}号匹配池");
    }

    public void ClosePool()
    {
        PoolID = Math.Max(PoolID--, 0);
        PoolPlayerDic.Clear();
        Console.WriteLine($"关闭{CurPoolID}号匹配池");
    }
    
}

public enum MatchingPoolState
{
    Empty,
    Wating,
    Full
}