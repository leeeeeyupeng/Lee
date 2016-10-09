using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	struct Vector3Ex
	{
		public float x;
		public float y;
		public float z;
		
		public Vector3Ex(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0;
		}
		public Vector3Ex(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Vector3Ex(Vector3 v)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}
		public Vector3Ex(Vector3Ex v)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}
		
	    public static implicit operator Vector3Ex(Vector3 v)
	    {
	        return new Vector3Ex(v);
	    }
	    public static implicit operator Vector3(Vector3Ex v)
	    {
	        return new Vector3(v.x, v.y, v.z);
	    }
		
		public Vector3Ex SetX(float v){
			this.x = v;
			return this;
		}
		public Vector3Ex SetY(float v){
			this.y = v;
			return this;
		}
		public Vector3Ex SetZ(float v){
			this.z = v;
			return this;
		}
		
		public Vector3Ex AddX(float v){
			this.x += v;
			return this;
		}
		public Vector3Ex AddY(float v){
			this.y += v;
			return this;
		}
		public Vector3Ex AddZ(float v){
			this.z += v;
			return this;
		}
		
		public Vector3Ex SubX(float v){
			this.x -= v;
			return this;
		}
		public Vector3Ex SubY(float v){
			this.y -= v;
			return this;
		}
		public Vector3Ex SubZ(float v){
			this.z -= v;
			return this;
		}
	};
}
