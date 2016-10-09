/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     ActorEditor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-26
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using System.IO;

using UnityEditor;

public class ActorEditor
{
    [MenuItem("KOL/Actor/Auto Gen")]
    public static void AutoGen()
    {
        AvatarEditor.Split();

        AnimatorEditor.GenController();

        HumanEditor.GenPrefab();

        EquipEditor.Split();

        WeaponEditor.Split();

        MonsterEditor.GenMonster();
    }
}
