using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GBlue
{
	public static class Util
	{
		#region Base
		
		public static string AppLocatedUrl
		{
			get
			{
				var appLocatedUrl = Application.dataPath;
				var i = appLocatedUrl.LastIndexOfAny("/\\".ToCharArray());
				if (i >= 0)
					appLocatedUrl = appLocatedUrl.Substring(0, i);
				return appLocatedUrl;
			}
		}
		
		public static string StoragePath
		{
			get
			{
#if (UNITY_IPHONE || UNITY_ANDROID)
				return Application.persistentDataPath;
#else
				return Util.AppLocatedUrl;
#endif
			}
		}
		
		public static bool IsValidEmail(string email)
		{
			var regExp = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
			return System.Text.RegularExpressions.Regex.Match(email, regExp).Success;
		}
		
		public static void Swap<T>(ref T a, ref T b)
		{
			T c = a;
			a = b;
			b = c;
		}
		
		public static bool Hasflag(int v, int flag)
		{
			return (v & flag) == flag;
		}
		
		public static bool Inrange(float a, float min, float max)
		{
			return a >= min && a<= max;
		}
		
		public static bool Inrange(int a, int min, int max)
		{
			return a >= min && a<= max;
		}
		
		public static bool Contains(Rect rc, Rect target)
		{
			return Util.Contains(rc, target.xMin, target.yMin) || 
				Util.Contains(rc, target.xMax, target.yMax);
		}
		
		public static bool Contains(Rect rc, float x, float y)
		{
			return Util.Inrange(x, rc.xMin, rc.xMax) && 
				Util.Inrange(y, rc.yMin, rc.yMax);
		}
		
		public static int Align(int stand, int size)
		{
			if (size < stand)
				return stand;
			
			var value = 0;
			while (value < size)
				value += stand;
			return value;
		}
		
		private static System.Random random = new System.Random();
		public static int Seed
		{
			set { random = new System.Random(value); }
		}
		
		public static int Rand(int min, int max)
		{
			return random.Next(min, max);
		}
		
		public static int Rand2(int min, int max)
		{
			return random.Next(min, max + 1);
		}
		
		public static void Shuffle<T>(this IList<T> list)
		{
			System.Random rng = new System.Random();
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		
		public static int[] ShuffledArray(int start, int end)
		{
			var arr = new int[end+1 - start];
			for (var i=start; i<=end; ++i)
				arr[i - start] = i;
			Util.Shuffle(arr);
			return arr;
		}
		
		public static float ValueUpdate(float currentValue, float targetValue, float speed)
		{
			var V_0 = targetValue - currentValue;
			currentValue = currentValue + ((V_0 * speed) * Time.deltaTime);
			return currentValue;
		}
		
		// index 0 is "current file name and line" index X is "Xth caller's file name and line"
		public static string Stacktraceinfo(int index)
		{
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
			System.Diagnostics.StackFrame sf = st.GetFrame(1 + index);
	//		return string.Format("{0}(L:{1})", 
	//			sf.GetFileName(), sf.GetFileLineNumber());
			return sf.GetMethod().DeclaringType.FullName;
		}
		
	    public static int HexToInt(string value)
	    {
			var v = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
			return v;
	    }
	
		public static string IntToHex(int value)
	    {
	        var hex = value.ToString("X");
	        if (hex.Length == 1)
	        {
	            hex = "0" + hex;
	        }
	        return hex;
	    }
	
	    public static string FloatToHex(float value)
	    {
	        return Util.IntToHex((int)value);
	    }
	
	    public static Color HexToColor(string value)
	    {
			if (value.Length == 6)
	            value += "ff";
			
	        if (value.Length != 8)
	            return Color.white;
			
	        var rText = value.Substring(0, 2);
	        var gText = value.Substring(2, 2);
	        var bText = value.Substring(4, 2);
	        var aText = value.Substring(6, 2);
			
	        var r = Util.HexToInt(rText) / 255.0f;
	        var g = Util.HexToInt(gText) / 255.0f;
	        var b = Util.HexToInt(bText) / 255.0f;
	        var a = Util.HexToInt(aText) / 255.0f;
	        if (r < 0 || g < 0 || b < 0 || a < 0)
	            return Color.white;
	            
	        return new Color(r, g, b, a);
	    }
	
	    public static string ColorToHex(Color color)
	    {
	        return Util.FloatToHex(color.r * 255.0f)
	             + Util.FloatToHex(color.g * 255.0f)
	             + Util.FloatToHex(color.b * 255.0f)
	             + Util.FloatToHex(color.a * 255.0f);
	    }
		
		#endregion
		
		#region Unity3D Depended
		
		public static Transform Clone(Transform t)
		{
			return Util.Clone(t.gameObject).transform;
		}
		public static GameObject Clone(GameObject go)
		{
			var clone = GameObject.Instantiate(go) as GameObject;
			clone.transform.parent = go.transform.parent;
			clone.transform.localPosition = go.transform.localPosition;
			clone.transform.localScale = go.transform.localScale;
			return clone;
		}
		
		public static GameObject CreatePrefab(string prefabPath)
		{
			return Util.CreatePrefab(prefabPath, (Transform)null);
		}
		public static GameObject CreatePrefab(string prefabPath, MonoBehaviour owner)
		{
			return Util.CreatePrefab(prefabPath, owner.transform);
		}
		public static GameObject CreatePrefab(string prefabPath, GameObject owner)
		{
			return Util.CreatePrefab(prefabPath, owner.transform);
		}
		public static GameObject CreatePrefab(string prefabPath, Transform owner)
		{
			var go = GameObject.Instantiate(Resources.Load(prefabPath)) as GameObject;
			go.transform.parent = owner;
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			return go;
		}
		
		public static GameObject CreateEmptyGameObject()
		{
			return Util.CreateEmptyGameObject(null as GameObject, null);
		}
		public static GameObject CreateEmptyGameObject(Transform owner, string name)
		{
			return Util.CreateEmptyGameObject(owner.gameObject, name);
		}
		public static GameObject CreateEmptyGameObject(GameObject owner, string name)
		{
			var go = new GameObject(name);
			if (owner != null)
			{
				go.transform.parent = owner.transform;
				go.layer = owner.layer;
			}
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			return go;
		}
		
		public static GameObject CreateEmptyGameObject(MonoBehaviour owner)
		{
			return Util.CreateEmptyGameObject(owner.gameObject, "");
		}
		public static GameObject CreateEmptyGameObject(Transform owner)
		{
			return Util.CreateEmptyGameObject(owner.gameObject, "");
		}
		public static GameObject CreateEmptyGameObject(GameObject owner)
		{
			return Util.CreateEmptyGameObject(owner, "");
		}
		
		public static GameObject FindGameObject(MonoBehaviour owner, string name)
		{
			return Util.FindGameObject(owner.gameObject, name, false);
		}
		public static GameObject FindGameObject(Transform owner, string name)
		{
			return Util.FindGameObject(owner.gameObject, name, false);
		}
		public static GameObject FindGameObject(GameObject owner, string name)
		{
			return Util.FindGameObject(owner, name, false);
		}
		
		public static GameObject FindGameObject(MonoBehaviour owner, string name, bool createWhenNotFound)
		{
			return Util.FindGameObject(owner.transform, name, createWhenNotFound);
		}
		public static GameObject FindGameObject(GameObject owner, string name, bool createWhenNotFound)
		{
			return Util.FindGameObject(owner.transform, name, createWhenNotFound);
		}
		public static GameObject FindGameObject(Transform owner, string name, bool createWhenNotFound)
		{
			var arr = name.Split('/');
			for (var k=0; k<arr.Length; ++k)
			{
				var child = Util.FindGameObjectImple(owner, arr[k]);
				if (child != null)
				{
					if (k == (arr.Length - 1))
						return child.gameObject;
					else
						owner = child;
				}
				else
					break;
			}
			if (createWhenNotFound)
				return Util.CreateEmptyGameObject(owner, name);
			return null;
		}
		private static Transform FindGameObjectImple(Transform owner, string name)
		{
			var t = owner;
			for (var i=0; i<t.childCount; ++i)
			{
				var child = t.GetChild(i);
				if (child.name == name)
				{
					return child;
				}
			}
			return null;
		}
		
		public static GameObject FindGameObjectRecursively(string name)
		{
			var roots = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			foreach (var root in roots)
			{
				if (root.transform.parent == null) // real root
				{
					var go = Util.FindGameObjectRecursively(root, name);
					if (go != null)
						return go;
				}
			}
			return null;
		}
		public static GameObject FindGameObjectRecursively(MonoBehaviour owner, string name)
		{
			return Util.FindGameObjectRecursively(owner.transform, name, false);
		}
		public static GameObject FindGameObjectRecursively(GameObject owner, string name)
		{
			return Util.FindGameObjectRecursively(owner.transform, name, false);
		}
		public static GameObject FindGameObjectRecursively(Transform owner, string name)
		{
			return Util.FindGameObjectRecursively(owner, name, false);
		}
		public static GameObject FindGameObjectRecursively(MonoBehaviour owner, string name, bool onlyChildren)
		{
			return Util.FindGameObjectRecursively(owner.transform, name, onlyChildren);
		}
		public static GameObject FindGameObjectRecursively(GameObject owner, string name, bool onlyChildren)
		{
			return Util.FindGameObjectRecursively(owner.transform, name, onlyChildren);
		}
		public static GameObject FindGameObjectRecursively(Transform owner, string name, bool onlyChildren)
		{
			if (!onlyChildren && owner.name == name)
				return owner.gameObject;
			
			for (var i=0; i<owner.childCount; ++i)
			{
				var child = owner.GetChild(i);
				if (child.name == name)
				{
					return child.gameObject;
				}
				else if (child.childCount > 0)
				{
					var go = Util.FindGameObjectRecursively(child.gameObject, name);
					if (go != null)
						return go;
				}
			}
			return null;
		}
		
		public static GameObject FindGameObjectRecursivelyByTag(string tag)
		{
			return GameObject.FindGameObjectWithTag(tag);
		}
		public static GameObject FindGameObjectRecursivelyByTag(Transform owner, string tag)
		{
			return Util.FindGameObjectRecursivelyByTag(owner.gameObject, tag);
		}
		public static GameObject FindGameObjectRecursivelyByTag(GameObject owner, string tag)
		{
			if (owner.transform.tag == tag)
				return owner;
			
			var t = owner.transform;
			for (var i=0; i<t.childCount; ++i)
			{
				var child = t.GetChild(i);
				if (child.tag == tag)
				{
					return child.gameObject;
				}
				else if (child.childCount > 0)
				{
					var go = Util.FindGameObjectRecursivelyByTag(child.gameObject, tag);
					if (go != null)
						return go;
				}
			}
			return null;
		}
		
		// search upward
		public static T ClosestComponent<T>(MonoBehaviour owner) where T : Component
		{
			return Util.ClosestComponent<T>(owner.transform);
		}
		// search upward
		public static T ClosestComponent<T>(GameObject owner) where T : Component
		{
			return Util.ClosestComponent<T>(owner.transform);
		}
		// search upward
		public static T ClosestComponent<T>(Transform owner) where T : Component
		{
			var p = owner;
			while (p != null)
			{
				var comp = p.GetComponent<T>();
				if (comp != null)
					return comp;
				p = p.parent;
			}
			return null;
		}
		
		// find from all GameObject components
		public static T FindComponent<T>() where T : Component
		{
			var go = GameObject.FindObjectOfType(typeof(GameObject)) as GameObject;
			return Util.FindComponent<T>(go.transform.root, false);
		}
		// search downward
		public static T FindComponent<T>(MonoBehaviour owner) where T : Component
		{
			return Util.FindComponent<T>(owner.gameObject, false);
		}
		// search downward
		public static T FindComponent<T>(GameObject owner) where T : Component
		{
			return Util.FindComponent<T>(owner.gameObject, false);
		}
		// search downward
		public static T FindComponent<T>(Transform owner) where T : Component
		{
			return Util.FindComponent<T>(owner.gameObject, false);
		}
		// search downward
		public static T FindComponent<T>(MonoBehaviour owner, bool addWhenNotFound) where T : Component
		{
			return Util.FindComponent<T>(owner.transform, addWhenNotFound);
		}
		// search downward
		public static T FindComponent<T>(GameObject owner, bool addWhenNotFound) where T : Component
		{
			return Util.FindComponent<T>(owner.transform, addWhenNotFound);
		}
		// search downward
		public static T FindComponent<T>(Transform owner, bool addWhenNotFound) where T : Component
		{
			var comp = owner.GetComponent<T>();
			if (comp != null)
				return comp;
			
			var count = owner.childCount;
			for (var i=0; i<count; ++i)
			{
				var child = owner.GetChild(i);
				comp = child.GetComponent<T>();
				if (comp != null)
					return comp;
				else
				{
					comp = Util.FindComponent<T>(child, false);
					if (comp != null)
						return comp;
				}
			}
			if (addWhenNotFound)
				return owner.gameObject.AddComponent<T>();
			return null;
		}
		
		public static T[] FindComponents<T>(MonoBehaviour owner) where T : Component
		{
			return owner.GetComponentsInChildren<T>(true);
		}
		public static T[] FindComponents<T>(Transform owner) where T : Component
		{
			return owner.GetComponentsInChildren<T>(true);
		}
		public static T[] FindComponents<T>(GameObject owner) where T : Component
		{
			return owner.GetComponentsInChildren<T>(true);
		}
		
		public static Vector3 GetGlobalScale(GameObject go)
		{
			return Util.GetGlobalScale(go.transform);
		}
		public static Vector3 GetGlobalScale(Transform t)
		{
			var scale = Vector3.one;
			while (t != null)
			{
				scale.x *= t.localScale.x;
				scale.y *= t.localScale.y;
				scale.z *= t.localScale.z;
				t = t.parent;
			}
			return scale;
		}
		
		public static Rect MergeRect(Rect r1, Rect r2)
		{
			var r = new Rect();
			r.xMin = Mathf.Min(r1.xMin, r2.xMin);
			r.yMin = Mathf.Min(r1.yMin, r2.yMin);
			r.xMax = Mathf.Max(r1.xMax, r2.xMax);
			r.yMax = Mathf.Max(r1.yMax, r2.yMax);
			return r;
		}
		
		public static void SetXYCenterAmongChildren(GameObject go)
		{
			Util.SetXYCenterAmongChildren(go.transform, true);
		}
		public static void SetXYCenterAmongChildren(Transform t)
		{
			Util.SetXYCenterAmongChildren(t, true);
		}
		public static void SetXYCenterAmongChildren(GameObject go, bool roundVal)
		{
			Util.SetXYCenterAmongChildren(go.transform, roundVal);
		}
		public static void SetXYCenterAmongChildren(Transform t, bool roundVal)
		{
			var bounds = NGUIMath.CalculateRelativeWidgetBounds(t);
			var children = new Transform[t.childCount];
			{
				for (var i=0; i<children.Length; ++i)
				{
					var child = t.GetChild(0);
					child.parent = t.parent;
					children[i] = child;
				}
			}
			
			if (roundVal)
			{
				t.localPosition = new Vector3(
					Mathf.RoundToInt(bounds.center.x), Mathf.RoundToInt(bounds.center.y), Mathf.RoundToInt(t.localPosition.z));
			}
			else
			{
				t.localPosition = new Vector3(
					bounds.center.x, bounds.center.y, t.localPosition.z);
			}
			
			foreach (var child in children)
			{
				child.parent = t;
			}
		}
		
		public static bool IsActive(MonoBehaviour mo)
		{
			return Util.IsActive(mo.gameObject);
		}
		public static bool IsActive(Transform t)
		{
			return Util.IsActive(t.gameObject);
		}
		public static bool IsActive(GameObject go)
		{
#if UNITY_3_5
			return go && go.active;
#else
			return go && go.activeInHierarchy;
#endif
		}
		
		public static void SetActiveSelf(MonoBehaviour mo, bool active)
		{
			Util.SetActiveSelf(mo.gameObject, active);
		}
		public static void SetActiveSelf(Transform t, bool active)
		{
			Util.SetActiveSelf(t.gameObject, active);
		}
		public static void SetActiveSelf(GameObject go, bool active)
		{
#if UNITY_3_5
			go.active = active;
#else
			go.SetActive(active);
#endif
		}
		
		public static void SetActive(MonoBehaviour mo, bool active)
		{
			Util.SetActive(mo.gameObject, active);
		}
		public static void SetActive(GameObject go, bool active)
		{
			Util.SetActive(go.transform, active);
		}
		public static void SetActive(Transform t, bool active)
		{
#if UNITY_3_5
			t.gameObject.SetActiveRecursively(active);
#else
			Util.SetActiveSelf(t.gameObject, active);
#endif
		}
		
		public static void SetBoxColliderZPosition(MonoBehaviour target, float z)
		{
			Util.SetBoxColliderZPosition(target.transform, z);
		}
		public static void SetBoxColliderZPosition(GameObject target, float z)
		{
			Util.SetBoxColliderZPosition(target.transform, z);
		}
		public static void SetBoxColliderZPosition(Transform target, float z)
		{
			var box = Util.FindComponent<BoxCollider>(target, true);
			box.center = new Vector3(box.center.x, box.center.y, z);
		}
		
		public static void SetBoxColliderSize(MonoBehaviour target, Transform src)
		{
			Util.SetBoxColliderSize(target.transform, src);
		}
		public static void SetBoxColliderSize(MonoBehaviour target, Vector3 srcSize)
		{
			Util.SetBoxColliderSize(target.transform, srcSize);
		}
		public static void SetBoxColliderSize(GameObject target, Transform src)
		{
			Util.SetBoxColliderSize(target.transform, src);
		}
		public static void SetBoxColliderSize(GameObject target, Vector3 srcSize)
		{
			Util.SetBoxColliderSize(target.transform, srcSize);
		}
		public static void SetBoxColliderSize(Transform target, Transform src)
		{
			Util.SetBoxColliderSize(target, src.localScale);
		}
		public static void SetBoxColliderSize(Transform target, Vector3 srcSize)
		{
			var box = Util.FindComponent<BoxCollider>(target, true);
			box.size = srcSize;
		}
		
		public static void ScaleBoxColliderSize(MonoBehaviour target, Vector3 scale)
		{
			Util.ScaleBoxColliderSize(target.transform, scale);
		}
		public static void ScaleBoxColliderSize(GameObject target, Vector3 scale)
		{
			Util.ScaleBoxColliderSize(target.transform, scale);
		}
		public static void ScaleBoxColliderSize(Transform target, Vector3 scale)
		{
			var box = Util.FindComponent<BoxCollider>(target, true);
			box.size = Vector3.Scale(box.size, scale);
		}
		
		public static float ClampAngle(float angle)
		{
			return Util.ClampAngle(angle, 0, 360);
		}
		public static float ClampAngle(float angle, float min, float max)
		{
		    while (angle < 0f || angle > 360f)
		    {
				if (angle < 0f)
					angle += 360f;
				if (angle > 360f)
					angle -= 360f;
		    }
		    return Mathf.Clamp(angle, min, max);
		}
		
		public static Matrix4x4 GetMVP(ref Matrix4x4 model)
		{
			return Util.GetMVP(Camera.main, ref model);
		}
		public static Matrix4x4 GetMVP(Camera camera, ref Matrix4x4 model)
		{
			var d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
			var M = model;
			var V = camera.worldToCameraMatrix;
			var P = camera.projectionMatrix;
			if (d3d) {
				// Invert Y for rendering to a render texture
				for (int i = 0; i < 4; i++) {
					P[1,i] = -P[1,i];
				}
				// Scale and bias from OpenGL -> D3D depth range
				for (int i = 0; i < 4; i++) {
					P[2,i] = P[2,i]*0.5f + P[3,i]*0.5f;
				}
			}
			return P*V*M;
		}
		
		public static string OnlyNumbers(string text){
			return new System.String(text.Where(System.Char.IsDigit).ToArray());
		}
		
		#endregion
	};
	
	public static class Util4NGUI
	{
		public class DepthSaver
		{
			public static void ChangeDepth(MonoBehaviour mo, int startDepth)
			{
				DepthSaver.ChangeDepth(mo.gameObject, startDepth);
			}
			public static void ChangeDepth(Transform t, int startDepth)
			{
				DepthSaver.ChangeDepth(t.gameObject, startDepth);
			}
			public static void ChangeDepth(GameObject go, int startDepth)
			{
				DepthSaver saver = new DepthSaver();
				saver.Save(go);
				saver.Load(startDepth, false);
			}
				
			#region Child Classes
			
			private class State
			{
				public UIWidget w;
				public int depth;
				
				public State(UIWidget w)
				{
					this.w = w;
					this.depth = w.depth;
				}
			};
			
			private List<State> states = new List<State>();
			
			#endregion
			
			public void Save(MonoBehaviour mo)
			{
				this.Save(mo.gameObject);
			}
			public void Save(Transform t)
			{
				this.Save(t.gameObject);
			}
			public void Save(GameObject go)
			{
				this.states.Clear();
				
				var widget = go.GetComponent<UIWidget>();
				if (widget != null)
				{
					this.states.Add(new State(widget));
				}
				
				var widgets = new List<UIWidget>(go.GetComponentsInChildren<UIWidget>(true));
				if (widgets.Count > 0)
				{
					widgets.Sort((a, b)=>a.depth.CompareTo(b.depth));
					
					foreach (var widget2 in widgets)
					{
						this.states.Add(new State(widget2));
					}
				}
			}
			
			public void Load()
			{
				this.Load(0);
			}
			
			public void Load(int additionalDepth)
			{
				this.Load(additionalDepth, true);
			}
			public void Load(int startDepth, bool addToOldDepth)
			{
				foreach (var s in states)
				{
					if (addToOldDepth)
						s.w.depth = s.depth + startDepth;
					else
						s.w.depth = startDepth++;
				}
			}
		};
		
		public static void SetColor(MonoBehaviour target, Color color)
		{
			Util4NGUI.SetColor(target.transform, color);
		}
		public static void SetColor(GameObject target, Color color)
		{
			Util4NGUI.SetColor(target.transform, color);
		}
		public static void SetColor(Transform target, Color color)
		{
			var checkbox = target.GetComponent<UIToggle>();
			var btn = target.GetComponent<UIButton>();
			if (checkbox != null)
			{
				checkbox.activeSprite.color = color;
				var tweenColor = Util.FindComponent<TweenColor>(checkbox.activeSprite.gameObject);
				if (tweenColor != null)
					tweenColor.to = color;
			}
			if (btn != null)
			{
				btn.defaultColor = color;
				//btn.UpdateColor(btn.isEnabled, true);
				var tweenColor = Util.FindComponent<TweenColor>(btn.gameObject);
				if (tweenColor != null)
					tweenColor.to = color;
			}
			else
				target.GetComponent<UIWidget>().color = color;
		}
		
		public static float GetButtonOpacity(UIButton button)
		{
			return button.defaultColor.a;
		}
		public static void SetButtonOpacity(UIButton button, float opacity)
		{
			button.defaultColor = new Color(
				button.defaultColor.r,
				button.defaultColor.g,
				button.defaultColor.b, opacity);
			
			//button.UpdateColor(button.isEnabled, true);
			if (!Util.IsActive(button.gameObject) && opacity > 0)
				Util.SetActive(button.gameObject, true);
			else if (opacity == 0)
				Util.SetActive(button.gameObject, false);
			
			var box = button.transform.parent.GetComponent<BoxCollider>();
			if (box != null)
				box.enabled = opacity >= 1;
		}
		
		public static float GetWidgetOpacity(UIWidget w)
		{
			return w.alpha;
		}
		public static void SetWidgetOpacity(UIWidget w, float opacity)
		{
			w.alpha = opacity > 1 ? 1 : opacity;
			if (!Util.IsActive(w.gameObject) && opacity > 0)
				Util.SetActive(w.gameObject, true);
			else if (opacity == 0)
				Util.SetActive(w.gameObject, false);
			
			var box = w.transform.parent.GetComponent<BoxCollider>();
			if (box != null)
				box.enabled = opacity >= 1;
		}
		
		public static float GetWidgetOpacity(UIWidget[] widgets)
		{
			return widgets[0].alpha;
		}
		public static void SetWidgetOpacity(UIWidget[] widgets, float opacity)
		{
			foreach (var w in widgets)
			{
				w.alpha = opacity > 1 ? 1 : opacity;
				if (!Util.IsActive(w.gameObject) && opacity > 0)
					Util.SetActive(w.gameObject, true);
				else if (opacity == 0)
					Util.SetActive(w.gameObject, false);
				
				var box = w.transform.parent.GetComponent<BoxCollider>();
				if (box != null)
					box.enabled = opacity >= 1;
			}
		}
		
		public static string ParseTextOnly(string text)
		{
			var pa = new TextParser(text);
			var ret = "";
			while (!pa.isEndOfParsing)
			{
				if (pa.ch == '[')
				{
					pa.block('[', ']');
				}
				else
				{
					ret += pa.ch;
					pa.pos++;
				}
			}
			return ret;
		}
	};
}