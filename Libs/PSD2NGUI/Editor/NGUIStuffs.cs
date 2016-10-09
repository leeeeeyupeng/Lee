using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NGUIRootCreator : UICreateNewUIWizard{
};

public class NGUIFontCreator
{
	private static string fontPrefabPath;
	private static Object fontPrefab;
	
	public static void CreateDynamic(string preName, PsdLayerToNGUI.Data data, 
		PsdLayerCommandParser.Control c)
	{
#if UNITY_3_5
#else
		if (data.trueTypeFont == null){
			Debug.LogError("True Type Font doens't set");
			return;
		}
		
		NGUIFontCreator.fontPrefabPath = preName + "_Font_" + c.fontSize + ".prefab";
		
		var name = data.trueTypeFont.name;
		GameObject go = null;
		if (!System.IO.File.Exists(NGUIFontCreator.fontPrefabPath))
		{
			NGUIFontCreator.fontPrefab = PrefabUtility.CreateEmptyPrefab(NGUIFontCreator.fontPrefabPath);

			// Create a new game object for the font
			go = new GameObject(name);
			
			var font = go.AddComponent<UIFont>();
			{
				font.atlas = null;
				font.dynamicFont = data.trueTypeFont;
				font.dynamicFontStyle = FontStyle.Normal;
                font.defaultSize = c.fontSize;
			}
			
			// Update the prefab
			PrefabUtility.ReplacePrefab(go, NGUIFontCreator.fontPrefab);
			GameObject.DestroyImmediate(go);
			AssetDatabase.Refresh();
		}
		go = AssetDatabase.LoadAssetAtPath(NGUIFontCreator.fontPrefabPath, typeof(GameObject)) as GameObject;

        //NGUISettings.bitmapFont = null;
        NGUISettings.ambigiousFont = go.GetComponent<UIFont>();
        NGUISettings.dynamicFont = go.GetComponent<UIFont>().dynamicFont;
		NGUISettings.fontSize = c.fontSize;
#endif
	}
	
	public static void PrepareBitmap(string preName, PsdLayerToNGUI.Data data){
		NGUIFontCreator.fontPrefabPath = preName + "_Font.prefab";
		
#if UNITY_3_4
		NGUIFontCreator.fontPrefab = EditorUtility.CreateEmptyPrefab(NGUIFontCreator.fontPrefabPath);
#else
		NGUIFontCreator.fontPrefab = PrefabUtility.CreateEmptyPrefab(NGUIFontCreator.fontPrefabPath);
#endif
	}
	
	public static void CreateBitmap(PsdLayerToNGUI.Data data){
		// Create a new game object for the font
		var name = data.fontTexture.name;
		var go = new GameObject(name);
		
		NGUISettings.BMFont = go.AddComponent<UIFont>();
		NGUISettings.fontData = data.fontData;
		NGUISettings.fontTexture = data.fontTexture;
		{
			BMFontReader.Load(NGUISettings.BMFont.bmFont, 
				NGUITools.GetHierarchy(NGUISettings.BMFont.gameObject), NGUISettings.fontData.bytes);

			NGUISettings.BMFont.spriteName = NGUISettings.fontTexture.name;
			NGUISettings.BMFont.atlas = NGUISettings.atlas;
		}
		
		// Update the prefab
#if UNITY_3_4
		EditorUtility.ReplacePrefab(go, NGUIFontCreator.fontPrefab);
#else
		PrefabUtility.ReplacePrefab(go, NGUIFontCreator.fontPrefab);
#endif
		GameObject.DestroyImmediate(go);
		AssetDatabase.Refresh();
		
		// Select the atlas
		go = AssetDatabase.LoadAssetAtPath(fontPrefabPath, typeof(GameObject)) as GameObject;
		
		NGUISettings.dynamicFont = null;
		NGUISettings.BMFont = data.fontPrefab = go.GetComponent<UIFont>();
		NGUISettings.BMFont.spriteName = name;
	}
};

public class NGUIWidgetCreator
{
	// UICreateWidgetWizard
	
	public static string mButton = "";
	public static string mImage0 = "";
	public static string mImage1 = "";
	public static string mImage2 = "";
	public static string mImage3 = "";
	public static string mSliderBG = "";
	public static string mSliderFG = "";
	public static string mSliderTB = "";
	public static string mCheckBG = "";
	public static string mCheck = "";
	public static string mInputBG = "";
	public static string mListFG = "";
	public static string mListBG = "";
	public static string mListHL = "";
	public static string mScrollBG = "";
	public static string mScrollFG = "";
	
