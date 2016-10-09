using UnityEngine;
using System.Collections;

namespace GBlue
{
	public class iTweenSimple
	{
		#region callbacks
		
		private System.Action whenRestart;
		private System.Action whenComplete;
		private System.Action<float> whenUpdate;
		
		#endregion
		
		#region variables
		
		public enum LoopType{
			/// <summary>
			/// Do not loop.
			/// </summary>
			none,
			/// <summary>
			/// Rewind and replay.
			/// </summary>
			loop,
			/// <summary>
			/// Ping pong the animation back and forth.
			/// </summary>
			pingPong
		};
		
		private bool reverse = false;
		
		public LoopType loopType = LoopType.none;
		public bool running = false;
		private float percentage = 0;
		
		private float runningTime;
		private float time_;
		public float time
		{
			get { return this.time_; }
			set {
				this.time_ = value;
				this.runningTime = this.percentage * value;
			}
		}
		
	    public bool useRealTime; // Added by PressPlay
	    private float lastRealTime; // Added by PressPlay
		
		#endregion
		
		#region #1 excute method
		
		public iTweenSimple()
		{
		}
		
		public iTweenSimple(float time, 
			System.Action<float> whenUpdate, 
			System.Action whenComplete)
		{
			this.to(time, whenUpdate, whenComplete);
		}
		
		public iTweenSimple(float time, LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action whenComplete)
		{
			this.to(time, type, whenUpdate, whenRestart, whenComplete);
		}
		
		public void to(float time, 
			System.Action<float> whenUpdate, 
			System.Action whenComplete)
		{
			this.to(time, LoopType.none, whenUpdate, null, whenComplete);
		}
		
		public void to(float time, LoopType type, 
			System.Action<float> whenUpdate, 
			System.Action whenRestart, 
			System.Action whenComplete)
		{
			this.time_ = time < 0 ? 0 : time;
			this.loopType = type;
			this.percentage = 0;
			
			this.TweenStart();
			
			this.whenUpdate = whenUpdate;
			this.whenRestart = whenRestart;
			this.whenComplete = whenComplete;
		}
		
		public void update()
		{
			if(this.running){
				if(!this.reverse){
					if(this.percentage < 1f){
						this.TweenUpdate();
					}else{
						this.TweenComplete();	
					}
				}else{
					if(this.percentage > 0){
						this.TweenUpdate();
					}else{
						this.TweenComplete();	
					}
				}
			}
		}
		
		#endregion
		
		#region #2 tween steps
		
		void TweenStart(){
			this.running = true;
			this.runningTime = 0;
			this.lastRealTime = Time.realtimeSinceStartup;
		}
		
		void TweenRestart(){
			this.TweenStart();
			if (this.whenRestart != null)
				this.whenRestart();
		}
		
		void TweenUpdate(){
			if (this.whenUpdate != null)
				this.whenUpdate(this.percentage);
			this.UpdatePercentage();
		}
		
		void TweenComplete(){
			this.running = false;
			
			//dial in percentage to 1 or 0 for final run:
			if (this.percentage > .5f){
				this.percentage = 1f;
			}else{
				this.percentage = 0;	
			}
			
			//apply dial in and final run:
			if (this.whenUpdate != null)
				this.whenUpdate(this.percentage);
			
			//loop or dispose?
			if (this.loopType != LoopType.none)
				this.TweenLoop();
			else
			{
				if (this.whenComplete != null)
					this.whenComplete();
			}
		}
		
		void TweenLoop(){
			switch(this.loopType){
				case LoopType.loop:
					//rewind:
					this.percentage = 0;
					this.runningTime = 0;
					
					//replay:
					this.TweenRestart();
					break;
				
				case LoopType.pingPong:
					this.reverse = !this.reverse;
					this.runningTime = 0;
					
					//replay:
					this.TweenRestart();
					break;
			}
		}
		
