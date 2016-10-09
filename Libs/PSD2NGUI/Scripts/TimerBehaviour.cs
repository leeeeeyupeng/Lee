using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GBlue
{
	public class TimerBehaviour : MonoBehaviour
	{
		#region static members
		
		public static TimerBehaviour Create(Transform owner)
		{
			return TimerBehaviour.Create(owner.gameObject);
		}
		public static TimerBehaviour Create(GameObject owner)
		{
			return owner.AddComponent<TimerBehaviour>();
		}
		
		#endregion
		
		private Timer timer = new Timer();
		
		private TimerBehaviour()
		{
		}
		
		void Start()
		{
		}
		
		void Update()
		{
			if (!this.timer.CoroutineEnabled)
				this.timer.Update();
		}
		
		public void Play(float time, System.Action whenFinish)
		{
			this.enabled = true;
			this.timer.Play(this, time, whenFinish);
		}
		
		public void Stop()
		{
			this.timer.Stop();
			this.enabled = false;
		}
		
		public void Reset()
		{
			this.timer.Reset();
		}
	};
}