using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Drawables
{
    abstract class Drawable : RegistryEntry<Drawable>
    {
        public Drawable(string id) : base(id)
        {
        }

        public abstract void Draw(ICurio curio, SceneGame scene, DrawPass pass);

        public abstract void DrawIcon(ICurio curio, SceneGame scene, Vector2 pos);

        public abstract IEnumerable<DrawPass> GetDrawPasses();

        public static DrawableFloor Floor = new DrawableFloor("floor", SpriteLoader.Instance.AddSprite("content/terrain_floor"), new Color(124, 88, 114), DrawPass.Tile);
        public static DrawableFloor Corridor = new DrawableFloor("corridor", SpriteLoader.Instance.AddSprite("content/terrain_floor"), new Color(155, 99, 74), DrawPass.Tile);
        public static DrawableTile Wall = new DrawableTile("wall", SpriteLoader.Instance.AddSprite("content/terrain_wall"), DrawPass.WallTop);
        public static DrawableWallSpiked SpikedWall = new DrawableWallSpiked("wall_spiked", SpriteLoader.Instance.AddSprite("content/terrain_wall"), DrawPass.Tile);
        public static DrawableTile WraithWall = new DrawableTile("wall_wraith", SpriteLoader.Instance.AddSprite("content/terrain_wraith"), DrawPass.WallTop);
        public static DrawableChasm Chasm = new DrawableChasm("chasm");
        public static DrawablePlayer Player = new DrawablePlayer("player");
        public static DrawableGrunt Grunt = new DrawableGrunt("grunt", SpriteLoader.Instance.AddSprite("content/grunt_body"), SpriteLoader.Instance.AddSprite("content/grunt_weapon"));
        public static DrawableGrunt Bulwark = new DrawableGrunt("bulwark", SpriteLoader.Instance.AddSprite("content/bulwark_body"), SpriteLoader.Instance.AddSprite("content/bulwark_weapon"));
        public static DrawableGrunt Executioner = new DrawableGrunt("executioner", SpriteLoader.Instance.AddSprite("content/executioner_body"), SpriteLoader.Instance.AddSprite("content/executioner_weapon"));
        public static DrawableLich Lich = new DrawableLich("lich");
        public static DrawableNemesis Nemesis = new DrawableNemesis("nemesis");
        public static DrawableRat Rat = new DrawableRat("rat");
        public static DrawableProjectile EnergyKnife = new DrawableProjectile("energy_knife", SpriteLoader.Instance.AddSprite("content/energy_dagger"));
    }
}
