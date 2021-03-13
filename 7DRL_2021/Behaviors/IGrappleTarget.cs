using _7DRL_2021.Results;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    interface IGrappleTarget
    {
        void AddGrappleAction(List<ActionWrapper> wrappers, ICurio origin, Vector2 direction);
    }
}
