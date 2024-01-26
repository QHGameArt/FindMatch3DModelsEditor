using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{

     class MyClass
    {
        public  string Id;
        public  string PrefabName;
        public  string IconName;
        public  string ObjectType;
        public  List<string> TagList;
    }
     
    public static class SetObjConfig
    {
        [MenuItem("Custom/生成配置文件")]
        public static void SetConfig()
        {
            List<MyClass> myClasss=new List<MyClass>();
            for (int i = 1; i < 200; i++)
            {
                string folderPath = GameCommPath.ObjPath + i + GameCommPath.ObjPathPrefab;
                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath);

                    for (int j = 0; j < files.Length; j++)
                    {

                        if (files[j].EndsWith(".Prefab", System.StringComparison.OrdinalIgnoreCase))
                        {
                            MyClass myClass = new MyClass();

                            myClass.PrefabName = Path.GetFileName(files[j]).Replace(".prefab", "");
                            myClass.Id = myClass.PrefabName.Replace("obj_", "");
                            myClass.IconName = GameCommPath.IconStart + myClass.PrefabName;
                            myClass.ObjectType = "Normal";
                            myClass.TagList = new List<string>() { "New" };
                            myClasss.Add(myClass);
                        }
                    }
                }
            }


            // 将数据对象转换为 JSON 格式的字符串
            string json = LitJson.JsonMapper.ToJson(myClasss);

            // 指定文件路径
            string filePath = GameCommPath.ConfigPath;

            // 创建一个写入器
            StreamWriter writer = new StreamWriter(filePath);
        
            // 写入 JSON 字符串
            writer.Write(json);

            // 关闭写入器
            writer.Close();
        
            Debug.Log("JSON文件写入完成");
        }
    }
}