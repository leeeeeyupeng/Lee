/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     AssetBundleJson.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-09
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

public class AssetbundleJsonData
{
    public string resName;
    public string hashCode;

    public AssetbundleJsonData(string name, string code)
    {
        resName = name;
        hashCode = code;
    }

    public AssetbundleJsonData()
    {
    }
}

public class AssetbundleJsonMap
{
    public AssetbundleJsonData[] arrayRes;
    public AssetbundleJsonMap()
    {
    }

    public AssetbundleJsonData GetJsonData(string name)
    {
        foreach (AssetbundleJsonData data in arrayRes)
        {
            if (data.resName == name)
                return data;
        }

        return null;
    }
}

public class AssetBundleJson : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
