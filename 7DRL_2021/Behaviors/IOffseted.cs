using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    interface IOffseted
    {
        Vector2 GetOffset();

        double GetOffsetPriority();
    }
}
