using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorOnTile : Behavior
    {
        public MapTile MapTile;
        public Curio Curio;

        public BehaviorOnTile()
        {
        }

        public BehaviorOnTile(MapTile mapTile, Curio curio)
        {
            MapTile = mapTile;
            Curio = curio;
        }

        public override void Apply()
        {
            MapTile.AddBehavior(this);
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            if (mapper.Map(Curio) == Curio) //Duplicating a tile should not place a curio in two locations at once
                return;
            var mapTile = (MapTile)mapper.Map(MapTile);
            var curio = (Curio)mapper.Map(Curio);
            Apply(new BehaviorOnTile(mapTile, curio));
        }

        public override string ToString()
        {
            return $"On tile {MapTile}";
        }
    }
}
