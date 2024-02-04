using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;


public class GetSelectedPrefab : EditorWindow
{

    #region 自动选择/选择重复的模型删除掉()
    
    [MenuItem("Custom/整理模型流程/1:自动选择ModelPath，选择后重复的模型删除掉")]
    private static void GetSelectedPrefabInfo()
    {
        //GameObject[] selectedObjects = Selection.gameObjects;
        TextureName = new Dictionary<string, int>();
        DeletgameObj = new List<GameObject>();
        //if (selectedObjects.Length > 0)
        //{
            
            //for (int i = 0; i < selectedObjects.Length; i++)
            //{
            //    Debug.Log(selectedObjects[i].name);
//
            //}
            GetAllChildObjects(GameObject.Find(GameCommPath.ScenceModePath).transform);
            for (int i = 0; i < DeletgameObj.Count; i++)
            {
                Debug.LogError(DeletgameObj[i].name);
                DestroyImmediate(DeletgameObj[i]);
            }
        //}
        //else
        //{
        //    Debug.LogError("请先选择一个或多个物体！");
        //}
    }

    private static Dictionary<string, int> TextureName;
    private static List<GameObject> DeletgameObj ;
    private static void GetAllChildObjects(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            MeshRenderer m= child.GetComponent<MeshRenderer>();
            if (m!=null)
            {
                try
                {
                    string name = m.sharedMaterial.name;
                    if (TextureName.ContainsKey(name))
                    {
                        DeletgameObj.Add(child.gameObject);
                    }
                    else
                    {
                        TextureName.Add(name,0);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(child.name);
                    throw;
                    return;
                }
              

            }
            
            GetAllChildObjects(child);
        }
    }
    
    #endregion
    
    
    #region 自动选择/删除掉模型之后统一摆放到modelpath下()

    
    private static List<GameObject> MovegameObj ;
    [MenuItem("Custom/整理模型流程/2:删除掉模型之后统一摆放到modelpath下")]
    private static void GetSelectedPrefabInfo2()
    {
        MovegameObj = new List<GameObject>();
        GetAllChildObjects2(GameObject.Find(GameCommPath.ScenceModePath).transform);
        int H=0,W = 0;
        for (int i = 0; i < MovegameObj.Count; i++)
        {
            H++;
            if (H>20)
            {
                W++;
                H=0;
            }
            MovegameObj[i].transform.SetParent(GameObject.Find(GameCommPath.ScenceModePath).transform);
            MovegameObj[i].transform.position = new Vector3(W * 2, 0,H * 2);
            MovegameObj[i].transform.rotation = quaternion.identity;
            DestroyImmediate(MovegameObj[i].GetComponent<Rigidbody>()); 
            
             var scase = new Vector3(MovegameObj[i].transform.localScale.x, MovegameObj[i].transform.localScale.y,MovegameObj[i].transform.localScale.z);
             if (scase.x>4)
             {
                 MovegameObj[i].transform.localScale = new Vector3(scase.x/100f, scase.y/100f,scase.z/100f);
             }
          
        }
    }
    private static void GetAllChildObjects2(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            MeshRenderer m= child.GetComponent<MeshRenderer>();
            if (m!=null)
            {
                MovegameObj.Add(child.gameObject);
            }
            
            GetAllChildObjects2(child);
        }
    }
    
    
    #endregion
    
    
        
    #region 把物体整理成预制体放入文件夹
    
    private static List<GameObject> SetPrefabList ;

    [MenuItem("Custom/整理模型流程/3:调整模型并且保存成预制体")]
    private static void GetSelectedPrefabInfo3()
    {
        SetPrefabList = new List<GameObject>();
        GetAllChildObjects3(GameObject.Find(GameCommPath.ScenceModePath).transform);

        for (int i = 0; i < SetPrefabList.Count; i++)
        {
            GameObject newObj=  new GameObject("new");
            newObj.transform.SetParent(GameObject.Find(GameCommPath.ScenceModePath).transform);
            newObj.AddComponent<Rigidbody>();
            newObj.transform.localPosition=Vector3.zero;
            newObj.transform.rotation=Quaternion.identity;
            newObj.transform.localScale=Vector3.one;
            SetPrefabList[i].transform.SetParent(newObj.transform);
            SetPrefabList[i].transform.localPosition=Vector3.zero;
            string name = SetPrefabList[i].GetComponent<MeshRenderer>().sharedMaterial.mainTexture.name;
            newObj.transform.name = GameCommPath.ObjStart + name;
            newObj.tag = "LevelObject";
            SetPrefabList[i].name = name;
            if (SetPrefabList[i].GetComponent<Rigidbody>()!=null)
            {
              DestroyImmediate( SetPrefabList[i].GetComponent<Rigidbody>()); 
            }
            // 获取保存的文件路径
            string folderPath =GameCommPath.ObjPath; // 替换为您想要保存的文件夹路径
            string oldPath= getPath2($"/{GameCommPath.ObjStart + name}.prefab");
            string prefabPath  ;
            if (!string.IsNullOrEmpty(oldPath))
            {
                prefabPath = oldPath;
            }
            else
            {
                prefabPath=$"{getPath()}/{GameCommPath.ObjStart + name}.prefab";
            }
            
            // 创建预制体
            Object prefab = PrefabUtility.SaveAsPrefabAsset(newObj, prefabPath);
        }
        
        
        
        int H=0,W = 0;
        for (int i = 0; i < SetPrefabList.Count; i++)
        {
            H++;
            if (H>20)
            {
                W++;
                H=0;
            }
            SetPrefabList[i].transform.position = new Vector3(W * 2, 0,H * 2);
            SetPrefabList[i].transform.rotation = quaternion.identity;
            if (SetPrefabList[i].GetComponent<Rigidbody>()!=null)
            {
                DestroyImmediate(SetPrefabList[i].GetComponent<Rigidbody>()); 
            }
            
            
            var scase = new Vector3(SetPrefabList[i].transform.localScale.x, SetPrefabList[i].transform.localScale.y,SetPrefabList[i].transform.localScale.z);
            if (scase.x>4)
            {
                SetPrefabList[i].transform.localScale = new Vector3(scase.x/100f, scase.y/100f,scase.z/100f);
            }
        }
        
    }
