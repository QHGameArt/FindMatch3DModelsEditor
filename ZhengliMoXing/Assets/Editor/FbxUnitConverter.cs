using System.IO;
using UnityEditor;
using UnityEngine;


public class FbxUnitConverter : AssetPostprocessor
{
    // 在导入FBX之前调用
    void OnPreprocessModel()
    {
        if (assetPath.EndsWith(".fbx"))
        {
            ModelImporter modelImporter = (ModelImporter)assetImporter;
            if (modelImporter != null)
            {
                // 设置比例因子
                modelImporter.globalScale = 1.0f; // 设置为你想要的缩放比例
                modelImporter.useFileScale = false; // 设置为你想要的缩放比例
                modelImporter. isReadable= true; // 设置为你想要的缩放比例

                // 应用修改
                modelImporter.SaveAndReimport();
            }





            if (modelImporter.materialLocation != ModelImporterMaterialLocation.External)
            {
                modelImporter.materialLocation = ModelImporterMaterialLocation.External;
                modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
                modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;

                // 获取FBX文件所在目录路径
                string fbxDirectory = Path.GetDirectoryName(assetPath);

                // 创建Material文件夹路径
                //string materialFolder = Path.Combine(fbxDirectory, "Material");

                // 创建Material文件夹
                //if (!Directory.Exists(materialFolder))
                //{
                //    Directory.CreateDirectory(materialFolder);
                //}
//
                //// 设置导入的材质的路径
                //string materialPath = Path.Combine(materialFolder, Path.GetFileNameWithoutExtension(assetPath) + ".mat");

                // 创建新的材质
                //Material material = new Material(Shader.Find("Standard"));

                // 获取原始材质列表
                ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;
                string[] originalMaterials = new string[clipAnimations.Length];

                for (int i = 0; i < clipAnimations.Length; i++)
                {
                    originalMaterials[i] = clipAnimations[i].name;
                }

                // 保存原始材质列表到材质文件
                //material.SetFloat("_OriginalMaterialsCount", originalMaterials.Length);
                //for (int i = 0; i < originalMaterials.Length; i++)
                //{
                //    material.SetTexture("_OriginalMaterial_" + i, AssetDatabase.LoadAssetAtPath<Texture>(originalMaterials[i]));
                //}

                // 生成材质球文件
                //AssetDatabase.CreateAsset(material, materialPath);

                // 重新导入模型，以便应用新的设置
                modelImporter.SaveAndReimport();
            }
        }
        
    }
    
    private void OnPreprocessTexture()
    {
        if (assetPath.Contains("/Sprites/")) // 检查是否在指定的Sprites文件夹下
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spritePixelsPerUnit = 100; // 可根据需要设置像素每单位值
            textureImporter.mipmapEnabled = false; // 关闭mipmap以节省内存，可根据需要设置
            textureImporter.alphaIsTransparency = true; // 如果透明度用于图像，请将此设置为true
        }
    }
    
}
