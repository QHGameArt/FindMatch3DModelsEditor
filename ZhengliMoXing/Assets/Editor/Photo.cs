using System;
using System.Collections.Generic;
using System.IO;
using EPOOutline;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    //生成选中的贴图
    //生成所有的贴图
    public class Photo
    {
        [MenuItem("Custom/照相/手动选择，选择预制体后生成到场景中")]
        public static void CelectObj()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length > 0)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    string assetPath = AssetDatabase.GetAssetPath(selectedObjects[i]);
                    
                    if (assetPath.EndsWith(".Prefab", System.StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(assetPath) .StartsWith("Obj_", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (selectedObjects != null)
                        {
                            // 创建预制体实例并添加到场景中
                            GameObject newObject = PrefabUtility.InstantiatePrefab(selectedObjects[i],  GameObject.Find(GameCommPath.ScenceModePath).transform)as GameObject  ;
                            instanceGameObj(newObject);
                        }
                        else
                        {
                            Debug.Log("Failed to generate Prefab.");
                        }

                    }
                }
            }
            GC.Collect();
        }

        [MenuItem("Custom/照相/自动选择，选择所有生成的预制体生成到场景中")]
        public static void CelectAllObj()
        {
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
                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[j]);
                            // 创建预制体实例并添加到场景中
                            GameObject newObject = PrefabUtility.InstantiatePrefab(prefab,
                                GameObject.Find(GameCommPath.ScenceModePath).transform) as GameObject;
                            instanceGameObj(newObject);
                        }
                    }
                }
            }
            
        }


        static void instanceGameObj(GameObject newObject)
        {
            newObject.GetComponent<Rigidbody>().useGravity=false;
            newObject.transform.position = Vector3.zero;
            newObject.transform.localScale = Vector3.one*6;
            var child = newObject.transform.GetChild(0);
            var outlinable = child.gameObject.AddComponent<Outlinable>();
            outlinable.OutlineParameters.Color=Color.white;
            SetLayerRecursively(newObject, "ShouJiBanZi");
        }

        static void SetLayerRecursively(GameObject obj, string layerName)
        {
            obj.layer = LayerMask.NameToLayer(layerName);

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layerName);
            }
        }
        private static Rect _rect;
        public static Rigidbody[] objs;
        
        
        
        
        
        [MenuItem("Custom/照相/根据ModelPath下生成目标icon")]
        public static void StartPhoto()
        {
            _camera = Camera.main;
            _rect = new Rect(0, 0, _camera.pixelWidth, _camera.pixelHeight);
            objs =GameObject.Find(GameCommPath.ScenceModePath) .GetComponentsInChildren<Rigidbody>();

            queue = new Queue<Action<string>>();
            foreach (var obj in objs)
            {
                var name = obj.name;
                queue.Enqueue(value =>CaptureAlphaCamera(name,obj.gameObject,GameCommPath.IconStart+$"{name}"));
            }
            Debug.Log(queue.Count);
            while (queue.Count>0)
            {
                capture = queue.Dequeue();
                capture.Invoke(default);
            }
            for (int i = 0; i < objs.Length; i++)
            {
                GameObject o = objs[i].gameObject;
                o.SetActive(true);
            }
            Debug.Log(queue.Count);
            GC.Collect();
        }
        
            private static Action<string> capture;
            private static Queue<Action<string>> queue = new Queue<Action<string>>();
        
        
            private static Camera _camera;
            public static void CaptureAlphaCamera(string name,GameObject obj,string imgName)
            {
                //yield return new WaitForEndOfFrame();
                for (int i = 0; i < objs.Length; i++)
                {
                    GameObject o = objs[i].gameObject;
                    o.SetActive(o.name == $"{name}");
                }
        
        
                RenderTexture rt = new RenderTexture((int) Screen.width, (int) Screen.height, 0);
                // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
                _camera.targetTexture = rt;
                _camera.Render();
        
                RenderTexture.active = rt;
                Texture2D screenShot = new Texture2D((int) _rect.width, (int) _rect.height, TextureFormat.ARGB32, false);
                screenShot.ReadPixels(_rect, 0, 0);
                screenShot.Apply();
        
                // 重置相关参数，以使用camera继续在屏幕上显示
                _camera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                GameObject.DestroyImmediate(rt);
        
                // 最后将这些纹理数据，成一个png图片文件
                byte[] bytes = screenShot.EncodeToPNG();
                
                // 获取物体所在的预制体实例
                PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(obj);
        
                // 检查物体是否为预制体实例
                if (prefabAssetType == PrefabAssetType.MissingAsset)
                {
                    Debug.Log("物体不是预制体实例");
                }
                else
                {
                    // 获取预制体实例的路径
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    string foldPath = prefabPath.Replace( Path.Combine(GameCommPath.ObjPathPrefab , Path.GetFileName(prefabPath)), "") +
                                     GameCommPath.ObjPathIcon;
                    string rootName=Path.Combine(foldPath,$"{imgName}.png");
        
                    Debug.Log("预制体实例的文件路径: " + rootName);

                    if (!Directory.Exists(foldPath))
                    {
                        Directory.CreateDirectory(foldPath);
                    }
                    System.IO.File.WriteAllBytes(rootName, bytes);
                    Debug.Log($"截屏了一张照片: {rootName}");
        
                }
               

            }

            [MenuItem("Custom/照相/生成物体和照片")]
            public static void InstanceAllIconAndObj()
            {
                Transform parent2 = new GameObject(GameCommPath.ScenceModePath2).transform;
                int H=0,W = 0;
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
                                H++;
                                if (H>20)
                                {
                                    W++;
                                    H=0;
                                }
                                
                                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[j]);
                                // 创建预制体实例并添加到场景中
                                GameObject newObject = PrefabUtility.InstantiatePrefab(prefab,parent2) as GameObject;
                                newObject.transform.localPosition  = new Vector3(W * 5, 0,H * 5);;
                                newObject.transform.localScale = Vector3.one;
                                
                                string folderPath2 = GameCommPath.ObjPath + i + GameCommPath.ObjPathIcon+"/"+GameCommPath.IconStart+Path.GetFileName(files[j]).Replace(".prefab",".png");
                                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(folderPath2);
                                if (sprite != null)
                                {
                                    // 在场景中创建一个GameObject
                                    GameObject spriteObject = new GameObject("SpriteObject");
                                    spriteObject.transform.SetParent(parent2);
                                    // 将SpriteRenderer组件添加到GameObject
                                    SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();

                                    // 将加载的Sprite资源赋值给SpriteRenderer
                                    spriteRenderer.sprite = sprite;

                                    // 重置位置和缩放
                                    spriteObject.transform.localPosition =new Vector3(W * 5f, 0,H * 5f)-Vector3.one;
                                    spriteObject.transform.localRotation = Quaternion.Euler(90,0,0);
                                    spriteObject.transform.localScale = Vector3.one;

                                    Debug.Log("Sprite加载完毕！");
                                }
                            }
                        }
                    }
                }

            }



    }
    
    
    
    
    
}