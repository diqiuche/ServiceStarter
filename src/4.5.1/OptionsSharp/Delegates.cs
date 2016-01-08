using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.OptionsSharp
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}
