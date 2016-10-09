/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     ModelImport.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-22
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using UnityEditor;

public class ModelImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (assetPath.Contains("@"))
        {
            ModelImporter modelImporter  = assetImporter as ModelImporter;
            //modelImporter.importMaterials = false;
        }
    }

    void OnPostprocessModel()
    {

    }
}
