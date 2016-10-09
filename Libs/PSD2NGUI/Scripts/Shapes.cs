using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	public class RectEx
	{
		public RectEx(Rect rc)
		{
			this.Left = rc.xMin;
			this.Top = rc.yMin;
			this.Right = rc.xMax;
			this.Bottom = rc.yMax;
		}
		
		public RectEx(float x, float y, float w, float h)
		{
			this.Left = x;
			this.Top = y;
			this.Width = w;
			this.Height = h;
		}
		
		public float X { get; set; }
		public float Y { get; set; }
		
		public float W { get; set; }
		public float Width
		{
			get { return this.W; }
			set { this.W = value; }
		}
		
		public float H { get; set; }
		public float Height
		{
			get { return this.H; }
			set { this.H = value; }
		}
		
		public float Left
		{
			get { return this.X; }
			set { this.X = value; }
		}
		public float Top
		{
			get { return this.Y; }
			set { this.Y = value; }
		}
		public float Right
		{
			get { return this.X + this.W; }
			set { this.W = value - this.X; }
		}
		public float Bottom
		{
			get { return this.Y + this.W; }
			set { this.H = value - this.Y; }
		}
		
		public Rect ToRect()
		{
			return new Rect(this.X, this.Y, this.W, this.H);
		}
		
		public bool Contains(Vector2 v)
		{
			return this.Contains(v.x, v.y);
		}
		public bool Contains(Vector3 v)
		{
			return this.Contains(v.x, v.y);
		}
		public bool Contains(float x, float y)
		{
			return x >= this.Left && x < this.Right && y >= this.Top && y < this.Bottom;
		}
		
		public bool Contains(Rect r)
		{
			return this.Contains(r.x, r.y, r.width, r.height);
		}
		public bool Contains(RectEx r)
		{
			return this.Contains(r.X, r.Y, r.Width, r.Height);
		}
		public bool Contains(float x, float y, float w, float h)
		{
			return this.Contains(x, y) || this.Contains(x + w, y) || 
				this.Contains(x + w, y + h) || this.Contains(x, y + h);
		}
		
		public override int GetHashCode()
		{
			return 
				this.X.GetHashCode() ^ 
				this.Width.GetHashCode() << 2 ^ 
				this.Y.GetHashCode() >> 2 ^ 
				this.Height.GetHashCode() >> 1;
		}
		public override bool Equals(object other)
		{
			if (other is Rect)
			{
				var rect = (Rect)other;
				return 
					this.X.Equals(rect.x) && 
					this.Y.Equals(rect.y) && 
					this.Width.Equals(rect.width) && 
					this.Height.Equals(rect.height);
			}
			else if (other is RectEx)
			{
				var rect = (RectEx)other;
				return 
					this.X.Equals(rect.X) && 
					this.Y.Equals(rect.Y) && 
					this.Width.Equals(rect.Width) && 
					this.Height.Equals(rect.Height);
			}
			return false;
		}
		
		public static bool operator !=(RectEx lhs, Rect rhs)
		{
			return lhs.X != rhs.x || lhs.Y != rhs.y || lhs.Width != rhs.width || lhs.Height != rhs.height;
		}
		public static bool operator ==(RectEx lhs, Rect rhs)
		{
			return lhs.X == rhs.x && lhs.Y == rhs.y && lhs.Width == rhs.width && lhs.Height == rhs.height;
		}
		public static bool operator !=(RectEx lhs, RectEx rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Width != rhs.Width || lhs.Height != rhs.Height;
		}
		public static bool operator ==(RectEx lhs, RectEx rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Width == rhs.Width && lhs.Height == rhs.Height;
		}
	};
}