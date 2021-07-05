using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Systems
{
    interface IBehaviorSystem
    {
        bool Accepts(ICurio curio, Behavior behavior);

        void Add(ICurio curio, Behavior behavior);
    }

    abstract class BehaviorSystem : IBehaviorSystem
    {
        public static List<IBehaviorSystem> AllSystems = new List<IBehaviorSystem>();

        protected DeferredList<Behavior> Behaviors = new DeferredList<Behavior>();

        public BehaviorSystem()
        {
            AllSystems.Add(this);
        }

        public abstract bool Accepts(ICurio curio, Behavior behavior);

        public void Add(ICurio curio, Behavior behavior)
        {
            Behaviors.Add(behavior);
        }
    }

    class BehaviorSystemDrawable : IBehaviorSystem
    {
        protected DeferredList<Behavior> DrawableBehaviors = new DeferredList<Behavior>();
        protected DeferredList<Behavior> ContainerBehaviors = new DeferredList<Behavior>();

        public BehaviorSystemDrawable()
        {
            BehaviorSystem.AllSystems.Add(this);
        }

        public bool Accepts(ICurio curio, Behavior behavior)
        {
            return (behavior is IDrawable || behavior is IDrawableContainer) && !curio.IsTemplate();
        }

        public void Add(ICurio curio, Behavior behavior)
        {
            if(behavior is IDrawable)
                DrawableBehaviors.Add(behavior);
            if (behavior is IDrawableContainer)
                ContainerBehaviors.Add(behavior);
        }

        public IEnumerable<IDrawable> GetDrawables()
        {
            DrawableBehaviors.RemoveAll(x => x.Removed);
            ContainerBehaviors.RemoveAll(x => x.Removed);
            return DrawableBehaviors.Cast<IDrawable>().Concat(ContainerBehaviors.Cast<IDrawableContainer>().SelectMany(x => x.GetDrawables()));
        }
    }

    class BehaviorSystemPreDrawable : BehaviorSystem
    {
        public override bool Accepts(ICurio curio, Behavior behavior)
        {
            return behavior is IPreDrawable && !curio.IsTemplate();
        }

        public IEnumerable<IPreDrawable> GetPreDrawables()
        {
            Behaviors.RemoveAll(x => x.Removed);
            return Behaviors.Cast<IPreDrawable>();
        }
    }

    class BehaviorSystemTickable : BehaviorSystem
    {
        public override bool Accepts(ICurio curio, Behavior behavior)
        {
            return behavior is ITickable && !curio.IsTemplate();
        }

        public IEnumerable<ITickable> GetTickables()
        {
            Behaviors.RemoveAll(x => x.Removed);
            return Behaviors.Cast<ITickable>();
        }
    }
}