	static Color mColor = Color.white;
	static bool mScrollCL = true;
	static UIScrollBar.FillDirection mScrollDir = UIScrollBar.FillDirection.LeftToRight;

	/// <summary>
	/// Convenience function -- creates the "Add To" button and the parent object field to the right of it.
	/// </summary>

	private static bool ShouldCreate (GameObject go, bool isValid)
	{
		return true;
	}

	/// <summary>
	/// Label creation function.
	/// </summary>

	public static void CreateLabel (GameObject go)
	{
		if (ShouldCreate(go, NGUISettings.ambigiousFont != null))
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.ambigiousFont = NGUISettings.ambigiousFont;
			lbl.fontSize = NGUISettings.fontSize;
			lbl.color = mColor;
			lbl.AssumeNaturalSize();
			Selection.activeGameObject = lbl.gameObject;
		}
	}

	/// <summary>
	/// Sprite creation function.
	/// </summary>

	public static void CreateSprite (GameObject go, string field, bool slicedSprite)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			var sprite = NGUITools.AddWidget<UISprite>(go);
			sprite.type = slicedSprite ? UISprite.Type.Sliced : UISprite.Type.Simple;
			sprite.name = sprite.name + " (" + field + ")";
			sprite.atlas = NGUISettings.atlas;
			sprite.spriteName = field;
			sprite.pivot = NGUISettings.pivot;
			sprite.MakePixelPerfect();
			Selection.activeGameObject = sprite.gameObject;
		}
	}

	/// <summary>
	/// UI Texture doesn't do anything other than creating the widget.
	/// </summary>

	public static void CreateSimpleTexture (GameObject go)
	{
		if (ShouldCreate(go, true))
		{
			UITexture tex = NGUITools.AddWidget<UITexture>(go);
			Selection.activeGameObject = tex.gameObject;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	public static void CreateButton (GameObject go)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Button";
			
			UISprite bg = NGUITools.AddWidget<UISprite>(go);
			bg.type = UISprite.Type.Sliced;
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mButton;
			bg.width = 200;
			bg.height = 50;
			bg.MakePixelPerfect();
			
			if (NGUISettings.ambigiousFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.ambigiousFont = NGUISettings.ambigiousFont;
				lbl.text = go.name;
				lbl.AssumeNaturalSize();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UIButton>().tweenTarget = bg != null ? bg.gameObject : null;
			go.AddComponent<UIPlaySound>();

			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Button creation function.
	/// </summary>

	public static void CreateImageButton (GameObject go)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Image Button";
			
			UISpriteData sp = NGUISettings.atlas.GetSprite(mImage0);
			UISprite sprite = NGUITools.AddWidget<UISprite>(go);
			sprite.type = sp.hasBorder ? UISprite.Type.Sliced : UISprite.Type.Simple;
			sprite.name = "Background";
			sprite.depth = depth;
			sprite.atlas = NGUISettings.atlas;
			sprite.spriteName = mImage0;
			sprite.transform.localScale = new Vector3(150f, 40f, 1f);
			sprite.MakePixelPerfect();

			if (NGUISettings.ambigiousFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.ambigiousFont = NGUISettings.ambigiousFont;
				lbl.text = go.name;
				lbl.AssumeNaturalSize();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			UIImageButton ib	= go.AddComponent<UIImageButton>();
			ib.target			= sprite;
			ib.normalSprite		= mImage0;
			ib.hoverSprite		= mImage1;
			ib.pressedSprite	= mImage2;
			ib.disabledSprite	= mImage3;
			go.AddComponent<UIPlaySound>();

			Selection.activeGameObject = go;
		}
	}
	
	/// <summary>
	/// Toggle creation function.
	/// </summary>

	public static void CreateToggle (GameObject go)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Toggle";

			UISprite bg = NGUITools.AddWidget<UISprite>(go);
			bg.type = UISprite.Type.Sliced;
			bg.name = "1-Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mCheckBG;
			bg.width = 26;
			bg.height = 26;
			bg.MakePixelPerfect();
			
			UISprite fg = NGUITools.AddWidget<UISprite>(go);
			fg.name = "2-Checkmark";
			fg.atlas = NGUISettings.atlas;
			fg.spriteName = mCheck;
			fg.MakePixelPerfect();

			if (NGUISettings.ambigiousFont != null)
			{
				UILabel lbl = NGUITools.AddWidget<UILabel>(go);
				lbl.ambigiousFont = NGUISettings.ambigiousFont;
				lbl.text = go.name;
				lbl.pivot = UIWidget.Pivot.Left;
				lbl.transform.localPosition = new Vector3(16f, 0f, 0f);
				lbl.MakePixelPerfect();
			}

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the scripts
			go.AddComponent<UIToggle>().activeSprite = fg;
			go.AddComponent<UIButton>().tweenTarget = bg.gameObject;
			go.AddComponent<UIButtonScale>().tweenTarget = bg.transform;
			go.AddComponent<UIPlaySound>();

			Selection.activeGameObject = go;
		}
	}
	
	/// <summary>
	/// Scroll bar template.
	/// </summary>

	public static void CreateScrollBar (GameObject go)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Scroll Bar";
			
			UISprite bg = NGUITools.AddWidget<UISprite>(go);
			bg.type = UISprite.Type.Sliced;
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mScrollBG;
			
			Vector4 border = bg.border;
			bg.width = Mathf.RoundToInt(400f + border.x + border.z);
			bg.height = Mathf.RoundToInt(14f + border.y + border.w);
			bg.MakePixelPerfect();
			
			UISprite fg = NGUITools.AddWidget<UISprite>(go);
			fg.type = UISprite.Type.Sliced;
			fg.name = "Foreground";
			fg.atlas = NGUISettings.atlas;
			fg.spriteName = mScrollFG;
			
			UIScrollBar sb = go.AddComponent<UIScrollBar>();
			sb.backgroundWidget = bg;
			sb.foregroundWidget = fg;
			sb.fillDirection = mScrollDir;
			sb.barSize = 0.3f;
			sb.value = 0.3f;
			sb.ForceUpdate();

			if (mScrollCL)
			{
				NGUITools.AddWidgetCollider(bg.gameObject);
				NGUITools.AddWidgetCollider(fg.gameObject);
			}
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Progress bar creation function.
	/// </summary>

	public static void CreateSlider (GameObject go, bool slider)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = slider ? "Slider" : "Progress Bar";

			// Background sprite
			UISpriteData bgs = NGUISettings.atlas.GetSprite(mSliderBG);
			UISprite back = (UISprite)NGUITools.AddWidget<UISprite>(go);
			
			back.type = bgs.hasBorder ? UISprite.Type.Sliced : UISprite.Type.Simple;
			back.name = "Background";
			back.depth = depth;
			back.pivot = UIWidget.Pivot.Left;
			back.atlas = NGUISettings.atlas;
			back.spriteName = mSliderBG;
			back.width = 200;
			back.height = 30;
			back.transform.localPosition = Vector3.zero;
			back.MakePixelPerfect();

			// Fireground sprite
			UISpriteData fgs = NGUISettings.atlas.GetSprite(mSliderFG);
			UISprite front = NGUITools.AddWidget<UISprite>(go);
			front.type = fgs.hasBorder ? UISprite.Type.Sliced : UISprite.Type.Simple;
			front.name = "Foreground";
			front.pivot = UIWidget.Pivot.Left;
			front.atlas = NGUISettings.atlas;
			front.spriteName = mSliderFG;
			front.width = 200;
			front.height = 30;
			front.transform.localPosition = Vector3.zero;
			front.MakePixelPerfect();

			// Add a collider
			if (slider) NGUITools.AddWidgetCollider(go);

			// Add the slider script
			UISlider uiSlider = go.AddComponent<UISlider>();
			uiSlider.foregroundWidget = front.transform.GetComponent<UIWidget>();

			// Thumb sprite
			if (slider)
			{
				UISpriteData tbs = NGUISettings.atlas.GetSprite(mSliderTB);
				UISprite thb = NGUITools.AddWidget<UISprite>(go);
				
				thb.type = tbs.hasBorder ? UISprite.Type.Sliced : UISprite.Type.Simple;
				thb.name = "Thumb";
				thb.atlas = NGUISettings.atlas;
				thb.spriteName = mSliderTB;
				thb.width = 20;
				thb.height = 40;
				thb.transform.localPosition = new Vector3(200f, 0f, 0f);
				thb.MakePixelPerfect();

				NGUITools.AddWidgetCollider(thb.gameObject);
				thb.gameObject.AddComponent<UIButtonColor>();
				thb.gameObject.AddComponent<UIButtonScale>();

				uiSlider.thumb = thb.transform;
			}
			uiSlider.value = 1f;

			// Select the slider
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Input field creation function.
	/// </summary>

	public static void CreateInput (GameObject go, bool isPassword)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null && NGUISettings.ambigiousFont != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = "Input";
			int padding = 3;
			
			UISprite bg = NGUITools.AddWidget<UISprite>(go);
			bg.type = UISprite.Type.Sliced;
			bg.name = "Background";
			bg.depth = depth;
			bg.atlas = NGUISettings.atlas;
			bg.spriteName = mInputBG;
			bg.pivot = UIWidget.Pivot.Left;
			bg.width = 400;
			if (NGUISettings.ambigiousFont != null)
				bg.height = NGUISettings.fontSize + padding * 2;
			else
				bg.height = 32;
			bg.transform.localPosition = Vector3.zero;
			bg.MakePixelPerfect();

			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.ambigiousFont = NGUISettings.ambigiousFont;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.transform.localPosition = new Vector3(padding, 0f, 0f);
			lbl.multiLine = false;
			lbl.supportEncoding = false;
			lbl.width = Mathf.RoundToInt(400f - padding * 2f);
			lbl.text = "You can type here";
			lbl.AssumeNaturalSize();

			// Add a collider to the background
			NGUITools.AddWidgetCollider(go);

			// Add an input script to the background and have it point to the label
			UIInput input = go.AddComponent<UIInput>();
			input.label = lbl;
			input.inputType = isPassword ? UIInput.InputType.Password : UIInput.InputType.Standard;

			// Update the selection
			Selection.activeGameObject = go;
		}
	}

	/// <summary>
	/// Create a popup list or a menu.
	/// </summary>

	public static void CreatePopup (GameObject go, bool isDropDown)
	{
		if (ShouldCreate(go, NGUISettings.atlas != null && NGUISettings.ambigiousFont != null))
		{
			int depth = 0;//NGUITools.CalculateNextDepth(go);
			go = NGUITools.AddChild(go);
			go.name = isDropDown ? "Popup List" : "Popup Menu";
			
			UISpriteData sphl = NGUISettings.atlas.GetSprite(mListHL);
			UISpriteData spfg = NGUISettings.atlas.GetSprite(mListFG);
			
			Vector2 hlPadding = new Vector2(Mathf.Max(4f, sphl.paddingLeft), Mathf.Max(4f, sphl.paddingTop));
			Vector2 fgPadding = new Vector2(Mathf.Max(4f, spfg.paddingLeft), Mathf.Max(4f, spfg.paddingTop));

			// Background sprite
			UISprite sprite = NGUITools.AddSprite(go, NGUISettings.atlas, mListFG);
			sprite.depth = depth;
			sprite.atlas = NGUISettings.atlas;
			sprite.pivot = UIWidget.Pivot.Left;
			sprite.width = Mathf.RoundToInt(150f + fgPadding.x * 2f);
			sprite.height = Mathf.RoundToInt(NGUISettings.fontSize + fgPadding.y * 2f);
			sprite.transform.localPosition = Vector3.zero;
			sprite.MakePixelPerfect();

			// Text label
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.ambigiousFont = NGUISettings.ambigiousFont;
			lbl.fontSize = NGUISettings.fontSize;
			lbl.text = go.name;
			lbl.pivot = UIWidget.Pivot.Left;
			lbl.cachedTransform.localPosition = new Vector3(fgPadding.x, 0f, 0f);
			lbl.MakePixelPerfect();

			// Add a collider
			NGUITools.AddWidgetCollider(go);

			// Add the popup list
			UIPopupList list = go.AddComponent<UIPopupList>();
			list.atlas = NGUISettings.atlas;
			if (NGUISettings.BMFont != null)
				list.bitmapFont = NGUISettings.BMFont;
			else
				list.trueTypeFont = NGUISettings.dynamicFont;
			list.backgroundSprite = mListBG;
			list.highlightSprite = mListHL;
			list.padding = hlPadding;
			//if (isDropDown) list.textLabel = lbl;
			for (int i = 0; i < 5; ++i) list.items.Add(isDropDown ? ("List Option " + i) : ("Menu Option " + i));

			// Add the scripts
			go.AddComponent<UIButton>().tweenTarget = sprite.gameObject;
			go.AddComponent<UIPlaySound>();

			Selection.activeGameObject = go;
		}
	}
};

