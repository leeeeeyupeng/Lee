using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Xml;

public static class PsdLayerPrefs
{
	public static bool verbose = true;
	private static string filePath;
	private static XmlDocument xml = new XmlDocument();
	private static XmlNode root;
		
	static PsdLayerPrefs()
	{
		var pathes = Directory.GetFiles(Directory.GetCurrentDirectory(), "PsdLayerPrefs.cs", SearchOption.AllDirectories);
		PsdLayerPrefs.filePath = Path.GetDirectoryName(pathes[0]) + "/PsdLayerPrefs.xml";
		if (File.Exists(PsdLayerPrefs.filePath))
		{
			PsdLayerPrefs.xml.Load(PsdLayerPrefs.filePath);
		}
		else
		{
			var node = PsdLayerPrefs.xml.CreateNode(XmlNodeType.Element, "Keys", null);
			PsdLayerPrefs.xml.AppendChild(node);
			PsdLayerPrefs.xml.Save(PsdLayerPrefs.filePath);
		}
		PsdLayerPrefs.root = PsdLayerPrefs.xml.DocumentElement;
	}
	
	public static void Load()
	{
		PsdLayerPrefs.xml.Load(PsdLayerPrefs.filePath);
	}
	
	public static void Save()
	{
		PsdLayerPrefs.xml.Save(PsdLayerPrefs.filePath);
	}
	
	public static bool HasKey(string key)
	{
		try
		{
			var node = PsdLayerPrefs.root.SelectSingleNode("//Key[@Name='" + key + "']");
			return node != null;
		}
		catch (Exception e)
		{
			if (PsdLayerPrefs.verbose)
				Debug.LogError(e.Message);
			return false;
		}
	}
	
	public static bool DeleteKey(string key)
	{
		try
		{
			var node = PsdLayerPrefs.root.SelectSingleNode("//Key[@Name='" + key + "']");
			PsdLayerPrefs.root.RemoveChild(node);
			return node != null;
		}
		catch (Exception e)
		{
			if (PsdLayerPrefs.verbose)
				Debug.LogError(e.Message);
			return false;
		}
	}
	
	#region Default SetTypes
	
	public static bool SetInt(string key, int v)
	{
		return PsdLayerPrefs.SetString(key, v.ToString());
	}
	
	public static bool SetFloat(string key, float v)
	{
		return PsdLayerPrefs.SetString(key, v.ToString());
	}
	
	public static bool SetString(string key, string v)
	{
		try
		{
			var child = PsdLayerPrefs.xml.CreateNode(XmlNodeType.Element, "Key", null);
			var name = PsdLayerPrefs.xml.CreateAttribute("Name");
			var val = PsdLayerPrefs.xml.CreateAttribute("Value");
			name.Value = key;
			val.Value = v;
			child.Attributes.Append(name);
			child.Attributes.Append(val);
			
			var node = PsdLayerPrefs.root.SelectSingleNode("//Key[@Name='" + key + "']");
			if (node != null)
				node = PsdLayerPrefs.root.ReplaceChild(child, node);
			else
				node = PsdLayerPrefs.root.AppendChild(child);
			
			PsdLayerPrefs.Save();
		}
		catch (Exception e)
		{
			if (PsdLayerPrefs.verbose)
				Debug.LogError(e.Message);
			return false;
		}
		return true;
	}
	
	#endregion
	
	#region Default GetTypes
	
	public static int GetInt(string key)
	{
		return PsdLayerPrefs.GetInt(key, 0);
	}
	
	public static float GetFloat(string key)
	{
		return PsdLayerPrefs.GetFloat(key, 0);
	}
	
	public static string GetString(string key)
	{
		return PsdLayerPrefs.GetString(key, "");
	}
	
	public static int GetInt(string key, int defaultValue)
	{
		var ret = PsdLayerPrefs.GetString(key);
		return !string.IsNullOrEmpty(ret) ? int.Parse(ret) : defaultValue;
	}
	
	public static float GetFloat(string key, float defaultValue)
	{
		var ret = PsdLayerPrefs.GetString(key);
		return !string.IsNullOrEmpty(ret) ? float.Parse(ret) : defaultValue;
	}
	
	public static string GetString(string key, string defaultValue)
	{
		try
		{
			var node = PsdLayerPrefs.root.SelectSingleNode("//Key[@Name='" + key + "']");
			return node != null ? node.Attributes["Value"].Value : defaultValue;
		}
		catch (Exception e)
		{
			if (PsdLayerPrefs.verbose)
				Debug.LogError(e.Message);
			return defaultValue;
		}
	}
	
	#endregion

	#region String Array

	public static bool SetStringArray(string key, char separator, params string[] strings)
	{
		try
		{
			if (strings.Length == 0)
				PsdLayerPrefs.SetString(key, "");
			else
				PsdLayerPrefs.SetString(key, string.Join(separator.ToString(), strings));
		}
		catch (Exception e)
		{
			if (PsdLayerPrefs.verbose)
				Debug.LogError(e.Message);
			return false;
		}
		return true;
	}

	public static bool SetStringArray(string key, params string[] strings)
	{
		if (!PsdLayerPrefs.SetStringArray(key, "\n"[0], strings))
			return false;
		return true;
	}

	public static string[] GetStringArray(string key, char separator)
	{
		if (PsdLayerPrefs.HasKey(key))
			return PsdLayerPrefs.GetString(key).Split(separator);
		return new string[0];
	}

	public static string[] GetStringArray(string key)
	{
		if (PsdLayerPrefs.HasKey(key))
			return PsdLayerPrefs.GetString(key).Split("\n"[0]);
		return new string[0];
	}

	public static string[] GetStringArray(string key, char separator, string defaultValue, int defaultSize)
	{
		if (PsdLayerPrefs.HasKey(key))
			return PsdLayerPrefs.GetString(key).Split(separator);
		
		var strings = new string[defaultSize];
		for (int i = 0; i < defaultSize; i++)
			strings[i] = defaultValue;
		return strings;
	}

	public static string[] GetStringArray(string key, string defaultValue, int defaultSize)
	{
		return PsdLayerPrefs.GetStringArray(key, "\n"[0], defaultValue, defaultSize);
	}

	#endregion
};