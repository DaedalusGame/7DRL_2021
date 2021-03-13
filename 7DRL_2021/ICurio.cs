using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    interface ICurio
    {
        Guid GlobalID
        {
            get;
            set;
        }
        bool Removed
        {
            get;
            set;
        }
    }
}