public class NGUIAtlasMaker
{
	// UIAtlasMaker
	
	public class SpriteEntry : UISpriteData
	{
		// Sprite texture -- original texture or a temporary texture
		public Texture2D tex;
		
		// Whether the texture is temporary and should be deleted
		public bool temporaryTexture = false;
	}
	
	public static void CreateAtlas(string currentPath)
	{
		string prefabPath = "";
		string matPath = "";
		{
			if (string.IsNullOrEmpty(prefabPath))
				prefabPath = currentPath + ".prefab";
			
			if (string.IsNullOrEmpty(matPath))
				matPath = currentPath + ".mat";
		}
		
		// Try to load the material
		Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

		// If the material doesn't exist, create it
		if (mat == null)
		{
			Shader shader = Shader.Find("Unlit/Transparent");
			mat = new Material(shader);

			// Save the material
			AssetDatabase.CreateAsset(mat, matPath);
			AssetDatabase.Refresh();

			// Load the material so it's usable
			mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
		}
		
		GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		{
			// Create a new prefab for the atlas
#if UNITY_3_4
			Object prefab = (go != null) ? go : EditorUtility.CreateEmptyPrefab(prefabPath); 
#else
			Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
			// Create a new game object for the atlas
			go = new GameObject(System.IO.Path.GetFileName(prefabPath));
			go.AddComponent<UIAtlas>().spriteMaterial = mat;

			// Update the prefab
#if UNITY_3_4
			EditorUtility.ReplacePrefab(go, prefab);
#else
			PrefabUtility.ReplacePrefab(go, prefab);
#endif
			GameObject.DestroyImmediate(go);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Select the atlas
			go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
			NGUISettings.atlas = go.GetComponent<UIAtlas>();
		}
	}
	
