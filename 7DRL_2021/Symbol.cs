using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class Symbol
    {
        public static List<Symbol> AllSymbols = new List<Symbol>();

        public static Symbol MouseLeft = new Symbol(SpriteLoader.Instance.AddSprite("content/mouse_left"));
        public static Symbol MouseRight = new Symbol(SpriteLoader.Instance.AddSprite("content/mouse_right"));
        public static Symbol MouseMiddle = new Symbol(SpriteLoader.Instance.AddSprite("content/mouse_middle"));
        public static Symbol KeySpace = new Symbol(SpriteLoader.Instance.AddSprite("content/key_space"));
        public static Symbol KeyShift = new Symbol(SpriteLoader.Instance.AddSprite("content/key_shift"));
        public static Symbol KeyCtrl = new Symbol(SpriteLoader.Instance.AddSprite("content/key_control"));
        public static Symbol KeyEscape = new Symbol(SpriteLoader.Instance.AddSprite("content/key_escape"));
        public static Symbol KeyEnter = new Symbol(SpriteLoader.Instance.AddSprite("content/key_enter"));
        public static Symbol Heart = new Symbol(SpriteLoader.Instance.AddSprite("content/ui_heart_fill"));

        public int ID;
        public SpriteReference Sprite;

        public Symbol(SpriteReference sprite)
        {
            ID = AllSymbols.Count;
            Sprite = sprite;
            AllSymbols.Add(this);
        }

        public string FormatStatus(string text, Color color)
        {
            return $"{Game.FormatColor(color)}{Game.FormatBorder(Color.Black)}{Game.FORMAT_BOLD}{Game.FormatSymbol(this)} {text}{Game.FORMAT_RESET}";
        }

        public string FormatStored(int amount)
        {
            return FormatStatus(amount.ToString(), Color.White);
        }

        public string FormatGain(int amount)
        {
            return FormatDescribe(amount.ToString("+#;-#;0"));
        }

        public string FormatDescribe(int amount)
        {
            return FormatDescribe(amount.ToString());
        }

        public string FormatDescribe(string amount)
        {
            return $"{Game.FormatColor(Color.White)}{Game.FormatBorder(Color.Black)}{Game.FORMAT_BOLD}{amount} {Game.FormatSymbol(this)}{Game.FORMAT_RESET}";
        }

        public string FormatDamage(int damage)
        {
            return $"{Game.FormatColor(Color.White)}{Game.FormatBorder(Color.Black)}{Game.FORMAT_BOLD}{damage:+0;-#} {Game.FormatSymbol(this)}{Game.FORMAT_RESET}";
        }

        public virtual void DrawIcon(Scene scene, Vector2 pos, float slide)
        {
            scene.DrawSprite(Sprite, 0, pos, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }

        public virtual void DrawIcon(Scene scene, Vector2 pos, TextParameters parameters, float slide)
        {
            DrawIcon(scene, pos, slide);
        }
    }

    class SymbolTinted : Symbol
    {
        public SymbolTinted(SpriteReference sprite) : base(sprite)
        {
        }

        public override void DrawIcon(Scene scene, Vector2 pos, TextParameters parameters, float slide)
        {
            scene.DrawSpriteExt(Sprite, 0, pos, Sprite.Middle, 0, Vector2.One, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, parameters.Color(0), 0);
        }
    }

    class SymbolBar : Symbol
    {
        public SymbolBar(SpriteReference sprite) : base(sprite)
        {
        }

        public override void DrawIcon(Scene scene, Vector2 pos, float slide)
        {
            int amount = (int)Math.Round(slide * Sprite.Height);
            if (amount < 0)
                amount = 0;
            if (amount > Sprite.Height)
                amount = Sprite.Height;
            if (amount <= 0 && slide > 0)
                amount = 1;
            scene.SpriteBatch.Draw(Sprite.Texture, pos, new Rectangle(Sprite.Width * 1, 0, Sprite.Width, Sprite.Height), Color.White);
            scene.SpriteBatch.Draw(Sprite.Texture, pos + new Vector2(0, Sprite.Height - amount), new Rectangle(Sprite.Width * 0, Sprite.Height - amount, Sprite.Width, amount), Color.White);
        }
    }
}
