using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

using System.IO;

using System.Threading;
public class AssetBundleMarkFromRecord
{
    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Copy Record")]
    public static void CopyTo()
    {
        string assetRecordFile = string.Format("{0}/{1}", AssetBundles.BuildScript.AssetBundlesOutputPath(), "assetrecord.dat");
        string recordFile = "preRes/AssetBundle/Record.dat";
        FileUtilTool.CopyFile(recordFile, assetRecordFile);
    }
    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Auto From Record")]
    public static void MarkAutoFromRecord()
    {
        AssetBundleMark.ClearMarkDir("Assets");

        string recordFile = "preRes/AssetBundle/Record.dat";
        MarkFromRecord(recordFile);
    }

    public static void MarkFromRecord(string recordFile)
    {
        List<string> record = ReadRecord(recordFile);

        foreach (string r in record)
        {
            string file = AssetBundleMark.GetFileFromRecord(r);
            string assetBundleName = AssetBundleMark.GetNameFromRecord(r);
            AssetBundleMark.MarkForFile(file, assetBundleName);
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static List<string> ReadRecord(string recordFile)
    {
        string content = FileUtilTool.ReadFile(recordFile);
        string[] lines = new string[0];
        if(!string.IsNullOrEmpty(content))
            lines = content.Split('\n');
        return new List<string>(lines);
    }
    [MenuItem("Tools/程序狗专用/AssetBundles/Clear Auto From Record")]
    public static void ClearAutoFromRecord()
    {
        string recordFile = "preRes/AssetBundle/Record.dat";
        MarkClearFromRecord(recordFile);
    }

    public static void MarkClearFromRecord(string recordFile)
    {
        List<string> record = ReadRecord(recordFile);

        foreach (string r in record)
        {
            string file = AssetBundleMark.GetFileFromRecord(r);
            string assetBundleName = AssetBundleMark.GetNameFromRecord(r);
            AssetBundleMark.MarkForFile(file, "");
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static bool Exits(string asset)
    {
        string recordFile = "preRes/AssetBundle/Record.dat";
        List<string> record = ReadRecord(recordFile);

        foreach (string r in record)
        {
            string file = AssetBundleMark.GetFileFromRecord(r);
            string assetBundleName = AssetBundleMark.GetNameFromRecord(r);
            if (file == asset)
                return true;
        }
        return false;
    }
}