	public static void UpdateAtlas(List<Texture> textures, bool appendSprites)
	{
		// Create a list of sprites using the collected textures
		List<NGUIAtlasMaker.SpriteEntry> sprites = NGUIAtlasMaker.CreateSprites(textures);

		if (sprites.Count > 0)
		{
			// Extract sprites from the atlas, filling in the missing pieces
			if (appendSprites) NGUIAtlasMaker.ExtractSprites(NGUISettings.atlas, sprites);

			// NOTE: It doesn't seem to be possible to undo writing to disk, and there also seems to be no way of
			// detecting an Undo event. Without either of these it's not possible to restore the texture saved to disk,
			// so the undo process doesn't work right. Because of this I'd rather disable it altogether until a solution is found.

			// The ability to undo this action is always useful
			//NGUIEditorTools.RegisterUndo("Update Atlas", UISettings.atlas, UISettings.atlas.texture, UISettings.atlas.material);

			// Update the atlas
			NGUIAtlasMaker.UpdateAtlas(NGUISettings.atlas, sprites);
		}
	}
	
	/// <summary>
	/// Create a list of sprites using the specified list of textures.
	/// </summary>

	public static List<SpriteEntry> CreateSprites (List<Texture> textures)
	{
		List<SpriteEntry> list = new List<SpriteEntry>();

		foreach (Texture tex in textures)
		{
			Texture2D oldTex = NGUIEditorTools.ImportTexture(tex, true, false, true);
			if (oldTex == null) continue;

			// If we aren't doing trimming, just use the texture as-is
			if (!NGUISettings.atlasTrimming && !NGUISettings.atlasPMA)
			{
				SpriteEntry sprite = new SpriteEntry();
				sprite.SetRect(0, 0, oldTex.width, oldTex.height);
				sprite.tex = oldTex;
				sprite.name = oldTex.name;
				sprite.temporaryTexture = false;
				list.Add(sprite);
				continue;
			}

			// If we want to trim transparent pixels, there is more work to be done
			Color32[] pixels = oldTex.GetPixels32();

			int xmin = oldTex.width;
			int xmax = 0;
			int ymin = oldTex.height;
			int ymax = 0;
			int oldWidth = oldTex.width;
			int oldHeight = oldTex.height;

			// Find solid pixels
			if (NGUISettings.atlasTrimming)
			{
				for (int y = 0, yw = oldHeight; y < yw; ++y)
				{
					for (int x = 0, xw = oldWidth; x < xw; ++x)
					{
						Color32 c = pixels[y * xw + x];

						if (c.a != 0)
						{
							if (y < ymin) ymin = y;
							if (y > ymax) ymax = y;
							if (x < xmin) xmin = x;
							if (x > xmax) xmax = x;
						}
					}
				}
			}
			else
			{
				xmin = 0;
				xmax = oldWidth - 1;
				ymin = 0;
				ymax = oldHeight - 1;
			}

			int newWidth  = (xmax - xmin) + 1;
			int newHeight = (ymax - ymin) + 1;

			if (newWidth > 0 && newHeight > 0)
			{
				SpriteEntry sprite = new SpriteEntry();
				sprite.x = 0;
				sprite.y = 0;
				sprite.width = oldTex.width;
				sprite.height = oldTex.height;

				// If the dimensions match, then nothing was actually trimmed
				if (!NGUISettings.atlasPMA && (newWidth == oldWidth && newHeight == oldHeight))
				{
					sprite.tex = oldTex;
					sprite.name = oldTex.name;
					sprite.temporaryTexture = false;
				}
				else
				{
					// Copy the non-trimmed texture data into a temporary buffer
					Color32[] newPixels = new Color32[newWidth * newHeight];

					for (int y = 0; y < newHeight; ++y)
					{
						for (int x = 0; x < newWidth; ++x)
						{
							int newIndex = y * newWidth + x;
							int oldIndex = (ymin + y) * oldWidth + (xmin + x);
							if (NGUISettings.atlasPMA) newPixels[newIndex] = NGUITools.ApplyPMA(pixels[oldIndex]);
							else newPixels[newIndex] = pixels[oldIndex];
						}
					}

					// Create a new texture
					sprite.temporaryTexture = true;
					sprite.name = oldTex.name;
					sprite.tex = new Texture2D(newWidth, newHeight);
					sprite.tex.SetPixels32(newPixels);
					sprite.tex.Apply();

					// Remember the padding offset
					sprite.SetPadding(xmin, ymin, oldWidth - newWidth - xmin, oldHeight - newHeight - ymin);
				}
				list.Add(sprite);
			}
		}
		return list;
	}
	
