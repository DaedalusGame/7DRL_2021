using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    interface IBehaviorUpdateable
    {
        void Update();

        double UpdateOrder
        {
            get;
        }

        bool IsUpdateValid();
    }

    interface IBehaviorUpdateableCurio : IBehaviorUpdateable
    {
        ICurio Curio
        {
            get;
        }
    }
}
