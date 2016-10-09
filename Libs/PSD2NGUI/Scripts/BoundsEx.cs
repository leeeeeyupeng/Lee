using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
    public class BoundsEx
    {
        #region Static
		
        public static Bounds GetBounds(Transform t)
		{
			return BoundsEx.GetBounds(t, true, false);
		}
        public static Bounds GetBounds(Transform t, bool local)
		{
			return BoundsEx.GetBounds(t, local, false);
		}
        public static Bounds GetBounds(Transform t, bool local, bool onlyChildren)
        {
            var children = t.GetComponentsInChildren<Transform>(true) as Transform[];
            if (children.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);
			
			var p = t.parent;
            var toLocal = p.worldToLocalMatrix;
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (var i=0; i<children.Length; ++i)
            {
                var child = children[i];
				var self = child == t;
				if (self && onlyChildren)
					continue;
				
				// change parent
				
				var orgP = child.parent;
				if (!self)
					child.parent = p;
				
				// calculate
				
				var x = child.localPosition.x;
				var y = child.localPosition.y;
				var s = child.localScale;
				s *= 0.5f;

				System.Action<Vector3> calulate = delegate(Vector3 v)
				{
					if (p != null)
					{
						v = p.TransformPoint(v);
						if (local)
							v = toLocal.MultiplyPoint3x4(v);
					}
					max = Vector3.Max(v, max);
					min = Vector3.Min(v, min);
				};

				// left top
				
				calulate(new Vector3(x - s.x, y + s.y, 0f));
				
				// left bottom
				
				calulate(new Vector3(x - s.x, y - s.y, 0f));
				
				// right bottom
				
				calulate(new Vector3(x + s.x, y - s.y, 0f));
				
				// right top
				
				calulate(new Vector3(x + s.x, y + s.y, 0f));
				
				// restore parent
				
				if (!self)
					child.parent = orgP;
            }

			var bounds = new Bounds(min, Vector3.zero);
			bounds.Encapsulate(min);
			bounds.Encapsulate(max);
			return bounds;
        }

        #endregion

        public BoundsEx()
        {
        }

        public BoundsEx(ComponentEx c)
        {
            this.Set(c);
        }

        public BoundsEx(ComponentEx c, bool swapYZ)
        {
            this.Set(c, swapYZ);
        }

        public BoundsEx(Transform t)
        {
            this.Set(t, false);
        }

        public BoundsEx(Transform t, bool swapYZ)
        {
            this.Set(t, swapYZ);
        }

        public bool IsExist
        {
            get
            {
                return this.Bounds.size != Vector3.zero;
            }
        }

        public Transform Transform
        {
            get; internal set;
        }

        public Bounds Bounds
        {
            get; internal set;
        }

        public Bounds LocalBounds
        {
            get; internal set;
        }

        public void Set(ComponentEx c)
        {
            this.Set(c.transform, false);
        }

        public void Set(ComponentEx c, bool swapYZ)
        {
            this.Set(c.transform, swapYZ);
        }

        public void Set(Transform t)
        {
            this.Set(t, false);
        }
        public void Set(Transform t, bool swapYZ)
        {
            this.Transform = t;
            var bounds = BoundsEx.GetBounds(t, false);
            var localBounds = BoundsEx.GetBounds(t, true);

            if (swapYZ)
            {
                var center = bounds.center;
                var size = bounds.size;
                {
                    Util.Swap<float>(ref center.y, ref center.z);
                    Util.Swap<float>(ref size.y, ref size.z);
                    bounds.center = center;
                    bounds.size = size;
                }

                center = localBounds.center;
                size = localBounds.size;
                {
                    Util.Swap<float>(ref center.y, ref center.z);
                    Util.Swap<float>(ref size.y, ref size.z);
                    localBounds.center = center;
                    localBounds.size = size;
                }
            }

            this.Bounds = bounds;
            this.LocalBounds = localBounds;
        }

        public RectEx XYRect
        {
            get
            {
                var bounds = this.Bounds;
                return new RectEx(
                    bounds.center.x - bounds.extents.x,
                    bounds.center.y - bounds.extents.y,
                    bounds.size.x, bounds.size.y);
            }
        }

        public RectEx XZRect
        {
            get
            {
                var bounds = this.Bounds;
                return new RectEx(
                    bounds.center.x - bounds.extents.x,
                    bounds.center.z - bounds.extents.z,
                    bounds.size.x, bounds.size.z);
            }
        }

        public RectEx YZRect
        {
            get
            {
                var bounds = this.Bounds;
                return new RectEx(
                    bounds.center.y - bounds.extents.y,
                    bounds.center.z - bounds.extents.z,
                    bounds.size.y, bounds.size.z);
            }
        }

        public RectEx LocalXYRect
        {
            get
            {
                var bounds = this.LocalBounds;
                return new RectEx(
                    bounds.center.x - bounds.extents.x,
                    bounds.center.y - bounds.extents.y,
                    bounds.size.x, bounds.size.y);
            }
        }

        public RectEx LocalXZRect
        {
            get
            {
                var bounds = this.LocalBounds;
                return new RectEx(
                    bounds.center.x - bounds.extents.x,
                    bounds.center.z - bounds.extents.z,
                    bounds.size.x, bounds.size.z);
            }
        }

        public RectEx LocalYZRect
        {
            get
            {
                var bounds = this.LocalBounds;
                return new RectEx(
                    bounds.center.y - bounds.extents.y,
                    bounds.center.z - bounds.extents.z,
                    bounds.size.y, bounds.size.z);
            }
        }

        public void DrawCubeGizmo()
        {
            var pos = this.Transform.position;
            Gizmos.DrawCube(pos + this.Bounds.center, this.Bounds.size);
        }

        public void DrawXYRect()
        {
            this.DrawRectGizmo(this.XYRect);
        }
        public void DrawXZRect()
        {
            this.DrawRectGizmo(this.XZRect);
        }
        public void DrawYZRect()
        {
            this.DrawRectGizmo(this.YZRect);
        }
        public void DrawRectGizmo(RectEx rc)
        {
            Gizmos.DrawLine(
                new Vector3(rc.X, 10, rc.Y),
                new Vector3(rc.X, 10, rc.Y + rc.H)
            );
            Gizmos.DrawLine(
                new Vector3(rc.X, 10, rc.Y + rc.H),
                new Vector3(rc.X + rc.W, 10, rc.Y + rc.H)
            );
            Gizmos.DrawLine(
                new Vector3(rc.X + rc.W, 10, rc.Y + rc.H),
                new Vector3(rc.X + rc.W, 10, rc.Y)
            );
            Gizmos.DrawLine(
                new Vector3(rc.X + rc.W, 10, rc.Y),
                new Vector3(rc.X, 10, rc.Y)
            );
        }
    };
}