using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    interface ICurioMapper
    {
        ICurio Map(ICurio curio);

        Guid Map(Guid guid);
    }
}
