using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    interface ITooltipProvider
    {
        void AddTooltip(TextBuilder text);
    }

    class TooltipProviderFunction : ITooltipProvider
    {
        Action<TextBuilder> Provider;

        public TooltipProviderFunction(Action<TextBuilder> provider)
        {
            Provider = provider;
        }

        public void AddTooltip(TextBuilder text)
        {
            Provider(text);
        }
    }
}
