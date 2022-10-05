using Google.Protobuf;

 public class MsgHandler
 {
     private readonly Dictionary<int, Type> msgTypeDic;
     private Dictionary<int, Action<INetMsg, ClientSocket>> netMsgHandlerDic;
     
     private const int ID_NETMSG_QUIT = 777;
     private const int ID_NETMSG_USER = 1001;
     private const int ID_NETMSG_CREATEROLE = 1002;
     private const int ID_NETMSG_BORN = 1003;
     private const int ID_NETMSG_NETID = 1004;
     private const int ID_NETMSG_TF = 1005;
     private const int ID_NETMSG_REQUIRE = 1006;
     private const int ID_NETMSG_MULNETID = 1007;
     private const int ID_NETMSG_MATCHGAME = 1009;
     private const int ID_NETMSG_RESPONSE = 1999;
     
     public const int ID_RESPONSE_LOGIN = 1;
     public const int ID_RESPONSE_WRONG = 2;
     public const int ID_RESPONSE_NOTEXISTENT = 3;
     public const int ID_RESPONSE_REPEATEDNUM = 4;
     public const int ID_RESPONSE_REGISTER = 5;
     public const int ID_RESPONSE_REPETEDNAME = 6;
     public const int ID_RESPONSE_ROLE = 7;
     private ServerSocket server;
       
     public MsgHandler(ServerSocket server)
     {
         this.server = server;

         msgTypeDic = new Dictionary<int, Type>();
         netMsgHandlerDic = new Dictionary<int, Action<INetMsg, ClientSocket>>();
         Register(ID_NETMSG_QUIT, typeof(QuitNetMsg), QuitMsgHandler);
         Register(ID_NETMSG_USER, typeof(UserNetMsg), UserMsgHandler);
         Register(ID_NETMSG_CREATEROLE, typeof(CreateRoleNetMsg), CreateRoleMsgHandler);
         Register(ID_NETMSG_BORN, typeof(BornNetMsg), BornNetMsgHandler);
         Register(ID_NETMSG_TF, typeof(TransformNetMsg), TransformMsgHandler);
         Register(ID_NETMSG_REQUIRE, typeof(RequireUpdateNetMsg), RequireUpdateMsgHandler);
         Register(ID_NETMSG_MULNETID,typeof(RequestMulNetIDMsg),MulNetIDMsgHandler);
         Register(ID_NETMSG_MATCHGAME,typeof(MatchGameNetMsg),MatchGameMsgHandler);
     }

     private void Register(int netMsgID, Type msgType, Action<INetMsg, ClientSocket> action)
     {
         msgTypeDic.Add(netMsgID, msgType);
         netMsgHandlerDic.Add(netMsgID, action);
     }

     public INetMsg GetNetMsg(int netMsgId, byte[] bytes, int startIndex)
     {
         if (msgTypeDic.ContainsKey(netMsgId))
         {
             var msg = Activator.CreateInstance(msgTypeDic[netMsgId]) as INetMsg;
             msg?.Reading(bytes, startIndex);
             return msg!;
         }

         return null!;
     }

     public void HandleMsg(int msgID, ClientSocket client, INetMsg msg)
     {
         if (netMsgHandlerDic.ContainsKey(msgID))
         {
             netMsgHandlerDic[msgID].Invoke(msg, client);
         }
     }

     private void ResponseMsgHandler(INetMsg msg, ClientSocket client)
     {
            //服务端暂时不会直接收到响应信息
     }

     private void QuitMsgHandler(INetMsg msg, ClientSocket client)
     {
         server.CloseClient(client);
     }

     private void UserMsgHandler(INetMsg msg, ClientSocket client)
     {
         var userNetMsg = msg as UserNetMsg;
         ResponseNetMsg response = server.MatchPlayerAccountNumInfo(ref userNetMsg);

         server.SendMsgToOne(client.ClientID, response);

         if (userNetMsg!.DoRegister && response.ResponseID == ID_RESPONSE_REGISTER)
         {
             server.SendMsgToOne(client.ClientID, userNetMsg);
         }
     }

     private void CreateRoleMsgHandler(INetMsg msg, ClientSocket client)
     {
         var createRoleNetMsg = msg as CreateRoleNetMsg;
         server.SendMsgToOne(client.ClientID, server.MatchPlayerRoleInfo(ref createRoleNetMsg!));
         server.SendMsgToOne(client.ClientID, createRoleNetMsg);
     }

     private void BornNetMsgHandler(INetMsg msg, ClientSocket client)
     {
         var born = msg as BornNetMsg;
         var selfNetIDMsg = new NetIDMsg();

         if (born!.IsNetPlayer)
         {
             server.GiveNetID(client, ref born);
             selfNetIDMsg.SelfNetID = born.NetID;

             server.SendMsgToOne(client.ClientID, selfNetIDMsg);
             server.BroadCast(born, client.ClientID);
         }
     }

     private void TransformMsgHandler(INetMsg msg, ClientSocket client)
     {
         var transformMsg = msg as TransformNetMsg;

         server.BroadCast(transformMsg!, client.ClientID);
     }

     private void RequireUpdateMsgHandler(INetMsg msg, ClientSocket client)
     {
         var require = msg as RequireUpdateNetMsg;
         require!.CurNetPlayerNetIDList = new List<int>();
         server.UpdateNetPlayer(client.ClientID, ref require);

         if (require.CurNetPlayerNetIDList.Count == 0) return;

         server.SendMsgToOne(client.ClientID, require);
         Console.WriteLine("目标客户端ID：" + client.ClientID);
     }

     private void MulNetIDMsgHandler(INetMsg msg, ClientSocket client)
     {
         var requestMulNetIdMsg = msg as RequestMulNetIDMsg;
         server.GiveNetID(requestMulNetIdMsg!);
         
         server.BroadCast(requestMulNetIdMsg);
     }
     
     private void MatchGameMsgHandler(INetMsg msg,ClientSocket client)
     {
         var matchGameMsg = msg as MatchGameNetMsg;

         if (matchGameMsg.DoMatch)
         {
             server.JoinAvailableMatchingPool(ref matchGameMsg);
         }
         else
         {
             server.LeaveMatchingPool(matchGameMsg);
         }
         
         server.SendMsgToOne(client.ClientID,matchGameMsg);

     }

   
 }
