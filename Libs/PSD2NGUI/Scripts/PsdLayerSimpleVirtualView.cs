using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GBlue;

// class PsdLayerSimpleVirtualViewItem

public class PsdLayerSimpleVirtualViewItem : PsdLayerVirtualViewItem
{
	internal override void Reset(){
	}
	
	internal override void Update(){
	}
};

// class PsdLayerSimpleVirtualViewSlot

public class PsdLayerSimpleVirtualViewSlot : PsdLayerVirtualViewSlot
{
	public UILabel label;
	
	internal override void Init(GameObject go){
		base.Init(go);
		
		this.label = GBlue.Util.FindComponent<UILabel>(go);
		if (this.label != null)
			this.label.color = Color.black;
	}
	
	internal override void Reset(){
	}
	
	internal override void Update(PsdLayerVirtualViewItem item, int slotIndex, int itemIndex){
		if (this.label != null)
			this.label.text = itemIndex.ToString();
	}
};

// class PsdLayerSimpleVirtualView

public class PsdLayerSimpleVirtualView : MonoBehaviour {
	
	void Start () {
		var view = this.GetComponent<PsdLayerVirtualView>();
		
		for (var i=0; i<1000; ++i){
			view.AddItem(new PsdLayerSimpleVirtualViewItem());
		}
		view.bgPadding = new RectOffset(14, 14, 14, 14);
		view.Init<PsdLayerSimpleVirtualViewSlot>();
	}
}