/// <summary>
/// 获得有没有已经生成过的预制体
/// </summary>
/// <returns></returns>
    public static string getPath2(string prefabName)
    {
        for (int i = 1; i < 200; i++)
        {
            string folderPath = GameCommPath.ObjPath + i + GameCommPath.ObjPathPrefab;
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                for (int j = 0; j < files.Length; j++)
                {
                    if (files[j].Equals(folderPath + prefabName))
                    {
                        return files[j];
                    }
                }
            }
        }
        return "";
    }
    
    
    /// <summary>
    /// 50个模型一个文件夹，算上mate*2
    /// </summary>
    /// <returns></returns>
    public static string getPath()
    {
        for (int i = 1; i < 200; i++)
        {
            string folderPath = GameCommPath.ObjPath + i + GameCommPath.ObjPathPrefab;

            // 检查文件夹路径是否存在，如果不存在则创建
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string[] files = Directory.GetFiles(folderPath);
            if (files.Length<=100)
            {
                return folderPath;
            }
        }
        return GameCommPath.ObjPath + 1 + GameCommPath.ObjPathPrefab;
    }
    private static void GetAllChildObjects3(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            MeshRenderer m= child.GetComponent<MeshRenderer>();
            if (m!=null)
            {
                SetPrefabList.Add(child.gameObject);
            }
            
            GetAllChildObjects2(child);
        }
    }

    #endregion
    
    #region 把物体整理成预制体放入文件夹



    public static void ArrangeModel()
    {
        //便利所有的预制体
        //按照名字排序
        //重新分装文件夹

        List<string> paths = new List<string>();
        List<string> paths2 = new List<string>();
        for (int i = 1; i < 200; i++)
        {
            string folderPath = GameCommPath.ObjPath + i + GameCommPath.ObjPathPrefab;
            if (Directory.Exists(folderPath))
            {
                string[] files =  Directory.GetFiles(folderPath, "*.prefab");
                for (int j = 0; j < files.Length; j++)
                {
                   string a= files[j].Replace("ObjModel" + i, "SSSSSSS");
                    paths.Add(a);
                }
                paths2.AddRange(files);
            }
        }

        paths.Sort(new CustomStringComparer());

        int d = 1;
        int e = 1;
        for (int i = 0; i < paths.Count; i++)
        {
            d++;
            if (d>=50)
            {
                e++;
                d = 0;
            }
            string a= paths[i].Replace("SSSSSSS","ObjModel" + e);
            string path="";
            int c = 0;
            for (int j = 0; j < 200; j++)
            {
                string b= paths[i].Replace("SSSSSSS","ObjModel" + j);
                if (paths2.Contains(b))
                {
                    c = j;
                    path = b;
                    break;
                }
            }
            string folderPathIconY = GameCommPath.ObjPath + c + GameCommPath.ObjPathIcon+"/"+GameCommPath.IconStart+Path.GetFileName(paths[i]).Replace(".prefab",".png");
            string folderPathIconM = GameCommPath.ObjPath + e + GameCommPath.ObjPathIcon+"/"+GameCommPath.IconStart+Path.GetFileName(a).Replace(".prefab",".png");

            if (!string.IsNullOrEmpty(path))
            {
               AssetDatabase.MoveAsset(path, a);
               AssetDatabase.MoveAsset(folderPathIconY, folderPathIconM);
            }
        }
    }
    
    
    public class CustomStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            
            int minLength = Mathf.Min(x.Length, y.Length);
        
            for (int i = 0; i < minLength; i++)
            {
                if (x[i] != y[i])
                {
                    return x[i].CompareTo(y[i]); // 比较当前位的ASCII码
                }
            }
            // 自定义比较规则，例如按字符串长度排序
            return x.Length.CompareTo(y.Length);
        }
    }


    
    [MenuItem("Custom/整理模型流程/4:生成预制体之后把他们引用的资源归类")]
    public static void SetZiYuan()
    {
        ArrangeModel();
        
        for (int i = 1; i < 200; i++)
        {
            string folderPath = GameCommPath.ObjPath + i + GameCommPath.ObjPathPrefab;
            if (Directory.Exists(folderPath))
            {
               
                string[] files =  Directory.GetFiles(folderPath, "*.prefab");
                for (int j = 0; j < files.Length; j++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[j]);
                    //获得图片，获得材质球，获得fbx
                    string fbxPath = AssetDatabase.GetAssetPath(prefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
                    string MeaPath = AssetDatabase.GetAssetPath(prefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial);
                    string TexPath = AssetDatabase.GetAssetPath(prefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.mainTexture);

                   string rootName=  files[j].Replace( Path.Combine(GameCommPath.ObjPathPrefab,Path.GetFileName(files[j])), "");
                    string newObjectPathFbx=rootName+GameCommPath.ObjPathFbx+"/"+Path.GetFileName(fbxPath);
                    string newObjectPathMeta=rootName+GameCommPath.ObjPathMaterial+"/"+Path.GetFileName(MeaPath);
                    string newObjectPathText=rootName+GameCommPath.ObjPathTextre+"/"+Path.GetFileName(TexPath);
                    
                    // 检查文件夹路径是否存在，如果不存在则创建
                    if (!Directory.Exists(rootName+GameCommPath.ObjPathFbx)) { Directory.CreateDirectory(rootName+GameCommPath.ObjPathFbx); }
                    if (!Directory.Exists(rootName+GameCommPath.ObjPathTextre)) { Directory.CreateDirectory(rootName+GameCommPath.ObjPathTextre); }
                    if (!Directory.Exists(rootName+GameCommPath.ObjPathMaterial)) { Directory.CreateDirectory(rootName+GameCommPath.ObjPathMaterial); }
                    
                    
                    AssetDatabase.MoveAsset(fbxPath, newObjectPathFbx);
                    AssetDatabase.MoveAsset(MeaPath, newObjectPathMeta);
                    AssetDatabase.MoveAsset(TexPath, newObjectPathText);
                    AssetDatabase.Refresh();
                }
            }
        }
        

    }

    #endregion
 
    
