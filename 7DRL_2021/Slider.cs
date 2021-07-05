using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Slider : ISlider
    {
        public float Time
        {
            get => MathHelper.Clamp(CurrentTime, 0, EndTime);
            set => CurrentTime = MathHelper.Clamp(value, 0, EndTime);
        }
        public float CurrentTime;
        public float EndTime;
        public float Slide => Time / EndTime;
        public bool Done => Time >= EndTime;

        public Slider(float time, float endTime)
        {
            CurrentTime = time;
            EndTime = endTime;
        }

        public Slider(float time) : this(0, time)
        {

        }

        public float GetSubSlide(float start, float end)
        {
            float time = Time - start;
            float delta = end - start;
            return MathHelper.Clamp(time / delta, 0, 1);
        }

        public void Reset()
        {
            CurrentTime -= EndTime;
        }

        public static Slider operator +(Slider slider, float i)
        {
            slider.CurrentTime += i;
            return slider;
        }

        public static Slider operator -(Slider slider, float i)
        {
            slider.CurrentTime -= i;
            return slider;
        }

        public static bool operator <(Slider slider, float i)
        {
            return slider.Time < i;
        }

        public static bool operator >(Slider slider, float i)
        {
            return slider.Time > i;
        }

        public static bool operator <=(Slider slider, float i)
        {
            return slider.Time <= i;
        }

        public static bool operator >=(Slider slider, float i)
        {
            return slider.Time >= i;
        }

        public override string ToString()
        {
            return $"{Time} ({Slide})";
        }
    }
}
