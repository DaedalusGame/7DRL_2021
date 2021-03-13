using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    static class Manager
    {
        class Drawer
        {
            Guid ID;
            WeakReference<ICurio> LastCurio = new WeakReference<ICurio>(null);
            List<Behavior> Behaviors = new List<Behavior>();

            public Drawer(ICurio curio)
            {
                ID = curio.GlobalID;
                LastCurio.SetTarget(curio);
            }

            public IEnumerable<Behavior> Get()
            {
                if (IsDirty())
                    return Enumerable.Empty<Behavior>();
                Behaviors.RemoveAll(x => x.Removed);
                return Behaviors;
            }

            public void Clear()
            {
                foreach (var behavior in Behaviors)
                    behavior.Remove();
            }

            public void Add(Behavior behavior)
            {
                if (!IsDirty())
                    Behaviors.Add(behavior);
            }

            private bool IsDirty()
            {
                return !LastCurio.TryGetTarget(out ICurio lastCurio);
            }
        }

        static List<ICurio> Curios = new List<ICurio>();
        static List<Behavior> Behaviors = new List<Behavior>();
        static Dictionary<Guid, Drawer> BehaviorLookup = new Dictionary<Guid, Drawer>();
        static BiDictionary<Guid, ICurio> CurioLookup = new BiDictionary<Guid, ICurio>();

        public static void Setup(this ICurio curio, Template template = null)
        {
            Setup(curio, Guid.NewGuid(), template);
        }

        public static void Setup(this ICurio curio, Guid guid, Template template = null)
        {
            curio.GlobalID = guid;
            Curios.Add(curio);
            CurioLookup.Add(guid, curio);
            SetupDrawer(curio);

            if (template != null)
            {
                ApplyTemplate(curio, template);
            }
        }

        public static Curio Copy(this Curio curio)
        {
            Curio copy = new Curio();
            CopyBehaviors(curio.GetBehaviors(), new CopyMapper(curio, copy));
            return copy;
        }

        public static void ApplyTemplate(this ICurio curio, Template template)
        {
            CopyBehaviors(template.Curio.GetBehaviors(), new CopyMapper(template.Curio, curio));
        }

        public static void RemoveTemplate(this ICurio curio, Template template)
        {
            RemoveBehaviors(curio, x => IsCopyOfTemplate(x, template));
        }

        public static void ClearTemplate(this ICurio curio)
        {
            RemoveBehaviors(curio, x => IsCopyOfTemplate(x));
        }

        private static bool IsCopyOfTemplate(Behavior behavior)
        {
            var origin = behavior.Origin;
            return origin is OriginCopyTemplate templateCopy;
        }

        private static bool IsCopyOfTemplate(Behavior behavior, Template template)
        {
            var origin = behavior.Origin;
            if (origin is OriginCopyTemplate templateCopy)
                return templateCopy.Template == template;
            return false;
        }

        private static void SetupDrawer(ICurio curio)
        {
            if (BehaviorLookup.TryGetValue(curio.GlobalID, out Drawer drawer))
                drawer.Clear();
            BehaviorLookup[curio.GlobalID] = new Drawer(curio);
        }

        public static void Reset()
        {
            foreach (var curio in Curios)
                if (!curio.HasBehaviors<BehaviorTemplate>())
                    curio.Destroy();
        }

        public static void Destroy(this ICurio curio)
        {
            curio.Removed = true;
            curio.ClearBehaviors();
            CurioLookup.Remove(curio);
        }

        public static ICurio GetCurio(Guid guid)
        {
            return CurioLookup.Forward.GetOrDefault(guid, null);
        }

        public static IEnumerable<ICurio> GetCurios()
        {
            return Curios;
        }

        public static IEnumerable<ICurio> GetCurios(Map map)
        {
            return Curios.Where(curio => curio.GetMap() == map);
        }

        public static IEnumerable<ICurio> GetCuriosWith<T>() where T : Behavior
        {
            return Curios.Where(curio => curio.GetBehaviors<T>().Any());
        }

        public static Map GetMap(this ICurio curio)
        {
            if (curio is MapTile tile)
                return tile.Map;
            if (curio is Curio c)
                return c.Map;
            return null;
        }

        public static void AddBehavior(this ICurio curio, Behavior behavior)
        {
            if (BehaviorLookup.TryGetValue(curio.GlobalID, out Drawer drawer))
            {
                Behaviors.Add(behavior);
                drawer.Add(behavior);
            }
        }

        public static void RemoveBehaviors(this ICurio curio, Predicate<Behavior> predicate)
        {
            foreach (var behavior in curio.GetBehaviors())
                if (predicate(behavior))
                    behavior.Remove();
        }

        public static void ClearBehaviors(this ICurio curio)
        {
            foreach (var behavior in curio.GetBehaviors())
                behavior.Remove();
        }

        public static IEnumerable<Behavior> GetBehaviors()
        {
            Behaviors.RemoveAll(x => x.Removed);
            return Behaviors.ToList();
        }

        public static IEnumerable<Behavior> GetBehaviors(this ICurio curio)
        {
            if (BehaviorLookup.TryGetValue(curio.GlobalID, out Drawer drawer))
            {
                return drawer.Get().ToList();
            }
            return Enumerable.Empty<Behavior>();
        }

        public static IEnumerable<T> GetBehaviors<T>(this ICurio curio)
        {
            return GetBehaviors(curio).OfType<T>();
        }

        public static T GetBehavior<T>(this ICurio curio) where T : Behavior
        {
            return GetBehaviors<T>(curio).FirstOrDefault();
        }

        public static bool HasBehaviors<T>(this ICurio curio)
        {
            if (BehaviorLookup.TryGetValue(curio.GlobalID, out Drawer drawer))
            {
                return drawer.Get().OfType<T>().Any();
            }
            return false;
        }

        public static void CopyBehaviors(IEnumerable<Behavior> behaviors, ICurioMapper guidMapper)
        {
            foreach (var behavior in behaviors)
            {
                behavior.Clone(guidMapper);
            }
        }

        private class CopyMapper : ICurioMapper
        {
            ICurio OldCurio;
            ICurio NewCurio;

            public CopyMapper(ICurio oldCurio, ICurio newCurio)
            {
                OldCurio = oldCurio;
                NewCurio = newCurio;
            }

            public ICurio Map(ICurio curio)
            {
                if (OldCurio == curio)
                    return NewCurio;
                return curio;
            }

            public Guid Map(Guid guid)
            {
                if (OldCurio.GlobalID == guid)
                    return NewCurio.GlobalID;
                return guid;
            }
        }
    }
}
