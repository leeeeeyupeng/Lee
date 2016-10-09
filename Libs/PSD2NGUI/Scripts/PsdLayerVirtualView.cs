using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GBlue;

// enum PsdLayerVirtualViewScrollDirection

public enum PsdLayerVirtualViewScrollDirection
{
	None = 0,
	Vertical = 1,
	Horizontal = 2,
	Free = Vertical | Horizontal,
};

// enum PsdLayerVirtualViewScrollEffect

public enum PsdLayerVirtualViewScrollEffect
{
	None,
	Momentum,
	MomentumAndSpring,
	Magnet
};

// class PsdLayerVirtualViewItem

public class PsdLayerVirtualViewItem
{
	internal virtual int Compare(PsdLayerVirtualViewItem b)
	{
		return 0;
	}

	internal virtual void Reset() { }

	internal virtual void Update() { }
};

// class PsdLayerVirtualViewSlot

public class PsdLayerVirtualViewSlot
{
	public Vector2 Pos
	{
		get { return this.transform.localPosition; }
		internal set { this.transform.localPosition = value; }
	}

	public bool Visibled
	{
		get { return Util.IsActive(this.transform); }
		set
		{
			if (Util.IsActive(this.transform) != value)
				Util.SetActive(this.transform, value);
		}
	}

	public PsdLayerVirtualViewItem Item
	{
		get; internal set;
	}

	public GameObject gameObject
	{
		get; internal set;
	}

	public Transform transform
	{
		get; internal set;
	}

	internal virtual void Init(GameObject go)
	{
		this.gameObject = go;
		this.transform = go.transform;
		this.Visibled = false;
	}

	internal virtual void Reset() { }

	internal virtual void Update(PsdLayerVirtualViewItem item, int slotIndex, int itemIndex) { }
};

// class PsdLayerVirtualView

public sealed class PsdLayerVirtualView : MonoBehaviourEx
{
	#region Callback

	public System.Action whenSpringAnimationFinished;
	public System.Action whenMagnetAnimationFinished;
	public System.Action whenSlideAnimationFinished;

	#endregion

	#region Child Classes

	class Transform2D
	{
		private Vector2 pos;

		public Transform2D(Transform target)
		{
			var bounds = NGUIMath.CalculateRelativeWidgetBounds(target);
            var rc = new RectEx(
                bounds.center.x - bounds.extents.x,
                bounds.center.y - bounds.extents.y,
                bounds.size.x, bounds.size.y);
			this.Size = new Vector2(rc.W, rc.H);
			
			var s = this.Size;
			var p = target.localPosition;
			p.x -= s.x * 0.5f;
			p.y += s.y * 0.5f;
			this.Pos = p;
		}

		public Vector2 Pos
		{
			get; private set;
		}

		public Vector2 Size
		{
			get; private set;
		}

		public Rect Area
		{
			get
			{
				var p = this.Pos;
				var s = this.Size;
				return new Rect(p.x, p.y, s.x, s.y);
			}
		}
	};

	class ItemPositionMaker
	{
		private PsdLayerVirtualView view;

		public ItemPositionMaker(PsdLayerVirtualView view)
		{
			this.view = view;
			this.view.currentColIndex = view.itemStartIndex % view.colCount;
			this.view.currentRowIndex = view.itemStartIndex / view.colCount;
		}

		public Vector2 NextPos
		{
			get
			{
				var w = this.view.ItemSize.x;
				var h = this.view.ItemSize.y;
				var pos = new Vector2(
					w * this.view.currentColIndex++,
					-h * this.view.currentRowIndex);
				
				if (this.view.currentColIndex >= this.view.colCount)
				{
					this.view.currentColIndex = 0;
					if (++this.view.currentRowIndex >= this.view.rowCount)
						this.view.rowCount++;
				}
				return this.view.ItemStartPos + pos + new Vector2(
					-this.view.scrolledPos.x, this.view.scrolledPos.y
				);
			}
		}
	};

	#endregion

	#region Properties

	public Transform bg;
	public RectOffset bgPadding;
	private Transform2D bg2d;

	public Transform item;
	public RectOffset itemMargin;
	private Transform2D item2d;

	private Vector2 scrolledPos;
	private int itemStartIndexOld = 0;
	private int itemStartIndex = 0;
	private List<PsdLayerVirtualViewItem> items;
	private List<PsdLayerVirtualViewSlot> slots;

	private int rowCount = 1;
	private int colCount = 1;

	private int currentRowIndex;
	private int currentColIndex;
	private int actualRowCount;
	private int actualColCount;

