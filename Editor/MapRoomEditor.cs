/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     MapRoomEditor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-14
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

using UnityEditor;

public class MapRoomEditor
{
    static string m_roomPrefabPath = "Assets/Resources/Arts/Level/Room";
    static string m_mapRoomPrefabPath = "Assets/Resources/Level/MapRoom";
    [MenuItem("KOL/Level/Room/SaveSelect")]
    public static void SaveSelect()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            string pathRoom = string.Format("{0}/{1}", m_roomPrefabPath, SceneManager.GetActiveScene().name, obj.name);
            if (!Directory.Exists(pathRoom))
                Directory.CreateDirectory(pathRoom);
            pathRoom = string.Format("{0}/{1}.prefab", pathRoom, obj.name);

            PrefabLightmap lightmap = obj.AddComponent<PrefabLightmap>();
            lightmap.SaveLightmap();
            GameObject prefab = Instantiate2Prefab(pathRoom, obj);
            
            //GameObject.DestroyImmediate(obj);
            PrefabLightmap prefabLightmap = prefab.GetComponent<PrefabLightmap>();
            prefabLightmap.LoadLightmap();
            GameObject.DestroyImmediate(lightmap);

            string pathMapRoom = string.Format("{0}/{1}", m_mapRoomPrefabPath, SceneManager.GetActiveScene().name, obj.name);
            if (!Directory.Exists(pathMapRoom))
                Directory.CreateDirectory(pathMapRoom);
            pathMapRoom = string.Format("{0}/{1}.asset", pathMapRoom, obj.name);
            GenMapRoom(pathMapRoom, pathRoom.Replace(m_roomPrefabPath + "/", "").Replace(".prefab", ""), obj);
        }
    }

    public static GameObject Instantiate2Prefab(string path, GameObject room)
    {
        GameObject roomCopy = null;
        GameObject prePrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        if (prePrefab != null)
        {
            roomCopy = PrefabUtility.ReplacePrefab(room, prePrefab);
        }
        else
        {
            roomCopy = PrefabUtility.CreatePrefab(path, room);
        }
        //RoomGenerator generator = roomCopy.GetComponent<RoomGenerator>();
        //if (generator == null)
        //    generator = roomCopy.AddComponent<RoomGenerator>();

        return roomCopy;
    }

    public static void GenMapRoom(string path, string prefabPath, GameObject room)
    {
        MapRoom dataRoom = ScriptableObject.CreateInstance<MapRoom>();
        dataRoom.m_model = prefabPath;
        Transform boundsNode = room.transform.FindChild("Bounds");
        BoxCollider box = boundsNode.GetComponent<BoxCollider>();
        dataRoom.m_bounds = new Bounds(boundsNode.localPosition + box.center, box.size);
        dataRoom.m_joinPoints = GetRoomJoinPoints(room);
        AssetDatabase.CreateAsset(dataRoom, path);
    }

    public static List<RoomJoinPoint> GetRoomJoinPoints(GameObject room)
    {
        List<RoomJoinPoint> joinPoints = new List<RoomJoinPoint>();

        Transform join = room.transform.FindChild("Join");
        for (int i = 0; i < join.childCount; i++)
        {
            Transform tr = join.GetChild(i);
            RoomJoinPoint joinPoint = new RoomJoinPoint();
            //joinPoint.m_room = mapRoom;
            joinPoint.m_position = tr.localPosition;
            joinPoint.m_rotation = tr.localRotation;
            joinPoint.m_scale = tr.localScale;
            Debug.Log(joinPoint.m_rotation.eulerAngles);
            joinPoints.Add(joinPoint);
        }

        return joinPoints;
    }
}
