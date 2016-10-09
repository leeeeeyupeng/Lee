/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     EquipEditor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-20
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;

using UnityEditor;

public class EquipEditor
{
    static string fbxPath = "Arts/Charactor/Equip";
    static string outPath = "Resources/Arts/Charactor/Equip";
    [MenuItem("KOL/Actor/Equip/Gen")]
    public static void Split()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            if (!(o is GameObject)) continue;
            //if (o.name.Contains("@")) continue;
            if (!AssetDatabase.GetAssetPath(o).Contains(fbxPath)) continue;

            GameObject fbx = (GameObject)o;
            Split(fbx);
        }
    }

    public static void Split(GameObject fbx)
    {
        string equipName = fbx.name;
        foreach (Renderer render in fbx.GetComponentsInChildren<Renderer>(true))
        {
            //mesh
            GameObject objInstantiate = PrefabUtility.InstantiatePrefab(render.gameObject) as GameObject;
            if (objInstantiate.transform.parent != null)
            {
                GameObject objRoot = objInstantiate.transform.parent.root.gameObject;
                objInstantiate.transform.parent = null;
                GameObject.DestroyImmediate(objRoot);
            }

            Renderer renderInstantiate = objInstantiate.GetComponent<Renderer>();

            string curFbxPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(fbx));
            string prefabPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(fbx));
            prefabPath = prefabPath.Replace(fbxPath, outPath);

            //renderInstantiate.sharedMaterial = null;

            if (!Directory.Exists(prefabPath))
                Directory.CreateDirectory(prefabPath);

            PrefabUtility.CreatePrefab(string.Format("{0}/{1}.prefab", prefabPath, equipName), renderInstantiate.gameObject);

            GameObject.DestroyImmediate(objInstantiate);

            //Bone
            if (renderInstantiate is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = (SkinnedMeshRenderer)render;
                List<string> boneNames = new List<string>();
                foreach (Transform t in smr.bones)
                    boneNames.Add(t.name);
                string bonesPath = string.Format("{0}/bones", prefabPath);

                if (!Directory.Exists(bonesPath))
                    Directory.CreateDirectory(bonesPath);

                StringHolder holder = ScriptableObject.CreateInstance<StringHolder>();
                holder.content = boneNames.ToArray();
                AssetDatabase.CreateAsset(holder, string.Format("{0}/{1}.asset", bonesPath, equipName));
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            else if (renderInstantiate is MeshRenderer)
            {
                MeshRenderer mr = (MeshRenderer)render;
                List<string> boneNames = new List<string>();
                //foreach (Transform t in smr.bones)
                //    boneNames.Add(t.name);
                boneNames.Add(render.transform.parent.name);
                string bonesPath = string.Format("{0}/bones", prefabPath);

                if (!Directory.Exists(bonesPath))
                    Directory.CreateDirectory(bonesPath);

                StringHolder holder = ScriptableObject.CreateInstance<StringHolder>();
                holder.content = boneNames.ToArray();
                AssetDatabase.CreateAsset(holder, string.Format("{0}/{1}.asset", bonesPath, equipName));
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

            //materials
            //List<Material> materials = EditorHelpers.CollectAll<Material>(curFbxPath + "/Materials");
            //foreach (Material mat in materials)
            //{
            //    string matPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mat));
            //    matPath = matPath.Replace(fbxPath, outPath);
            //    if (!Directory.Exists(matPath))
            //        Directory.CreateDirectory(matPath);
            //    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(mat), string.Format("{0}/{1}", matPath, Path.GetFileName(AssetDatabase.GetAssetPath(mat))));


            //    Material m = AssetDatabase.LoadAssetAtPath(string.Format("{0}/{1}", matPath, Path.GetFileName(AssetDatabase.GetAssetPath(mat))), typeof(Material)) as Material;
            //    m.shader = Shader.Find("Legacy Shaders/Self-Illumin/Diffuse");
            //}

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
