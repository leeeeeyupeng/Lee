using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PsdLayerSpriteFont : MonoBehaviour {
	
	#region Child Classes
	
	public class Letter{
		private char letter;
		private UISprite sprite;
		
		public GameObject gameObject{
			get{ return this.sprite.gameObject; }
		}
		public Transform transform{
			get{ return this.sprite.transform; }
		}
		public Vector2 Size{
			get{ return this.sprite.localSize; }
		}
		
		public Letter(char letter, UISprite sprite){
			this.letter = letter;
			this.sprite = sprite;
			GBlue.Util.SetActive(sprite.transform, false);
		}
		
		public void Update(Letter newLetter){
			this.letter = newLetter.letter;
			this.sprite.name = newLetter.letter.ToString();
			this.sprite.spriteName = newLetter.sprite.spriteName;
			this.sprite.MakePixelPerfect();
			GBlue.Util.SetActive(this.gameObject, true);
		}
		
		public Letter Clone(){
			var go = GameObject.Instantiate(this.sprite.gameObject) as GameObject;
			go.transform.parent = this.transform.parent;
			go.transform.localPosition = this.transform.localPosition;
			go.transform.localScale = this.transform.localScale;
			go.transform.localRotation = Quaternion.identity;
			return new Letter(this.letter, go.GetComponent<UISprite>());
		}
		
		public void Destroy(){
			GameObject.DestroyImmediate(this.gameObject);
		}
	};
	
	#endregion
	
	public enum Align
	{
		TopLeft,
		Top,
		TopRight,
		
		MiddleLeft,
		Middle,
		MiddleRight,
		
		BottomLeft,
		Bottom,
		BottomRight
	};
	
	public Align align = Align.MiddleRight;
	public Vector2 letterSize;
	public string usingLetters = "";
	
	public string text = "";
	private string lastText = "";
	
	private Hashtable letterSources;
	private List<Letter> letters;

	void Start () {
		if (this.letters != null)
		{
			for (var k=0; k<this.letters.Count; ++k)
				this.letters[k].Destroy();
			this.letters.Clear();
		}
		
		var sprites = new List<UISprite>();
		sprites.AddRange(this.gameObject.GetComponentsInChildren<UISprite>(true));
		sprites.Sort(delegate(UISprite a, UISprite b){
			return string.Compare(a.name, b.name);
		});
		
		this.letterSources = new Hashtable();
		this.letters = new List<Letter>();
		
		// make letter info
		
		if (string.IsNullOrEmpty(this.usingLetters)){
			this.usingLetters = "";
			foreach (var sprite in sprites){
				var ch = sprite.name[sprite.name.Length - 1];
				this.usingLetters += ch;
			}
		}
		
		var i = 0;
		var firstTime = this.letterSize == Vector2.zero;
		foreach (var sprite in sprites)
		{
			if (i < this.usingLetters.Length)
			{
				var ch = this.usingLetters[i++];
				this.letterSources.Add(ch, new Letter(ch, sprite));
				if (firstTime)
					this.letterSize = Vector2.Max(this.letterSize, sprite.localSize);
			}
			GBlue.Util.SetActive(sprite.transform, false);
		}
	}
	
	void OnEnable () {
#if UNITY_EDITOR
		if (!Application.isPlaying)
			this.Start();
#endif
	}
	
	void Update () {
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		if (this.lastText != this.text)
		{
			this.lastText = this.text;
			if (string.IsNullOrEmpty(this.lastText))
				return;
			
			var firstSource = this.letterSources[this.usingLetters[0]] as Letter;
			var diff = this.lastText.Length - this.letters.Count;
			if (diff > 0)
			{
				while (diff-- > 0)
					this.letters.Add(firstSource.Clone());
			}
			else if (diff < 0)
			{
				while (diff++ < 0)
				{
					this.letters[this.letters.Count-1].Destroy();
					this.letters.RemoveAt(this.letters.Count-1);
				}
			}
			
			var i = 0;
			var pos = firstSource.transform.localPosition;
			if (this.align == Align.TopRight || 
				this.align == Align.MiddleRight || 
				this.align == Align.BottomRight)
			{
				pos.x -= this.letterSize.x * (this.text.Length - 1);
			}
			else if (this.align == Align.Top || 
				this.align == Align.Middle || 
				this.align == Align.Bottom)
			{
				pos.x -= this.letterSize.x * (this.text.Length - 1) * 0.5f;
			}
			
			foreach (var letter in this.letters)
			{
				var source = this.letterSources[this.text[i++]] as Letter;
				var yGap = this.letterSize.y - source.Size.y;
				yGap *= 0.5f;
				
				letter.transform.localPosition = pos;
				letter.Update(source);
				
				if (this.align == Align.TopLeft || 
					this.align == Align.Top || 
					this.align == Align.TopRight)
				{
					letter.transform.localPosition += new Vector3(0, yGap, 0);
				}
				else if (this.align == Align.BottomLeft || 
					this.align == Align.Bottom || 
					this.align == Align.BottomRight)
				{
					letter.transform.localPosition -= new Vector3(0, yGap, 0);
				}
				
				pos.x += this.letterSize.x;
			}
		}
	}
}
