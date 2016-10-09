using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	public class iTweenSimpleBehaviour : MonoBehaviour
	{
		#region static members
		
		public static iTweenSimpleBehaviour Create(Transform owner)
		{
			return iTweenSimpleBehaviour.Create(owner.gameObject);
		}
		public static iTweenSimpleBehaviour Create(GameObject owner)
		{
			return owner.AddComponent<iTweenSimpleBehaviour>();
		}
		
		#endregion
		
		#region properties
		
		private iTweenSimplePlayer tweener = new iTweenSimplePlayer();
		
		public bool IsPlaying
		{
			get{ return this.tweener.IsPlaying; }
		}
		
		public float DelayTime
		{
			get{ return this.tweener.DelayTime; }
			set{ this.tweener.DelayTime = value; }
		}
		
		public float AnimationTime
		{
			get { return this.tweener.AnimationTime; }
			set { this.tweener.AnimationTime = value; }
		}
		
		#endregion
		
		void Update()
		{
			this.tweener.Update();
		}
		
		protected iTweenSimpleBehaviour()
		{
		}
		
		public void Play(float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.tweener.Play(this, 0, time, false, iTweenSimple.LoopType.none, 
				whenUpdate, null, whenFinish);
		}
		
		public void Play(float delay, float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.tweener.Play(this, delay, time, false, iTweenSimple.LoopType.none, 
				whenUpdate, null, whenFinish);
		}
		
		public void Play(float delay, float time, 
			iTweenSimple.LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action<bool> whenFinish)
		{
			this.tweener.Play(this, delay, time, false, type, 
				whenUpdate, whenRestart, whenFinish);
		}
		
		public void Stop()
		{
			this.tweener.Stop();
		}
	};
}