	public bool circulation;
	public PsdLayerVirtualViewScrollDirection scrollDirection = PsdLayerVirtualViewScrollDirection.Vertical;
	public PsdLayerVirtualViewScrollEffect scrollEffect = PsdLayerVirtualViewScrollEffect.MomentumAndSpring;

	public float momentumInertiaDuration = 0.5f;
	public float momentumLimit = 100;
	private float lastMovedTime = 0f;
	private float lastVelocity = 0f;
	private float addtionalInertiaDuration = 0f;

	public float magnetSensitive = 0.1f;
	public float magnetAnimationTime = .3f;
	
	private iTweenSimplePlayer tweener;
	private UIPanel uipanel;
	
	public bool IsInited
	{
		get { return this.slots.Count > 0; }
	}

	public Vector2 ViewSize
	{
		get
		{
			if (this.bg2d == null)
				this.bg2d = new Transform2D(this.bg);

			return this.bg2d.Size - new Vector2(
				this.bgPadding.horizontal,
				this.bgPadding.vertical);
		}
	}

	public Vector2 ItemSize
	{
		get
		{
			return this.item2d.Size + new Vector2(
				this.itemMargin.horizontal,
				this.itemMargin.vertical);
		}
	}

	public Rect ViewArea
	{
		get
		{
			var p = this.bg2d.Pos;
			p.x += this.bgPadding.left;
			p.y -= this.bgPadding.top;

			var s = this.bg2d.Size;
			s.x -= this.bgPadding.horizontal;
			s.y -= this.bgPadding.vertical;

			return new Rect(p.x, p.y, s.x, s.y);
		}
	}

	public Rect ScrollArea
	{
		get
		{
			var a = this.ViewArea;
			var s = this.ItemSize;
			var col = this.colCount;
			var row = this.currentRowIndex + 1;
			return new Rect(a.x, a.y, s.x * col, s.y * row);
		}
	}

	public int SlotCount
	{
		get { return this.items != null && this.items.Count > 0 ? this.slots.Count : 0; }
	}

	public int ItemCount
	{
		get { return this.items != null ? this.items.Count : 0; }
	}

	private Vector2 ItemStartPos
	{
		get
		{
			var a = this.ViewArea;
			a.x += this.item2d.Size.x * 0.5f + this.itemMargin.left;
			a.y -= this.item2d.Size.y * 0.5f + this.itemMargin.top;
			return new Vector2(a.x, a.y);
		}
	}

	public bool IsTouched
	{
		get; internal set;
	}

	public bool IsFirstPage
	{
		get { return this.itemStartIndex == 0; }
	}

	public bool IsLastPage
	{
		get { return this.items.Count - this.itemStartIndex <= this.actualColCount * this.actualRowCount; }
	}

	private Vector2 OutOfBoundsDelta
	{
		get
		{
			var delta = Vector2.zero;

			if (this.slots.Count > 0)
			{
				var varea = this.ViewArea;
				var sarea = this.ScrollArea;
				var diff = this.scrolledPos + new Vector2(varea.width, varea.height) -
						new Vector2(sarea.width, sarea.height);

				if (this.scrolledPos.y < 0)
					delta.y = this.scrolledPos.y;

				else if (sarea.height >= varea.height && diff.y > 0)
					delta.y = diff.y;

				else if (sarea.height < varea.height && this.scrolledPos.y > 0)
					delta.y = this.scrolledPos.y;

				if (this.scrolledPos.x < 0)
					delta.x = this.scrolledPos.x;

				else if (sarea.width >= varea.width && diff.x > 0)
					delta.x = diff.x;

				else if (sarea.width < varea.width && this.scrolledPos.x > 0)
					delta.x = this.scrolledPos.x;
			}
			return delta;
		}
	}

	#endregion

	void Awake()
	{
		this.items = new List<PsdLayerVirtualViewItem>();
		this.slots = new List<PsdLayerVirtualViewSlot>();
		this.tweener = new iTweenSimplePlayer();
		this.uipanel = this.GetComponent<UIPanel>();
	}
	
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (UnityEditor.Selection.activeGameObject != this.gameObject)
			return;
		
		if (this.itemMargin == null)
			this.itemMargin = new RectOffset();
		
		if (this.bgPadding == null)
			this.bgPadding = new RectOffset();
		
		if (this.item2d == null)
			this.item2d = new Transform2D(this.item);
		
		if (this.bg2d == null)
			this.bg2d = new Transform2D(this.bg);
		
		var area = this.ViewArea;
		var pos = new Vector3(area.x + area.width*0.5f, area.y - area.height*0.5f, -1);
		var size = new Vector3(area.width, area.height, 0.1f);
		
