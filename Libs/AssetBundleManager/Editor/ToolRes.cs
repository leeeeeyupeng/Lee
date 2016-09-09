using UnityEngine;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public class ToolRes
{
    static string resFolder = AssetBundles.Utility.ResPath();
    static string mdFile = resFolder + "/MD.dat";

    [MenuItem("Tools/程序狗专用/Res/Encrypt")]
    public static void GenEncrypt()
    {
        EncryptForFolder(resFolder);
    }

    [MenuItem("Tools/程序狗专用/Res/Decrypt")]
    public static void GenDecrypt()
    {
        DecryptForFolder(resFolder);
    }

    [MenuItem("Tools/程序狗专用/Res/Encrypt For Folder")]
    public static void GenEncryptForFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("Folder", resFolder, "");
        EncryptForFolder(folder);
    }

    [MenuItem("Tools/程序狗专用/Res/Encrypt For File")]
    public static void GenEncryptForFile()
    {
        string file = EditorUtility.OpenFilePanel("File", mdFile, "");
        EncryptForFile(file);
    }

    public static void GenEncryptMD5()
    {
        EncryptForFile(mdFile);
    }

    [MenuItem("Tools/程序狗专用/Res/Decrypt For File")]
    public static void GenDecryptForFile()
    {
        string file = EditorUtility.OpenFilePanel("File", mdFile, "");
        DecryptForFile(file);
    }

    public static void EncryptForFolder(string folder)
    {
        foreach (string file in Directory.GetFiles(folder))
        {
            EncryptForFile(file);
        }

        foreach (string dir in Directory.GetDirectories(folder))
        {
            EncryptForFolder(dir);
        }
        return;
    }

    public static void EncryptForFile(string filePath)
    {
        if (FileUtils.isEncrypt)
        {
            byte[] data = FileUtilTool.ReadBytesOfFile(filePath);
            FileUtils.Encrypt(ref data);
            FileUtilTool.WriteBytesOfFile(filePath, data);
        }
    }

    public static void DecryptForFolder(string folder)
    {
        foreach (string file in Directory.GetFiles(folder))
        {
            DecryptForFile(file);
        }

        foreach (string dir in Directory.GetDirectories(folder))
        {
            DecryptForFolder(dir);
        }
        return;
    }

    public static void DecryptForFile(string filePath)
    {
        if (FileUtils.isEncrypt)
        {
            byte[] data = FileUtilTool.ReadBytesOfFile(filePath);
            FileUtils.Decrypt(ref data);
            FileUtilTool.WriteBytesOfFile(filePath, data);
        }
    }

    [MenuItem("Tools/程序狗专用/Res/MD5")]
    public static void GenMD5()
    {
        FileUtilTool.DelFile(mdFile);

        string fresfolder = resFolder.Replace("\\", "/");
        List<AssetbundleJsonData> data = GetMD5ForFolder(resFolder, fresfolder);
        AssetbundleJsonMap map = new AssetbundleJsonMap();
        map.arrayRes = data.ToArray();
        string jsonStr = JsonMapper.ToJson(map);

        FileUtilTool.WriteFile(mdFile, jsonStr);
    }

    public static List<AssetbundleJsonData> GetMD5ForFolder(string folder,string relative)
    {
        List<AssetbundleJsonData> data = new List<AssetbundleJsonData>();
        foreach (string file in Directory.GetFiles(folder))
        {
            string md5 = FileUtils.GetFileMD5(file);
            Debug.Log("MD5 :" + file + ": " + resFolder );
            string ffile = file.Replace("\\", "/");

            string fileName = ffile.Replace(relative + "/", "");
            Debug.Log("MD5 :" + fileName);
            AssetbundleJsonData jData = new AssetbundleJsonData(fileName, md5);
            data.Add(jData);
        }

        foreach (string dir in Directory.GetDirectories(folder))
        {
            List<AssetbundleJsonData> dataFordir = GetMD5ForFolder(dir,relative);
            data.AddRange(dataFordir);
        }
        return data;
    }

    public static List<AssetbundleJsonData> GetMD5ForFolder(string folder,string relative,string withoutExtension)
    {
        List<AssetbundleJsonData> data = new List<AssetbundleJsonData>();
        foreach (string file in Directory.GetFiles(folder))
        {
            if (Path.GetExtension(file) == withoutExtension)
                continue;
            string md5 = FileUtils.GetFileMD5(file);
            Debug.Log("MD5 :" + file + ": " + resFolder);
            string ffile = file.Replace("\\", "/");
            string fileName = ffile.Replace(relative + "/", "");
            Debug.Log("MD5 :" + fileName);
            AssetbundleJsonData jData = new AssetbundleJsonData(fileName, md5);
            data.Add(jData);
        }

        foreach (string dir in Directory.GetDirectories(folder))
        {
            List<AssetbundleJsonData> dataFordir = GetMD5ForFolder(dir,relative, withoutExtension);
            data.AddRange(dataFordir);
        }
        return data;
    }
    [MenuItem("Tools/程序狗专用/Res/MD5 For Folder")]
    public static void GenMD5FileForFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("Folder", resFolder,"");
        string md5File = folder + "/MD.dat";

        FileUtilTool.DelFile(md5File);

        List<AssetbundleJsonData> data = GetMD5ForFolder(folder,folder);
        AssetbundleJsonMap map = new AssetbundleJsonMap();
        map.arrayRes = data.ToArray();
        string jsonStr = JsonMapper.ToJson(map);

        FileUtilTool.WriteFile(md5File, jsonStr);
    }

    [MenuItem("Tools/程序狗专用/Res/MD5 For Streaming")]
    public static void GenMD5ForFolderForStreaming()
    {
        GenMD5FileForFolder("Assets/StreamingAssets",".meta");
    }

    public static void GenMD5FileForFolder(string folder,string withoutExtension)
    {
        string md5File = folder + "/MD.dat";

        FileUtilTool.DelFile(md5File);

        List<AssetbundleJsonData> data = GetMD5ForFolder(folder,folder, withoutExtension);
        AssetbundleJsonMap map = new AssetbundleJsonMap();
        map.arrayRes = data.ToArray();
        string jsonStr = JsonMapper.ToJson(map);

        FileUtilTool.WriteFile(md5File, jsonStr);
    }

    [MenuItem("Tools/程序狗专用/Res/CopyStreaming")]
    public static void CopyStreaming()
    {
        FileUtilTool.CopyFolder(FileUtils.StreamingPath(), AssetBundles.Utility.ResPath());

        ClearFileWithExtension(AssetBundles.Utility.ResPath(),".meta");
    }

    public static void ClearFileWithExtension(string dir,string extension)
    {
        string[] arrFile = Directory.GetFiles(dir);
        for (int i = 0; i < arrFile.Length; i++)
        {
            string file = arrFile[i];
            if (Path.GetExtension(file) == extension)
            {
                File.Delete(file);
            }
        }

        foreach (string d in Directory.GetDirectories(dir))
        {
            ClearFileWithExtension(d,extension);
        }
    }
}