	/// <summary>
	/// Extract sprites from the atlas, adding them to the list.
	/// </summary>

	public static void ExtractSprites (UIAtlas atlas, List<SpriteEntry> finalSprites)
	{
		// Make the atlas texture readable
		Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false, !atlas.premultipliedAlpha);

		if (atlasTex != null)
		{
			Color32[] oldPixels = null;
			int oldWidth = atlasTex.width;
			int oldHeight = atlasTex.height;
			List<UISpriteData> existingSprites = atlas.spriteList;

			foreach (UISpriteData es in existingSprites)
			{
				bool found = false;

				foreach (SpriteEntry fs in finalSprites)
				{
					if (es.name == fs.name)
					{
						fs.CopyBorderFrom(es);
						found = true;
						break;
					}
				}

				if (!found)
				{
					// Read the atlas
					if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

					int xmin = Mathf.Clamp(es.x, 0, oldWidth);
					int ymin = Mathf.Clamp(es.y, 0, oldHeight);
					int newWidth = Mathf.Clamp(es.width, 0, oldWidth);
					int newHeight = Mathf.Clamp(es.height, 0, oldHeight);
					if (newWidth == 0 || newHeight == 0) continue;

					Color32[] newPixels = new Color32[newWidth * newHeight];

					for (int y = 0; y < newHeight; ++y)
					{
						for (int x = 0; x < newWidth; ++x)
						{
							int newIndex = (newHeight - 1 - y) * newWidth + x;
							int oldIndex = (oldHeight - 1 - (ymin + y)) * oldWidth + (xmin + x);
							newPixels[newIndex] = oldPixels[oldIndex];
						}
					}

					// Create a new sprite
					SpriteEntry sprite = new SpriteEntry();
					sprite.CopyFrom(es);
					sprite.SetRect(0, 0, newWidth, newHeight);
					sprite.temporaryTexture = true;
					sprite.tex = new Texture2D(newWidth, newHeight);
					sprite.tex.SetPixels32(newPixels);
					sprite.tex.Apply();
					finalSprites.Add(sprite);
				}
			}
		}

