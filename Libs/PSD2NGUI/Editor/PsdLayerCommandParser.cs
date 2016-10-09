using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PsdLayerRect
{
	#region static members
	
	public static PsdLayerRect zero
	{
		get { return new PsdLayerRect(); }
	}
	
	#endregion
	
	#region members
	
	public float left = 0;
	public float top = 0;
	public float right = 0;
	public float bottom = 0;
	public float width
	{
		get { return this.right - this.left; }
	}
	public float height
	{
		get { return this.bottom - this.top; }
	}
	
	public PsdLayerRect()
	{
	}
	public PsdLayerRect(float l, float t, float w, float h)
	{
		this.left = l;
		this.top = t;
		this.right = l + w;
		this.bottom = t + h;
	}
	
	#endregion
};

public class PsdLayerCommandParser
{
	// ControlType
	
	public enum ControlType
	{
		Script,
		
		Container,
		Panel,
		
		Sprite,
		SpriteFont,
		Label,
		LabelButton,
		Input,
		Password,
		Button,
		Toggle,
		ComboBox,
		VScrollBar,
		HScrollBar,
		ScrollView,
		VirtualView,
		Texture
	};
	
	// ControlParser
	
	public class ControlParser
	{
		public enum Align
		{
			TopLeft,
			Top,
			TopRight,
			Left,
			Center,
			Right,
			BottomLeft,
			Bottom,
			BottomRight,
		};
		
		public string srcFileDirPath = "";
		public string srcFilePath{
			get{ return this.srcFileDirPath + '/' + this.fullName; }
		}
		public string originalName = "";
		public string fullName = "";
		public string name = "";
		public string command = "";
		public PsdLayerRect area;
		
		public ControlType type = ControlType.Sprite;
		public bool hasBoxCollider = false;
		public PsdLayerRect sliceArea;
		public Color color = Color.white;
		public string text;
		public int fontSize;
		public bool bold;
		public bool shadow;
		public Align align;
		
		public ControlParser()
		{
			this.type = ControlType.Container;
		}
		public ControlParser(string srcFileDirPath, PsdLayerExtractor.Layer layer)
		{
			var errorPreMsg = "Parse error at '" +srcFileDirPath+ "/" +layer.name+ "'.";
			var name_cmd_attrs = layer.name.Split('@');
			
			this.srcFileDirPath = srcFileDirPath;
			this.originalName = layer.name;
			this.name = name_cmd_attrs[0];
			{
				for (var i=1; i<name_cmd_attrs.Length; ++i)
				{
					name_cmd_attrs[i] = name_cmd_attrs[i].Trim().ToLower();
				}
			}
			
			this.ParseCommandType(name_cmd_attrs, layer, errorPreMsg);
			if (this.type == ControlType.Script)
			{
				this.text = string.IsNullOrEmpty(layer.text) ? "" : layer.text.Trim();
			}
			else
			{
				this.fullName = this.name + 
					(string.IsNullOrEmpty(this.command) ? "" : '@' + this.command);
				
				var comment = this.ParseComment(name_cmd_attrs);
				if (!string.IsNullOrEmpty(comment))
					this.name = this.name + "(" + comment + ")";
				
				this.area = new PsdLayerRect(
					layer.psdLayer.area.left, 
					layer.psdLayer.area.top, 
					layer.psdLayer.area.width, 
					layer.psdLayer.area.height);
				
				this.hasBoxCollider = this.ParseCollider(name_cmd_attrs) == "box";
				this.sliceArea = this.ParseSliceArea(name_cmd_attrs);
				this.color = this.ParseColor(name_cmd_attrs);
				if (!string.IsNullOrEmpty(layer.text))
				{
					this.text = layer.text.Trim();
					var arr = this.text.Split('\n');
					this.fontSize = Mathf.FloorToInt(this.area.height / arr.Length * 0.92f);
				}
				else
					this.text = this.ParseText(name_cmd_attrs);
				
				this.bold = this.IsBold(name_cmd_attrs);
				this.shadow = this.IsShadow(name_cmd_attrs);
				this.align = this.ParseAlign(name_cmd_attrs);
			}
		}
		
		private Color StringToColor(string clr)
		{
			switch (clr)
			{
			case "white": return Color.white;
			case "black": return Color.black;
			case "blue": return Color.blue;
			case "green": return Color.green;
			case "red": return Color.red;
			case "cyan": return Color.cyan;
			case "gray": return Color.gray;
			case "magenta": return Color.magenta;
			case "yellow": return Color.yellow;
			default:
				if (!string.IsNullOrEmpty(clr) && clr.Length >= 6)
				{
					var r = "";
					var g = "";
					var b = "";
					var ch = clr[0];
					
					var k = 0;
					if (ch == '#')
						k++;
						
					r += clr[k++];
					r += clr[k++];
					g += clr[k++];
					g += clr[k++];
					b += clr[k++];
					b += clr[k++];
					
					var rr = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
					var gg = int.Parse(g, System.Globalization.NumberStyles.HexNumber);
					var bb = int.Parse(b, System.Globalization.NumberStyles.HexNumber);
					return new Color(rr / 255f, gg / 255f, bb / 255f);
				}
				break;
			}
			return Color.white;
		}
		
