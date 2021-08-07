using _7DRL_2021.Behaviors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Curryfy.PartialSubsetFuncExtensions;

namespace _7DRL_2021.Menus
{
    class PlayerUI : Menu
    {
        static Dictionary<float, int> AnglePositionMapper = new Dictionary<float, int>()
        {
            { -3 * MathHelper.PiOver4, -3 },
            { -2 * MathHelper.PiOver4, -2 },
            { -1 * MathHelper.PiOver4, -1 },
            { 0 * MathHelper.PiOver4, 0 },
            { +1 * MathHelper.PiOver4, +1 },
            { +2 * MathHelper.PiOver4, +2 },
            { +3 * MathHelper.PiOver4, +3 },
        };

        public SceneGame Scene;

        public SubMenuHandler<Menu> SubMenu = new SubMenuHandler<Menu>()
        {
            BlocksInput = true,
        };
        SubMenuHandler<GameOverMenu> GameOverMenu = new SubMenuHandler<GameOverMenu>()
        {
            BlocksInput = true,
        };
        SubMenuHandler<ControlInfo> ControlInfo = new SubMenuHandler<ControlInfo>()
        {
            AllowInput = () => false,
        };

        Slider GameOver = new Slider(100);

        public ICurio Player => Scene.PlayerCurio;
        public ICurio Camera => Scene.CameraCurio;
        public LerpFloat Score = new LerpFloat(0);
        public string Momentum {
            get
            {
                var player = Player.GetBehavior<BehaviorPlayer>();
                if (player != null)
                {
                    var momentum = player.Momentum;
                    return Game.ConvertToSmallPixelText($"+{momentum.Amount - 32}");
                }
                return string.Empty;
            }
        }

        protected override IEnumerable<SubMenuHandler> SubMenuHandlers
        {
            get
            {
                yield return SubMenu;
                yield return GameOverMenu;
                if(!SubMenu.IsOpen && !GameOverMenu.IsOpen)
                    yield return ControlInfo;
            }
        }

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();

        TextBuilder ScoreText;
        TextBuilder MomentumText;
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public override bool ShouldClose
        {
            get
            {
                return false;
            }
            set
            {
                //NOOP;
            }
        }

