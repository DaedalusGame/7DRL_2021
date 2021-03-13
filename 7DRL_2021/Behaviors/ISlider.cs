using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    interface ISlider
    {
        float Slide { get; }
    }

    class SliderScene : ISlider
    {
        Scene Scene;
        float Frame;
        float Time;
        public float Slide => MathHelper.Clamp((Scene.Frame - Frame) / Time, 0, 1);

        public SliderScene(Scene scene, float time)
        {
            Scene = scene;
            Frame = scene.Frame;
            Time = time;
        }
    }

    class SliderTime : ISlider
    {
        SceneGame Scene;
        Slider Frame;
        public float Slide => Frame.Slide;

        public SliderTime(SceneGame scene, float time)
        {
            Scene = scene;
            Frame = new Slider(time);
            Scene.Timers.Add(this);
        }

        public void Update()
        {
            Frame += Scene.TimeMod;
        }
    }
}
