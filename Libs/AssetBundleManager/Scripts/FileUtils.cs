/* FileUtil.cs
**  by liyupeng  **
 */

using System;
using System.Collections;

using System.IO;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;


public class FileUtils
{
    public static bool isEncrypt = true;
    public static Encoding encoding = Encoding.GetEncoding("UTF-8");
    static FileUtils()
    {
        string path = Application.persistentDataPath;

        //#if UNITY_IPHONE
        //    if (!Application.isEditor)
        //    {
        //        path = path.Substring(0, path.LastIndexOf('/'));
        //        path = path.Substring(0, path.LastIndexOf('/'));
        //    }
        //    path += "/Documents";
        //#else // Win32
        //    path += "/../Documents";
        //#endif

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string StreamingPath()
    {
        string path = Application.streamingAssetsPath;
        return path;
    }

    public static string StreamingUrl()
    {

        string url = "file://" + Application.streamingAssetsPath;
#if UNITY_EDITOR
        url = "file://" + Application.streamingAssetsPath;
#elif UNITY_ANDROID
        url = Application.streamingAssetsPath;
#elif UNITY_IPHONE
        url ="file://" + Application.streamingAssetsPath;
#endif
        return url;
    }

    public static bool ExistsInPersistent(string name)
    {
        string filename = Application.persistentDataPath + "/" + name;
        if (!File.Exists(filename))
        {
            return false;
        }
        return true;
    }

    private static void CreateFolder(string dir)
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

    private static void CreateFolderForFile(string fileName)
    {
        fileName = fileName.Replace(@"\", "/");
        if (fileName.LastIndexOf("/") != -1)
        {
            string dir = fileName.Substring(0, fileName.LastIndexOf("/"));
            CreateFolder(dir);
        }
    }

    //--------------------------------------------------------------------------------------------------------------
    private static string sKey = "esnefeDe";
    private static string sIV = "esnefeDe";
    //����
    public static string Decrypt(string pToDecrypt)
    {
        if (string.IsNullOrEmpty(pToDecrypt))
        {
            return null;
        }
        using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
        {

            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            //��ת
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            //�趨���ܽ�Կ(תΪByte)
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //�趨��ʼ������(תΪByte)
            des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    //�쳣����
                    try
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        //����
                        return System.Text.Encoding.Default.GetString(ms.ToArray());
                    }
                    catch (CryptographicException)
                    {
                        //����Կ����������
                        return "0";
                    }
                }
            }
        }
    }

    //����
    public static string Encrypt(string pToEncrypt)
    {
        StringBuilder ret = new StringBuilder();
        using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
        {
            //����Ԫת��ΪByte
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            //�趨���ܽ�Կ(תΪByte)
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //�趨��ʼ������(תΪByte)
            des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                }
                //����
                foreach (byte b in ms.ToArray())
                    ret.AppendFormat("{0:X2}", b);
            }
        }
        //����
        return ret.ToString();
    }

    static byte key = 0xab;
    //����
    public static void Decrypt(ref byte[] pToDecrypt)
    {
        int interval = (int)(pToDecrypt.Length * 0.001f) + 1;
        for (int i = 0; i < pToDecrypt.Length; )
        {
            pToDecrypt[i] ^= key;
            i += interval;
        }
        return;
    }

    //����
    public static void Encrypt(ref byte[] pToEncrypt)
    {
        int interval = (int)(pToEncrypt.Length * 0.001f) + 1;
        for (int i = 0; i < pToEncrypt.Length; )
        {
            pToEncrypt[i] ^= key;
            i += interval;
        }
        return;
    }
    ////����
    //public static byte[] Decrypt(byte[] pToDecrypt)
    //{
    //    if (pToDecrypt == null || pToDecrypt.Length == 0)
    //    {
    //        return null;
    //    }

    //    using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
    //    {

    //        byte[] inputByteArray = pToDecrypt;

    //        //�趨���ܽ�Կ(תΪByte)
    //        des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
    //        //�趨��ʼ������(תΪByte)
    //        des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
    //            {
    //                //�쳣����
    //                try
    //                {
    //                    cs.Write(inputByteArray, 0, inputByteArray.Length);
    //                    cs.FlushFinalBlock();
    //                    //����
    //                    return ms.ToArray();
    //                }
    //                catch (CryptographicException e)
    //                {
    //                    Debug.Log("error: " + e.ToString());
    //                    //����Կ����������
    //                    return null;
    //                }
    //            }
    //        }
    //    }
    //    return null;
    //}

    ////����
    //public static byte[] Encrypt(byte[] pToEncrypt)
    //{
    //    using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
    //    {
    //        //����Ԫת��ΪByte
    //        byte[] inputByteArray = pToEncrypt;

    //        //�趨���ܽ�Կ(תΪByte)
    //        des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
    //        //�趨��ʼ������(תΪByte)
    //        des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);

    //        using (MemoryStream ms = new MemoryStream())
    //        {

    //            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
    //            {
    //                cs.Write(inputByteArray, 0, inputByteArray.Length);
    //                cs.FlushFinalBlock();
    //            }
    //            //����
    //            //byte[] result = new byte[inputByteArray.Length];
    //            //byte[] array = ms.ToArray();
    //            //Array.Copy(array, result, result.Length);
    //            byte[] result = ms.ToArray();
    //            return result;
    //        }
    //    }
    //    //����
    //    return null;
    //}
    //-----------------------------------------------------------------------------------

    public static void WriteFileInPersistent(string name, byte[] content, bool bEncrypt = true)
    {
        string filename = Application.persistentDataPath + "/" + name;
        WriteFile(filename, content, bEncrypt);
    }

    public static void WriteFileInPersistent(string name, string content)
    {
        string filename = Application.persistentDataPath + "/" + name;
        WriteFile(filename, content);
    }

    public static void WriteFile(string filePath, string content)
    {
        WriteFile(filePath, encoding.GetBytes(content), true);
    }

    public static void WriteFile(string filePath, byte[] content, bool bEncrypt = true)
    {
        string filename = filePath;

        CreateFolderForFile(filename);
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

            if (bEncrypt && isEncrypt)
            {
                Encrypt(ref content);
            }

            file.Write(content, 0, content.Length);

            file.Close();
        }
        catch (Exception e)
        {
            Debug.Log("Save" + filename + " error : " + e.ToString());
        }
    }

    public static void ReadFileInPersistent(string name, ref string content, bool bDescrypt = true)
    {
        ReadFile(string.Format("{0}/{1}", Application.persistentDataPath, name), ref content, bDescrypt);
    }

    public static void ReadFile(string filePath, ref string content, bool bDescrypt = true)
    {
        byte[] bytes = null;
        ReadFile(filePath, ref bytes, bDescrypt);
        content = encoding.GetString(bytes);
    }

    public static void ReadFile(string filePath, ref byte[] content, bool bDecrypt = true)
    {
        string filename = filePath;
        if (!File.Exists(filename))
        {
            return;
        }

        try
        {
            FileStream file = new FileStream(filename, FileMode.Open);

            byte[] bytes = new byte[file.Length];
            file.Read(bytes, 0, bytes.Length);
            file.Close();

            if (bDecrypt && isEncrypt)
            {
                Decrypt(ref bytes);
            }

            content = bytes;
        }
        catch (Exception e)
        {
            Debug.Log("Load" + filename + " error : " + e.ToString());
        }
    }

    public static string ReadString(string fullpath, bool bDescrypt = true)
    {
        byte[] bytes = ReadBytes(fullpath, bDescrypt);
        if (bytes == null)
            return null;
        return encoding.GetString(bytes);
    }

    public static string ReadStringFromBytes(byte[] bytes, bool bDescrypt = true)
    {
        if (bytes == null)
            return null;
        if (bDescrypt && isEncrypt)
        {
            Decrypt(ref bytes);
        }
        return encoding.GetString(bytes);
    }

    public static byte[] ReadBytes(string fullpath, bool bDescrypt = true)
    {
        string filename = fullpath;
        if (!File.Exists(filename))
        {
            Debug.LogError("path not exit: " + fullpath);
            return null;
        }
        try
        {
            FileStream file = new FileStream(filename,FileMode.Open,FileAccess.Read);

            byte[] bytes = new byte[file.Length];
            file.Read(bytes, 0, bytes.Length);
            file.Close();

            if (bDescrypt && isEncrypt)
            {
                Decrypt(ref bytes);
			}

            return bytes;
        }
        catch (Exception e)
        {
            Debug.LogError("Load" + filename + " error" + e.ToString());
        }
        return null;
    }

    public static string ReadStringFromStreaming(string fileName)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		string url = FileUtils.StreamingUrl() + "/" + fileName;
        //Debug.Log(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        string str = www.text;
        //Debug.Log(fileName + "---content ---:" + str);
        return str;