        public PlayerUI(SceneGame scene)
        {
            Scene = scene;
            InitScore();
            InitMomentum();

            void relativeMovement(TextBuilder tooltip)
            {
                tooltip.StartLine(LineAlignment.Left);
                tooltip.AppendText("Relative to current facing.");
                tooltip.EndLine();
            }
            void heartInfo(TextBuilder tooltip)
            {
                tooltip.StartLine(LineAlignment.Left);
                tooltip.AppendText("Test");
                tooltip.EndLine();
            }

            ControlInfo.Open(new Menus.ControlInfo(scene, new[] {
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} or {1} Move.",
                        (TextElementer)(builder => builder.AppendAsKey("WASD")),
                        (TextElementer)(builder => builder.AppendAsKey("Numpad")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0}+{1} Move diagonally.",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyShift)),
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Moving {0} accelerates, adding some momentum.",
                        (TextElementer)(builder => {
                            builder.StartMenuArea(10, new TooltipProviderFunction(relativeMovement));
                            builder.AppendAsKey("↑", new Color(168, 247, 255));
                            builder.EndMenuArea();
                        }));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Moving {0} turns in that direction, using up some momentum.",
                        (TextElementer)(builder => {
                            builder.StartMenuArea(10, new TooltipProviderFunction(relativeMovement));
                            builder.AppendAsKey("←↖↗→", new Color(168, 247, 255));
                            builder.EndMenuArea();
                        }));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Moving {0} brakes, removing some momentum.",
                        (TextElementer)(builder => {
                            builder.StartMenuArea(10, new TooltipProviderFunction(relativeMovement));
                            builder.AppendAsKey("↓", new Color(168, 247, 255));
                            builder.EndMenuArea();
                        }));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Moving {0} while holding {1} moves sideways without changing direction.",
                        (TextElementer)(builder => {
                            builder.StartMenuArea(10, new TooltipProviderFunction(relativeMovement));
                            builder.AppendAsKey("←↖↗→", new Color(168, 247, 255));
                            builder.EndMenuArea();
                        }),
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyCtrl)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Skip turn",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeySpace)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Slash",
                        (TextElementer)(builder => builder.AppendAsKey("X")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Draw/Sheathe",
                        (TextElementer)(builder => builder.AppendAsKey("C")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Grappling Hook",
                        (TextElementer)(builder => builder.AppendAsKey("V")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Sheathing while holding a heart restores {0}.",
                        (TextElementer)(builder => {
                            builder.StartMenuArea(10, new TooltipProviderFunction(heartInfo));
                            builder.AppendDescribe(Symbol.Heart, "1", Color.White);
                            builder.EndMenuArea();
                        }));
                }),
            }));
        }

        private void InitMomentum()
        {
            var format = new TextFormatting()
            {
                Bold = false,
                GetParams = (pos) => new DialogParams()
                {
                    Color = Color.White,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            var dialog = new DialogFormattingIdentity();
            MomentumText = new TextBuilder(float.PositiveInfinity, float.PositiveInfinity);
            MomentumText.StartLine(LineAlignment.Left);
            MomentumText.AppendElement(new TextElementDynamic(() => Momentum, 64, format, dialog));
            MomentumText.EndLine();
            MomentumText.EndContainer();
            MomentumText.Finish();
        }

        private void InitScore()
        {
            var formatBack = new TextFormatting()
            {
                Bold = true,
                GetParams = (pos) => new DialogParams()
                {
                    Color = Color.Gray,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            var formatFront = new TextFormatting()
            {
                Bold = true,
                GetParams = (pos) => new DialogParams()
                {
                    Color = Color.White,
                    Border = Color.Black,
                    Scale = Vector2.One,
                }
            };
            var dialog = new DialogFormattingIdentity();
            ScoreText = new TextBuilder(float.PositiveInfinity, float.PositiveInfinity);
            ScoreText.StartLine(LineAlignment.Right);
            ScoreText.AppendElement(new TextElementCounter(() => (int)Math.Round(Score.Value), 8, formatFront, formatBack, dialog));
            ScoreText.EndLine();
            ScoreText.EndContainer();
            ScoreText.Finish();
        }

        private bool IsGameOver()
        {
            return Scene.IsGameOver;
        }

        public override void Update(Scene scene)
        {
            if (IsGameOver())
            {
                GameOver += 1;
            }

            if (Scene.Score != Score.End)
                Score.Set(Scene.Score, LerpHelper.Quadratic, 20);
            Score.Update();

            base.Update(scene);
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            InputTwinState state = Scene.InputState;

            if (IsGameOver())
            {
                if (GameOver.Done && !GameOverMenu.IsOpen)
                {
                    var gameOverMenu = new GameOverMenu(Scene);
                    GameOverMenu.Open(gameOverMenu);
                }
                return;
            }

            var player = Player.GetBehavior<BehaviorPlayer>();

            if (Scene.WaitForPlayer && !Scene.WaitForCutscene)
            {
                if (player.Gripped)
                    SubMenu.Open(new GripInput(Scene));

                var direction = InputUtil.WASDKeys.GetDirectionPressed(state, 15, 1);
                if (!direction.HasValue)
                    direction = InputUtil.GetDirectionNumpadPressed(state, 15, 1);
                if (direction.HasValue)
                {
                    var angleInput = MathHelper.WrapAngle(Util.PointToAngle(direction.Value));
                    var anglePlayer = MathHelper.WrapAngle(Player.GetVisualAngle());
                    var angleDelta = MathHelper.WrapAngle(angleInput - anglePlayer);
                    if (!state.IsKeyDown(Keys.LeftControl))
                    {
                        if (Math.Abs(Util.GetAngleDistance(angleDelta, MathHelper.Pi)) > 0.001f)
                        {
                            var position = AnglePositionMapper.GetClosestAngle(angleDelta);
                            if (position == 0)
                            {
                                player.Accelerate();
                            }
                            else if (position == -1 || position == -2 || position == -3)
                            {
                                player.TurnLeft();
                            }
                            else if (position == +1 || position == +2 || position == +3)
                            {
                                player.TurnRight();
                            }
                        }
                        else //Brake
                        {
                            player.Decelerate();
                        }
                    }
                    else
                    {
                        if (Math.Abs(Util.GetAngleDistance(angleDelta, MathHelper.Pi)) > 0.001f)
                        {
                            var position = AnglePositionMapper.GetClosestAngle(angleDelta);

                            if (position == -1 || position == -2)
                            {
                                player.Steppy(-MathHelper.PiOver4);
                            }
                            else if (position == +1 || position == +2)
                            {
                                player.Steppy(+MathHelper.PiOver4);
                            }
                        }
                    }
                }
                if (state.IsKeyPressed(Keys.Space, 15, 1))
                {
                    player.WaitForPassive();
                }
                if (state.IsKeyPressed(Keys.X) && player.SwordReady)
                {
                    SubMenu.Open(new SwordInput(Scene));
                }
                if (state.IsKeyPressed(Keys.V))
                {
                    SubMenu.Open(new GrappleInput(Scene));
                }
                if (state.IsKeyPressed(Keys.C))
                {
                    var sword = Player.GetBehavior<BehaviorSword>();
                    if(sword != null && Math.Abs(sword.Position) == 3)
                    {
                        player.SheatheSword();
                    }
                    else
                    {
                        player.DrawSword(3);
                    }
                }
            }
        }

        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };

        public override void Draw(Scene scene)
        {
            SceneGame sceneGame = (SceneGame)scene;

            var player = Player.GetBehavior<BehaviorPlayer>();
            var alive = Player.GetBehavior<BehaviorAlive>();
            if(alive != null)
                DrawHearts(scene, alive);
            if(player != null)
                DrawCompass(scene, player.Momentum);
            DrawScore(scene);
            DrawCards(scene);

            SubMenu.Draw(scene);

            var ui = SpriteLoader.Instance.AddSprite("content/ui_box");

            if (!SubMenu.IsOpen && !GameOverMenu.IsOpen)
            {
                ControlInfo.Draw(scene);
            }

            scene.SpriteBatch.Draw(scene.Pixel, new Rectangle(0, 0, scene.Viewport.Width, scene.Viewport.Height), new Color(0, 0, 0, GameOver.Slide * 0.5f));

            GameOverMenu.Draw(scene);
        }

        private void DrawCards(Scene scene)
        {
            var origin = new Vector2(scene.Viewport.Width - 32, scene.Viewport.Height * 1 / 5);
            for (int i = 0; i < Scene.Cards.Count; i++)
            {
                var card = Scene.Cards[i % Scene.Cards.Count];
                var pos = origin + new Vector2(0, 28) * (i % 16) + new Vector2(-16, 12) * (i / 16);
                scene.DrawSpriteExt(card.Sprite, 0, pos - card.Sprite.Middle, card.Sprite.Middle, 0, new Vector2(2), SpriteEffects.None, Color.White, 0);
            }
        }

        private void DrawScore(Scene scene)
        {
            /*int score = (int)Math.Round(Math.Min(Score.Value, 99999999));
            string text = score.ToString();
            string textBackground = "00000000";
            var textParameters = new TextParameters().SetColor(Color.White, Color.Black).SetBold(true);
            var textParametersBackground = new TextParameters().SetColor(Color.Gray, Color.Black).SetBold(true);
            var position = new Vector2(scene.Viewport.Width - 32, 32);
            scene.PushSpriteBatch(transform: Matrix.CreateTranslation(new Vector3(-position, 0)) * Matrix.CreateScale(new Vector3(2, 2, 0)) * Matrix.CreateTranslation(new Vector3(position, 0)));
            scene.DrawText(textBackground, position, Alignment.Right, textParametersBackground);
            scene.DrawText(text, position, Alignment.Right, textParameters);
            scene.PopSpriteBatch();*/
            ScoreText.Draw(new Vector2(scene.Viewport.Width - 32, 32), FontRenderer, Matrix.CreateScale(2,2,1));
        }

        private void DrawHearts(Scene scene, BehaviorAlive alive)
        {
            var spriteHeartEmpty = SpriteLoader.Instance.AddSprite("content/ui_heart_empty");
            var spriteHeartFill = SpriteLoader.Instance.AddSprite("content/ui_heart_fill");

            var compassX = scene.Viewport.Width / 2;
            var compassY = scene.Viewport.Height - 64;

            for (int i = (int)alive.HP - 1; i >= 0; i--)
            {
                scene.DrawSpriteExt(spriteHeartEmpty, 0, new Vector2(compassX - 160 - 24 * i, compassY - 8 + 40) - spriteHeartEmpty.Middle, spriteHeartEmpty.Middle, 0, SpriteEffects.None, 0);
                if(i >= alive.Damage)
                    scene.DrawSpriteExt(spriteHeartFill, 0, new Vector2(compassX - 160 - 24 * i, compassY - 8 + 40) - spriteHeartFill.Middle, spriteHeartFill.Middle, 0, SpriteEffects.None, 0);
            }
        }

        private void DrawCompass(Scene scene, Momentum momentum)
        {
            var spriteCompass = SpriteLoader.Instance.AddSprite("content/ui_compass");
            var spriteCompassNeedle = SpriteLoader.Instance.AddSprite("content/ui_compass_needle");
            var spriteMomentum = SpriteLoader.Instance.AddSprite("content/ui_momentum");
            var compassX = scene.Viewport.Width / 2;
            var compassY = scene.Viewport.Height - 64;
            scene.DrawSpriteExt(spriteCompass, 0, new Vector2(compassX, compassY) - spriteCompass.Middle, spriteCompass.Middle, 0, SpriteEffects.None, 0);
            if(momentum.Direction != Vector2.Zero)
                scene.DrawSpriteExt(spriteCompassNeedle, 0, new Vector2(compassX, compassY) - spriteCompassNeedle.Middle, spriteCompassNeedle.Middle, Util.VectorToAngle(momentum.Direction), SpriteEffects.None, 0);
            scene.DrawUI(SpriteLoader.Instance.AddSprite("content/ui_box"), new Rectangle(compassX - 128, compassY - 8 + 32, 256, 16), Color.White);
            for (int i = Math.Min(momentum.Amount, 32) - 1; i >= 0; i--)
            {
                scene.DrawSprite(spriteMomentum, 0, new Vector2(compassX - 128 + i * 8, compassY - 8 + 32), SpriteEffects.None, 0);
            }
            if (momentum.Amount > 32)
            {
                MomentumText.Draw(new Vector2(compassX + 128 + 8, compassY - 11 + 32), FontRenderer);
            }
        }
    }

    class SwordInput : Menu, IDrawable
    {
        static Dictionary<float, int> AnglePositionMapper = Enumerable.Range(-3, 7).ToDictionary(x => MathHelper.PiOver4 * x, x => x);

        SceneGame Scene;
        SubMenuHandler<ControlInfo> ControlInfo = new SubMenuHandler<ControlInfo>();
        
        public ICurio Player => Scene.PlayerCurio;
        public double DrawOrder => 0;
        public int TargetPosition;
        public LerpFloat TargetAngle = new LerpFloat(0);

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public SwordInput(SceneGame scene)
        {
            Scene = scene;
            ControlInfo.Open(new Menus.ControlInfo(scene, new[] {
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Select Slash direction.",
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0}+{1} Select diagonal Slash direction.",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyShift)),
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Confirm Slash.",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEnter)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Cancel.", (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEscape)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Wide slashes take more time to complete.");
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Slashing while stabbing an enemy rips the {0} out.", 
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.Heart)));
                }),
            }));
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            InputTwinState state = Scene.InputState;

            var player = Player.GetBehavior<BehaviorPlayer>();

            if (Scene.WaitForPlayer && !Scene.WaitForCutscene)
            {
                var direction = InputUtil.WASDKeys.GetDirectionPressed(state, 15, 1);
                if (!direction.HasValue)
                    direction = InputUtil.GetDirectionNumpadPressed(state, 15, 1);
                if (direction.HasValue)
                {
                    var angleInput = MathHelper.WrapAngle(Util.PointToAngle(direction.Value));
                    var anglePlayer = MathHelper.WrapAngle(Player.GetVisualAngle());
                    var angleDelta = MathHelper.WrapAngle(angleInput - anglePlayer);
                    if (Math.Abs(Util.GetAngleDistance(angleDelta,MathHelper.Pi)) > 0.001f)
                    {
                        var position = AnglePositionMapper.GetClosestAngle(angleDelta);
                        TargetPosition = position;
                        TargetAngle.Set(BehaviorSword.GetAngle(TargetPosition), LerpHelper.QuadraticOut, 20);
                    }
                }
                if(state.IsKeyPressed(Keys.Enter))
                {
                    player.SlashSword(TargetPosition);
                    Close();
                }
                if(state.IsKeyPressed(Keys.Escape))
                {
                    Close();
                }
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            TargetAngle.Update();
        }

        public override void Draw(Scene scene)
        {
            ControlInfo.Draw(scene);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var slashRight = SpriteLoader.Instance.AddSprite("content/ui_slash_right");
            var slashLeft = SpriteLoader.Instance.AddSprite("content/ui_slash_left");
            var pos = Player.GetVisualTarget();
            var bodyAngle = Player.GetVisualAngle();
            var sword = Player.GetBehavior<BehaviorSword>();

            float angleStart = bodyAngle + sword.VisualAngle();
            float angleEnd = bodyAngle + TargetAngle.Value;
            var sprite = angleStart < angleEnd ? slashRight : slashLeft;
            float h = angleStart;
            angleStart = angleEnd;
            angleEnd = h;
            float slide = (angleEnd - angleStart) / MathHelper.TwoPi;
            float precisionModified = 1 * slide;
            scene.DrawCircle(sprite, SamplerState, pos, 50, angleStart, angleEnd, 24, 0, precisionModified, 0.5f, 1f, ColorMatrix.Identity, scene.NonPremultiplied);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UIWorld;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return true;
        }
    }

    class GrappleInput : Menu, IDrawable
    {
        static Dictionary<float, int> AnglePositionMapper = Enumerable.Range(-3, 7).ToDictionary(x => MathHelper.PiOver4 * x, x => x);

        SceneGame Scene;
        SubMenuHandler<ControlInfo> ControlInfo = new SubMenuHandler<ControlInfo>();

        public ICurio Player => Scene.PlayerCurio;
        public double DrawOrder => 0;
        public int TargetPosition;
        public LerpFloat TargetAngle = new LerpFloat(0);

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public GrappleInput(SceneGame scene)
        {
            Scene = scene;
            ControlInfo.Open(new Menus.ControlInfo(scene, new[] {
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Select Grapple direction.",
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0}+{1} Select diagonal Grapple direction.", 
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyShift)),
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Confirm Grapple.", 
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEnter)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Cancel.", 
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEscape)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("You cannot grapple in the same direction as your sword.");
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Grappling an injured enemy rips the {0} out.", 
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.Heart)));
                }),
            }));
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            InputTwinState state = Scene.InputState;

            var player = Player.GetBehavior<BehaviorPlayer>();
            var sword = Player.GetBehavior<BehaviorSword>();

            if (Scene.WaitForPlayer && !Scene.WaitForCutscene)
            {
                var direction = InputUtil.WASDKeys.GetDirectionPressed(state, 15, 1);
                if (!direction.HasValue)
                    direction = InputUtil.GetDirectionNumpadPressed(state, 15, 1);
                if (direction.HasValue)
                {
                   
                    var angleInput = MathHelper.WrapAngle(Util.PointToAngle(direction.Value));
                    var anglePlayer = MathHelper.WrapAngle(Player.GetVisualAngle());
                    var angleDelta = MathHelper.WrapAngle(angleInput - anglePlayer);
                    if (Math.Abs(Util.GetAngleDistance(angleDelta, MathHelper.Pi)) > 0.001f)
                    {
                        var position = AnglePositionMapper.GetClosestAngle(angleDelta);
                        if (sword == null || sword.Position != position)
                        {
                            TargetPosition = position;
                            TargetAngle.Set(BehaviorSword.GetAngle(TargetPosition), LerpHelper.QuadraticOut, 20);
                        }
                    }
                }
                if (state.IsKeyPressed(Keys.Enter) && (sword == null || sword.Position != TargetPosition))
                {
                    player.Grapple(TargetPosition);
                    Close();
                }
                if (state.IsKeyPressed(Keys.Escape))
                {
                    Close();
                }
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            TargetAngle.Update();
        }

        public override void Draw(Scene scene)
        {
            ControlInfo.Draw(scene);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var sword = Player.GetBehavior<BehaviorSword>();
            if (sword == null || sword.Position != TargetPosition)
            {
                var target = SpriteLoader.Instance.AddSprite("content/ui_grapple_target");
                var tile = Player.GetMainTile();
                var pos = tile.VisualPosition + new Vector2(8, 8);
                var bodyAngle = Player.GetVisualAngle();
                float angle = bodyAngle + TargetAngle;
                scene.DrawSpriteExt(target, 0, pos - target.Middle, target.Middle, angle, SpriteEffects.None, 0);
            }
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UIWorld;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return true;
        }
    }

    class GripInput : Menu, IDrawable
    {
        static Dictionary<float, int> AnglePositionMapper = Enumerable.Range(-3, 7).ToDictionary(x => MathHelper.PiOver4 * x, x => x);

        SceneGame Scene;
        SubMenuHandler<ControlInfo> ControlInfo = new SubMenuHandler<ControlInfo>();

        public ICurio Player => Scene.PlayerCurio;
        public double DrawOrder => 0;
        public int TargetPosition;
        public LerpFloat TargetAngle = new LerpFloat(0);

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();
        static SamplerState SamplerState = new SamplerState()
        {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Clamp,
            Filter = TextureFilter.Point,
        };
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public GripInput(SceneGame scene)
        {
            Scene = scene;
            ControlInfo.Open(new Menus.ControlInfo(scene, new[] {
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Select Dash direction.",
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0}+{1} Select diagonal Dash direction.",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyShift)),
                        (TextElementer)(builder => builder.AppendAsKey("WASD")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Confirm Dash.",
                        (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEnter)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("{0} Cancel.", (TextElementer)(builder => builder.AppendSymbol(Symbol.KeyEscape)));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("You can only dash in {0}.", (TextElementer)(builder => builder.AppendAsKey("↖↑↗")));
                }),
                new ControlInfo.ControlTag(textBuilder =>
                {
                    textBuilder.AppendFormat("Dash Cancel resets momentum to 0.");
                }),
            }));
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            InputTwinState state = Scene.InputState;

            var player = Player.GetBehavior<BehaviorPlayer>();

            if (Scene.WaitForPlayer && !Scene.WaitForCutscene)
            {
                var direction = InputUtil.WASDKeys.GetDirectionPressed(state, 15, 1);
                if (!direction.HasValue)
                    direction = InputUtil.GetDirectionNumpadPressed(state, 15, 1);
                if (direction.HasValue)
                {
                    var angleInput = MathHelper.WrapAngle(Util.PointToAngle(direction.Value));
                    var anglePlayer = MathHelper.WrapAngle(Player.GetVisualAngle());
                    var angleDelta = MathHelper.WrapAngle(angleInput - anglePlayer);
                    if (Math.Abs(Util.GetAngleDistance(angleDelta, MathHelper.Pi)) > 0.001f)
                    {
                        var position = AnglePositionMapper.GetClosestAngle(angleDelta);
                        if (Math.Abs(position) <= 1)
                        {
                            TargetPosition = position;
                            TargetAngle.Set(BehaviorSword.GetAngle(TargetPosition), LerpHelper.QuadraticOut, 20);
                        }
                    }
                }
                if (state.IsKeyPressed(Keys.Enter))
                {
                    player.Dash(TargetPosition);
                    Close();
                }
                if (state.IsKeyPressed(Keys.Escape))
                {
                    player.DashCancel(TargetPosition);
                    Close();
                }
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            TargetAngle.Update();
        }

        public override void Draw(Scene scene)
        {
            ControlInfo.Draw(scene);
        }

        public void Draw(SceneGame scene, DrawPass pass)
        {
            var target = SpriteLoader.Instance.AddSprite("content/ui_grapple_target");
            var tile = Player.GetMainTile();
            var pos = tile.VisualPosition + new Vector2(8, 8);
            var bodyAngle = Player.GetVisualAngle();
            float angle = bodyAngle + TargetAngle;
            scene.DrawSpriteExt(target, 0, pos - target.Middle, target.Middle, angle, SpriteEffects.None, 0);
        }

        public void DrawIcon(SceneGame scene, Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.UIWorld;
        }

        public bool ShouldDraw(SceneGame scene, Vector2 cameraPosition)
        {
            return true;
        }
    }

    class GameOverMenu : MenuActNew
    {
        SceneGame Scene;
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public GameOverMenu(SceneGame scene) : base(scene, null, new Vector2(scene.Viewport.Width / 2, scene.Viewport.Height * 3 / 4), SpriteLoader.Instance.AddSprite("content/ui_box"), SpriteLoader.Instance.AddSprite("content/ui_box"), 300, 64)
        {
            Scene = scene;
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Return to Title");
                builder.NewLine();
                builder.AppendText("Return to Titlescreen.", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                scene.ReturnToTitle();
            }));
            Add(new ActActionNew((builder) => {
                builder.StartLine(LineAlignment.Center);
                builder.AppendText("Quit");
                builder.NewLine();
                builder.AppendText("Quit to Desktop.", FormatDescription);
                builder.EndLine();
            }, () =>
            {
                scene.Quit();
            }));
        }

        public override void Draw(Scene scene)
        {
            base.Draw(scene);

            /*string gameOverName = $"{Game.FormatColor(Color.White)}GAME OVER";
            if (Scene.GameOverWin)
                gameOverName = $"{Game.FormatColor(Color.White)}VICTORY ACHIEVED";

            int width = scene.Viewport.Width * 3 / 5;
            int x = scene.Viewport.Width / 2;
            int y = scene.Viewport.Height * 1 / 3;
            TextParameters parameters = new TextParameters().SetColor(Color.DarkGray, Color.Black).SetConstraints(width, null);
            string text = $"{Game.FORMAT_BOLD}{gameOverName}{Game.FORMAT_RESET}\n{Scene.GameOverReason}";
            int height = FontUtil.GetStringHeight(text, parameters);

            scene.DrawUI(SpriteLoader.Instance.AddSprite("content/ui_gab"), new Rectangle(x - width / 2, y, width, height), Color.White);
            scene.DrawText(text, new Vector2(x - width / 2, y), Alignment.Center, parameters);

            StringBuilder builderStats = new StringBuilder();
            builderStats.Append($"{Game.FORMAT_BOLD}Score:{Game.FORMAT_BOLD} {Scene.Score}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Level:{Game.FORMAT_BOLD} {Scene.Level}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Kills:{Game.FORMAT_BOLD} {Scene.Kills}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Gibs:{Game.FORMAT_BOLD} {Scene.Gibs}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Splats Collected:{Game.FORMAT_BOLD} {Scene.Splats}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Hearts Ripped:{Game.FORMAT_BOLD} {Scene.HeartsRipped}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Hearts Eaten:{Game.FORMAT_BOLD} {Scene.HeartsEaten}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Rats Hunted:{Game.FORMAT_BOLD} {Scene.RatsHunted}\n");
            builderStats.Append($"{Game.FORMAT_BOLD}Cards Crushed:{Game.FORMAT_BOLD} {Scene.CardsCrushed}\n");
            
            TextParameters parametersStats = new TextParameters().SetColor(Color.White, Color.Black).SetConstraints(width, null);
            scene.DrawText(builderStats.ToString(), new Vector2(x - width / 2, y + 64), Alignment.Center, parametersStats);*/
        }
    }

    class BetweenLevelMenu : Menu
    {
        class UICard
        {
            static Random Random = new Random();

            public Card Card;
            public bool Selected;
            public LerpFloat Raised = new LerpFloat(0);
            public LerpFloat Flash = new LerpFloat(0);
            public LerpFloat Shake = new LerpFloat(0);
            public LerpVector2 Position;

            public Vector2 CurrentPosition => Position + new Vector2(0, -Raised * 3);

            public UICard(Card card, Vector2 start)
            {
                Card = card;
                Position = new LerpVector2(start);
            }

            public void Update()
            {
                Position.Update();
                Raised.Update();
                Shake.Update();
                Flash.Update();
            }

            public void Draw(SceneGame scene)
            {
                /*var sprite = Card.Sprite;
                Vector2 shake = Util.AngleToVector(Random.NextAngle()) * Shake;
                Vector2 pos = CurrentPosition + shake;
                var rect = new Rectangle((int)pos.X-80,(int)pos.Y,160,256);
                var textParameters = new TextParameters().SetBold(true).SetColor(Color.White, Color.Black);
                var descriptionParameters = new TextParameters().SetBold(false).SetColor(Color.White, Color.Black).SetConstraints(rect);

                scene.PushSpriteBatch(shader: scene.Shader, shaderSetup: (transform, projection) =>
                {
                    scene.SetupColorMatrix(ColorMatrix.Lerp(ColorMatrix.Identity, ColorMatrix.Flat(Color.White), Flash), transform, projection);
                });
                scene.DrawText(Card.Name, pos + new Vector2(0, -64), Alignment.Center, textParameters);
                if(sprite != null)
                    scene.DrawSpriteExt(sprite, 0, pos - sprite.Middle, sprite.Middle, 0, new Vector2(3), SpriteEffects.None, Color.White, 0);
                scene.DrawText(Card.Description, rect.Location.ToVector2() + new Vector2(0, 64), Alignment.Center, descriptionParameters);
                scene.PopSpriteBatch();*/
            }
        }

        static Random Random = new Random();

        SceneGame Scene;
        SubMenuHandler<ControlInfo> ControlInfo = new SubMenuHandler<ControlInfo>();
        Wait CurrentAction = Wait.NoWait;
        List<UICard> Cards = new List<UICard>();

        protected override IEnumerable<IMenuArea> MenuAreas => Enumerable.Empty<IMenuArea>();
        public override FontRenderer FontRenderer => Scene.FontRenderer;

        public BetweenLevelMenu(SceneGame scene)
        {
            Scene = scene;
            /*ControlInfo.Open(new Menus.ControlInfo(new[] {
                $"{FormatAsKey("WASD")} Select Card",
                $"{FormatAsKey("1-9")} Select Card",
                $"{Game.FormatSymbol(Symbol.KeyEnter)} Accept selected Card",
                $"{Game.FormatSymbol(Symbol.KeyEscape)} Reject all Cards",
            }));*/
            CurrentAction = Scheduler.Instance.RunAndWait(RoutineSetup());
        }

        public void Select(int n)
        {
            n = Util.PositiveMod(n, Cards.Count);
            for(int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];
                card.Selected = i == n;
                if (card.Selected)
                {
                    card.Flash.Set(1, 0, LerpHelper.Flick, 5);
                    card.Raised.Set(16, LerpHelper.Quadratic, 10);
                }
                else
                    card.Raised.Set(0, LerpHelper.Quadratic, 10);
            }
        }

        UICard GetSelected()
        {
            return Cards.Find(x => x.Selected);
        }

        public int? GetSelectedIndex()
        {
            var index = Cards.FindIndex(x => x.Selected);
            if(index < 0)
                return null;
            return index;
        }

        public IEnumerable<Wait> RoutineSetup()
        {
            var center = new Vector2(Scene.Viewport.Width / 2, Scene.Viewport.Height / 4);
            var deck = new Deck(Scene);
            deck.FillStandard();
            //Collect flames here
            yield return new WaitTime(100);
            deck.Shuffle();
            for (int i = 0; i < 3; i++)
            {
                var card = new UICard(deck.Draw(), center);
                Cards.Add(card);
            }
            int cardWidth = 64;
            int width = 600;
            int offset = (Scene.Viewport.Width - width) / 2;
            int widthInterior = width - cardWidth;
            int segmentWidth = widthInterior / (Cards.Count - 1);
            for (int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];
                int x = offset + i * segmentWidth + cardWidth / 2;
                card.Position.Set(new Vector2(x, Scene.Viewport.Height / 2), LerpHelper.CubicOut, 30);
                yield return new WaitTime(5);
            }
            yield return new WaitTime(30);
            foreach (var card in Cards)
            {
                card.Flash.Set(1, 0, LerpHelper.Flick, 5);
            }
        }

        public IEnumerable<Wait> RoutineReject()
        {
            foreach (var card in Cards)
            {
                card.Flash.Set(1, LerpHelper.QuadraticIn, 100);
                card.Shake.Set(4, LerpHelper.QuadraticIn, 100);
                card.Raised.Set(0, LerpHelper.Quadratic, 10);
            }
            yield return new WaitTime(100);
            foreach (var card in Cards)
            {
                Vector2 center = card.Position;
                for (int i = 0; i < 60; i++)
                {
                    var emit = center + Util.AngleToVector(Random.NextAngle()) * (32 + (Random.NextFloat() - 0.5f) * 16);
                    var velocity = Vector2.Normalize(emit - center);
                    int time = Random.Next(10, 30);
                    new SmokeParticleTimeless(Scene, SpriteLoader.Instance.AddSprite("content/effect_shard"), emit, (time * time) / 10)
                    {
                        StartVelocity = velocity * Random.Next(100, 200),
                        EndVelocity = velocity * Random.Next(100, 200),
                        StartTime = 2f,
                        EndTime = 1,
                        StartVelocityLerp = LerpHelper.QuadraticIn,
                        EndVelocityLerp = LerpHelper.QuadraticOut,
                        FlickerTime = Random.Next(10, 50),
                        DrawPass = DrawPass.UI,
                    };
                }
                Scene.AddUIScore(5000, card.Position, ScoreType.Big);
                Scene.CardsCrushed += 1;
            }
            Cards.Clear();
            yield return new WaitTime(100);
            Scene.WaitBetweenLevel.Complete();
            Close();
        }

        public IEnumerable<Wait> RoutineSelect()
        {
            var selected = GetSelected();
            selected.Raised.Set(32, LerpHelper.QuadraticOut, 20);
            selected.Flash.Set(1, LerpHelper.QuadraticOut, 10);
            yield return new WaitTime(25);
            Cards.Remove(selected);
            selected.Card.Apply(Scene, selected.CurrentPosition);
            Scene.Cards.Add(selected.Card);
            yield return new WaitTime(100);
            Scene.WaitBetweenLevel.Complete();
            Close();
        }

        public override void HandleInput(Scene scene)
        {
            base.HandleInput(scene);

            if (InputBlocked)
                return;

            InputTwinState state = Scene.InputState;

            if(CurrentAction.Done)
            {
                int? selectionIndex = GetSelectedIndex();

                if (state.IsKeyPressed(Keys.W) && Cards.Count % 2 == 1)
                {
                    Select(Cards.Count / 2);
                }
                if (state.IsKeyPressed(Keys.A, 15, 5))
                {
                    if (selectionIndex.HasValue)
                        Select(selectionIndex.Value - 1);
                    else
                        Select(0);
                }
                if (state.IsKeyPressed(Keys.D, 15, 5))
                {
                    if (selectionIndex.HasValue)
                        Select(selectionIndex.Value + 1);
                    else
                        Select(Cards.Count - 1);
                }

                var number = InputUtil.NumberKeys.GetNumberPressed(state);

                if(number.HasValue && number.Value <= Cards.Count)
                {
                    Select(number.Value - 1);
                }

                if (state.IsKeyPressed(Keys.Escape))
                {
                    CurrentAction = Scheduler.Instance.RunAndWait(RoutineReject());
                }
                if (state.IsKeyPressed(Keys.Enter) && selectionIndex.HasValue)
                {
                    CurrentAction = Scheduler.Instance.RunAndWait(RoutineSelect());
                }
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            foreach(var card in Cards)
                card.Update();
        }

        public override void Draw(Scene scene)
        {
            ControlInfo.Draw(scene);

            foreach (var card in Cards)
                card.Draw(Scene);
        }
    }
}
