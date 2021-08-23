using _7DRL_2021.Behaviors;
using _7DRL_2021.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class TitleStateMachine : StateMachine<TitleState, None, TitleStateMachine.Data>
    {
        public class Data : ITickableState
        {
            public Slider Frame;
            public float Slide => Frame.Slide;

            public Data(float time)
            {
                Frame = new Slider(time);
            }

            public void Update(StateMachine<TitleState, None, Data> stateMachine)
            {
                var content = stateMachine.Content;
                content.Frame += 1;
                if (content.Frame.Done)
                    stateMachine.Next();
            }
        }

        public class Transfer : IStateTransfer
        {
            TitleState NextState;

            public Transfer(TitleState nextState)
            {
                NextState = nextState;
            }

            public bool Accepts(Optional input)
            {
                return !input.HasValue;
            }

            public TitleState Next(Optional input)
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

            public TitleState Next(Optional input)
            {
                throw new NotImplementedException();
            }
        }
    }

    enum TitleState
    {
        FadeIn,
        SpeedUp,
        MorphoAppear,
        CastleAppear,
        TextSlash,
        TextArrange,
        Finish,
    }

    class SceneTitle : Scene
    {
        class ParticleGround
        {
            public ProtoEffectDrawable ProtoEffect;
            public Vector2 Position;
            public LerpVector2 Offset = new LerpVector2(Vector2.Zero);

            public ParticleGround(ProtoEffectDrawable protoEffect, Vector2 position)
            {
                ProtoEffect = protoEffect;
                Position = position;
            }

            public void Update(Scene scene)
            {
                Offset.Update(scene.TimeModCurrent);
            }

            public void Draw(Scene scene)
            {
                ProtoEffect.Draw(scene, Position + Offset.Value);
            }
        }

        class Particle
        {
            public ProtoEffectDrawable ProtoEffect;
            public Vector2 Position;
            public LerpVector2 Offset = new LerpVector2(Vector2.Zero);

            public Particle(ProtoEffectDrawable protoEffect, Vector2 position)
            {
                ProtoEffect = protoEffect;
                Position = position;
            }

            public void Update(Scene scene)
            {
                Offset.Update(scene.TimeModCurrent);
            }

            public void Draw(Scene scene)
            {
                ProtoEffect.Draw(scene, Position + Offset.Value);
            }
        }

        public TitleUINew Menu;

        int GrassWidth = 32;
        int GrassHeight = 32;
        RenderTarget2D GrassNoise;
        RenderTarget2D GrassMap;
        public DoubleBuffer CameraTarget = new DoubleBuffer();

        PrimitiveBatch<VertexPositionNormalTexture> FloorBatch;

        static SimpleNoise Noise = new SimpleNoise(0);

        public TitleStateMachine TitleSM = new TitleStateMachine();

        LerpVector2 MorphoPositionBase;
        Vector2 MorphoPosition => MorphoPositionBase.Value + new Vector2((float)Math.Cos(Frame * 0.03f) * 4, (float)Math.Sin(Frame * 0.03f) * 4);
        LerpFloat MorphoHeight;
        Vector2 MorphoFacing = new Vector2(2, 6);

        Vector2 GroundOffset;
        LerpVector2 GroundVelocityBase;
        Vector2 GroundVelocity => GroundVelocityBase + new Vector2((float)Math.Sin(Frame * 0.02f), (float)Math.Cos(Frame * 0.03f)) * GroundVelocityBase.Value.Length() * 0.2f;
        Matrix GroundTransform = Matrix.Identity;

        LerpFloat CastleHeight;

        FlashHelper MorphoFlash = new FlashHelper();
        FlashHelper TextFlash = new FlashHelper();

        List<ParticleGround> GroundParticles = new List<ParticleGround>();
        List<Particle> Particles = new List<Particle>();

        public override float TimeMod => 1;

        public SceneTitle(Game game) : base(game)
        {
            Menu = new TitleUINew(this);
            FloorBatch = new PrimitiveBatch<VertexPositionNormalTexture>(GraphicsDevice);

            MorphoPositionBase = new LerpVector2(new Vector2(650, Viewport.Height + 160));
            MorphoHeight = new LerpFloat(40);
            GroundVelocityBase = new LerpVector2(new Vector2(2, 6) * 0.2f);
            CastleHeight = new LerpFloat(0);

            var fade = new ScreenFade(this, () => ColorMatrix.Tint(Color.Black), 1, true);
           

            TitleSM.AddState(TitleState.FadeIn, new TitleStateMachine.Transfer(TitleState.SpeedUp), (state, content, input) => new TitleStateMachine.Data(60));
            TitleSM.AddState(TitleState.SpeedUp, new TitleStateMachine.Transfer(TitleState.MorphoAppear), (state, content, input) => new TitleStateMachine.Data(60));
            TitleSM.AddState(TitleState.MorphoAppear, new TitleStateMachine.Transfer(TitleState.CastleAppear), (state, content, input) => new TitleStateMachine.Data(60));
            TitleSM.AddState(TitleState.CastleAppear, new TitleStateMachine.Transfer(TitleState.TextSlash), (state, content, input) => new TitleStateMachine.Data(320));
            TitleSM.AddState(TitleState.TextSlash, new TitleStateMachine.Transfer(TitleState.TextArrange), (state, content, input) => new TitleStateMachine.Data(40));
            TitleSM.AddState(TitleState.TextArrange, new TitleStateMachine.Transfer(TitleState.Finish), (state, content, input) => new TitleStateMachine.Data(40));
            TitleSM.AddState(TitleState.Finish, new TitleStateMachine.Finish(), (state, content, input) => new TitleStateMachine.Data(0));
            TitleSM.AddOnLeave(TitleState.FadeIn, (input) =>
            {
                fade.Lerp.Set(0, LerpHelper.QuadraticOut, 60);
            });
            TitleSM.AddOnEnter(TitleState.SpeedUp, (input) =>
            {
                GroundVelocityBase.Set(new Vector2(2, 6) * 1.5f, LerpHelper.QuadraticIn, 90);
            });
            TitleSM.AddOnEnter(TitleState.MorphoAppear, (input) =>
            {
                MorphoPositionBase.Set(new Vector2(550, 550), LerpHelper.QuadraticOut, 60);
                MorphoHeight.Set(80, LerpHelper.QuadraticOut, 60);
                CastleHeight.Set(400, LerpHelper.QuadraticOut, 400);
            });
            TitleSM.AddOnEnter(TitleState.TextSlash, (input) =>
            {
                int textX = Viewport.Width / 2;
                int textY = Viewport.Height / 10 + 40;
                MorphoFlash.AddFlash(ColorMatrix.Flat(Color.White), 10);
                TextFlash.AddFlash(ColorMatrix.Flat(Color.Red), 10);
                Particles.Add(new Particle(new SlashEffect(this, 300, 15) {
                    Angle = Random.NextFloat(MathHelper.PiOver2 - 0.1f, MathHelper.PiOver2 + 0.1f),
                }, new Vector2(textX, textY)));
                new ScreenShakeRandom(this, 10, 10, LerpHelper.QuadraticOut);
            });
            TitleSM.AddOnLeave(TitleState.TextArrange, (input) =>
            {
                TextFlash.AddFlash(ColorMatrix.Flat(Color.White), 10);
                new ScreenShakeRandom(this, 10, 20, LerpHelper.QuadraticOut);
            });
            TitleSM.Start(TitleState.FadeIn);

            

            ProtoEffects.Update();
        }

        private void DrawTextures()
        {
            var grass = SpriteLoader.Instance.AddSprite("content/title_grass");

            Menu.PreDraw(this);

            SetRenderTarget(GrassNoise);
            GraphicsDevice.Clear(Color.Black);

            var proj = Matrix.CreateOrthographicOffCenter(0, GrassNoise.Width, GrassNoise.Height, 0, 0, -1);
            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, projection: proj, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, transform, projection);
            });

            for (int x = 0; x < GrassWidth; x++)
            {
                for (int y = 0; y < GrassHeight; y++)
                {
                    DrawSprite(grass, Noise.GetValue(x, y), new Vector2(x, y) * 16, SpriteEffects.None, 0);
                }
            }

            PopSpriteBatch();

            SetRenderTarget(GrassMap);
            GraphicsDevice.Clear(Color.Black);

            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, transform: Matrix.Identity, projection: Projection, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            var shadow = SpriteLoader.Instance.AddSprite("content/title_morpho_shadow");
            var shadowSword = SpriteLoader.Instance.AddSprite("content/title_morpho_shadow_sword");
            shadowSword.ShouldLoad = true;
            PushSpriteBatch(shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupUVScroll(GroundOffset * -1 / new Vector2(GrassNoise.Width, GrassNoise.Height), transform, projection);
            });
            SpriteBatch.Draw(GrassNoise, Viewport.Bounds, Viewport.Bounds, Color.White);
            PopSpriteBatch();
            if (TitleSM.CurrentState >= TitleState.TextSlash)
                DrawSpriteExt(shadowSword, Frame / 10, MorphoPosition - shadowSword.Middle, shadowSword.Middle, Util.VectorToAngle(MorphoFacing) + MathHelper.Pi, new Vector2(0.5f), SpriteEffects.None, new Color(255, 255, 255, 64), 0);
            else
                DrawSpriteExt(shadow, Frame / 10, MorphoPosition - shadow.Middle, shadow.Middle, Util.VectorToAngle(MorphoFacing) + MathHelper.Pi, new Vector2(0.5f), SpriteEffects.None, new Color(255, 255, 255, 64), 0);
            foreach (var particle in GroundParticles)
                particle.Draw(this);

            PopSpriteBatch();
        }

        public void SetupLight(Vector3 lightPos, Matrix transform, Matrix projection)
        {
            ShaderLight.CurrentTechnique = ShaderLight.Techniques["Light"];
            ShaderLight.Parameters["lightPosition"].SetValue(lightPos);
            ShaderLight.Parameters["WorldViewProjection"].SetValue(transform * projection);
        }

        private Vector3 MapGroundToAir(Vector2 pos)
        {
            var circlePos = pos / new Vector2(GrassMap.Width, GrassMap.Height);
            float alphaOffset = +MathHelper.PiOver2;
            float betaOffset = 0;
            float alpha = alphaOffset + circlePos.X * MathHelper.Pi;
            float beta = betaOffset + circlePos.Y * MathHelper.Pi;

            float xy = (float)Math.Cos(alpha);
            float z = (float)Math.Sin(alpha);

            return Vector3.Transform(new Vector3(xy * (float)Math.Cos(beta), xy * (float)Math.Sin(beta), z), GroundTransform);
        }

        private void DrawGround()
        {
            var grass = SpriteLoader.Instance.AddSprite("content/title_grass");

            float precision = 50;
            float skewAmount = 0.1f * 0;
            Matrix skew = skew = new Matrix(
                1, 0, 0, 0,
                skewAmount, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
            Matrix idiotRotate = Matrix.CreateRotationX(0.01f * Frame) * Matrix.CreateRotationY(0.03f * Frame) * Matrix.CreateRotationZ(0.07f * Frame);
            GroundTransform = Matrix.CreateRotationY(-MathHelper.PiOver2) * skew * Matrix.CreateScale(Viewport.Width * 0.7f, 300, 1) * Matrix.CreateTranslation(Viewport.Width * 0.55f, Viewport.Height, 0);

            SetupLight(new Vector3(0, 3, 2), GroundTransform * WorldTransform, Projection);
            FloorBatch.Begin(PrimitiveType.TriangleList, texture: GrassMap, blendState: BlendState.Opaque, rasterizerState: RasterizerState.CullNone, samplerState: SamplerState.PointWrap, transform: GroundTransform * WorldTransform, projection: Projection, effect: ShaderLight);

            float alphaOffset = +MathHelper.PiOver2;
            float betaOffset = 0;
            float precisionX = precision / 2;
            float precisionY = precision / 2;

            for (int i = 0; i < precisionX; i++)
            {
                float alphaMin = alphaOffset + i * MathHelper.TwoPi / precision;
                float alphaMax = alphaOffset + (i + 1) * MathHelper.TwoPi / precision;
                float uMin = i / precisionX;
                float uMax = (i + 1) / precisionX;

                float xyMin = (float)Math.Cos(alphaMin);
                float xyMax = (float)Math.Cos(alphaMax);
                float zMin = (float)Math.Sin(alphaMin);
                float zMax = (float)Math.Sin(alphaMax);

                for (int j = 0; j < precisionY; j++)
                {
                    float betaMin = betaOffset + j * MathHelper.TwoPi / precision;
                    float betaMax = betaOffset + (j + 1) * MathHelper.TwoPi / precision;
                    float vMin = j / precisionY;
                    float vMax = (j + 1) / precisionY;

                    Vector3 a = new Vector3(xyMin * (float)Math.Cos(betaMin), xyMin * (float)Math.Sin(betaMin), zMin);
                    Vector3 b = new Vector3(xyMin * (float)Math.Cos(betaMax), xyMin * (float)Math.Sin(betaMax), zMin);
                    Vector3 c = new Vector3(xyMax * (float)Math.Cos(betaMin), xyMax * (float)Math.Sin(betaMin), zMax);
                    Vector3 d = new Vector3(xyMax * (float)Math.Cos(betaMax), xyMax * (float)Math.Sin(betaMax), zMax);

                    FloorBatch.AddVertex(new VertexPositionNormalTexture(a, a, new Vector2(uMin, vMin)));
                    FloorBatch.AddVertex(new VertexPositionNormalTexture(b, b, new Vector2(uMin, vMax)));
                    FloorBatch.AddVertex(new VertexPositionNormalTexture(c, c, new Vector2(uMax, vMin)));
                    FloorBatch.AddVertex(new VertexPositionNormalTexture(b, b, new Vector2(uMin, vMax)));
                    FloorBatch.AddVertex(new VertexPositionNormalTexture(d, d, new Vector2(uMax, vMax)));
                    FloorBatch.AddVertex(new VertexPositionNormalTexture(c, c, new Vector2(uMax, vMin)));
                }
            }

            FloorBatch.End();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetupRenderTarget(ref GrassNoise, GrassWidth * 16, GrassHeight * 16);
            GraphicsDevice.SetupRenderTarget(ref GrassMap, Viewport.Width, Viewport.Height);
            CameraTarget.Setup(this, Viewport.Width, Viewport.Height);

            WorldTransform = Matrix.Identity;
            ApplyScreenShake(ref WorldTransform);

            Projection = Matrix.CreateOrthographicOffCenter(0, Viewport.Width, Viewport.Height, 0, 0, -1);

            DrawTextures();

            //SetRenderTarget(null);

            SetRenderTarget(CameraTarget.A);

            PushSpriteBatch(blendState: NonPremultiplied, samplerState: SamplerState.PointWrap, transform: WorldTransform, projection: Projection, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, transform, projection);
            });

            DrawBackground();

            FlushSpriteBatch();

            DrawGround();
            DrawMorpho();

            if (TitleSM.CurrentState >= TitleState.TextSlash)
                DrawText();

            PopSpriteBatch();

            SetRenderTarget(CameraTarget.B);
            CameraTarget.Swap();

            //Draw screenflashes
            ColorMatrix color = ColorMatrix.Identity;
            ApplyScreenFlash(ref color);

            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, transform: Matrix.Identity, projection: Projection, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, transform, projection);
            });

            PushSpriteBatch(shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(color, transform, projection);
            });
            SpriteBatch.Draw(CameraTarget.B, CameraTarget.B.Bounds, Color.White);
            PopSpriteBatch();

            CameraTarget.Swap();

            SetRenderTarget(null);

            SpriteBatch.Draw(CameraTarget.B, CameraTarget.B.Bounds, Color.White);

            Menu.Draw(this);

            DrawCursor();

            foreach (var particle in Particles)
                particle.Draw(this);

            PopSpriteBatch();
        }

        private void DrawBackground()
        {
            float precision = 12;

            PushSpriteBatch(transform: Matrix.Identity);

            SpriteBatch.Draw(Pixel, new Rectangle(0, 0, Viewport.Width, Viewport.Height), Pixel.Bounds, new Color(77, 21, 13), 0, Vector2.Zero, SpriteEffects.None, 0f);

            for (int i = 0; i <= precision; i++)
            {
                var y = (int)LerpHelper.QuadraticOut(Viewport.Height * 0.3f, Viewport.Height * 0.7f, i / precision);
                var color = Color.Lerp(new Color(77, 21, 13), new Color(238, 170, 45), i / precision);

                SpriteBatch.Draw(Pixel, new Rectangle(0, y, Viewport.Width, Viewport.Height), Pixel.Bounds, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
            }

            var castle = SpriteLoader.Instance.AddSprite("content/title_castle");

            DrawSprite(castle, 0, new Vector2(0, 1000 - CastleHeight.Value - castle.Height), SpriteEffects.None, 0);

            PopSpriteBatch();
        }

        private void DrawMorpho()
        {
            ColorMatrix flash = MorphoFlash.ColorMatrix;

            PushSpriteBatch(shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(flash, transform, projection);
            });

            var morpho = SpriteLoader.Instance.AddSprite("content/title_morpho");
            var morphoSword = SpriteLoader.Instance.AddSprite("content/title_morpho_sword");
            var morphoPos = MapGroundToAir(MorphoPosition);
            var height = MorphoHeight.Value;
            var floatOffset = new Vector2(0, (float)Math.Sin(Frame * 0.02f) * 10);
            if (TitleSM.CurrentState >= TitleState.TextSlash)
            {
                int slashFrame = morphoSword.SubImageCount - 1;
                if (TitleSM.CurrentState == TitleState.TextSlash)
                {
                    float slashSlide = TitleSM.Content.Frame.GetSubSlide(0, 10);
                    slashFrame = AnimationFrame(morphoSword, slashSlide);
                }
                DrawSprite(morphoSword, slashFrame, new Vector2(morphoPos.X, morphoPos.Y - height + (float)Math.Sin(Frame * 0.02f - MathHelper.PiOver2) * 5) + floatOffset - morphoSword.Middle, SpriteEffects.None, 0);
            }
            DrawSprite(morpho, Frame / 10, new Vector2(morphoPos.X, morphoPos.Y - height) + floatOffset - morpho.Middle, SpriteEffects.None, 0);

            PopSpriteBatch();
        }

        private float HalfCircle(float slide)
        {
            if (slide <= 0 || slide >= 1)
                return 0;
            return (float)Math.Sqrt(1 - Math.Pow(slide * 2 - 1, 2));
        }

        private void DrawText()
        {
            var textTop = SpriteLoader.Instance.AddSprite("content/title_text_top");
            var textBottom = SpriteLoader.Instance.AddSprite("content/title_text_bottom");

            int textX = Viewport.Width / 2;
            int textY = Viewport.Height / 10 + 40;

            Vector2 offset = Vector2.Zero;
            if (TitleSM.CurrentState == TitleState.TextSlash)
            {
                float slashSlide = TitleSM.Content.Slide;
                offset = new Vector2((float)LerpHelper.QuinticOut(0, 80, slashSlide), 0);
            }
            else if (TitleSM.CurrentState == TitleState.TextArrange)
            {
                float rearrangeSlide = (float)LerpHelper.CubicIn(0, 1, TitleSM.Content.Slide);
                offset = new Vector2((float)LerpHelper.Linear(80, 0, rearrangeSlide), HalfCircle(rearrangeSlide) * -80);
            }
            else
            {
                offset = new Vector2(0, -40);
            }

            ColorMatrix flash = TextFlash.ColorMatrix;

            PushSpriteBatch(shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(flash, transform, projection);
            });

            GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Viewport.Width, textY);
            DrawSprite(textTop, 0, new Vector2(textX, textY) + offset - textTop.Middle, SpriteEffects.None, 0);
            FlushSpriteBatch();
            GraphicsDevice.ScissorRectangle = new Rectangle(0, textY, Viewport.Width, Viewport.Height);
            DrawSprite(textBottom, 0, new Vector2(textX, textY) - offset - textBottom.Middle, SpriteEffects.None, 0);
            FlushSpriteBatch();
            GraphicsDevice.ScissorRectangle = Viewport.Bounds;
            PopSpriteBatch();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimeModifier();

            Menu.Update(this);
            Menu.HandleInput(this);

            TitleSM.Update();

            MorphoFlash.Update(TimeModCurrent);
            TextFlash.Update(TimeModCurrent);
            UpdateProtoEffects();

            GroundVelocityBase.Update(TimeModCurrent);
            MorphoPositionBase.Update(TimeModCurrent);
            MorphoHeight.Update(TimeModCurrent);
            CastleHeight.Update(TimeModCurrent);
            GroundOffset += GroundVelocity;

            foreach (var particle in GroundParticles)
            {
                particle.Position += GroundVelocity;
                particle.Update(this);
            }
            foreach (var particle in Particles)
            {
                particle.Update(this);
            }
            GroundParticles.RemoveAll(x => x.ProtoEffect.Destroyed);
            Particles.RemoveAll(x => x.ProtoEffect.Destroyed);

            if (MorphoHeight.Value >= 60 && Frame % 6 == 0)
            {
                var direction = Vector2.Normalize(MorphoFacing);
                var lateral = direction.TurnRight();
                var offset = direction * 32 + lateral * (Frame % 12 == 0 ? -16 : +16);
                var lifetime = 30;
                var velocity = Util.AngleToVector(Random.NextAngle()) * 10;

                var particle = new ParticleGround(new Explosion(this, SpriteLoader.Instance.AddSprite("content/effect_moon_big"), lifetime)
                {
                    Angle = Util.VectorToAngle(GroundVelocity),
                }, MorphoPosition + offset);
                particle.Offset.Set(velocity, LerpHelper.Linear, lifetime);
                GroundParticles.Add(particle);
            }

            TooltipCursor = new TooltipCursorMenu(Menu.GetMouseOver(InputState.MouseX, InputState.MouseY));
        }

        public void NewGame()
        {
            Game.Scene = new SceneGame(Game);
        }
    }
}
