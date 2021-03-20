using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    public enum ActionSlot
    {
        Active,
        Passive,
    }

    class BehaviorActionHolder : Behavior, ITickable
    {
        public Curio Curio;
        public ActionSlot Type;
        public List<IAction> CurrentActions = new List<IAction>();

        public bool Done => CurrentActions.All(x => x.Done);
        
        public BehaviorActionHolder()
        {
        }

        public BehaviorActionHolder(Curio curio, ActionSlot type)
        {
            Curio = curio;
            Type = type;
        }

        public void Clear()
        {
            CurrentActions.Clear();
        }

        public void Cleanup()
        {
            CurrentActions.RemoveAll(x => x.Done);
        }

        public void Add(IAction action)
        {
            CurrentActions.Add(action);
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            Apply(new BehaviorActionHolder((Curio)mapper.Map(Curio), Type), Curio);
        }

        public override string ToString()
        {
            return $"Action";
        }

        public void Tick(SceneGame scene)
        {
            foreach (var action in CurrentActions.OfType<ITickable>().ToList())
                action.Tick(scene);
        }
    }
}
