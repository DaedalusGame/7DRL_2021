using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Events
{
    class EventCurio : Event
    {
        public Curio Curio;

        public EventCurio(Curio curio)
        {
            Curio = curio;
        }
    }

    class EventMove : EventCurio
    {
        public class Finish : EventMove
        {
            public Finish(Curio curio, MapTile source, MapTile destination) : base(curio, source, destination)
            {
            }
        }

        public MapTile Source;
        public MapTile Destination;

        public EventMove(Curio curio, MapTile source, MapTile destination) : base(curio)
        {
            Source = source;
            Destination = destination;
        }
    }

    class EventSeen : Event
    {
        public ICurio Viewer;
        public MapTile SeenPosition;

        public EventSeen(ICurio viewer, MapTile seenPosition)
        {
            Viewer = viewer;
            SeenPosition = seenPosition;
        }
    }
}
