using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PsdLayerToPngFiles : EditorWindow
{
	#region Properties
	
	private static List<PsdLayerExtractor> extractors = new List<PsdLayerExtractor>();
	private static Vector2 scrollPosition;
	
	#endregion
	
	void OnGUI()
	{
		if (GUILayout.Button("Run", GUILayout.MaxWidth(200)))
		{
			foreach (var extractor in extractors)
				extractor.SaveLayersToPNGs();
		}
		GUILayout.Space(20);
		
		// layers
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		{
			for (var i=0; i<extractors.Count; ++i){
				GUILayout.BeginVertical();
				extractors[i].OnGUI(false, delegate(){ extractors.RemoveAt(i--); });
				GUILayout.EndVertical();
			}
		}
		GUILayout.EndScrollView();
	}
	
	[MenuItem ("Assets/Save PSD Layers to PNG files", true, 20000)]
	private static bool saveLayersEnabled()
	{
	    for (var i=0; i<Selection.objects.Length; ++i)
	    {
	        var obj = Selection.objects[i];
	        var filePath = AssetDatabase.GetAssetPath(obj);
			if (filePath.EndsWith(".psd", System.StringComparison.CurrentCultureIgnoreCase))
				return true;
	    }
		
		return false;
	}
	
	[MenuItem ("Assets/Save PSD Layers to PNG files", false, 20000)]
	private static void saveLayers()
	{
		extractors.Clear();
		
	    for (var i=0; i<Selection.objects.Length; ++i)
	    {
	        var obj = Selection.objects[i];
	        var filePath = AssetDatabase.GetAssetPath(obj);
			if (!filePath.EndsWith(".psd", System.StringComparison.CurrentCultureIgnoreCase))
				continue;
			
			extractors.Add(new PsdLayerExtractor(null, null, filePath, null));
	    }
		
        var window = EditorWindow.GetWindow<PsdLayerToPngFiles>(
			true, "Save PSD Layers to PNG files");
		window.Show();
	}
};