		// The atlas no longer needs to be readable
		NGUIEditorTools.ImportTexture(atlas.texture, false, false, !atlas.premultipliedAlpha);
	}

	/// <summary>
	/// Update the sprite atlas, keeping only the sprites that are on the specified list.
	/// </summary>

	public static void UpdateAtlas (UIAtlas atlas, List<SpriteEntry> sprites)
	{
		if (sprites.Count > 0)
		{
			// Combine all sprites into a single texture and save it
			if (UpdateTexture(atlas, sprites))
			{
				// Replace the sprites within the atlas
				ReplaceSprites(atlas, sprites);
			}

			// Release the temporary textures
			ReleaseSprites(sprites);
			return;
		}
		else
		{
			atlas.spriteList.Clear();
			string path = NGUIEditorTools.GetSaveableTexturePath(atlas);
			atlas.spriteMaterial.mainTexture = null;
			if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
		}
		
		atlas.MarkAsChanged();
		Selection.activeGameObject = (NGUISettings.atlas != null) ? NGUISettings.atlas.gameObject : null;
	}
	
	/// <summary>
	/// Combine all sprites into a single texture and save it to disk.
	/// </summary>

	static bool UpdateTexture (UIAtlas atlas, List<SpriteEntry> sprites)
	{
		// Get the texture for the atlas
		Texture2D tex = atlas.texture as Texture2D;
		string oldPath = (tex != null) ? AssetDatabase.GetAssetPath(tex.GetInstanceID()) : "";
		string newPath = NGUIEditorTools.GetSaveableTexturePath(atlas);

		// Clear the read-only flag in texture file attributes
		if (System.IO.File.Exists(newPath))
		{
#if !UNITY_4_1 && !UNITY_4_0 && !UNITY_3_5
			if (!AssetDatabase.IsOpenForEdit(newPath))
			{
				Debug.LogError(newPath + " is not editable. Did you forget to do a check out?");
				return false;
			}
#endif
			System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(newPath);
			newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
			System.IO.File.SetAttributes(newPath, newPathAttrs);
		}

		bool newTexture = (tex == null || oldPath != newPath);

		if (newTexture)
		{
			// Create a new texture for the atlas
			tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		}
		else
		{
			// Make the atlas readable so we can save it
			tex = NGUIEditorTools.ImportTexture(oldPath, true, false, !atlas.premultipliedAlpha);
		}

		// Pack the sprites into this texture
		if (PackTextures(tex, sprites))
		{
			byte[] bytes = tex.EncodeToPNG();
			System.IO.File.WriteAllBytes(newPath, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			tex = NGUIEditorTools.ImportTexture(newPath, false, true, !atlas.premultipliedAlpha);

			// Update the atlas texture
			if (newTexture)
			{
				if (tex == null) Debug.LogError("Failed to load the created atlas saved as " + newPath);
				else atlas.spriteMaterial.mainTexture = tex;
				ReleaseSprites(sprites);
				
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			return true;
		}
		else
		{
			if (!newTexture) NGUIEditorTools.ImportTexture(oldPath, false, true, !atlas.premultipliedAlpha);
			
			//Debug.LogError("Operation canceled: The selected sprites can't fit into the atlas.\n" +
			//	"Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead.");
			
			EditorUtility.DisplayDialog("Operation Canceled", "The selected sprites can't fit into the atlas.\n" +
					"Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead", "OK");
			return false;
		}
	}
	
	/// <summary>
	/// Replace the sprites within the atlas.
	/// </summary>

	static void ReplaceSprites (UIAtlas atlas, List<SpriteEntry> sprites)
	{
		// Get the list of sprites we'll be updating
		List<UISpriteData> spriteList = atlas.spriteList;
		List<UISpriteData> kept = new List<UISpriteData>();

		// Run through all the textures we added and add them as sprites to the atlas
		for (int i = 0; i < sprites.Count; ++i)
		{
			SpriteEntry se = sprites[i];
			UISpriteData sprite = AddSprite(spriteList, se);
			kept.Add(sprite);
		}

		// Remove unused sprites
		for (int i = spriteList.Count; i > 0; )
		{
			UISpriteData sp = spriteList[--i];
			if (!kept.Contains(sp)) spriteList.RemoveAt(i);
		}

		// Sort the sprites so that they are alphabetical within the atlas
		atlas.SortAlphabetically();
		atlas.MarkAsChanged();
	}

	/// <summary>
	/// Release all temporary textures created for the sprites.
	/// </summary>

	static void ReleaseSprites (List<SpriteEntry> sprites)
	{
		foreach (SpriteEntry se in sprites)
		{
			if (se.temporaryTexture)
			{
				NGUITools.Destroy(se.tex);
				se.tex = null;
			}
		}
		Resources.UnloadUnusedAssets();
	}
	
	/// <summary>
	/// Used to sort the sprites by pixels used
	/// </summary>
	
	static int Compare (SpriteEntry a, SpriteEntry b)
	{
		// A is null b is not b is greater so put it at the front of the list
		if (a == null && b != null) return 1;

		// A is not null b is null a is greater so put it at the front of the list
		if (a == null && b != null) return -1;

		// Get the total pixels used for each sprite
		int aPixels = a.width * a.height;
		int bPixels = b.width * b.height;

		if (aPixels > bPixels) return -1;
		else if (aPixels < bPixels) return 1;
		return 0;
	}
	
	/// <summary>
	/// Pack all of the specified sprites into a single texture, updating the outer and inner rects of the sprites as needed.
	/// </summary>

	static bool PackTextures (Texture2D tex, List<SpriteEntry> sprites)
	{
		Texture2D[] textures = new Texture2D[sprites.Count];
		Rect[] rects;

#if UNITY_3_5 || UNITY_4_0
		int maxSize = 4096;
#else
		int maxSize = SystemInfo.maxTextureSize;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
#if !UNITY_3_5 && !UNITY_4_0
		if (PlayerSettings.targetGlesGraphics == TargetGlesGraphics.OpenGLES_1_x)
		{
			maxSize = Mathf.Min(maxSize, 1024);
		}
		else
#endif
		{
			maxSize = Mathf.Min(maxSize, NGUISettings.allow4096 ? 4096 : 2048);
		}
#endif

		if (NGUISettings.unityPacking)
		{
			for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
			rects = tex.PackTextures(textures, NGUISettings.atlasPadding, maxSize);
		}
		else
		{
			sprites.Sort(Compare);
			for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
			rects = UITexturePacker.PackTextures(tex, textures, 4, 4, NGUISettings.atlasPadding, maxSize);
		}

		for (int i = 0; i < sprites.Count; ++i)
		{
			Rect rect = NGUIMath.ConvertToPixels(rects[i], tex.width, tex.height, true);

			// Make sure that we don't shrink the textures
			if (Mathf.RoundToInt(rect.width) != textures[i].width) return false;

			SpriteEntry se = sprites[i];
			se.x = Mathf.RoundToInt(rect.x);
			se.y = Mathf.RoundToInt(rect.y);
			se.width = Mathf.RoundToInt(rect.width);
			se.height = Mathf.RoundToInt(rect.height);
		}
		return true;
	}
	
	/// <summary>
	/// Helper function that creates a single sprite list from both the atlas's sprites as well as selected textures.
	/// Dictionary value meaning:
	/// 0 = No change
	/// 1 = Update
	/// 2 = Add
	/// </summary>

	Dictionary<string, int> GetSpriteList (List<Texture> textures)
	{
		Dictionary<string, int> spriteList = new Dictionary<string, int>();

		if (NGUISettings.atlas != null && NGUISettings.atlas.name == NGUISettings.atlas.name)
		{
			BetterList<string> spriteNames = NGUISettings.atlas.GetListOfSprites();
			foreach (string sp in spriteNames) spriteList.Add(sp, 0);
		}

		// If we have textures to work with, include them as well
		if (textures.Count > 0)
		{
			List<string> texNames = new List<string>();
			foreach (Texture tex in textures) texNames.Add(tex.name);
			texNames.Sort();

			foreach (string tex in texNames)
			{
				if (spriteList.ContainsKey(tex)) spriteList[tex] = 1;
				else spriteList.Add(tex, 2);
			}
		}
		return spriteList;
	}

	/// <summary>
	/// Add a new sprite to the atlas, given the texture it's coming from and the packed rect within the atlas.
	/// </summary>

	static UISpriteData AddSprite (List<UISpriteData> sprites, SpriteEntry se)
	{
		// See if this sprite already exists
		foreach (UISpriteData sp in sprites)
		{
			if (sp.name == se.name)
			{
				sp.CopyFrom(se);
				return sp;
			}
		}

		UISpriteData sprite = new UISpriteData();
		sprite.CopyFrom(se);
		sprites.Add(sprite);
		return sprite;
	}
};