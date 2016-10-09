using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	public class iTweenSimplePlayer
	{
		private MonoBehaviour owner;
		private bool runDelay = false;
		private float playStartTime = 0f;
		private iTweenSimple tweener = new iTweenSimple();
		
		private System.Action<bool> whenInCompleteFinish;
		
		public bool IsPlaying
		{
			get; private set;
		}
		
		public bool CoroutineEnabled
		{
			get; private set;
		}
		
		public float DelayTime
		{
			get; set;
		}
		
		public float AnimationTime
		{
			get { return this.tweener.time; }
			set { this.tweener.time = value; }
		}
		
		public iTweenSimplePlayer()
		{
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, time, true, whenUpdate, whenFinish);
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float time, bool enableCoroutine, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, time, enableCoroutine, whenUpdate, whenFinish);
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float delay, float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, true, whenUpdate, whenFinish);
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float delay, float time, bool enableCoroutine, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, enableCoroutine, whenUpdate, whenFinish);
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float delay, float time, 
			iTweenSimple.LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, type, whenUpdate, whenRestart, whenFinish);
		}
		
		public iTweenSimplePlayer(MonoBehaviour owner, float delay, float time, bool enableCoroutine, 
			iTweenSimple.LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, enableCoroutine, type, 
				whenUpdate, whenRestart, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, 0, time, false, iTweenSimple.LoopType.none, 
				whenUpdate, null, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float time, bool enableCoroutine, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, 0, time, enableCoroutine, whenUpdate, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float delay, float time, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, false, iTweenSimple.LoopType.none, 
				whenUpdate, null, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float delay, float time, bool enableCoroutine, 
			System.Action<float> whenUpdate, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, enableCoroutine, iTweenSimple.LoopType.none, 
				whenUpdate, null, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float delay, float time, 
			iTweenSimple.LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action<bool> whenFinish)
		{
			this.Play(owner, delay, time, false, iTweenSimple.LoopType.none, 
				whenUpdate, whenRestart, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float delay, float time, bool enableCoroutine, 
			iTweenSimple.LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action<bool> whenFinish)
		{
			this.owner = owner;
			this.runDelay = true;
			
			this.playStartTime = Time.time;
			this.DelayTime = delay < 0 ? 0 : delay;
			this.IsPlaying = true;
			this.whenInCompleteFinish = whenFinish;
			
			this.tweener.to(time, type, delegate(float percentage){
				
				if (whenUpdate != null)
					whenUpdate(percentage);
				
			}, delegate(){
				
				this.runDelay = true;
				
				if (whenRestart != null)
					whenRestart();
				
			}, delegate(){
				
				this.IsPlaying = false;
				
				if (whenFinish != null)
					whenFinish(true);
			});
			
			if (this.CoroutineEnabled = enableCoroutine)
			{
				this.owner.StopCoroutine("PlayTween");
				this.owner.StartCoroutine(this.PlayTween());
			}
		}
		
		public void Stop()
		{
			this.tweener.running = false;
		}
		
		public void Update()
		{
#if UNITY_EDITOR
			if (this.CoroutineEnabled)
			{
				Debug.LogError("Already playing in coroutine");
				Debug.Break();
			}
#endif
			if (this.tweener.running)
			{
				if (this.runDelay)
				{
					var elpased = Time.time - this.playStartTime;
					if (this.DelayTime > elpased)
						return;
					else
						this.runDelay = false;
				}
				else
					this.tweener.update();
			}
			else if (this.IsPlaying)
			{
				this.IsPlaying = false;
				
				if (this.whenInCompleteFinish != null)
					this.whenInCompleteFinish(false);
			}
		}
		
		private IEnumerator PlayTween()
		{
			while (this.tweener.running)
			{
				while (this.runDelay)
				{
					var elpased = Time.time - this.playStartTime;
					if (this.DelayTime > elpased)
						yield return null;
					else
						this.runDelay = false;
				}
				
				this.tweener.update();
				yield return null;
			}
			
			if (this.IsPlaying)
			{
				this.IsPlaying = false;
				
				if (this.whenInCompleteFinish != null)
					this.whenInCompleteFinish(false);
			}
		}
	};
}