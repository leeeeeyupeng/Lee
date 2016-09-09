using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AssetBundleAssetRecord
{
    public static string GetAssetName(string assetName)
    {
        AssetRecord record = Instance.GetRecord(assetName);
        return record.assetName.ToLower();
    }

    public static string GetAssetBundleName(string assetName)
    {
        AssetRecord record = Instance.GetRecord(assetName);
        return record.assetBundleName; ;
    }

    public struct AssetRecord
    {
        public string assetName;
        public string assetBundleName;
    }

    static AssetBundleAssetRecord m_instance;
    public static AssetBundleAssetRecord Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new AssetBundleAssetRecord();
                m_instance.Load("assetrecord.dat");
            }
            return m_instance;
        }
        set
        {
            m_instance = value;
        }
    }

    Dictionary<string, AssetRecord> m_dicAssetRecord;

    void Load(string recordFile)
    {
        m_dicAssetRecord = new Dictionary<string,AssetRecord>();
//#if UNITY_EDITOR || UNITY_STANDALONE_WIN
//        string fullPath = string.Format("{0}/{1}/{2}", "../../TankClient",AssetBundles.Utility.AssetBundlesPath(),recordFile);
//        string content = FileUtils.GetStringFromFile(fullPath);
//#elif UNITY_ANDROID
//        string content = FileUtils.LoadFromStreaming(recordFile);
//#endif

        string content = FileUtils.LoadStringFromFileSystem(string.Format("assetbundles/{0}", recordFile));
        Debug.Log("Record : " + content);
        string[] lines = content.Split('\n');
        foreach (string line in lines)
        {
            string[] row = line.Split('\t');
            if (row != null && row.Length == 2)
            {
                AssetRecord record = new AssetRecord();

                record.assetName = row[0];
                record.assetBundleName = row[1];
                string assetNameWithoutExtension = record.assetName.Replace(Path.GetExtension(record.assetName),"");

                //Debug.Log("Record : " + record.assetName + "  " + assetNameWithoutExtension);
                if(!m_dicAssetRecord.ContainsKey(record.assetName))
                    m_dicAssetRecord.Add(record.assetName,record);

                if (!m_dicAssetRecord.ContainsKey(assetNameWithoutExtension))
                {
                    //Debug.Log("Record : " + assetNameWithoutExtension);
                    m_dicAssetRecord.Add(assetNameWithoutExtension, record);
                }
            }
        }
    }

    public AssetRecord GetRecord(string assetName)
    {
#if UNITY_EDITOR
        if (!m_dicAssetRecord.ContainsKey(assetName))
        {
            Debug.LogError("AssetRecord : " + assetName + " not exit");
        }
#endif
        if (!m_dicAssetRecord.ContainsKey(assetName))
        {
            Debug.LogError("AssetRecord : " + assetName + " not exit");
            return new AssetRecord();
        }
        return m_dicAssetRecord[assetName];
    }
}
