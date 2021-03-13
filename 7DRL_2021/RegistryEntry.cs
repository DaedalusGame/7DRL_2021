using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    abstract class RegistryEntry<T> where T : RegistryEntry<T>
    {
        public static List<T> Registry = new List<T>();

        public int Index;
        public string ID;

        public RegistryEntry(string id)
        {
            Index = Registry.Count;
            ID = id;
            Registry.Add((T)this);
            Console.WriteLine($"[{GetType()}] Registering {id}");
        }

        public static T Get(string id)
        {
            return Registry.Find(x => x.ID == id);
        }
    }
}