		void UpdatePercentage(){
	        // Added by PressPlay
	        if (useRealTime)
	        {
	            runningTime += (Time.realtimeSinceStartup - lastRealTime);      
	        }
	        else
	        {
	            runningTime += Time.smoothDeltaTime;
	        }
	
			if(reverse){
				percentage = 1f - runningTime/time;
			}else{
				percentage = runningTime/time;
			}
	
	        lastRealTime = Time.realtimeSinceStartup; // Added by PressPlay
		}
		
		#endregion
		
		#region Easing Curves
		
		public static float linear(float start, float end, float value){
			return Mathf.Lerp(start, end, value);
		}
		
		public static float clerp(float start, float end, float value){
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) / 2.0f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half){
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}else if ((end - start) > half){
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}else retval = start + (end - start) * value;
			return retval;
	    }
	
		public static float spring(float start, float end, float value){
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}
	
		public static float easeInQuad(float start, float end, float value){
			end -= start;
			return end * value * value + start;
		}
	
		public static float easeOutQuad(float start, float end, float value){
			end -= start;
			return -end * value * (value - 2) + start;
		}
	
		public static float easeInOutQuad(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value + start;
			value--;
			return -end / 2 * (value * (value - 2) - 1) + start;
		}
	
		public static float easeInCubic(float start, float end, float value){
			end -= start;
			return end * value * value * value + start;
		}
	
		public static float easeOutCubic(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}
	
		public static float easeInOutCubic(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value + 2) + start;
		}
	
		public static float easeInQuart(float start, float end, float value){
			end -= start;
			return end * value * value * value * value + start;
		}
	
		public static float easeOutQuart(float start, float end, float value){
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}
	
		public static float easeInOutQuart(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value + start;
			value -= 2;
			return -end / 2 * (value * value * value * value - 2) + start;
		}
	
		public static float easeInQuint(float start, float end, float value){
			end -= start;
			return end * value * value * value * value * value + start;
		}
	
		public static float easeOutQuint(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}
	
		public static float easeInOutQuint(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value * value * value + 2) + start;
		}
	
		public static float easeInSine(float start, float end, float value){
			end -= start;
			return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
		}
	
		public static float easeOutSine(float start, float end, float value){
			end -= start;
			return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
		}
	
		public static float easeInOutSine(float start, float end, float value){
			end -= start;
			return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
		}
	
		public static float easeInExpo(float start, float end, float value){
			end -= start;
			return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
		}
	
		public static float easeOutExpo(float start, float end, float value){
			end -= start;
			return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
		}
	
		public static float easeInOutExpo(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
			value--;
			return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
		}
	
		public static float easeInCirc(float start, float end, float value){
			end -= start;
			return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
		}
	
		public static float easeOutCirc(float start, float end, float value){
			value--;
			end -= start;
			return end * Mathf.Sqrt(1 - value * value) + start;
		}
	
		public static float easeInOutCirc(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
		}
	
		public static float bounce(float start, float end, float value){
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f)){
				return end * (7.5625f * value * value) + start;
			}else if (value < (2 / 2.75f)){
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}else if (value < (2.5 / 2.75)){
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}else{
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}
	
		public static float easeInBack(float start, float end, float value){
			end -= start;
			value /= 1;
			float s = 1.70158f;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}
	
		public static float easeOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value = (value / 1) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}
	
		public static float easeInOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value /= .5f;
			if ((value) < 1){
				s *= (1.525f);
				return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
			}
			value -= 2;
			s *= (1.525f);
			return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}
	
		public static float punch(float amplitude, float value){
			float s = 9;
			if (value == 0){
				return 0;
			}
			if (value == 1){
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
	    }
		
		public static float elastic(float start, float end, float value){
			//Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
			end -= start;
			
			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;
			
			if (value == 0) return start;
			
			if ((value /= d) == 1) return start + end;
			
			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
				}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			
			return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
		}
		
		#endregion
	};
}