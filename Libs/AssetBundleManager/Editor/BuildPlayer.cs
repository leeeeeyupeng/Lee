using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class BuildPlayer
{
    public static void SwitchActiveBuildTarget(BuildTarget target)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(target);
    }
    //[MenuItem("Tools/打包/安卓/安卓包(无资源)")]
    public static void BuildAndroid()
    {
        SwitchActiveBuildTarget(BuildTarget.Android);
        BuildRes();
        ToolRes.CopyStreaming();
        ToolRes.GenMD5();
        UploadRes();

        BuildAPK();
        UploadAPK();
    }

    [MenuItem("Tools/打包/安卓/安卓完整包")]
    public static void BuildAndroidAll()
    {
        SwitchActiveBuildTarget(BuildTarget.Android);
        BuildRes();
        ToolRes.CopyStreaming();
        ToolRes.GenMD5();
        UploadRes();

        BuildAPKAll();
        UploadAPK();
    }

    [MenuItem("Tools/打包/安卓/安卓资源CDN(热更新)")]
    public static void BuildAndroidRes()
    {
        SwitchActiveBuildTarget(BuildTarget.Android);
        BuildRes();
        ToolRes.CopyStreaming();
        ToolRes.GenMD5();

        UploadRes();
    }

    [MenuItem("Tools/程序狗专用/Res/Build Res")]
    public static void BuildRes()
    {
        AssetBundleMark.ClearMarkForce();
        AssetBundleMarkFromRecord.MarkAutoFromRecord();
        AssetBundles.BuildScript.BuildAssetBundles();
        AssetBundleMarkFromRecord.ClearAutoFromRecord();
        AssetBundleMarkFromRecord.CopyTo();
        FileUtilTool.CopyFolder(AssetBundles.BuildScript.AssetBundlesOutputPath(), AssetBundles.Utility.AssetBundlesPath());
        AssetBundles.BuildScript.BuildAssetBundlesClearMainfestForDir(AssetBundles.Utility.AssetBundlesPath());        
    }

    [MenuItem("Tools/程序狗专用/Res/Upload")]
    public static void UploadRes()
    {
        string sourcePath = AssetBundles.Utility.ResPath();
        string targetPath = string.Format("\\\\192.168.2.85\\FtpData\\{1}\\{0}", AssetBundles.Utility.GetPlatformName(),AssetBundles.Utility.GetResVersion());
        ToolRes.GenEncrypt();
        FileUtilTool.CopyFolder(sourcePath, targetPath,true);
        ToolRes.GenDecrypt();
    }
    [MenuItem("Tools/程序狗专用/Res/UploadAPK")]
    public static void UploadAPK()
    {
        string sourcePath = "asset/TankGirl.apk";
        string targetPath = string.Format("\\\\192.168.2.85\\FtpData\\{1}\\{0}", "TankGirl.apk", AssetBundles.Utility.GetResVersion());
        FileUtilTool.CopyFile(sourcePath, targetPath);


        sourcePath = "asset/version.dat";
        targetPath = string.Format("\\\\192.168.2.85\\FtpData\\{1}\\{0}", "version.dat", AssetBundles.Utility.GetResVersion());
        FileUtilTool.CopyFile(sourcePath, targetPath);
    }

    [MenuItem("Tools/程序狗专用/Build/Android APK")]
    public static void BuildAPK()
    {
        if (AssetBundleBuildPackage.isUsePackage)
            AssetBundleBuildPackage.BuildPackage();
        //AssetImporter ai = AssetImporter.GetAtPath("Assets/Resources");
        //Debug.Log(ai);
        //if (ai)
        //{
        //    Debug.Log(ai.hideFlags);
        //    ai.hideFlags = HideFlags.HideAndDontSave;
        //    Debug.Log(ai.hideFlags);
        //    ai.SaveAndReimport();
        //}
        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();

        DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Hidden;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        if (AssetBundleBuildPackage.isUsePackage)
        {
            AssetBundleBuildPackage.ImportToStreaming();
        }
        ToolRes.GenMD5ForFolderForStreaming();

        string[] levels = new string[] { "Assets/Download.unity"};
        try
        {
            string result = BuildPipeline.BuildPlayer(levels, "asset/TankGirl.apk", BuildTarget.Android, BuildOptions.None);
        }
        catch (Exception e)
        {
            Debug.LogError("build android error： " + e.ToString());
        }

        //DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Directory;
        }

        if (AssetBundleBuildPackage.isUsePackage)
        {
            AssetBundleBuildPackage.DelForStreaming();
        }
        ToolRes.GenMD5ForFolderForStreaming();

        string bundleVersion = PlayerSettings.bundleVersion;
        FileUtilTool.WriteFile("asset/version.dat", bundleVersion);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static void ResourceForFolder(string dir,bool hidden)
    {
        if (dir.EndsWith("Resources"))
        {
            FolderAttributes(dir,hidden);
        }

        foreach (string d in Directory.GetDirectories(dir))
        {
            ResourceForFolder(d,hidden);
        }
    }

    public static void FolderAttributes(string dir, bool hidden)
    {
        DirectoryInfo info = new DirectoryInfo(dir);
        if (info.Exists)
        {
            info.Attributes = hidden ? FileAttributes.Hidden : FileAttributes.Directory;
        }
    }

    [MenuItem("Tools/程序狗专用/Build/Android APK For script")]
    public static void BuildAPKForScript()
    {
        ResourceForFolder("Assets/Resources",true);

        FolderAttributes("Assets/StreamingAssets/Live2D", true);
        FolderAttributes("Assets/StreamingAssets/Config", true);
        FolderAttributes("Assets/StreamingAssets/Lua", true);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        string[] levels = new string[] { "Assets/Download.unity" };
        try
        {
            string result = BuildPipeline.BuildPlayer(levels, "asset/TankGirl.apk", BuildTarget.Android, BuildOptions.None);
        }
        catch (Exception e)
        {
            Debug.LogError("build android error： " + e.ToString());
        }

        ResourceForFolder("Assets/Resources", false);

        FolderAttributes("Assets/StreamingAssets/Live2D", false);
        FolderAttributes("Assets/StreamingAssets/Config", false);
        FolderAttributes("Assets/StreamingAssets/Lua", false);

        string bundleVersion = PlayerSettings.bundleVersion;
        FileUtilTool.WriteFile("asset/version.dat", bundleVersion);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    //完整包
    [MenuItem("Tools/程序狗专用/Build/Android APK For All")]
    public static void BuildAPKAll()
    {
        DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Hidden;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        FileUtilTool.CopyFolder(AssetBundles.Utility.AssetBundlesPath(), "Assets/StreamingAssets/assetbundles");
        ToolRes.GenMD5ForFolderForStreaming();

        string[] levels = new string[] { "Assets/Download.unity" };
        try
        {
            string result = BuildPipeline.BuildPlayer(levels, "asset/TankGirl.apk", BuildTarget.Android, BuildOptions.None);
        }
        catch (Exception e)
        {
            Debug.LogError("build android error： " + e.ToString());
        }

        //DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Directory;
        }

        FileUtilTool.DeleteFolder("Assets/StreamingAssets/assetbundles");

        ToolRes.GenMD5ForFolderForStreaming();

        string bundleVersion = PlayerSettings.bundleVersion;
        FileUtilTool.WriteFile("asset/version.dat", bundleVersion);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/程序狗专用/Build/Android Project Export")]
    public static void BuildJavaProject()
    {
        string path = "Android/";
        FileUtilTool.CreateFolder(path);
        //AssetImporter ai = AssetImporter.GetAtPath("Assets/Resources");
        //Debug.Log(ai);
        //if (ai)
        //{
        //    Debug.Log(ai.hideFlags);
        //    ai.hideFlags = HideFlags.HideAndDontSave;
        //    Debug.Log(ai.hideFlags);
        //    ai.SaveAndReimport();
        //}
        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();

        DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Hidden;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        //AssetBundleBuildPackage.ImportToStreaming();
        //ToolRes.GenMD5ForFolderForStreaming();

        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        string[] levels = new string[] { "Assets/Download.unity"};
        try
        {
            string result = BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
        }
        catch (Exception e)
        {
            Debug.LogError("build android error： " + e.ToString());
        }

        //DirectoryInfo info = new DirectoryInfo("Assets/Resources");
        if (info.Exists)
        {
            info.Attributes = FileAttributes.Directory;
        }

        //AssetBundleBuildPackage.DelForStreaming();
        //ToolRes.GenMD5ForFolderForStreaming();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}
