using _7DRL_2021.Behaviors;
using _7DRL_2021.Drawables;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Template : RegistryEntry<Template>
    {
        Func<ICurio> LazyInitFunc;
        public ICurio Curio;

        static Template()
        {
            Init();
        }

        public Template(string id, Func<ICurio> lazyInit) : base(id)
        {
            LazyInitFunc = lazyInit;
        }

        public void LazyInit()
        {
            Curio = LazyInitFunc();
            Behavior.Apply(new BehaviorTemplate(Curio, this));
        }

        public static void Init()
        {
            Console.WriteLine($"Initializing {Registry.Count} templates");
            foreach (var template in Registry)
            {
                template.LazyInit();
            }
        }

        public static Mask MaskSingle = new Mask(new[] { Point.Zero });

        public static Template Camera = new Template("camera", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "camera"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            return template;
        });

        public static Template Pointer = new Template("pointer", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "pointer"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            return template;
        });
        public static Template BellTower = new Template("belltower", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "belltower"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorBellTower(template, 300));
            return template;
        });
        public static Template Wraith = new Template("wraith", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "wraith"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            return template;
        });
        public static Template EnergyKnife = new Template("energy_knife", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "energy_knife"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.EnergyKnife, 10));
            Behavior.Apply(new BehaviorProjectile(template, LerpHelper.Linear));
            Behavior.Apply(new BehaviorProjectileEnergyKnife(template));
            return template;
        });
        public static Template BloodThorn = new Template("blood_thorn", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "blood_thorn"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorProjectile(template, LerpHelper.Linear));
            Behavior.Apply(new BehaviorProjectileBloodThorn(template));
            return template;
        });
        public static Template BloodThornTrail = new Template("blood_thorn_trail", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "blood_thorn_trail"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorTrailBloodThorn(template));
            return template;
        });

        public static Template Player = new Template("player", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "player"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 3, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Player, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Passive));
            Behavior.Apply(new BehaviorPlayer(template));
            Behavior.Apply(new BehaviorGrapplingHook(template, -3));
            Behavior.Apply(new BehaviorHitboxPlayer(template));
            Behavior.Apply(new BehaviorShadow(template));
            //Behavior.Apply(new BehaviorSkillDestructionWave(template)); //DEBUG
            //Behavior.Apply(new BehaviorSkillButterflyKnives(template)); //DEBUG
            //Behavior.Apply(new BehaviorSkillBloodThorn(template)); //DEBUG
            //Behavior.Apply(new BehaviorSkillVampireBlade(template)); //DEBUG
            //Behavior.Apply(new BehaviorSkillBloodfireBlade(template)); //DEBUG
            return template;
        });
        public static Template Grunt = new Template("grunt", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "grunt"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 1, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Grunt, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorDagger(template));
            Behavior.Apply(new BehaviorGruntGeneric(template, 10, 5));
            Behavior.Apply(new BehaviorKnifeAttack(template, 10, 3f, 5f, 2.5f));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            Behavior.Apply(new BehaviorDecay(template, 50, 200, 30, 32, SoundLoader.AddSound("content/sound/splat.wav")));
            Behavior.Apply(new BehaviorGrappleHeart(template));
            return template;
        });
        public static Template Twitch = new Template("twitch", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "twitch"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 1, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Bulwark, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorMace(template, 48));
            Behavior.Apply(new BehaviorGruntGeneric(template, 8, 4));
            Behavior.Apply(new BehaviorMaceAttack(template, 10, 3f, 20f, 5f));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            Behavior.Apply(new BehaviorDecay(template, 50, 300, 30, 32, SoundLoader.AddSound("content/sound/splat.wav")));
            Behavior.Apply(new BehaviorGrappleHeart(template));
            return template;
        });
        public static Template Bulwark = new Template("bulwark", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "bulwark"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 1, 1));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Bulwark, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorMace(template, 48));
            Behavior.Apply(new BehaviorGruntGeneric(template, 8, 4));
            Behavior.Apply(new BehaviorMaceAttack(template, 10, 3f, 20f, 5f));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            Behavior.Apply(new BehaviorDecay(template, 50, 400, 30, 32, SoundLoader.AddSound("content/sound/splat.wav")));
            Behavior.Apply(new BehaviorGrappleHeart(template));
            return template;
        });
        public static Template Executioner = new Template("executioner", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "executioner"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 4, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Executioner, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorMaceGore(template, 48 * 3));
            Behavior.Apply(new BehaviorGruntGeneric(template, 8, 4));
            Behavior.Apply(new BehaviorMaceGoreAttack(template, 10, 3f, 40f, 20f, 5f));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            Behavior.Apply(new BehaviorDecay(template, 50, 300, 30, 32, SoundLoader.AddSound("content/sound/splat.wav")));
            Behavior.Apply(new BehaviorGrappleHeart(template));
            return template;
        });
        public static Template Rat = new Template("rat", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "rat"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 1, 0));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Rat, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorRat(template));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            return template;
        });
        public static Template Lich = new Template("lich", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "lich"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 1, 1));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Lich, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorLich(template));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorDecay(template, 30, 1000, 90, 96, SoundLoader.AddSound("content/sound/big_splat.wav")));
            Behavior.Apply(new BehaviorHitboxLich(template));
            return template;
        });
        public static Template Nemesis = new Template("nemesis", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "nemesis"));
            Behavior.Apply(new BehaviorMovable(template, MaskSingle.Copy()));
            Behavior.Apply(new BehaviorOrientable(template, 0));
            Behavior.Apply(new BehaviorAlive(template, 5, 3));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Nemesis, 10));
            Behavior.Apply(new BehaviorActionHolder(template, ActionSlot.Active));
            Behavior.Apply(new BehaviorPathfinder(template));
            Behavior.Apply(new BehaviorLastSeen(template));
            Behavior.Apply(new BehaviorNemesis(template));
            Behavior.Apply(new BehaviorShadow(template));
            Behavior.Apply(new BehaviorHitboxNormal(template));
            return template;
        });

        public static Template Floor = new Template("floor", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "floor"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Floor, 10));
            return template;
        });
        public static Template Corridor = new Template("corridor", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "corridor"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Corridor, 10));
            return template;
        });
        public static Template Wall = new Template("wall", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "wall"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Wall, 10));
            Behavior.Apply(new BehaviorSolid(template));
            Behavior.Apply(new BehaviorGrappleWall(template));
            return template;
        });
        public static Template WraithWall = new Template("wraith_wall", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "wraith_wall"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.WraithWall, 10));
            Behavior.Apply(new BehaviorSolid(template));
            Behavior.Apply(new BehaviorGrappleWall(template));
            Behavior.Apply(new BehaviorWraithEmitter(template));
            return template;
        });
        public static Template SpikeWall = new Template("spike_wall", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "spike_wall"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.SpikedWall, 10));
            Behavior.Apply(new BehaviorSolid(template));
            Behavior.Apply(new BehaviorSpiky(template));
            return template;
        });
        public static Template Chasm = new Template("chasm", () =>
        {
            Curio template = new Curio.Templated();
            Behavior.Apply(new BehaviorIdentity(template, "chasm"));
            Behavior.Apply(new BehaviorDrawable(template, Drawable.Chasm, 10));
            Behavior.Apply(new BehaviorChasm(template));
            return template;
        });
    }
}