#elif UNITY_IPHONE
		string filePath = FileUtils.StreamingPath() + "/" + fileName;
		Debug.Log(filePath);
		return ReadString(filePath,false);
#elif UNITY_ANDROID
        return AndroidHelper.GetStringFromAsset(fileName);
#endif
    }

    public static byte[] ReadBytesFromStreaming(string fileName)
    {
        Debug.Log("read from streaming = " + fileName);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		string url = FileUtils.StreamingUrl() + "/" + fileName;
		Debug.Log(url);
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            //Debug.Log("LoadBytesFromStreaming");
            //if (!string.IsNullOrEmpty(www.error))
            //{
            //    Debug.LogError(www.error);
            //    return null;
            //}
        }
        Debug.Log("LoadBytesFromStreaming");
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
        }

        byte[] bytes = www.bytes;
        return bytes;
#elif UNITY_IPHONE
		string filePath = FileUtils.StreamingPath() + "/" + fileName;
		Debug.Log(filePath);
		return ReadBytes(filePath,false);

#elif UNITY_ANDROID
        return AndroidHelper.GetBytesFromAsset(fileName);
#endif
        return null;
    }

    public static string DownloadStringFromCdn(string url, string filePath)
    {
        WWW www = new WWW(string.Format("{0}/{1}", url, filePath));

        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        return www.text;
    }

    public static byte[] DownloadBytesFromCdn(string url, string filePath)
    {
        WWW www = new WWW(string.Format("{0}/{1}", url, filePath));

        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        return www.bytes;
    }

    public static Texture2D LoadTextureFromStreaming(string filename)
    {
        string url = FileUtils.StreamingUrl() + "/" + filename;

        WWW www = new WWW(url);
        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        Debug.Log("texture loaded");
        return www.texture;
    }

    public delegate void OnAudioLoaded(AudioClip a);
    public static IEnumerator LoadAudioClipFromStreaming(string filename, OnAudioLoaded a)
    {
        string url = FileUtils.StreamingUrl() + "/" + filename;
        WWW www = new WWW(url);
        yield return www;

        if ( a != null)
        {
            a(www.audioClip);
        }
        Debug.Log("audio loaded");
    }

    public static AudioClip LoadAudioClipFromFileSystem(string fileName)
    {
        byte[] array = LoadBytesFromFileSystem(fileName);
        Debug.Log(fileName + ":" + array.Length);
        AudioWav wav = new AudioWav(array);
        Debug.Log(wav.ToString());
        AudioClip audioClip = AudioClip.Create(fileName, wav.SampleCount, 1, wav.Frequency, false, false);
        audioClip.SetData(wav.LeftChannel, 0);
        return audioClip;
        //float[] floatArr = new float[array.Length / 4];
        //for (int i = 0; i < floatArr.Length; i++)
        //{
        //    if (BitConverter.IsLittleEndian)
        //        Array.Reverse(array, i * 4, 4);
        //    floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
        //}

        //AudioClip audioClip = AudioClip.Create("testSound", floatArr.Length, 1, 44100, false, false);
        //audioClip.SetData(floatArr, 0);

        //return audioClip;
    }

    public static AudioClip LoadAudioClipFromStreaming(string filename)
    {    
        string url = FileUtils.StreamingUrl() + "/" + filename;
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        Debug.Log("audio loaded");
        return www.audioClip;
    }

    public static string url = "http://192.168.2.85";
    public static byte[] LoadBytesFromUrl(string url)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        return www.bytes;
    }

    public static string LoadStringFromUrl(string url)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                return null;
            }
        }
        return www.text;
    }

    public static byte[] LoadBytesFromFileSystem(string path)
    {
        string fullPathInPersistent = string.Format("{0}/{1}", Application.persistentDataPath, path);
        if (File.Exists(fullPathInPersistent))
        {
            byte[] content = null;
            ReadFile(fullPathInPersistent, ref content);
            return content;
        }

#if UNITY_EDITOR
        string packagePath = string.Format("{0}/{1}", AssetBundles.Utility.ResPackagePath(), path);
        if (File.Exists(packagePath))
        {
            return ReadBytes(packagePath, false);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidHelper.Exits(path))
            return ReadBytesFromStreaming(path);
#else
        string streamingPath = string.Format("{0}/{1}", Application.streamingAssetsPath, path);
        if (File.Exists(streamingPath))
            return ReadBytes(streamingPath,false);
#endif

        byte[] bytes = LoadBytesFromUrl(string.Format("{0}/{1}/{2}/{3}",url
            ,AssetBundles.Utility.GetResVersion()
            ,AssetBundles.Utility.GetPlatformName(),path));

        WriteFileInPersistent(path,bytes,false);

        if (isEncrypt)
        {
            Decrypt(ref bytes);
        }

        return bytes;
    }

    public static string LoadStringFromFileSystem(string path)
    {
        string fullPathInPersistent = string.Format("{0}/{1}", Application.persistentDataPath, path);
        if (File.Exists(fullPathInPersistent))
        {
            string content = "";
#if UNITY_EDITOR
            Debug.Log("Cache : " + fullPathInPersistent);
#endif
            ReadFile(fullPathInPersistent, ref content);
            return content;
        }

#if UNITY_EDITOR
        string packagePath = string.Format("{0}/{1}", AssetBundles.Utility.ResPackagePath(), path);
        if (File.Exists(packagePath))
        {
            return ReadString(packagePath, false);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidHelper.Exits(path))
            return ReadStringFromStreaming(path);
#else
        string streamingPath = string.Format("{0}/{1}", Application.streamingAssetsPath, path);
        if (File.Exists(streamingPath))
            return ReadString(streamingPath,false);
#endif

        byte[] bytes = LoadBytesFromUrl(string.Format("{0}/{1}/{2}/{3}", url
    , AssetBundles.Utility.GetResVersion()
    , AssetBundles.Utility.GetPlatformName(), path));

        WriteFileInPersistent(path, bytes, false);

        if (isEncrypt)
        {
            Decrypt(ref bytes);
        }

        Debug.Log("LoadBytesFromUrl : " + path);

        return encoding.GetString(bytes);
    }

    public static string GetFileMD5(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("GetMD5HashFromFile() fail,error:" + ex.Message);
            return "";
        }
    }
}
