/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     Lua2StringHolder.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-29
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

public class Lua2StringHolder
{
    public static string luaPath = "Assets/Resources/Lua";
    public static string outPath = "Assets/Resources/LuaStringHolder/lua";

    [MenuItem("KOL/Lua/Lua2StringHolder")]
    public static void Lua2StringHolderExe()
    {
        List<string> listLua = new List<string>(); 
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new string[] { luaPath });
        foreach (string guid in guids)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            string path = AssetDatabase.GUIDToAssetPath(guid);
            path = path.Replace(luaPath + "/","");
            path = path.Substring(0, path.LastIndexOf("."));
            listLua.Add(path);
        }
        StringHolder holder = ScriptableObject.CreateInstance<StringHolder>();
        holder.content = listLua.ToArray();
        AssetDatabase.CreateAsset(holder, string.Format("{0}.asset", outPath));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}
