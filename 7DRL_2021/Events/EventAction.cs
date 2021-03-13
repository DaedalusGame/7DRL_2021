using _7DRL_2021.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Events
{
    class EventAction : Event
    {
        public readonly List<ActionWrapper> Actions;

        public EventAction(List<ActionWrapper> actions)
        {
            Actions = actions;
        }
    }

    class EventBetweenTurn : Event
    {
        public readonly SceneGame World;
        public readonly UpdateQueue UpdateQueue;

        public EventBetweenTurn(SceneGame world, UpdateQueue updateQueue)
        {
            World = world;
            UpdateQueue = updateQueue;
        }
    }
}
