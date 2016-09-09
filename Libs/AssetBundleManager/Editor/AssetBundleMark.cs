using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

using System.IO;

using System.Threading;

public class AssetBundleMark
{
    static string m_markRecord = "preRes/AssetBundle/Record";
    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Auto")]
    public static void AutoMark()
    {
        string[] scenes = GetSceneArray();
        foreach (string scene in scenes)
        {
            AutoMarkForFile(scene);
        }
        //AutoMarkForFile("Assets\\Scenes\\Login.unity");

        //AutoMarkForDir("Assets\\Resources");
        AutoMarkForResources();

        AssetDatabase.RemoveUnusedAssetBundleNames();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Clear Auto")]
    public static void AutoClearMark()
    {
        ClearMarkRecord(GetRecordFileForFile("Assets\\Arts\\OutPutScene\\School_new\\school_new.unity"));
    }

    public static void AutoMarkForResources()
    {
        AutoMarkForResourcesForDir("Assets/");
    }

    public static void AutoMarkForResourcesForDir(string dir)
    {
        if (dir.EndsWith("Resources"))
        {
            AutoMarkForResourcesDir(dir);
        }
        foreach (string d in Directory.GetDirectories(dir))
        {
            AutoMarkForResourcesForDir(d);
        }
    }

    public static void AutoMarkForResourcesDir(string dir)
    {
        ClearMarkRecordForDir(dir);
        GenAssetRecordForDir(dir);
        MarkForRecord(GetRecordFileForDir(dir));
    }

    public static void AutoMarkForDir(string dir)
    {
        ClearMarkRecordForDir(dir);

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();

        GenAssetRecordForDir(dir);

        MarkForRecord(GetRecordFileForDir(dir));

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();
    }

    public static void AutoMarkForFile(string file)
    {
        ClearMarkRecord(GetRecordFileForFile(file));

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();

        GenAssetRecordForFile(file);
        MarkForRecord(GetRecordFileForFile(file));

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/Mark From Record")]
    public static void MarkForRecord()
    {
        if (File.Exists(m_markRecord))
        {
            string[] arrFile = GetRecordArray();
            MarkForArray(arrFile);
        }
    }

    public static void MarkForRecord(string recordFile)
    {
        string fileRecord = recordFile;
        foreach (string record in GetRecordArray(fileRecord))
        {
            string file = GetFileFromRecord(record);
            string assetBundleName = GetNameFromRecord(record);
            MarkForFile(file, assetBundleName);
        }
    }

    public static void MarkForArray(string[] arrFile)
    {
        foreach (string file in arrFile)
        {
            MarkForFile(file, GetAssetBundleNameFromRecord(file));
        }
    }

    public static void MarkForFile(string file, string assetBundleName)
    {
        AssetImporter ai = AssetImporter.GetAtPath(file);
        if (ai && ai.assetBundleName != assetBundleName)
        {
            ai.assetBundleName = assetBundleName;
            //ai.SaveAndReimport();
        }

    }

    //public static void MarkForFile(string file, string assetBundleName)
    //{
    //    bool update = false;
    //    string metaFile = file + ".meta";
    //    if (File.Exists(metaFile))
    //    {
    //        string dataMeta = FileUtilTool.ReadFile(metaFile);
    //        string[] lines = dataMeta.Split('\n');
    //        string[] newLines = new string[lines.Length];
    //        for (int i = 0; i < lines.Length; i++)
    //        {
    //            string line = lines[i];
    //            if (line.IndexOf("  assetBundleName: ") != -1)
    //            {
    //                string name = line.Replace("  assetBundleName: ", "");
    //                if (name != assetBundleName)
    //                {
    //                    line = "  assetBundleName: " + assetBundleName;
    //                    update = true;
    //                }
    //            }
    //            newLines[i] = line;
    //        }
    //        if (update)
    //        {
    //            FileUtilTool.DelFile(metaFile);
    //            FileUtilTool.WriteFile(metaFile, string.Join("\n", newLines));
    //        }
    //    }
    //}

    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Clear")]
    public static void ClearMark()
    {
        //ClearMarkDir("Assets/Resources/Sound");

        if (File.Exists(m_markRecord))
        {
            string[] deps = GetRecordArray();
            foreach (string dep in deps)
            {
                string[] depData = dep.Split('\t');
                ClearMarkFile(depData[0]);
            }
        }

        FileUtilTool.DeleteFolder(m_markRecord);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        AssetDatabase.RemoveUnusedAssetBundleNames();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Clear Force")]
    public static void ClearMarkForce()
    {
        ClearMarkDir("Assets");

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        AssetDatabase.RemoveUnusedAssetBundleNames();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static void ClearMarkDir(string dir)
    {
        ClearMarkFile(dir);
        string[] arrDir = Directory.GetDirectories(dir);
        foreach (string d in arrDir)
        {
            ClearMarkDir(d);
        }
        string[] arrFile = Directory.GetFiles(dir);
        foreach (string file in arrFile)
        {
            if (Path.GetExtension(file) != ".meta")
            {
                ClearMarkFile(file);
            }
        }
    }

    public static void ClearMarkFile(string file)
    {
        MarkForFile(file, "");
    }

    public static void ClearMarkRecordForDir(string dir)
    {
        string recordFile = GetRecordFileForDir(dir);
        ClearMarkRecord(recordFile);
    }

    public static void ClearMarkRecord(string recordFile)
    {
        if (File.Exists(recordFile))
        {
            string[] arrFile = GetRecordArray(recordFile);
            foreach (string record in arrFile)
            {
                ClearMarkFile(GetFileFromRecord(record));
            }
        }

        FileUtilTool.DelFile(recordFile);

        //AssetDatabase.RemoveUnusedAssetBundleNames();

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();
    }

    public static string GetAssetBundleName(string file)
    {
        string f = file.Replace("\\", "/");
        f = f.Replace(Application.dataPath.Replace("\\", "/"), "Assets");
        string name = AssetBundleFolder.GetAssetBundleName(f);
        name = ChineseHelper.GetPinyin(name);

        if (Path.HasExtension(name) && Path.GetExtension(name) == ".unity")
        {
            name = string.Format("Assets/Scenes/{0}", Path.GetFileNameWithoutExtension(name));
        }

        if (name.Contains("Resources"))
        {
            //name = string.Format("Assets/Resources/{0}", Path.GetFileName(file));
            name = string.Format("Assets/{0}", name.Substring(name.IndexOf("Resources")));
        }

        //if (Path.GetExtension(name) == ".shader")
        //{
        //    name = string.Format("Assets/Shaders/Shader.shader");
        //}

        name = name.ToLower();
        name += ".dat";

        name = name.Replace("\\", "/");

        name = name.Replace("-", "_");
        while (name.IndexOf("/_") != -1)
        {
            name = name.Replace("/_", "/");
        }

        while (name.IndexOf(" ") != -1)
        {
            name = name.Replace(" ", "");
        }

        return name;
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/Mark Auto Record")]
    public static void GenAssetRecord()
    {
        GenAssetRecordForDir("Assets/Resources/Sound");
    }

    public static void GenAssetRecordForDir(string path)
    {
        string[] dependencies = GenAssetDepsForDir(path);
        //PrintStringArray(dependencies);

        string data = string.Join("\n", dependencies);

        WriteFile(GetRecordFileForDir(path), data);
    }

    public static void GenAssetRecordForFile(string file)
    {
        string[] dependencies = GenDepsForFile(file);

        string data = string.Join("\n", dependencies);

        WriteFile(GetRecordFileForFile(file), data);
    }

    public static string[] SetAssetBundleNameForRecord(string[] deps)
    {
        string[] ds = new string[deps.Length];
        for (int i = 0; i < deps.Length; i++)
        {
            ds[i] = string.Format("{0}\t{1}", deps[i], GetAssetBundleName(deps[i]));
        }
        return ds;
    }

    public static string[] GenAssetDepsForDir(string path)
    {
        List<string> dependencies = new List<string>();
        string[] arrFileName = Directory.GetFiles(path);
        foreach (string fileName in arrFileName)
        {
            if (Path.GetExtension(fileName) != ".meta")
            {
                string[] deps = GenDepsForFile(fileName.Replace(Application.dataPath, "Assets"));
                List<string> depsList = new List<string>(deps);
                dependencies.AddRange(depsList);
            }
        }

        string[] arrDir = Directory.GetDirectories(path);
        foreach (string dir in arrDir)
        {
            string[] deps = GenAssetDepsForDir(dir);
            List<string> depsList = new List<string>(deps);
            dependencies.AddRange(depsList);
        }
        return dependencies.ToArray();
    }

    public static string[] GenDepsForFile(string file)
    {
        string[] dependencies = AssetDatabase.GetDependencies(new string[1] { file });
        dependencies = FilterDependencies(dependencies);
        List<string> depsList = new List<string>(dependencies);
        depsList.Add(file);
        dependencies = depsList.ToArray();
        string[] deps = SetAssetBundleNameForRecord(dependencies);
        return deps;
    }

    public static void PrintStringArray(string[] ArrStr)
    {
        foreach (string str in ArrStr)
        {
            Debug.Log(str);
        }
    }

    public static string[] FilterDependencies(string[] deps)
    {
        List<string> listDep = new List<string>();
        foreach (string dep in deps)
        {
            bool success = false;
            Object assetObject = AssetDatabase.LoadMainAssetAtPath(dep);
            string extend = Path.GetExtension(dep);
            if (assetObject is GameObject)
            {
                success = true;
            }
            else if (assetObject is Material)
            {
                success = true;
            }
            else if (assetObject is AudioClip)
            {
                success = true;
            }
            else if (assetObject is Texture)
            {
                success = true;
            }
            else if (assetObject is Shader)
            {
                success = true;
            }

            if (extend == ".unity")
            {
                success = true;
            }

            //Debug.Log("Filter : " + dep + ": " + assetObject.name + ": " + success);
            if (success)
            {
                listDep.Add(dep);
            }
        }
        return listDep.ToArray();
    }

    public static void WriteFile(string outFile, string content)
    {
        FileUtilTool.CreateFolderForFile(outFile);
        FileUtilTool.WriteFile(outFile, content);
    }
    public static string ReadFile(string inFile)
    {
        return FileUtilTool.ReadFile(inFile);
    }

    public static string[] GetRecordArray(string inFile)
    {
        string data = ReadFile(inFile);
        return data.Split('\n');
    }

    public static bool IsMark(string file)
    {
        string[] arrMark = GetRecordArray();
        bool success = false;
        foreach (string one in arrMark)
        {
            string[] oneSplit = one.Split('\t');
            if (oneSplit[0] == file)
            {
                success = true;
            }
        }
        return success;
    }

    public static string GetFileFromRecord(string record)
    {
        string[] r = record.Split('\t');
        return r[0];
    }

    public static string GetNameFromRecord(string record)
    {
        string[] r = record.Split('\t');
        return r[1];
    }

    public static string GetAssetBundleNameFromRecord(string file)
    {
        string[] arrMark = GetRecordArray();
        string name = null;
        foreach (string one in arrMark)
        {
            string[] oneSplit = one.Split('\t');
            if (oneSplit[0] == file)
            {
                name = oneSplit[1];
            }
        }
        return name;
    }

    public static string[] GetAssetBundleDepsFromRecord()
    {
        string[] arrMark = GetRecordArray();
        string[] arrDeps = new string[arrMark.Length];
        string name = null;
        for (int i = 0; i < arrMark.Length; i++)
        {
            string one = arrMark[i];
            string[] oneSplit = one.Split('\t');
            arrDeps[i] = oneSplit[0];
        }
        return arrDeps;
    }

    public static string[] GetAssetBundleDepsFromRecord(string path)
    {
        string file = GetRecordFileForDir(path);
        string[] arrMark = GetRecordArray(file);
        string[] arrDeps = new string[arrMark.Length];
        for (int i = 0; i < arrMark.Length; i++)
        {
            string one = arrMark[i];
            string[] oneSplit = one.Split('\t');
            arrDeps[i] = oneSplit[0];
        }
        return arrDeps;
    }

    public static string GetRecordFileForDir(string path)
    {
        string file = string.Format("{0}/{1}", m_markRecord, path);
        return file;
    }

    public static string GetRecordFileForFile(string file)
    {
        string fileRecord = string.Format("{0}/{1}", m_markRecord, file.Replace(Path.GetExtension(file), ".record"));
        return fileRecord;
    }

    public static string[] GetRecordFileArray()
    {
        List<string> list = new List<string>();
        foreach (string file in Directory.GetFiles(m_markRecord))
        {
            list.Add(file);
        }
        return list.ToArray();
    }

    public static string[] GetRecordArray()
    {
        List<string> recordList = new List<string>();
        string[] arrFile = GetRecordFileArray();
        foreach (string file in arrFile)
        {
            string[] record = GetRecordArray(file);
            List<string> list = new List<string>(record);
            recordList.AddRange(list);
        }

        return recordList.ToArray();
    }

    [MenuItem("Tools/程序狗专用/AssetBundles/GetAssetType")]
    public static void GetAssetType()
    {
        string file = EditorUtility.OpenFilePanel("Select File", Application.dataPath, "*");
        Object assetObject = AssetDatabase.LoadMainAssetAtPath(file.Replace(Application.dataPath, "Assets"));
        Debug.Log(assetObject.GetType().ToString());
    }

    public static string[] GetSceneArray()
    {
        List<string> sceneList = new List<string>();
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.enabled)
            {
                sceneList.Add(scene.path);
            }
        }
        return sceneList.ToArray();
    }

    public static void ClearMarkForOnlyUseByOne()
    {

    }

    public static string[] GetOnlyUseByOne()
    {
        byte[] bytes = FileUtilTool.ReadBytesOfFile("AssetBundles/Android/AssetBundles/AssetBundles");
        AssetBundle mainfest = AssetBundle.LoadFromMemory(bytes);
        AssetBundleManifest assetBundleManifest = (AssetBundleManifest)mainfest.LoadAsset("assetbundlemanifest");
        Dictionary<string, int> m_dicTimes = new Dictionary<string, int>();
        foreach (string assetBundle in assetBundleManifest.GetAllAssetBundles())
        {
            //if (!m_dicTimes.ContainsKey(assetBundle))
            //{
            //    m_dicTimes.Add(assetBundle, 1);
            //}
            //else
            //{
            //    m_dicTimes[assetBundle] = m_dicTimes[assetBundle] + 1;
            //}
            foreach (string dep in assetBundleManifest.GetDirectDependencies(assetBundle))
            {
                if (m_dicTimes.ContainsKey(dep))
                {
                    m_dicTimes[dep] = m_dicTimes[dep] + 1;
                }
                else
                {
                    m_dicTimes.Add(dep, 1);
                }
            }
        }

        List<string> m_listAssetBundle = new List<string>();
        foreach (KeyValuePair<string, int> p in m_dicTimes)
        {
            if (p.Value == 1)
            {
                if (!p.Key.Contains("resources"))
                    m_listAssetBundle.Add(p.Key);
            }
        }

        return m_listAssetBundle.ToArray();
    }

    //[MenuItem("Tools/AssetBundles/Asset Record")]
    public static void AutoGenAssetRecordForResources()
    {
        string assetRecordFile = string.Format("{0}/{1}", AssetBundles.BuildScript.AssetBundlesOutputPath(), "assetrecord.dat");
        if (File.Exists(assetRecordFile))
            FileUtilTool.DelFile(assetRecordFile);
        string data = AutoGenAssetRecordForResourcesForDir("Assets/");
        FileUtilTool.WriteFile(assetRecordFile, data);
    }

    public static string AutoGenAssetRecordForResourcesForDir(string dir)
    {
        string data = "";
        if (dir.EndsWith("Resources"))
        {
            data += AutoGenAssetRecordForDir(dir);
        }
        foreach (string d in Directory.GetDirectories(dir))
        {
            data += AutoGenAssetRecordForResourcesForDir(d);
        }
        return data;
    }

    public static string AutoGenAssetRecordForDir(string dir)
    {
        string data = "";
        foreach (string file in Directory.GetFiles(dir))
        {
            if (Path.GetExtension(file) == ".meta")
            {
                continue;
            }

            data += AutoGenAssetRecordForFile(file);
        }

        foreach (string d in Directory.GetDirectories(dir))
        {
            data += AutoGenAssetRecordForDir(d);
        }

        //Debug.Log("  " + dir + "  " + data);
        return data;
    }

    public static string AutoGenAssetRecordForFile(string filePath)
    {
        string assetName = filePath.Substring(filePath.IndexOf("Resources") + 10);
        string assetNameWithoutExtension = assetName.Substring(0, assetName.LastIndexOf("."));
        string extension = Path.GetExtension(assetName);
        string assetBundleName = GetAssetBundleName(filePath);
        assetName = assetName.Replace("\\", "/");
        assetNameWithoutExtension = assetNameWithoutExtension.Replace("\\", "/");
        assetBundleName = assetBundleName.Replace("\\", "/");
        return string.Format("{0}\t{1}\t{2}\n", assetName, extension, assetBundleName);
    }
}
