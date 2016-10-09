/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     PrefabCopyArts2Resources.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-18
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using UnityEditor;

using System;
using System.IO;

public class PrefabCopyArts2Resources : Editor
{
    [MenuItem("KOL/FX/Arts2Res")]
    [MenuItem("游昊/特效/复制到Arts")]
    public static void CopyArts2Res()
    {
        string srcPath = "Assets/Arts/FX/Prefab";
        string targetPath = "Assets/Resources/Arts/FX/Prefab";
        Copy(srcPath, targetPath);
    }

    public static void Copy(string filter, string targetFilter)
    {
        if (!Directory.Exists(targetFilter))
        {
            Directory.CreateDirectory(targetFilter);
        }
        else
        {
            Directory.Delete(targetFilter, true);
        }

        AssetDatabase.Refresh();
        Debug.Log(filter + "  To " + targetFilter);
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { filter });
        foreach (string guid in guids)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string targetPath = path.Replace(filter, targetFilter);

            //Debug.Log(Path.GetDirectoryName(targetPath));
            string tFilter = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(tFilter))
            {
                Directory.CreateDirectory(tFilter);
            }

            AssetDatabase.CopyAsset(AssetDatabase.GUIDToAssetPath(guid), targetPath);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        //DirectoryInfo difilter = new DirectoryInfo(filter);
        //foreach (DirectoryInfo childFilter in difilter.GetDirectories())
        //{
        //    Debug.Log(childFilter.Name);

        //    Copy(string.Format("{0}/{1}", filter, childFilter.Name), string.Format("{0}/{1}", targetFilter, childFilter.Name));
        //}
    }
}
