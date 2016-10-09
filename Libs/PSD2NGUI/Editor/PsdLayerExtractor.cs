using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class PsdLayerExtractor
{
	#region Child Classes
	
	public class Layer
	{
		public bool canLoadLayer = true;
		public PsdParser.PSDLayer psdLayer;
		public List<Layer> children = new List<Layer>();
		
		public bool isContainer
		{
			get { return this.psdLayer.groupStarted; }
		}
		public bool isTextLayer
		{
			get { return this.psdLayer.isTextLayer; }
		}
		public bool isImageLayer
		{
			get { return this.psdLayer.isImageLayer; }
		}
		
		public string name
		{
			get { return this.psdLayer.name; }
		}
		
		public string text
		{
			get; internal set;
		}
		
		public Layer()
		{
		}
		
		public Layer(PsdParser.PSDLayer psdLayer)
		{
			this.psdLayer = psdLayer;
			if (this.psdLayer.isTextLayer)
				this.text = this.psdLayer.text.Replace('\r', '\n');
		}
		
		public void LoadData(BinaryReader br, int bpp)
        {
			var channelCount = this.psdLayer.channels.Length;
            for (var k=0; k<channelCount; ++k)
            {
                var channel = this.psdLayer.channels[k];
				if (this.canLoadLayer && this.psdLayer.isImageLayer)
					channel.loadData(br, bpp);
            }
        }
	};
	
	public class ImageFilePath
	{
		public string filePath;
		public string imageMd5;
		
		public ImageFilePath()
		{
		}
		public ImageFilePath(string info)
		{
			var arr = info.Split('=');
			
			this.filePath = arr[0];
			if (arr.Length > 1)
				this.imageMd5 = arr[1];
		}
        public ImageFilePath(string filePath, string imageMd5)
        {
            this.filePath = filePath;
            this.imageMd5 = imageMd5;
        }
		
		public override string ToString()
		{
			return this.filePath + "=" + this.imageMd5;
		}
	};
	
	#endregion
	
	#region Properties
	
	private bool foldout;
	
	private System.Timers.Timer watchingTimer;
	private FileSystemWatcher watcher;
	private System.Action<PsdLayerExtractor> whenPsdFileChanged;
	
	public Object PsdFileAsset
	{
		get; private set;
	}
	
	public string PsdFilePath
	{
		get { return AssetDatabase.GetAssetPath(this.PsdFileAsset); }
	}
	
	public string PsdFileName
	{
		get { return this.Psd.fileName; }
	}
	
	public GameObject RootGameObject
	{
		get; private set;
	}
	public Layer Root
	{
		get; private set;
	}
	
	public bool IsAddFont
	{
		get; set;
	}
	
	public bool IsLinked
	{
		get{ return this.RootGameObject != null && this.RootGameObject.transform.parent != null; }
	}
	
	public bool IsChanged
	{
		get{ return this.PsdMd5 != this.CalcMd5(); }
	}
	
	public PsdParser.PSD Psd
	{
		get; private set;
	}
	
	public string PsdMd5
	{
		get; private set;
	}
	
	public List<ImageFilePath> ImageFilePathes
	{
		get; private set;
	}
	public List<ImageFilePath> LastImageFilePathes
	{
		get; private set;
	}
	
	public override string ToString()
	{
		var imageFilePaths = "";
		if (this.LastImageFilePathes != null && this.LastImageFilePathes.Count > 0)
		{
			foreach (var imageFilePath in this.LastImageFilePathes)
			{
				if (!string.IsNullOrEmpty(imageFilePaths))
					imageFilePaths += ",";
				imageFilePaths += imageFilePath.ToString();
			}
		}
		if (string.IsNullOrEmpty(imageFilePaths))
			return this.PsdFilePath + ":" + this.PsdMd5 + ":" + (this.IsAddFont ? 1 : 0);
		else
			return this.PsdFilePath + ":" + this.PsdMd5 + ":" + (this.IsAddFont ? 1 : 0) + ":" + imageFilePaths;
	}
	
	#endregion
	
	public PsdLayerExtractor(GameObject uiRoot, Object psdFileAsset, 
		string info, System.Action<PsdLayerExtractor> whenPsdFileChanged)
	{
		/*! about info array
		 * 
		 *  0: path of psd file
		 *  1: md5 of psd file
		 *  2: add font boolean
		 *  3: image file infos
		*/
		
		// load psd header
		
		var arr = info.Split(':');
		var filePath = arr[0];
		
		if (psdFileAsset != null)
			this.PsdFileAsset = psdFileAsset;
		else
			this.PsdFileAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
		
		this.Psd = new PsdParser.PSD();
		this.Psd.loadHeader(filePath);
		
		// get md5
		
		if (arr.Length > 1)
		{
			this.PsdMd5 = arr[1];
			var nowMd5 = this.CalcMd5();
			if (this.PsdMd5 != nowMd5)
			{
				if (whenPsdFileChanged != null)
					whenPsdFileChanged(this);
				this.PsdMd5 = nowMd5;
			}
		}
		else
			this.PsdMd5 = this.CalcMd5();
		
		// check font option
		
		if (arr.Length > 2)
			this.IsAddFont = arr[2] == "1";
		else
			this.IsAddFont = true;
		
		// image files
		
		if (arr.Length > 3)
		{
			this.LastImageFilePathes = new List<ImageFilePath>();
			var pathAndMd5s = arr[3].Split(',');
			foreach (var pathAndMd5 in pathAndMd5s)
                this.LastImageFilePathes.Add(new ImageFilePath(pathAndMd5));
		}
		
		// set UIRoot object
		
		if (uiRoot != null)
			this.RootGameObject = GBlue.Util.FindGameObjectRecursively(uiRoot, this.PsdFileName);
		
		// load layers
		
		this.Root = new Layer();
		{
			var psdLayers = this.Psd.layerInfo.layers;
			this.LoadPsdLayers(this.Root, psdLayers, psdLayers.Length-1);
		}
		this.Root.children.Reverse();
		
		// monitor psd file
		
		this.whenPsdFileChanged = whenPsdFileChanged;
		this.BeginMonitoring();
	}
	
	private void BeginMonitoring()
	{
		//**monodevelop problem in Unity3D 3.5??
		// Mono.Debugger.Soft.ObjectCollectedException: The requested operation cannot be completed because the object has been garbage collected.
		if (this.watchingTimer != null){
			this.watchingTimer.Stop();
		}
		if (this.watcher != null){
			this.watcher.EnableRaisingEvents = false;
			this.watcher.Dispose();
		}
		var psdFilePath = this.PsdFilePath;
		
		this.watchingTimer = new System.Timers.Timer(1);
		this.watcher = new FileSystemWatcher();
		this.watcher.Path = Path.GetDirectoryName(psdFilePath);
		this.watcher.Filter = Path.GetFileName(psdFilePath);
		this.watcher.NotifyFilter = NotifyFilters.LastWrite;
		this.watcher.EnableRaisingEvents = true;
		this.watcher.Changed += new FileSystemEventHandler(delegate(object s, FileSystemEventArgs e){
			this.watcher.EnableRaisingEvents = false;
			
			if (PsdLayerToNGUI.verbose)
				Debug.Log(psdFilePath + " file changed");
			
			if (this.whenPsdFileChanged != null)
				this.whenPsdFileChanged(this);
			
			this.watchingTimer.Elapsed += delegate {
				this.watcher.EnableRaisingEvents = true;
				this.watchingTimer.Stop();
			};
			this.watchingTimer.Start();
		});
	}
	
	public GameObject Link()
	{
		this.RootGameObject = new GameObject(this.Psd.fileName);
		return this.RootGameObject;
	}
	
	private string CalcMd5Imple(Stream s)
	{
		using (var md5 = new MD5CryptoServiceProvider())
		{
			var bytes = md5.ComputeHash(s);
			var result = new StringBuilder(bytes.Length * 2);
			for (var i=0; i<bytes.Length; ++i)
				result.Append(bytes[i].ToString("x2"));

			return result.ToString();
		}
	}
	public string CalcMd5()
	{
		using (var stream = File.OpenRead(this.PsdFilePath))
		{
			return CalcMd5Imple(stream);
		}
	}
	
	public void Reload()
	{
		this.Psd = new PsdParser.PSD();
		this.Psd.loadHeader(this.PsdFilePath);
		this.PsdMd5 = this.CalcMd5();
		
		this.Root = new Layer();
		{
			var psdLayers = this.Psd.layerInfo.layers;
			this.LoadPsdLayers(this.Root, psdLayers, psdLayers.Length-1);
		}
		this.Root.children.Reverse();
	}
	
	public void Update(GameObject uiRoot)
	{
		if (this.Psd.filePath != this.PsdFilePath){
			this.Psd.filePath = this.PsdFilePath;
			this.BeginMonitoring();
		}
		
		if (this.RootGameObject == null && uiRoot != null)
			this.RootGameObject = GBlue.Util.FindGameObjectRecursively(uiRoot, this.PsdFileName);
	}
	
	private int LoadPsdLayers(Layer parent, PsdParser.PSDLayer[] psdLayers, int i)
	{
		while (i >= 0)
		{
			var psdLayer = psdLayers[i--];
			if (psdLayer.groupStarted)
			{
				var newParent = new Layer(psdLayer);
				parent.children.Add(newParent);
				i = this.LoadPsdLayers(newParent, psdLayers, i);

				if (psdLayer.name.Contains("@ignore"))
					parent.children.Remove(newParent);
			}
			else if (psdLayer.groupEnded)
			{
				parent.children.Reverse();
				break;
			}
			else if (!psdLayer.drop && !psdLayer.name.Contains("@ignore"))
			{
				parent.children.Add(new Layer(psdLayer));
			}
		}
		return i;
	}
	
	private Texture MakeSlicedSprites(ref byte[] data, Layer layer, PsdLayerRect area)
	{
		var channelCount = layer.psdLayer.channels.Length;
		var pitch = layer.psdLayer.pitch;
		var w = layer.psdLayer.area.width;
		var h = layer.psdLayer.area.height;
		
		var x1 = area.left;
		var y1 = area.top;
		var x2 = area.right;
		var y2 = area.bottom;
		
		var aaa = 0;
		var rc1 = new Rect(0, 0, x1 + aaa, y1 + aaa);
		var rc2 = new Rect(w - x2, 0, x2, y1 + aaa);
		var rc3 = new Rect(0, h - y2, x1 + aaa, y2);
		var rc4 = new Rect(w - x2, h - y2, x2, y2);
		var ww = (int)(rc1.width + rc2.width);
		var hh = (int)(rc1.height + rc3.height);
		
		var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
		var tex = new Texture2D(ww, hh, format, false);
		var opacity = (byte)Mathf.RoundToInt(layer.psdLayer.opacity * 255f);
		var colors = new Color32[ww * hh];
		var k = 0;
		for (var y=h-1; y>=0; --y)
		{
			var yy = h-1-y;
			var xx = 0;
			
			for (var x=0; x<pitch; x+=channelCount)
			{
				var n = x + y * pitch;
				var c = new Color32(1,1,1,1);
				if (channelCount == 3)
				{
					c.b = data[n++];
					c.g = data[n++];
					c.r = data[n++];
					c.a = opacity;
				}
				else
				{
					c.b = data[n++];
					c.g = data[n++];
					c.r = data[n++];
					c.a = (byte)Mathf.RoundToInt(data[n++]/255f * opacity);
				}
				
				var pt = new Vector2(xx++, yy);
				if (rc1.Contains(pt) || rc2.Contains(pt) || rc3.Contains(pt) || rc4.Contains(pt))
					colors[k++] = c;
			}
		}
		tex.SetPixels32(colors);
		tex.Apply();
		data = tex.EncodeToPNG();
		return tex;
	}
	
	private Texture MakeTexture(ref byte[] data, Layer layer)
	{
		var channelCount = layer.psdLayer.channels.Length;
		var pitch = layer.psdLayer.pitch;
		var w = layer.psdLayer.area.width;
		var h = layer.psdLayer.area.height;
		
		var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
		var tex = new Texture2D(w, h, format, false);
		var colors = new Color32[data.Length / channelCount];
		var k = 0;
		for (var y=h-1; y>=0; --y)
		{
			for (var x=0; x<pitch; x+=channelCount)
			{
				var n = x + y * pitch;
				var c = new Color32(1,1,1,1);
				if (channelCount == 4)
				{
					c.b = data[n++];
					c.g = data[n++];
					c.r = data[n++];
					c.a = (byte)Mathf.RoundToInt(data[n++]/255f * layer.psdLayer.opacity * 255f);
				}
				else
				{
					c.b = data[n++];
					c.g = data[n++];
					c.r = data[n++];
					c.a = (byte)Mathf.RoundToInt(layer.psdLayer.opacity * 255f);
				}
				colors[k++] = c;
			}
		}
		tex.SetPixels32(colors);
		tex.Apply();
		data = tex.EncodeToPNG();
		return tex;
	}
	
	public bool HasUnacceptibleChar(string str)
	{
		return str.IndexOfAny(new char[]{'\\', '/', ':', '*', '?', '"', '<', '>', '|'}) >= 0;
	}
	
	private void SaveLayersToPNGs_imple(string prePath, List<Layer> layers)
	{
		var psdFileStream = new FileStream(this.PsdFilePath, 
			FileMode.Open, FileAccess.Read, FileShare.Read);
		
		foreach (var layer in layers)
        {
            if (!layer.canLoadLayer)
				continue;
			
			if (layer.isContainer)
			{
				this.SaveLayersToPNGs_imple(prePath + "/" + layer.name, layer.children);
				continue;
			}
			
			var pa = new PsdLayerCommandParser.ControlParser(prePath, layer);
			if (pa.type == PsdLayerCommandParser.ControlType.Script || 
				pa.type == PsdLayerCommandParser.ControlType.Label || 
				pa.type == PsdLayerCommandParser.ControlType.LabelButton)
				continue;
			
			var fileName = pa.fullName;
			if (this.HasUnacceptibleChar(fileName))
			{
				Debug.LogError(fileName + " Contains wrong character '\\ / : * ? \" < > |' not allowed");
				continue;
			}
			
			var filePath = prePath + "/" + fileName + ".png";
			ImageFilePath newImageFilePath = null;
			{
				try
				{
					psdFileStream.Position = 0;
					var br = new BinaryReader(psdFileStream);
					{
						layer.LoadData(br, this.Psd.headerInfo.bpp);
						newImageFilePath = new ImageFilePath(filePath, "pass");
					}
				}
				catch (System.Exception e)
				{
				    Debug.LogError(e.Message);
				}
			}
			this.ImageFilePathes.Add(newImageFilePath);
			//**md5: too slow
//			ImageFilePath lastImageFilePath = null;
//			ImageFilePath newImageFilePath = null;
//			{
//				try
//				{
//					if (this.LastImageFilePathes != null && this.LastImageFilePathes.Count > 0)
//					{
//						lastImageFilePath = this.LastImageFilePathes.Find(t=>t.filePath == filePath);
//					}
//					
//					psdFileStream.Position = 0;
//					var br = new BinaryReader(psdFileStream);
//					{
//						layer.LoadData(br, this.Psd.headerInfo.bpp);
//						var imageMd5 = this.CalcMd5Imple(br.BaseStream);
//						var imageMd5 = "pass";
//						newImageFilePath = new ImageFilePath(filePath, imageMd5);
//					}
//				}
//				catch (System.Exception e)
//				{
//				    Debug.LogError(e.Message);
//				}
//			}
//			this.ImageFilePathes.Add(newImageFilePath);
//			
//			if (lastImageFilePath != null)
//			{
//				if (lastImageFilePath.imageMd5 == newImageFilePath.imageMd5 && File.Exists(filePath))
//					continue;
//				lastImageFilePath.imageMd5 = newImageFilePath.imageMd5;
//			}
//			else
//				this.LastImageFilePathes.Add(newImageFilePath);
			
			if (PsdLayerToNGUI.verbose)
				Debug.Log("Saving '" + newImageFilePath.filePath + "'");
			
            var data = layer.psdLayer.mergeChannels();
			if (data == null)
				continue;
			
			Texture tex = null;
			if (pa.sliceArea != null)
			{
				tex = MakeSlicedSprites(ref data, layer, pa.sliceArea);
			}
			else
			{
				tex = MakeTexture(ref data, layer);
			}
			
			if (tex != null)
			{
				if (!System.IO.Directory.Exists(prePath))
					System.IO.Directory.CreateDirectory(prePath);
				
				System.IO.File.WriteAllBytes(filePath, data);
				AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
				
				Texture2D.DestroyImmediate(tex);
			}
		}
	}
	
	public void SaveLayersToPNGs()
	{
		var psdFilePath = this.PsdFilePath;
		var prePath = psdFilePath.Substring(0, psdFilePath.Length - 4) + "_layers";
		
		if (PsdLayerToNGUI.verbose)
			Debug.Log("Saving layers from '" + psdFilePath + "'");
		
		this.ImageFilePathes = new List<ImageFilePath>();
		if (this.LastImageFilePathes == null)
			this.LastImageFilePathes = new List<ImageFilePath>();
		
		this.SaveLayersToPNGs_imple(prePath, this.Root.children);
	}
	
	private void OnGUI_selection(bool canLoadLayer, List<Layer> layers)
	{
		foreach (var layer in layers)
		{
			if (layer.isContainer)
				this.OnGUI_selection(canLoadLayer, layer.children);
			else
				layer.canLoadLayer = canLoadLayer;
		}
	}
	
	private void OnGUI_toggle(string groupName, List<Layer> layers)
	{
		foreach (var layer in layers)
		{
			if (layer.isContainer)
				this.OnGUI_toggle(layer.name, layer.children);
			else
			{
				GUILayout.BeginHorizontal();
				var preName = groupName + (!string.IsNullOrEmpty(groupName) ? "/" : "");
				layer.canLoadLayer = GUILayout.Toggle(layer.canLoadLayer, "", GUILayout.Width(15));
				GUILayout.Label(preName + layer.name);
				GUILayout.EndHorizontal();
				
			}
		}
	}
	
	public void OnGUI(bool isAddFontToggleVisible, System.Action whenRemove)
	{
		EditorGUILayout.BeginHorizontal();
		
		// remove
		
		EditorGUILayout.LabelField("", GUILayout.Width(5));
		
		if (GUILayout.Button("Remove", GUILayout.MaxWidth(55))){
			if (whenRemove != null)
				whenRemove();
		}
		
		// font adding
		
		if (isAddFontToggleVisible)
		{
			this.IsAddFont = GUILayout.Toggle(this.IsAddFont, "Add Font", GUILayout.Width(70));
		}
		
		// instance
		
		GUILayout.BeginHorizontal();
		{
			this.RootGameObject = EditorGUILayout.ObjectField(
				"", this.RootGameObject, typeof(GameObject), true, GUILayout.MaxWidth(150)) as GameObject;
		}
		
		EditorGUILayout.BeginVertical();
		this.foldout = EditorGUILayout.Foldout(this.foldout, this.PsdFileName);
		if (this.foldout)
		{
			// blank
			
			EditorGUILayout.LabelField("");
			
			// selection
			
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Select All", GUILayout.MaxWidth(100)))
				{
					this.OnGUI_selection(true, this.Root.children);
				}
				if (GUILayout.Button("Select None", GUILayout.MaxWidth(100)))
				{
					this.OnGUI_selection(false, this.Root.children);
				}
			}
			EditorGUILayout.EndHorizontal();
			
			// layers
			
			this.OnGUI_toggle("", this.Root.children);
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
};