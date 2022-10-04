using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GenerataeProtoFileTool
{
    private static readonly string PATH_PROTOFILES = Application.dataPath + "/Editor/Protobuf/ProtoFiles/";
    private static int index = 0;
    
    [MenuItem("NetMsgTool/GenerateProtoFile",false,-11)]
    static void GenerateProtobufFile()
    {
        var protobufContent = "syntax = \"proto3\";\r\n\r\npackage namespace;\r\n\r\nmessage NetMsg{\r\n\tsfixed32 msgID = 1;\r\n\tsfixed32 msgLength = 2;\r\n}";

        var fileName = $"NewProto{index++}.proto";
        var path = PATH_PROTOFILES + fileName;
        
        File.WriteAllText(path,protobufContent,Encoding.UTF8);
        
        AssetDatabase.Refresh();
    }
}

