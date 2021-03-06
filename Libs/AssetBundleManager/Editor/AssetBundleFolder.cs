﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleFolder
{
    static List<string> m_folderList;
    #region static stor.
    static AssetBundleFolder()
    {
        m_folderList = new List<string>();
        string dataFile = Application.dataPath + "/Libs/AssetBundleManager/Editor/folder.dat";
        string data = FileUtilTool.ReadFile(dataFile);
        string[] lines = data.Split('\n');
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                string content = line.Replace("\r", "");
                m_folderList.Add(content);
            }
        }
    }
    #endregion

    public static string GetAssetBundleName(string path)
    {
        foreach (string folder in m_folderList)
        {
            string[] f = folder.Split('\t');
            string folderName = f[0];
            string assetBundleName = f[1];
            if (path.Contains(folderName.ToLower()))
            {
                return assetBundleName;
            }

            if (path.Contains(folderName))
            {
                return assetBundleName;
            }
        }
        return path;
    }
}