		Gizmos.matrix = this.transform.localToWorldMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(pos, size);
	}
#endif
	
	void LateUpdate()
	{
		if (this.ItemCount == 0 || this.IsTouched || this.tweener.IsPlaying)
			return;

		var distance = 0f;
		if (this.scrollEffect != PsdLayerVirtualViewScrollEffect.None && this.lastVelocity != 0)
		{
			var inertiaDuration = this.momentumInertiaDuration;
			inertiaDuration += this.addtionalInertiaDuration;

			var t = (Time.time - this.lastMovedTime) / inertiaDuration;
			if (t < inertiaDuration)
			{
				var velocity = Mathf.Lerp(this.lastVelocity, 0, t);
				distance = velocity * Time.smoothDeltaTime;
			}

			//Debug.Log ("Momentum: "+ this.OutOfBoundsDelta+", "+distance);
		}

		var outOfBound = this.OutOfBoundsDelta != Vector2.zero;

		if (this.scrollEffect == PsdLayerVirtualViewScrollEffect.Magnet)
		{
			// increase inertia duration
			this.addtionalInertiaDuration += this.momentumInertiaDuration * -0.5f;
		}
		else if (outOfBound)
		{
			// increase inertia duration
			this.addtionalInertiaDuration += this.momentumInertiaDuration * -0.1f;
		}

		if (!outOfBound && distance != 0)
		{
			this.OnMoving(new Vector2(0, distance));
		}
		else
		{
			if (outOfBound)
			{
				if (this.scrollEffect == PsdLayerVirtualViewScrollEffect.MomentumAndSpring)
				{
					if (this.circulation)
					{
					}
					else
						this.PlaySpringBreak();
				}
				else
					this.PlayMagnetBreak();
			}
			this.addtionalInertiaDuration = 0;
			this.lastVelocity = 0.0f;
		}
	}

	void OnPress(bool pressed)
	{
		if (pressed)
			this.OnMovingStart();
		else
			this.OnMovingEnd();
	}

	void OnDrag(Vector2 delta)
	{
		this.OnMoving(delta);
	}

	internal void OnMovingStart()
	{
		if (this.ItemCount == 0)
			return;

		this.IsTouched = true;

		//**??
		//		this.IsSlideStarted = false;
		//		this.IsSlidePlaying = false;

		this.addtionalInertiaDuration = 0;
		this.lastVelocity = 0.0f;
		this.tweener.Stop();
	}

	internal void OnMoving(Vector2 delta)
	{
		if (this.ItemCount == 0 || delta == Vector2.zero)
			return;

		this.lastMovedTime = Time.time;
		this.lastVelocity = delta.y / Time.smoothDeltaTime;

		var outOfBound = this.OutOfBoundsDelta != Vector2.zero;
		if (outOfBound)
		{
			delta *= 0.7f;
		}

		this.AddScrollPos(delta);
		this.Scroll();
	}

	internal void OnMovingEnd()
	{
		if (this.ItemCount == 0)
			return;

		this.IsTouched = false;

		var outOfBound = this.OutOfBoundsDelta != Vector2.zero;
		if (!outOfBound && this.scrollEffect == PsdLayerVirtualViewScrollEffect.Magnet)
		{
			this.PlayMagnetBreak();
			this.addtionalInertiaDuration = 0;
			this.lastVelocity = 0.0f;
		}
	}

	public void Init<T>()
	{
		if (this.IsInited)
			return;

		if (this.bg == null)
		{
			Debug.LogError("You must set the background");
			return;
		}

		if (this.item == null)
		{
			Debug.LogError("You must set the first item");
			return;
		}
		
		if (this.itemMargin == null)
			this.itemMargin = new RectOffset();
		
		if (this.bgPadding == null)
			this.bgPadding = new RectOffset();
		
		this.item2d = new Transform2D(this.item);
		this.bg2d = new Transform2D(this.bg);
		this.bg.parent = this.transform.parent; // avoid clipping
		
		var area = this.ViewArea;
		this.uipanel.clipRange = new Vector4(
			area.x + area.width*0.5f, area.y - area.height*0.5f, 
			area.width, area.height
		);
		
		this.MakeSlots(typeof(T));
		this.Refresh();
	}

	private GameObject MakeSlot(bool firstTime)
	{
		var clone = firstTime ?
			this.item.gameObject : GameObject.Instantiate(this.item.gameObject) as GameObject;
		{
			clone.transform.parent = this.item.parent;
			clone.transform.localPosition = Vector2.zero;
			clone.transform.localRotation = this.item.localRotation;
			clone.transform.localScale = this.item.localScale;
		}
		return clone;
	}

	private void MakeSlots(System.Type type)
	{
		var vw = this.ViewSize.x;
		var vh = this.ViewSize.y;
		var w = this.ItemSize.x;
		var h = this.ItemSize.y;
		
		this.colCount = Mathf.FloorToInt(vw / w);
		if (this.colCount <= 1)
			this.actualColCount = this.colCount = 1;
		else
			this.actualColCount = this.colCount + 2;
		
		this.rowCount = Mathf.FloorToInt(vh / h);
		if (this.rowCount <= 1)
			this.actualRowCount = this.rowCount = 1;
		else
			this.actualRowCount = this.rowCount + 2;

		this.slots.Clear();

		for (var row = 0; row < this.actualRowCount; ++row)
		{
			for (var col = 0; col < this.actualColCount; ++col)
			{
				var clone = this.MakeSlot(row == 0 && col == 0);
				var slot = System.Activator.CreateInstance(type) as PsdLayerVirtualViewSlot;
				slot.Init(clone);
				this.slots.Add(slot);
			}
		}
	}

	public T GetSlot<T>(int i) where T : PsdLayerVirtualViewSlot
	{
		return this.slots[i] as T;
	}

	public T GetItem<T>(int i) where T : PsdLayerVirtualViewItem
	{
		return this.items[i] as T;
	}

	public void AddItem(PsdLayerVirtualViewItem item)
	{
		this.AddItem(item, false);
	}
	public void AddItem(PsdLayerVirtualViewItem item, bool refresh)
	{
		this.items.Add(item);
		if (refresh)
			this.Refresh();
	}

	public void RemoveItem(PsdLayerVirtualViewItem item)
	{
		this.RemoveItem(item, false);
	}
	public void RemoveItem(PsdLayerVirtualViewItem item, bool refresh)
	{
		if (this.items.Remove(item))
		{
			if (this.itemStartIndex >= this.items.Count)
				this.itemStartIndex = Mathf.Max(0, this.items.Count - 1);
		}
		if (refresh)
			this.Refresh();
	}

	public void RemoveItemAt(int index)
	{
		this.RemoveItemAt(index, false);
	}
	public void RemoveItemAt(int index, bool refresh)
	{
		this.items.RemoveAt(index);
		{
			if (this.itemStartIndex >= this.items.Count)
				this.itemStartIndex = Mathf.Max(0, this.items.Count - 1);
		}
		if (refresh)
			this.Refresh();
	}

	public void ClearItems()
	{
		this.items.Clear();
	}

	public void RefreshAll()
	{
		if (Util.IsActive(this.gameObject))
		{
			var i = 0;
			foreach (var item in this.items)
			{
				if (i < this.slots.Count){
					item.Update();
					this.slots[i].Update(item, i, i);
				}
				else
					item.Update();
				i++;
			}
		}
	}

	public void Refresh()
	{
		if (Util.IsActive(this.gameObject))
			this.Refresh(this.itemStartIndex);
	}

	private void Refresh(int itemIndex)
	{
		if (this.ItemCount == 0 || this.SlotCount == 0)
			return;

		var posMaker = new ItemPositionMaker(this);
		var slotIndex = 0;
		foreach (var slot in this.slots)
		{
			if (slot.Visibled = itemIndex < this.items.Count)
			{
				var item = this.items[itemIndex];
				{
					slot.Pos = posMaker.NextPos;
					slot.Item = item;
					slot.Item.Update();
					slot.Update(item, slotIndex, itemIndex);
				}
				slotIndex++;
			}
			if (++itemIndex >= this.items.Count && this.circulation)
				itemIndex = 0;
		}
	}

	public void Shuffle()
	{
		if (this.ItemCount == 0 || this.SlotCount == 0)
			return;

		Util.Shuffle(this.items);
	}

	public void Sort()
	{
		if (this.ItemCount == 0 || this.SlotCount == 0)
			return;

		this.items.Sort((a, b) => a.Compare(b));
	}

	public void Reset()
	{
		this.Reset(true, true);
	}

	public void Reset(bool resetSlotPosition, bool resetItemPosition)
	{
		if (this.ItemCount > 0)
		{
			for (var i = 0; i < this.slots.Count; ++i)
			{
				var slot = this.slots[i];
				slot.Reset();
				if (resetSlotPosition)
				{
					//**TODO
				}
			}
			foreach (var item in this.items)
				item.Reset();
		}
		if (resetItemPosition)
			this.itemStartIndex = 0;
		this.Refresh();
	}

	private void AddScrollPos(Vector2 delta)
	{
		if (Util.Hasflag((int)this.scrollDirection, (int)PsdLayerVirtualViewScrollDirection.Horizontal))
			this.scrolledPos.x -= delta.x;

		if (Util.Hasflag((int)this.scrollDirection, (int)PsdLayerVirtualViewScrollDirection.Vertical))
			this.scrolledPos.y += delta.y;
	}

	private void UpdateScrollPos(Vector2 pos)
	{
		if (Util.Hasflag((int)this.scrollDirection, (int)PsdLayerVirtualViewScrollDirection.Horizontal))
			this.scrolledPos.x = pos.x;

		if (Util.Hasflag((int)this.scrollDirection, (int)PsdLayerVirtualViewScrollDirection.Vertical))
			this.scrolledPos.y = pos.y;
	}

	private void Scroll()
	{
		if ((this.currentRowIndex + 1) > this.actualRowCount)
		{
			var outDelta = this.OutOfBoundsDelta;
			if (outDelta == Vector2.zero)
			{
				var s = this.ItemSize;
				var y = Mathf.Abs(Mathf.FloorToInt(this.scrolledPos.y / s.y));
				this.itemStartIndex = y * this.colCount;
			}
			else if (outDelta.y > 0)
			{
				var s = this.ItemSize;
				var y = Mathf.Abs(Mathf.FloorToInt((this.ScrollArea.height - this.ViewArea.height) / s.y));
				this.itemStartIndex = y * this.colCount;
			}
			else if (outDelta.y < 0)
				this.itemStartIndex = 0;
		}
		else
			this.itemStartIndex = 0;

		//Debug.Log (this.itemStartIndex +", "+ this.itemStartIndexOld);

		var refresh = this.itemStartIndex != this.itemStartIndexOld;
		var posMaker = new ItemPositionMaker(this);
		var itemIndex = this.itemStartIndex;
		var slotIndex = 0;
		foreach (var slot in this.slots)
		{
			if (slot.Visibled = itemIndex < this.items.Count)
			{
				var item = this.items[itemIndex];
				{
					slot.Pos = posMaker.NextPos;
					slot.Item = item;
					if (refresh)
					{
						item.Update();
						slot.Update(item, slotIndex, itemIndex);
					}
				}
				slotIndex++;
			}
			if (++itemIndex >= this.items.Count && this.circulation)
				itemIndex = 0;
		}
		if (refresh)
			this.itemStartIndexOld = this.itemStartIndex;
	}

	private void PlaySpringBreak()
	{
		var pos = this.scrolledPos;
		var end = this.OutOfBoundsDelta;

		this.tweener.Stop();
		this.tweener.Play(this, 0.35f, true, 
			delegate(float percentage)
			{
				var v = iTweenSimple.easeOutCubic(0, 1, percentage);
				this.UpdateScrollPos(pos - v * end);
				this.Scroll();

			}, delegate(bool compelete)
		{

			if (this.whenSpringAnimationFinished != null)
				this.whenSpringAnimationFinished();
		}
		);
	}

	private void PlayMagnetBreak()
	{
		//**??
		//		var h = this.RowHeight;
		//		var standard = h * this.magnetSensitive;
		//		
		//		var upward = this.lastVelocity >= 0;
		//		var gap = 0f;
		//		{
		//			this.SetVerticalEndPosition(upward);
		//			gap = h - Mathf.Abs(this.slots[0].distanceToEnd.y);
		//		}
		//		if (Mathf.Abs(gap) <= 0.001f)
		//			return;
		//		
		//		this.SetStartPositionByCurrentPosition();
		//		
		//		var gotoOrigin = gap < standard;
		//		if (gotoOrigin)
		//			this.SetVerticalEndPosition(!upward);
		//		
		//		var to = this.slots[0].distanceToEnd.y;
		//		var time = this.magnetAnimationTime > 0 ? this.magnetAnimationTime : 0.01f;
		//		
		//		this.tweener.Stop();
		//		this.tweener.Play(this, time, true, 
		//			delegate(float percentage){
		//				var v = iTweenSimple.easeOutSine(0, to, percentage);
		//				this.Scroll(new Vector2(0, v), false);
		//				
		//				if (this.tweener.AnimationTime != this.magnetAnimationTime)
		//					this.tweener.AnimationTime = this.magnetAnimationTime;
		//			}
		//			, delegate(bool compelete){
		//				this.SetStartPositionByMap();
		//				
		//				if (this.whenMagnetAnimationFinished != null)
		//					this.whenMagnetAnimationFinished();
		//			}
		//		);
	}
};