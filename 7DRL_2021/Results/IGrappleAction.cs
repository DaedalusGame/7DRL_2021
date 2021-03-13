using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    interface IGrappleAction : IActionPaired
    {
        Vector2 Direction { get; }
    }
}
