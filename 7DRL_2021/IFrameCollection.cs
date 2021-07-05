using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class IFrameCollection
    {
        Dictionary<ICurio, float> RecentlyHit = new Dictionary<ICurio, float>();

        public void Add(ICurio curio)
        {
            RecentlyHit[curio] = 0;
        }

        public bool IsInvincible(ICurio curio, float time)
        {
            float timeSinceHit;
            return RecentlyHit.TryGetValue(curio, out timeSinceHit) && timeSinceHit <= time;
        }

        public void Tick(float time)
        {
            foreach(var curio in RecentlyHit.Keys.ToList())
            {
                RecentlyHit[curio] += time;
            }
        }
    }
}
