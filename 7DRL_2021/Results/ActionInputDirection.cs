using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Results
{
    class ActionInputDirection : IAction
    {
        public bool Done => true;
        public Point Direction;

        public ActionInputDirection(Point direction)
        {
            Direction = direction;
        }

        public void Run()
        {
            //NOOP
        }
    }
}