		private bool IsBold(string[] name_cmd_attrs)
		{
			var s = this.ParseCommand("bold", name_cmd_attrs);
			return s == "true" || s == "1";
		}
		
		private bool IsShadow(string[] name_cmd_attrs)
		{
			var s = this.ParseCommand("shadow", name_cmd_attrs);
			return s == "true" || s == "1";
		}
		
		private string ParseComment(string[] name_cmd_attrs)
		{
			return this.ParseCommand("comment", name_cmd_attrs);
		}
		
		private string ParseText(string[] name_cmd_attrs)
		{
			return this.ParseCommand("text", name_cmd_attrs);
		}
		
		private Align ParseAlign(string[] name_cmd_attrs)
		{
			var align = this.ParseCommand("align", name_cmd_attrs);
			switch (align)
			{
			case "topleft": return Align.TopLeft;
			case "top": return Align.Top;
			case "topright": return Align.TopRight;
			case "middleleft": 
			case "left": return Align.Left;
			case "middle": 
			case "center": return Align.Center;
			case "middleright": 
			case "right": return Align.Right;
			case "bottomleft": return Align.BottomLeft;
			case "bottom": return Align.Bottom;
			case "bottomright": return Align.BottomRight;
			}
			return Align.Center;
		}
		
		private Color ParseColor(string[] name_cmd_attrs)
		{
			return this.StringToColor(this.ParseCommand("color", name_cmd_attrs));
		}
		
		private string ParseCollider(string[] name_cmd_attrs)
		{
			return this.ParseCommand("collider", name_cmd_attrs);
		}
		
		private int ParseSliceValue(string v, bool vertical)
		{
			var isPercentage = v.Length > 0 ? (v[v.Length-1] == '%') : false;
			if (isPercentage)
			{
				v = v.Remove(v.Length-1).Trim();
				var per = float.Parse(v);
				if (per > 50)
					per = 50;
				
				if (vertical)
					return (int)((float)this.area.height * (per / 100f));
				else
					return (int)((float)this.area.width * (per / 100f));
			}
			else
			{
				var ret = int.Parse(v);
				return ret > 2 ? ret : 2;
			}
		}
		
		private PsdLayerRect ParseSliceArea(string[] name_cmd_attrs)
		{
			var area = this.ParseCommand("slice", name_cmd_attrs);
			if (!string.IsNullOrEmpty(area))
			{
				var values = area.Split('x');
				var rc = new PsdLayerRect();
				{
					rc.left = this.ParseSliceValue(values[0], false);
					rc.top = values.Length < 2 ? rc.left : this.ParseSliceValue(values[1], true);
					rc.right = values.Length < 3 ? rc.left : this.ParseSliceValue(values[2], false);
					rc.bottom = values.Length < 4 ? rc.top : this.ParseSliceValue(values[3], true);
				}
				return rc;
			}
			return null;
		}
		
		private string ParseCommand(string command, string[] name_cmd_attrs)
		{
			return this.ParseCommand(command, name_cmd_attrs, true);
		}
		private string ParseCommand(string command, string[] name_cmd_attrs, bool removeCommandName)
		{
			var reg = new Regex(@"\s*" + command + @"\s*=");
			for (var n=1; n<name_cmd_attrs.Length; ++n)
			{
				if (reg.IsMatch(name_cmd_attrs[n]))
				{
					if (removeCommandName)
						return reg.Replace(name_cmd_attrs[n], "").Trim();
					else
						return name_cmd_attrs[n];
				}
			}
			return null;
		}
		
