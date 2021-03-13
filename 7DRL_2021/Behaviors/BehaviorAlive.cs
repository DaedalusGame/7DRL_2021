using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Behaviors
{
    class BehaviorAlive : Behavior, IColored
    {
        public ICurio Curio;
        public double HP;
        public double Damage;
        public double Armor;

        public bool Dead;

        public double CurrentHP => Math.Max(HP - Damage, 0);
        public bool CurrentDead => Damage >= HP;

        public BehaviorAlive()
        {
        }

        public BehaviorAlive(ICurio curio, double hp, double armor) : this()
        {
            Curio = curio;
            HP = hp;
            Armor = armor;
        }

        public void SetDamage(double damage)
        {
            Damage = Math.Max(0, damage);
            if (!CurrentDead)
            {
                Dead = false;
            }
        }

        public void TakeDamage(double damage)
        {
            SetDamage(Damage + damage);
        }

        public void HealDamage(double damage)
        {
            SetDamage(Damage - damage);
        }

        public override void Apply()
        {
            Curio.AddBehavior(this);
        }

        public override void Clone(ICurioMapper mapper)
        {
            var curio = mapper.Map(Curio);
            BehaviorAlive behavior = new BehaviorAlive(curio, HP, Armor)
            {
                Damage = Damage,
            };
            Apply(behavior, Curio);
        }

        public ColorMatrix GetColor()
        {
            if (CurrentDead)
            {
                return ColorMatrix.Greyscale() * ColorMatrix.Tint(Color.IndianRed);
            }
            return ColorMatrix.Identity;
        }

        public double GetColorPriority()
        {
            return 10;
        }

        public bool IsUpdateValid()
        {
            return !Removed;
        }
    }
}