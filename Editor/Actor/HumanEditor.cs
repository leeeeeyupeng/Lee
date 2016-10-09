/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     HumanEditor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-26
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using UnityEditor;

public class HumanEditor
{
    static string m_fbxPath = "Assets/Arts/Charactor/Human/Human.fbx";
    static string m_prefabPath = "Assets/Resources/Arts/Charactor/Human/Human.prefab";
    static string m_animatorPath = "Assets/Project/Arts/Charactor/Human/Human.controller";
    [MenuItem("KOL/Actor/Human Prefab")]
    public static void GenPrefab()
    {
        GameObject objModel = AssetDatabase.LoadAssetAtPath(m_fbxPath, typeof(GameObject)) as GameObject;

        GameObject prefab = PrefabUtility.CreatePrefab(m_prefabPath, objModel);
        GameObject prefabInstantiate = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        prefabInstantiate.layer = LayerMask.NameToLayer("Player");
        Animator animator = prefabInstantiate.GetComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath(m_animatorPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
        CapsuleCollider sc = prefabInstantiate.AddComponent<CapsuleCollider>();
        sc.center = new Vector3(0f, 0.5f, 0f);
        sc.radius = 0.5f;
        sc.height = 1;
        //sc.direction = 
        Rigidbody rb = prefabInstantiate.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        prefabInstantiate.AddComponent<AvatarBehaviour>();
        prefabInstantiate.AddComponent<PlayerController>();
        //GameObject detect = new GameObject("detect");
        //detect.transform.parent = prefabInstantiate.transform;
        //detect.transform.localPosition = Vector3.zero;
        //detect.transform.localRotation = Quaternion.identity;
        //detect.layer = LayerMask.NameToLayer("MonsterDetect");
        //detect.AddComponent<MonsterDetectArea>();
        //SphereCollider sc = detect.AddComponent<SphereCollider>();
        //sc.radius = 3f;
        //sc.isTrigger = true;
        //detect.transform.parent = prefabInstantiate.transform;

        PrefabUtility.ReplacePrefab(prefabInstantiate, prefab, ReplacePrefabOptions.ConnectToPrefab);
        //PrefabUtility.a
        EditorUtility.SetDirty(prefab);

        GameObject.DestroyImmediate(prefabInstantiate);
    }
}
