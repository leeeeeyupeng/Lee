using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	public class Timer
	{
		private MonoBehaviour owner;
		private float startTime = 0f;
		private bool reset = false;
		private bool stop = false;
		
		private System.Action whenFinish;
		
		public bool IsPlaying
		{
			get; private set;
		}
		
		public bool IsRealTimer
		{
			get; private set;
		}
		
		public bool CoroutineEnabled
		{
			get; private set;
		}
		
		public float Interval
		{
			get; set;
		}
		
		public Timer()
		{
		}
		
		public Timer(MonoBehaviour owner, float interval, System.Action whenFinish)
		{
			this.Play(owner, interval, true, whenFinish);
		}
		
		public Timer(MonoBehaviour owner, float interval, bool enableCoroutine, System.Action whenFinish)
		{
			this.Play(owner, interval, enableCoroutine, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float interval, System.Action whenFinish)
		{
			this.Play(owner, interval, false, false, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float interval, bool enableCoroutine, System.Action whenFinish)
		{
			this.Play(owner, interval, enableCoroutine, false, whenFinish);
		}
		
		public void Play(MonoBehaviour owner, float interval, bool enableCoroutine, bool enableRealTimer, 
			System.Action whenFinish)
		{
			this.owner = owner;
			this.IsRealTimer = enableRealTimer;
			this.startTime = this.IsRealTimer ? Time.realtimeSinceStartup : Time.time;
			this.Interval = interval < 0 ? 0 : interval;
			this.reset = true;
			this.stop = false;
			this.IsPlaying = true;
			this.whenFinish = whenFinish;
			
			if (this.CoroutineEnabled = enableCoroutine)
			{
				this.owner.StopCoroutine("PlayTimerRoutine");
				this.owner.StartCoroutine(this.PlayTimerRoutine());
			}
		}
		
		public void Stop()
		{
			this.stop = true;
		}
		
		public void Reset()
		{
			this.startTime = this.IsRealTimer ? Time.realtimeSinceStartup : Time.time;
			this.stop = false;
			this.reset = true;
		}
		
		public void Update()
		{
			if (!this.CoroutineEnabled && 
				!this.stop && this.reset)
			{
				var nowtime = this.IsRealTimer ? Time.realtimeSinceStartup : Time.time;
				var elpased = nowtime - this.startTime;
				if (this.Interval > elpased)
				{
					return;
				}
				
				this.reset = false;
				this.IsPlaying = false;
				{
					if (whenFinish != null)
						whenFinish();
				}
				this.startTime = nowtime - (elpased - this.Interval);
			}
		}
		
		private IEnumerator PlayTimerRoutine()
		{
			while (!this.stop && this.reset)
			{
				var nowtime = this.IsRealTimer ? Time.realtimeSinceStartup : Time.time;
				var elpased = nowtime - this.startTime;
				if (this.Interval > elpased)
				{
					yield return null;
					continue;
				}
				this.startTime = nowtime - (elpased - this.Interval);
				
				this.reset = false;
				this.IsPlaying = false;
				
				if (this.whenFinish != null)
					this.whenFinish();
			}
		}
	};
}