using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
public class AssetBundleBuildPackage
{
    public static bool isUsePackage = false;
    [MenuItem("Tools/程序狗专用/AssetBundles/BuildPackage")]
    public static void BuildPackage()
    {
        string dataFile = "preRes/AssetBundle/AssetPackage.dat";

        AssetBundleMarkFromRecord.MarkFromRecord(dataFile);
        AssetBundles.BuildScript.BuildAssetBundles(AssetBundles.Utility.AssetBundlesPathPackage());
        AssetBundleMarkFromRecord.MarkClearFromRecord(dataFile);
        AssetBundles.BuildScript.BuildAssetBundlesClearMainfestForDir(AssetBundles.Utility.AssetBundlesPathPackage());
        //string content = FileUtilTool.ReadFile(dataFile);
        //string[] lines = content.Split('\n');
        //foreach (string line in lines)
        //{
        //    //Debug.Log(line);
        //    //Debug.Log(AssetBundles.Utility.AssetBundlesPathPackage() + "/" + line);
        //    FileUtilTool.CopyFile(AssetBundles.Utility.AssetBundlesPath() + "/" + line, AssetBundles.Utility.AssetBundlesPathPackage() + "/" + line);
        //}
        FileUtilTool.CopyFile(dataFile, AssetBundles.Utility.AssetBundlesPathPackage() + "/assetrecord.dat");
        //FileUtilTool.CopyFolder(Application.streamingAssetsPath, AssetBundles.Utility.ResPackagePath());
        //ToolRes.EncryptForFolder(AssetBundles.Utility.ResPackagePath());
    }

    public static void ImportToStreaming()
    {
        FileUtilTool.CopyFolder(AssetBundles.Utility.AssetBundlesPathPackage(), "Assets/StreamingAssets/assetbundles");
    }

    public static void DelForStreaming()
    {
        FileUtilTool.DeleteFolder("Assets/StreamingAssets/assetbundles");
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/Copy Folder")]
    public static void CopyFolder()
    {
        string folder = Application.dataPath;
        folder = EditorUtility.OpenFolderPanel("Source", folder, "");

        string targetFolder = Application.dataPath;
        targetFolder = EditorUtility.OpenFolderPanel("Target", targetFolder, "");

        FileUtilTool.CopyFolder(folder,targetFolder);
    }
}
