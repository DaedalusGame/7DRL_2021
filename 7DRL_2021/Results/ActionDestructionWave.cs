using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionDestructionWave : IActionHasOrigin
    {
        public ICurio Origin { get; set; }
        public int Radius;
        public int Score;
        public bool Done => true;

        public static SoundReference Sound = SoundLoader.AddSound("content/sound/destruction_wave.wav");

        public ActionDestructionWave(ICurio origin, int radius, int score)
        {
            Origin = origin;
            Radius = radius;
            Score = score;
        }

        public void Run()
        {
            var world = Origin.GetWorld();
            var center = Origin.GetMainTile();
            var sword = Origin.GetBehavior<BehaviorSword>();

            if (sword != null)
                sword.HasHeart = false;
            
            new ScreenFlashSimple(world, ColorMatrix.Tint(Color.Red), LerpHelper.QuadraticOut, 20);
            new ScreenShakeRandom(world, 15, 20, LerpHelper.QuadraticOut);
            foreach(var target in center.GetNearby(Radius).SelectMany(x => x.Contents))
            {
                if (target == Origin || !target.HasBehaviors<BehaviorAlive>())
                    continue;
                var actions = new List<ActionWrapper>();
                actions.Add(new ActionGib(Origin, target, Score, SoundLoader.AddSound("content/sound/big_splat.wav")).InSlot(ActionSlot.Active));
                actions.Apply(target);
            }
            Sound.Play(1.0f, 0f, 0f);
        }
    }
}
