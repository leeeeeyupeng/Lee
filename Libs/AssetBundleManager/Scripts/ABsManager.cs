using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class ABsManager : MonoBehaviour
{
    public class LoadedAssetBundle
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    static ABsManager m_instance;
    public static ABsManager Instance
    {
        get
        {
            if (m_instance == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    m_instance = new ABsManager();
                    m_instance.Init();
                    return m_instance;
                }
#endif

                GameObject obj = new GameObject("ABsManager");
                //obj.transform.parent = ResManager.Instance.transform;
                m_instance = obj.AddComponent<ABsManager>();
            }
            return m_instance;
        }
    }

    public static AssetBundle Load(string assetBundleName)
    {
        if (Instance.m_LoadedAssetBundles.ContainsKey(assetBundleName))
        {
            Instance.m_LoadedAssetBundles[assetBundleName].m_ReferencedCount++;
            return Instance.m_LoadedAssetBundles[assetBundleName].m_AssetBundle;

        }

        string[] deps = Instance.m_AssetBundleManifest.GetDirectDependencies(assetBundleName);
        //print("path :" + assetBundleName + "  Deps : " + deps.Length);
        foreach (string dep in deps)
        {
            Load(dep);
        }

        AssetBundle ab = LoadInternal(assetBundleName);
        Instance.m_LoadedAssetBundles.Add(assetBundleName, new LoadedAssetBundle(ab));
        return ab;
    }

    public static void Unload(string assetBundleName)
    {
        if (Instance && Instance.m_LoadedAssetBundles.ContainsKey(assetBundleName))
        {
            Instance.m_LoadedAssetBundles[assetBundleName].m_ReferencedCount--;
            if (Instance.m_LoadedAssetBundles[assetBundleName].m_ReferencedCount == 0)
            {
                Instance.m_LoadedAssetBundles[assetBundleName].m_AssetBundle.Unload(true);
                //Debug.LogError("Asset Unload : " + assetBundleName);

                Instance.m_LoadedAssetBundles.Remove(assetBundleName);

                string[] deps = Instance.m_AssetBundleManifest.GetDirectDependencies(assetBundleName);
                foreach (string dep in deps)
                {
                    Unload(dep);
                }
            }
        }
    }

    public static string GetAssetName(AssetBundle ab, string assetName)
    {
        string[] assets = ab.GetAllAssetNames();
        string assetNameFull = assetName;
        foreach (string asset in assets)
        {
            if (asset.Replace(Path.GetExtension(asset), "") == assetName)
            {
                assetNameFull = asset;
                break;
            }
        }
        return assetNameFull;
    }

    static AssetBundle LoadInternal(string assetBundleName)
    {
        string path = string.Format("assetbundles/{0}", assetBundleName);

        byte[] bs = FileUtils.LoadBytesFromFileSystem(path);

        if (bs != null && bs.Length > 0)
        {
            AssetBundle ab = AssetBundle.LoadFromMemory(bs);
            Debug.Log("load internal : " + assetBundleName);
            return ab;
        }
        else
        {
            Debug.LogError("Load AssetBundle:" + assetBundleName + " error");
        }
        return null;
    }

    AssetBundleManifest m_AssetBundleManifest = null;
    Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

    void Awake()
    {
        Init();
    }

    void OnDestroy()
    {
        Instance.Clear();
        m_instance = null;
    }

    void Init()
    {
        AssetBundle mainfest = LoadInternal("assetbundles");
        m_LoadedAssetBundles.Add("assetbundles", new LoadedAssetBundle(mainfest));
        //print(mainfest.GetAllAssetNames());
        m_AssetBundleManifest = (AssetBundleManifest)mainfest.LoadAsset("assetbundlemanifest");

        //print(m_AssetBundleManifest.GetAllAssetBundles());
        //print(m_AssetBundleManifest.GetAllAssetBundles());
    }

    public void Clear()
    {
        foreach (LoadedAssetBundle lab in m_LoadedAssetBundles.Values)
        {
            lab.m_AssetBundle.Unload(true);
        }
        m_LoadedAssetBundles.Clear();
    }

    static void print(string[] strArr)
    {
        foreach (string str in strArr)
        {
            Debug.Log(str);
        }
    }
    static void print(string title, string[] strArr)
    {
        string info = title + "\t";
        foreach (string str in strArr)
        {
            info += str + "\t";
        }
        Debug.Log(info);
    }
}
