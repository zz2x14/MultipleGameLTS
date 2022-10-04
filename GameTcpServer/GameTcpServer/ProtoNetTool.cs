using Google.Protobuf;

public static class ProtoNetTool
{
     public static byte[] Writing(IMessage netMsg)
     {
         using MemoryStream memoryStream = new MemoryStream();
         netMsg.WriteTo(memoryStream);
         byte[] bytes = memoryStream.ToArray();
         memoryStream.Close();
         return bytes;
     }

     public static T GetNetMsg<T>(byte[] bytes, int startIndex, int length)where T : class,IMessage
     {
         var msgType = typeof(T);
         var parser = msgType.GetProperty("Parser");
         var parserObj = parser?.GetValue(null, null);
         var parserFunc = parserObj?.GetType().GetMethod("ParseFrom",new []{typeof(byte[]),typeof(int),typeof(int)});

         var msgObj = parserFunc?.Invoke(parserObj, new object[]{bytes, startIndex, length});
         return msgObj as T;
     }
     
     public static int GetNetMsgLength(IMessage netMsg)
     {
         return netMsg.ToByteArray().Length - 8;
     }
    
}