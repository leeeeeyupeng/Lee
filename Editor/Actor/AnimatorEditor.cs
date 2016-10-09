/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     AnimatorEditor.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-09
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

public class AnimatorEditor : Editor
{
    static string m_fbxPath = "Arts\\Charactor\\Human";
    static string m_animatorPath = "Project\\Arts\\Charactor\\Human";
    [MenuItem("KOL/Actor/Mecanim/Gen Controller")]
    public static void GenController()
    {
        GenController(m_fbxPath);
    }

    public static void GenController(string fbxPath)
    {
        DirectoryInfo folder = new DirectoryInfo("Assets/" + fbxPath);
        string fbxPathBones = "";
        List<string> fbxPathClip = new List<string>();
        foreach(FileInfo file in folder.GetFiles())
        {
            if(file.Extension == ".fbx" || file.Extension == ".FBX")
            {
                if(file.Name.Contains("@"))
                {
                    fbxPathClip.Add(file.FullName);
                }
                else
                {
                    fbxPathBones = file.FullName;
                }
            }
        }

        string animatorPath = fbxPathBones.Replace(m_fbxPath, m_animatorPath);
        animatorPath = string.Format("{0}\\{1}.controller", Path.GetDirectoryName(animatorPath), Path.GetFileNameWithoutExtension(animatorPath));
        animatorPath = animatorPath.Replace("\\","/");
        animatorPath = animatorPath.Replace(Application.dataPath,"Assets");
        //animatorPath = "Assets/Mecanim/StateMachineTransitions.controller";
        if (!Directory.Exists(Path.GetDirectoryName(animatorPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(animatorPath));
        }
        //创建animationController文件，保存在Assets路径下
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);
        //得到它的Layer， 默认layer为base 你可以去拓展
        AnimatorControllerLayer layer = animatorController.layers[0];

        foreach(string clip in fbxPathClip)
        {
            AddStateTransition(clip,layer);
        }
    }

    private static void AddStateTransition(string path, AnimatorControllerLayer layer)
    {
        path = path.Replace("\\", "/");
        path = path.Replace(Application.dataPath, "Assets");

        AnimatorStateMachine sm = layer.stateMachine;
        //根据动画文件读取它的AnimationClip对象
        AnimationClip newClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
        //取出动画名子 添加到state里面
        AnimatorState state = sm.AddState(newClip.name);
        state.motion = newClip;
        //把state添加在layer里面
        //AnimatorStateTransition trans = sm.AddAnyStateTransition(state);
        //把默认的时间条件删除
        //trans.hasExitTime = true;
        //trans.exitTime = 0;

        if (path.Contains("Idle") || path.Contains("idle"))
        {
            sm.defaultState = state;
        }
    }
}
