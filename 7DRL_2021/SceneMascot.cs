using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _7DRL_2021
{
    enum None { };

    abstract class StateMachine<T, C> : StateMachine<T, None, C>
    {
    }

    abstract class StateMachine<T, V, C>
    {
        State Current;
        public T CurrentState => Current.Identifier;
        public C Content { get; private set; }
        Dictionary<T, State> StateLookup = new Dictionary<T, State>();

        public void AddState(T identifier, IStateTransfer stateTransfer, GeneratorDelegate generator)
        {
            StateLookup.Add(identifier, new State(identifier, stateTransfer, generator));
        }

        public void AddOnEnter(T identifier, Action<Optional> onEnter)
        {
            StateLookup[identifier].OnEnter += onEnter;
        }

        public void AddOnLeave(T identifier, Action<Optional> onLeave)
        {
            StateLookup[identifier].OnLeave += onLeave;
        }

        public void Update()
        {
            if (Content is ITickableState tickable)
                tickable.Update(this);
        }

        public void Start(T state)
        {
            Current = StateLookup[state];
            Content = Current.Generator(CurrentState, Content, new Optional());
        }

        public void Next()
        {
            Next(new Optional());
        }

        public void Next(V input)
        {
            Next(new Optional(input));
        }

        private void Next(Optional input)
        {
            if (Current.StateTransfer.Accepts(input))
            {
                var lastState = Current.Identifier;
                var lastContent = Content;
                var nextState = Current.StateTransfer.Next(input);
                Current.OnLeave?.Invoke(input);
                Current = StateLookup[nextState];
                Content = Current.Generator(lastState, lastContent, input);
                Current.OnEnter?.Invoke(input);
            }
        }

        public struct Optional
        {
            public V Value { get; private set; }
            public bool HasValue { get; private set; }

            public Optional(V value)
            {
                Value = value;
                HasValue = true;
            }
        }

        public struct NextState
        {
            public T Identifier { get; private set; }
            public C Content { get; private set; }

            public NextState(T identifier, C content) : this()
            {
                Identifier = identifier;
                Content = content;
            }
        }

        public delegate C GeneratorDelegate(T lastState, C lastContent, Optional input);

        public class State
        {
            public Action<Optional> OnEnter;
            public Action<Optional> OnLeave;
            public GeneratorDelegate Generator;

            public T Identifier;
            public IStateTransfer StateTransfer;

            public State(T identifier, IStateTransfer stateTransfer, GeneratorDelegate generator)
            {
                Identifier = identifier;
                StateTransfer = stateTransfer;
                Generator = generator;
            }
        }

        public interface IStateTransfer
        {
            bool Accepts(Optional input);

            T Next(Optional input);
        }

        public interface ITickableState
        {
            void Update(StateMachine<T, V, C> stateMachine);
        }
    }   

    class MascotMachine : StateMachine<MascotState, None, MascotMachine.Data>
    {
        public class Data : ITickableState
        {
            public Slider Frame;
            public float Slide => Frame.Slide;

            public Data(float time)
            {
                Frame = new Slider(time);
            }

            public void Update(StateMachine<MascotState, None, Data> stateMachine)
            {
                var content = stateMachine.Content;
                content.Frame += 1;
                if(content.Frame.Done)
                    stateMachine.Next();
            }
        }

        public class Transfer : IStateTransfer
        {
            MascotState NextState;

            public Transfer(MascotState nextState)
            {
                NextState = nextState;
            }

            public bool Accepts(Optional input)
            {
                return !input.HasValue;
            }

            public MascotState Next(Optional input)
            {
                return NextState;
            }
        }

        public class Finish : IStateTransfer
        {
            public bool Accepts(Optional input)
            {
                return false;
            }

            public MascotState Next(Optional input)
            {
                throw new NotImplementedException();
            }
        }
    }

    enum MascotState
    {
        Empty,
        FadeIn,
        OpenEye,
        Idle,
        FadeOut,
        Finish,
    }

    class SceneMascot : Scene
    {
        class VisualEffect
        {
            public ProtoEffectDrawable ProtoEffect;
            public Vector2 Position;

            public VisualEffect(ProtoEffectDrawable protoEffect, Vector2 position)
            {
                ProtoEffect = protoEffect;
                Position = position;
            }

            public void Draw(Scene scene)
            {
                ProtoEffect.Draw(scene, Position);
            }
        }

        const int EmptyTime = 60;
        const int FadeInTime = 30;
        const int OpenEyeTime = 30;
        const int IdleTime = 120;
        const int FadeOutTime = 30;

        MascotMachine MascotSM;
        FlashHelper MascotFlash = new FlashHelper();
        Slider MascotBlink = new Slider(1, 1);

        List<VisualEffect> VisualEffects = new List<VisualEffect>();

        public override float TimeMod => TimeModStandard;

        public SceneMascot(Game game) : base(game)
        {
            MascotSM = new MascotMachine();
            MascotSM.AddState(MascotState.Empty, new MascotMachine.Transfer(MascotState.FadeIn), (state, content, input) => new MascotMachine.Data(EmptyTime));
            MascotSM.AddState(MascotState.FadeIn, new MascotMachine.Transfer(MascotState.OpenEye), (state, content, input) => new MascotMachine.Data(FadeInTime));
            MascotSM.AddState(MascotState.OpenEye, new MascotMachine.Transfer(MascotState.Idle), (state, content, input) => new MascotMachine.Data(OpenEyeTime));
            MascotSM.AddState(MascotState.Idle, new MascotMachine.Transfer(MascotState.FadeOut), (state, content, input) => new MascotMachine.Data(IdleTime));
            MascotSM.AddState(MascotState.FadeOut, new MascotMachine.Transfer(MascotState.Finish), (state, content, input) => new MascotMachine.Data(FadeOutTime));
            MascotSM.AddState(MascotState.Finish, new MascotMachine.Finish(), (state, content, input) => new MascotMachine.Data(0));
            MascotSM.AddOnLeave(MascotState.OpenEye, (input) =>
            {
                Vector2 pos = new Vector2(Viewport.Width / 2, Viewport.Height / 2);
                MascotFlash.AddFlash(ColorMatrix.Flat(Color.Red), 10);
                var star = new BigStar(this, SpriteLoader.Instance.AddSprite("content/effect_star_big"))
                {
                    Color = Color.Red,
                };
                star.Angle.Set(0, MathHelper.Pi, LerpHelper.QuadraticOut, 30);
                star.Scale.Set(1f, 0.1f, LerpHelper.QuadraticIn, 30);
                star.ShouldDestroy.Set(true, LerpHelper.Linear, 30);
                var wave = new Wave(this, SpriteLoader.Instance.AddSprite("content/ring_spark_thin"), 30)
                {
                    Radius = 128,
                    Precision = 8,
                    StartRadius = 0.3f,
                    Thickness = 0.5f,
                    InnerLerp = LerpHelper.QuadraticOut,
                    OuterLerp = LerpHelper.QuadraticOut,
                    Color = ColorMatrix.Tint(Color.Red),
                };
                VisualEffects.Add(new VisualEffect(star, pos + new Vector2(17, -17) * 2));
                VisualEffects.Add(new VisualEffect(wave, pos + new Vector2(17, -17) * 2));
            });
            MascotSM.AddOnEnter(MascotState.Finish, (input) =>
            {
                Game.Scene = new SceneTitle(Game);
            });
            MascotSM.Start(MascotState.Empty);
        }

        public override void Draw(GameTime gameTime)
        {
            var mascot_dark = SpriteLoader.Instance.AddSprite("content/intro_mascot_dark");
            var mascot = SpriteLoader.Instance.AddSprite("content/intro_mascot");

            Color fadeColor;

            WorldTransform = Matrix.Identity;
            Projection = Matrix.CreateOrthographicOffCenter(0, Viewport.Width, Viewport.Height, 0, 0, -1);
            ColorMatrix colorMatrix = MascotFlash.ColorMatrix;

            PushSpriteBatch();
            PushSpriteBatch(shader: Shader, projection: Projection, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(colorMatrix, transform, projection);
            });

            Vector2 pos = new Vector2(Viewport.Width / 2, Viewport.Height / 2);
            switch (MascotSM.CurrentState)
            {
                case MascotState.FadeIn:
                    float fadeIn = MascotSM.Content.Slide;
                    fadeColor = Color.Lerp(Color.Black, Color.White, (float)LerpHelper.QuadraticOut(0, 1, fadeIn));
                    DrawSpriteExt(mascot_dark, AnimationFrame(mascot_dark, 0), pos - mascot_dark.Middle, mascot_dark.Middle, 0, new Vector2(2), SpriteEffects.None, fadeColor, 0);
                    break;
                case MascotState.OpenEye:
                    float openEye = MascotSM.Content.Slide;
                    DrawSpriteExt(mascot_dark, AnimationFrame(mascot_dark, (float)LerpHelper.QuinticIn(0, 1, openEye)), pos - mascot_dark.Middle, mascot_dark.Middle, 0, new Vector2(2), SpriteEffects.None, Color.White, 0);
                    break;
                case MascotState.Idle:
                    var blink = SlideRepeat(MascotBlink.Slide, 2);
                    DrawSpriteExt(mascot, AnimationFrame(mascot, (float)LerpHelper.QuinticIn(0, 1, blink)), pos - mascot.Middle, mascot.Middle, 0, new Vector2(2), SpriteEffects.None, Color.White, 0);
                    break;
                case MascotState.FadeOut:
                    float fadeOut = MascotSM.Content.Slide;
                    fadeColor = Color.Lerp(Color.White, Color.Black, (float)LerpHelper.QuadraticIn(0, 1, fadeOut));
                    fadeOut = MascotSM.Content.Slide;
                    DrawSpriteExt(mascot, 3, pos - mascot.Middle, mascot.Middle, 0, new Vector2(2), SpriteEffects.None, fadeColor, 0);
                    break;
            }
            PopSpriteBatch();

            foreach (var visualEffect in VisualEffects)
                visualEffect.Draw(this);

            PopSpriteBatch();
        }

        private float SlideRepeat(float slide, int times)
        {
            slide *= times;
            while (slide > 1)
                slide -= 1;
            return slide;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimeModifier();

            MascotSM.Update();
            MascotFlash.Update(1);

            MascotBlink += 1;
            if (MascotSM.CurrentState == MascotState.Idle && MascotSM.Content.Frame.Time == 60)
                MascotBlink = new Slider(20);

            UpdateProtoEffects();

            VisualEffects.RemoveAll(x => x.ProtoEffect.Destroyed);
        }
    }
}
