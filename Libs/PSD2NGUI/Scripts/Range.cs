using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	[System.Serializable]
	public class Range
	{
		public float min;
		public float max;
		
		public Range()
		{
		}
		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
		
		public float RandomValue
		{
			get { return Random.Range(min, max); }
		}
	};
}