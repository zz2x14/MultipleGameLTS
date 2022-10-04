using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public static class GenerateNetMsgTool 
{
   private static readonly string PATH_PROTOC = Application.dataPath + "/Editor/Protobuf/Protoc/protoc.exe";
   private static readonly string PATH_PROTOFILES = Application.dataPath + "/Editor/Protobuf/ProtoFiles";
   private static readonly string PATH_CSHARPSAVE = Application.dataPath + "/MyScripts/Protobuf";
   private static readonly string EXTENSION_PROTO = ".proto";

   private static readonly List<string> protoList = new List<string>();

   [MenuItem("NetMsgTool/GenerateCSharp")]
   static void GenerateNetMsgByCSharp()
   {
      DirectoryInfo directoryInfo = Directory.CreateDirectory(PATH_PROTOFILES);

      FileInfo[] fileInfos = directoryInfo.GetFiles();

      foreach (var fileInfo in fileInfos)
      {
         if (fileInfo.Extension == EXTENSION_PROTO && !protoList.Contains(fileInfo.Name))
         {
            protoList.Add(fileInfo.Name);
            
            Process cmd = new Process();

            cmd.StartInfo.FileName = PATH_PROTOC;
            cmd.StartInfo.Arguments = $"-I={PATH_PROTOFILES} --csharp_out={PATH_CSHARPSAVE} {fileInfo.Name}";

            cmd.Start();
            
            Debug.Log($"{fileInfo.Name}生成C#文件成功");
         }
      }
      
      AssetDatabase.Refresh();
   }
   
   [MenuItem("NetMsgTool/GenerateCSharpOverwrite")]
   static void GenerateNetMsgByCSharpOverwrite()
   {
      DirectoryInfo directoryInfo = Directory.CreateDirectory(PATH_PROTOFILES);

      FileInfo[] fileInfos = directoryInfo.GetFiles();

      foreach (var fileInfo in fileInfos)
      {
         if (fileInfo.Extension == EXTENSION_PROTO)
         {
            Process cmd = new Process();

            cmd.StartInfo.FileName = PATH_PROTOC;
            cmd.StartInfo.Arguments = $"-I={PATH_PROTOFILES} --csharp_out={PATH_CSHARPSAVE} {fileInfo.Name}";

            cmd.Start();
            
            Debug.Log($"{fileInfo.Name}生成C#文件成功");
         }
      }
      
      AssetDatabase.Refresh();
   }
}
