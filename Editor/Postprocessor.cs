/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     Postprocessor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-06
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using UnityEditor;

public class Postprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            Debug.Log("Reimported Asset: " + str);
        }
        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }
    }

    void  OnPreprocessModel()
    {
        //if (assetPath.Contains("@"))
        {
            ModelImporter modelImporter = (ModelImporter)assetImporter;
            Debug.Log(modelImporter.name);
        }
    }
}