		public void ParseCommandType(string[] name_cmd_attrs, 
			PsdLayerExtractor.Layer layer, string errorPreMsg)
		{
			this.command = "";
			this.type = ControlType.Sprite;
			
			for (var n=1; n<name_cmd_attrs.Length; ++n)
			{
				this.command = name_cmd_attrs[n];
				
				if (this.command == "script")
					this.type = ControlType.Script;
				
				else if (this.command == "panel")
					this.type = ControlType.Panel;
				
				else if (this.command == "scrollview" || this.command.StartsWith("scrollview."))
					this.type = ControlType.ScrollView;
					
				else if (this.command == "virtualview" || this.command.StartsWith("virtualview."))
					this.type = ControlType.VirtualView;
					
				else if (this.command == "button" || this.command.StartsWith("button.")){
					if (layer.isTextLayer)
						this.type = ControlType.LabelButton;
					else
						this.type = ControlType.Button;
				}
					
				else if (this.command == "checkbox" || this.command.StartsWith("checkbox.") || 
					this.command == "toggle" || this.command.StartsWith("toggle."))
					this.type = ControlType.Toggle;
					
				else if (this.command == "combobox" || this.command.StartsWith("combobox."))
					this.type = ControlType.ComboBox;
					
				else if (this.command == "input" || this.command.StartsWith("input.") ||
					this.command == "editbox" || this.command.StartsWith("editbox."))
					this.type = ControlType.Input;
					
				else if (this.command == "password" || this.command.StartsWith("password"))
					this.type = ControlType.Password;
				
				else if (this.command == "vscrollbar" || this.command.StartsWith("vscrollbar."))
					this.type = ControlType.VScrollBar;
					
				else if (this.command == "hscrollbar" || this.command.StartsWith("hscrollbar."))
					this.type = ControlType.HScrollBar;
					
				else if (this.command == "spritefont")
					this.type = ControlType.SpriteFont;
				
				else if (this.command == "texture")
					this.type = ControlType.Texture;
				
				if (this.type != ControlType.Sprite)
					return; // type setted
			}
			
			if (layer.isContainer)
			{
				this.type = ControlType.Container;
			}
			else if (layer.isTextLayer)
			{
				this.type = ControlType.Label;
			}
			else{
				if (!string.IsNullOrEmpty(this.command) &&
					!this.command.StartsWith("comment") &&
					!this.command.StartsWith("color") &&
					!this.command.StartsWith("text") &&
					!this.command.StartsWith("bold") &&
					!this.command.StartsWith("shadow") &&
					!this.command.StartsWith("collider") &&
					!this.command.StartsWith("slice") &&
					!this.command.StartsWith("align") &&
					!this.command.StartsWith("ignore"))
				{
					Debug.LogError(errorPreMsg + "'" + this.command + "'" + " is wrong command or attribute");
				}
				this.command = "sprite";
				this.type = ControlType.Sprite;
			}
		}
	} ;
	
	// Control
	
	public class Control
	{
		public ControlParser pa;
		public List<Control> sources = new List<Control>();
		public List<Control> children = new List<Control>();
		
		public ControlType type{
			get{ return this.pa.type; }
		}
		public string fullName{
			get{ return this.pa.fullName; }
		}
		public string name{
			get{ return this.pa.name; }
		}
		public string command{
			get{ return this.pa.command; }
		}
		public PsdLayerRect area{
			get{ return this.pa.area; }
		}
		public PsdLayerRect sliceArea{
			get{ return this.pa.sliceArea; }
		}
		public string text{
			get{ return this.pa.text; }
		}
		public int fontSize{
			get{ return this.pa.fontSize; }
		}
		public bool bold{
			get{ return this.pa.bold; }
		}
		public bool shadow{
			get{ return this.pa.shadow; }
		}
		public ControlParser.Align align{
			get{ return this.pa.align; }
		}
		public Color color{
			get{ return this.pa.color; }
		}
		public bool hasBoxCollider{
			get{ return this.pa.hasBoxCollider; }
		}
		
		public Control()
		{
			this.pa = new ControlParser();
		}
		
		public Control(ControlParser pa)
		{
			this.pa = pa;
		}
	} ;
	
	// members
	
	public Control root = new Control();
	
	private Control ParseControl(ref int i, ref List<PsdLayerExtractor.Layer> layers, 
		ControlParser pa)
	{
		var layer = layers[i];
		if (!layer.canLoadLayer)
			return null;
		
		var control = null as Control;
		var list = new List<Control>();
		for (; i<layers.Count; ++i)
		{
			layer = layers[i];
			if (!layer.canLoadLayer)
				continue;
			
			pa = new ControlParser(pa.srcFileDirPath, layer);
			
			if (list.Count > 0 && list[0].name != pa.name)
			{
				i--;
				break;
			}
			
			var source = null as Control;
			if (layer.isContainer)
			{
				source = new Control(pa);
				this.ParseImple(source, pa.srcFileDirPath, layer.children);
			}
			else
			{
				source = new Control(pa);
			}
			
			if (list.Find(s => s.fullName == pa.fullName) != null)
			{
				i--;
				break;
			}
			list.Add(source);
		}
		
		if (list.Count > 0)
		{
			list.Sort((a, b) => string.Compare(a.command, b.command));
			control = list[0];
			control.sources = list;
		}
		return control;
	}
	
	private void ParseImple(Control container, string srcFileDirPath, List<PsdLayerExtractor.Layer> layers)
	{
		for ( var i=0; i<layers.Count; ++i)
		{
			var layer = layers[i];
			if (!layer.canLoadLayer)
				continue;
			
			var pa = new ControlParser(srcFileDirPath, layer);
			var control = this.ParseControl(ref i, ref layers, pa);
			if (control != null)
				container.children.Add(control);
		}
	}
	
	public void Parse(string srcFileDirPath, List<PsdLayerExtractor.Layer> layers)
	{
		this.ParseImple(this.root, srcFileDirPath, layers);
	}
};