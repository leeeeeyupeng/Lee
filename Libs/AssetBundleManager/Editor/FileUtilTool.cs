using UnityEngine;
using System.Collections;

using System.Text;
using System;

using System.IO;

public class FileUtilTool : MonoBehaviour
{
    public static Encoding encoding = Encoding.GetEncoding("UTF-8");
    public static void CopyFolder(string inDir, string outDir, bool isDeep = true)
    {
        CreateFolder(outDir);
        if (isDeep)
        {
            string[] dirNames = Directory.GetDirectories(inDir);
            foreach (string dirName in dirNames)
            {
                CopyFolder(dirName, dirName.Replace(inDir, outDir), true);
            }
        }

        string[] fileNames = Directory.GetFiles(inDir);
        foreach (string fileName in fileNames)
        {
            //Debug.Log(fileName);
            CopyFile(fileName, fileName.Replace(inDir, outDir));
        }
    }

    public static void CopyFile(string inFile, string outFile)
    {
        CreateFolderForFile(outFile);
        if (File.Exists(outFile))
        {
            File.Copy(inFile, outFile, true);
            //File.Replace(inFile, outFile, null);
        }
        else
        {
            File.Copy(inFile, outFile);
        }
    }

    public static void CreateFolderForFile(string fileName)
    {
        fileName = fileName.Replace(@"\", "/");
        string dir = fileName.Substring(0, fileName.LastIndexOf("/"));
        CreateFolder(dir);
    }

    public static void CreateFolder(string dir)
    {
        dir = dir.Replace(@"\", "/");
        if (!Directory.Exists(dir))
        {
            if (dir.IndexOf(@"/") != -1)
            {
                CreateFolder(dir.Substring(0, dir.LastIndexOf(@"/")));
            }

            Directory.CreateDirectory(dir);
        }
    }

    public static void DeleteFolder(string dir)
    {
        string[] dirNames = Directory.GetDirectories(dir);
        foreach (string dirName in dirNames)
        {
            DeleteFolder(dirName);
        }

        string[] fileNames = Directory.GetFiles(dir);
        foreach (string fileName in fileNames)
        {
            File.Delete(fileName);
        }

        Directory.Delete(dir);
    }

    public static void WriteFile(string outFile, string content)
    {
        try
        {
            FileStream file = new FileStream(outFile, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(file);
            sw.Write(encoding.GetString(encoding.GetBytes(content)));
            sw.Close();
            file.Close();
        }
        catch
        {
            Debug.LogError("Write" + outFile + " error");
        }
    }

    public static string ReadFile(string inFile)
    {
        try
        {
            string content;
            FileStream file = new FileStream(inFile, FileMode.Open);
            StreamReader sr = new StreamReader(file);
            content = sr.ReadToEnd();
            sr.Close();
            file.Close();
            return content;
        }
        catch
        {
            Debug.LogError("Read" + inFile + " error");
        }
        return null;
    }

    public static byte[] ReadBytesOfFile(string inFile)
    {
        try
        {
            byte[] content;
            FileStream file = new FileStream(inFile, FileMode.Open);
            BinaryReader br = new BinaryReader(file);
            content = br.ReadBytes((int)br.BaseStream.Length);
            br.Close();
            file.Close();
            return content;
        }
        catch (Exception e)
        {
            Debug.LogError("Read" + inFile + " error : " + e.ToString());
        }
        return null;
    }

    public static void WriteBytesOfFile(string filePath, byte[] data)
    {
        try
        {
            FileStream file;
            if (File.Exists(filePath))
            {
                file = new FileStream(filePath, FileMode.Truncate, FileAccess.ReadWrite);
            }
            else
            {
                file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            }
            BinaryWriter bw = new BinaryWriter(file);
            
            bw.Write(data);
            bw.Close();
            file.Close();
            return ;
        }
        catch(Exception e)
        {
            Debug.LogError("Write" + filePath + " error : " + e.ToString());
        }
        return;
    }

    public static void DelFile(string file)
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
}