#region 选中物体生成到场景中

[MenuItem("Custom/美术流程优化/手动选择Model，选择后生成到场景中")]
public static void CelectObj()
{
    GameObject[] selectedObjects = Selection.gameObjects;

    if (selectedObjects.Length > 0)
    {
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObjects[i]);
            if (assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                if (selectedObjects != null)
                {
                    // 创建预制体实例并添加到场景中
                    GameObject newObject = PrefabUtility.InstantiatePrefab(selectedObjects[i],  GameObject.Find(GameCommPath.ScenceModePath).transform)as GameObject  ; 
                    //GameObject newObject = Instantiate(selectedObjects[i], GameObject.Find(GameCommPath.ScenceModePath).transform); // 在场景中实例化新的Prefab对象
                    newObject.name = selectedObjects[i].name; // 给新的Prefab对象重命名
                    newObject.AddComponent<Rigidbody>();
                    MeshCollider a=newObject.AddComponent<MeshCollider>();
                    a.convex = true;
                }
                else
                {
                    Debug.Log("Failed to generate Prefab.");
                }

            }
        }

    }
}



[MenuItem("Custom/美术流程优化/增加mesh，")]
public static void CelectObj2()
{
    GameObject[] selectedObjects = Selection.gameObjects;

    if (selectedObjects.Length > 0)
    {
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            MeshCollider a = selectedObjects[i].transform.GetChild(0).GetComponent<MeshCollider>();

            if (a==null)
            {
                Debug.Log(selectedObjects[i].name);
                a = selectedObjects[i].transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
            }
            a.convex = true;
        }
    }
}
[MenuItem("Custom/x+90，")]
public static void CelectObj3()
{
    GameObject[] selectedObjects = Selection.gameObjects;

    if (selectedObjects.Length > 0)
    {
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].transform.GetChild(0).localRotation=selectedObjects[i].transform.localRotation; 
        }
    }
}
#endregion
    
}

