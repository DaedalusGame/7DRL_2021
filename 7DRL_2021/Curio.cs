using _7DRL_2021.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Curio : ICurio
    {
        public class Templated : Curio
        {

        }

        public Guid GlobalID
        {
            get;
            set;
        }
        public bool Removed
        {
            get;
            set;
        }

        public int X => Tile.X;
        public int Y => Tile.Y;
        public MapTile Tile
        {
            get
            {
                var tiles = Manager.GetBehaviors<BehaviorOnTile>(this);
                if (tiles.Any())
                    return tiles.First().MapTile;
                return null;
            }
        }
        public Map Map
        {
            get
            {
                return Tile?.Map;
            }
            set
            {
                //NOOP
            }
        }

        public IEnumerable<Behavior> Behaviors => this.GetBehaviors();

        public Curio(Template template = null)
        {
            this.Setup(template);
        }

        public Curio(Guid guid, Template template = null)
        {
            this.Setup(guid, template);
        }
    }
}
