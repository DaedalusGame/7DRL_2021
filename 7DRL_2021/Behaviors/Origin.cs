using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    interface IOrigin
    {

    }

    class OriginCopy : IOrigin
    {
        public ICurio Curio;

        public OriginCopy(ICurio curio)
        {
            Curio = curio;
        }
    }

    class OriginCopyTemplate : IOrigin
    {
        public Template Template;

        public OriginCopyTemplate(Template template)
        {
            Template = template;
        }
    }
}
