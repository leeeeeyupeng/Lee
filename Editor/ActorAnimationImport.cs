/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     ActorAnimationImport.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-22
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

public class ActorAnimationImport : AssetPostprocessor
{
    void OnPreprocessAnimation()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;

        List<ModelImporterClipAnimation> animations = new List<ModelImporterClipAnimation>();
        foreach(ModelImporterClipAnimation animation in  modelImporter.defaultClipAnimations)
        {
            Debug.Log(assetPath);
            if(assetPath.Contains("idle"))
            {
                Debug.Log("idle");
                animation.loop = true;
                animation.loopTime = true;
                animation.loopPose = true;
            }
            else if (assetPath.Contains("run"))
            {
                animation.loopTime = true;
                animation.loopPose = true;
            }
            else if (assetPath.Contains("attack"))
            {
                Debug.Log("attack");
                //animation.wrapMode = WrapMode.Loop;
                animation.loop = true;
                animation.loopTime = false;
                animation.loopPose = false;
                Debug.Log(animation.loopTime);
            }

            animations.Add(animation);
        }

        modelImporter.clipAnimations = animations.ToArray();
    }
}
