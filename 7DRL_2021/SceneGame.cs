using _7DRL_2021.Behaviors;
using _7DRL_2021.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    class DeferredList<T> : List<T>
    {
        List<T> ToAdd = new List<T>();

        public DeferredList()
        {

        }

        public void AddLater(T item)
        {
            ToAdd.Add(item);
        }

        public void Update()
        {
            AddRange(ToAdd);
            ToAdd.Clear();
        }
    }

    class UpdateQueue
    {
        List<IBehaviorUpdateable> Updateables = new List<IBehaviorUpdateable>();

        public void Setup(IEnumerable<IBehaviorUpdateable> updateables)
        {
            Updateables.AddRange(updateables.OrderBy(x => x.UpdateOrder));
        }

        public bool Any()
        {
            return Updateables.Any();
        }

        public bool Empty()
        {
            return Updateables.Empty();
        }

        public IBehaviorUpdateable Dequeue()
        {
            var updateable = Updateables.First();
            Updateables.RemoveAt(0);
            return updateable;
        }

        public void AddImmediate(IBehaviorUpdateable updateable)
        {
            Updateables.Insert(0, updateable);
        }

        public void AddEnd(IBehaviorUpdateable updateable)
        {
            Updateables.Add(updateable);
        }

        public void RemoveAll(Predicate<IBehaviorUpdateable> predicate)
        {
            Updateables.RemoveAll(predicate);
        }

        public void RemoveByCurio(ICurio curio)
        {
            Updateables.RemoveAll(x => x is IBehaviorUpdateableCurio update && update.Curio == curio);
        }

        public IEnumerable<IBehaviorUpdateable> GetByCurio(ICurio curio)
        {
            return Updateables.Where(x => x is IBehaviorUpdateableCurio update && update.Curio == curio);
        }
    }

    public class WaitForInput : Wait
    {
        private bool Completed;
        public override bool Done => Completed;

        public WaitForInput()
        {
        }

        public void Complete()
        {
            Completed = true;
        }

        public override void Update()
        {
            //NOOP
        }
    }

    class SceneGame : Scene
    {
        const float ViewScale = 2.0f;

        public Vector2 Camera => CameraCurio.GetVisualTarget();
        public Vector2 CameraPosition => Camera - CameraSize / 2;
        public Vector2 CameraSize => new Vector2(Viewport.Width / 2, Viewport.Height / 2);

        public PlayerUI Menu;
        public Map Map;
        public List<SliderTime> Timers = new List<SliderTime>();
        public DeferredList<VisualEffect> VisualEffects = new DeferredList<VisualEffect>();

        public DoubleBuffer CameraTarget = new DoubleBuffer();
        public RenderTarget2D DistortionMap;
        public RenderTarget2D BloodMapAdditive;
        public RenderTarget2D BloodMapMultiply;

        TextBuilder TooltipText;
        public Point? TileCursor;

        public bool IsGameOver => RunStats.GameOverType != null;
        public GameOverType GameOverType => RunStats.GameOverType;

        public ICurio PlayerCurio;
        public ICurio CameraCurio;
        public Wait Cutscene = Wait.NoWait;
        public override float TimeMod => (WaitForPlayer || WaitForCutscene) ? 0 : TimeModStandard;
        
        public bool WaitForPlayer => PlayerCurio.GetActionHolder(ActionSlot.Active)?.Done ?? false;
        public bool WaitForCutscene => !Cutscene.Done;

        public WaitForInput WaitBetweenLevel;

        /*public int Level;
        public int Score;
        public List<Card> Cards = new List<Card>();
        public int Kills;
        public int Gibs;
        public int Splats;
        public int HeartsRipped;
        public int HeartsEaten;
        public int RatsHunted;
        public int CardsCrushed;*/
        public RunStats RunStats = new RunStats();

        HighscoreRunFile HighscoreRunFile;

        SoundReference SoundIngress = SoundLoader.AddSound("content/sound/ingress.wav");
        SoundReference SoundEgress = SoundLoader.AddSound("content/sound/escape.wav");
        SoundReference SoundBloodstain = SoundLoader.AddSound("content/sound/score_blood.wav");
        SoundReference SoundBloodstainImpact = SoundLoader.AddSound("content/sound/score_blob.wav");
        SoundReference SoundScore = SoundLoader.AddSound("content/sound/score.wav");
        SoundReference Theme = SoundLoader.AddSound("content/sound/music_movement.wav");
        SoundReference ThemeGameOver = SoundLoader.AddSound("content/sound/music_gameover.wav");
        MusicEffect CurrentTheme;
        MusicEffect CurrentGameOver;

        public SceneGame(Game game) : base(game)
        {
            PlayerCurio = new Curio(Template.Player);
            CameraCurio = new Curio(Template.Camera);

            Behavior.Apply(new BehaviorFollowCamera(PlayerCurio, CameraCurio));

            Map = new Map(this, 100, 100);

            GenerateMap();

            Menu = new PlayerUI(this);

            Cutscene = Scheduler.Instance.RunAndWait(RoutineStartLevel());

            string runName = $"run{DateTime.Now.ToString("ddMMyyyyHHmm")}";
            HighscoreRunFile = new HighscoreRunFile($"stats/runs/{runName}.json", RunStats);
        }

        public bool IsWithinCamera(Vector2 camera, Vector2 pos)
        {
            return Math.Max(Math.Abs(camera.X - pos.X), Math.Abs(camera.Y - pos.Y)) < 12 * 16; 
        }

        private void GenerateMap()
        {
            var generator = new MapGenerator();
            generator.Generate();
            generator.Print(Map);

            RunStats.Level += 1;

            new Curio(Template.BellTower).MoveTo(Map.GetTile(0, 0));

            var positions = Map.EnumerateTiles().Shuffle(Random);
            Point? start = null;

            var weak = new List<Template>();
            var strong = new List<Template>();

            bool hasOmicron = PlayerCurio.HasBehaviors<BehaviorOmicron>();

            //TODO: There was gonna be a tutorial with blood urns for smashing
            if (RunStats.Level >= 1)
                weak.Add(Template.Grunt);
            if (RunStats.Level >= 2)
                weak.Add(Template.Twitch);
            if (RunStats.Level >= 3)
                weak.Add(Template.Bulwark);
            if (RunStats.Level >= 4)
                strong.Add(Template.Lich);
            if (RunStats.Level >= 5)
                weak.Remove(Template.Grunt);
            if (RunStats.Level % 2 == 0)
                weak.Add(Template.Rat);

            weak.Add(Template.Executioner);
            weak.Add(Template.Lich);

            var toSpawn = new List<Template>();
            if (weak.Any())
                for (int i = 0; i < 30; i++)
                {
                    toSpawn.Add(weak.Pick(Random));
                }
            if (strong.Any())
                for (int i = 0; i < 5; i++)
                {
                    toSpawn.Add(strong.Pick(Random));
                }
            if (hasOmicron || (RunStats.Level >= 6 && RunStats.Level % 3 == 0))
                toSpawn.Add(Template.Nemesis);

            var spawned = new List<ICurio>();

            foreach (var tile in positions)
            {
                if (toSpawn.Any() && !tile.IsSolid() && !tile.IsChasm())
                {
                    var template = toSpawn[0];
                    toSpawn.RemoveAt(0);
                    var enemy = new Curio(template);
                    enemy.MoveTo(tile);
                    if(template != Template.Nemesis && template != Template.Rat)
                        spawned.Add(enemy);
                    if(template == Template.Nemesis && hasOmicron)
                        Behavior.Apply(new BehaviorKillTarget(enemy));
                }
                else if (tile.HasBehaviors<BehaviorLevelStart>())
                {
                    start = new Point(tile.X, tile.Y);
                }
                if (toSpawn.Empty() && start.HasValue)
                    break;
            }

            if(RunStats.Level % 3 == 0)
                foreach (var enemy in spawned.Shuffle(Random).Take(3))
                {
                    Behavior.Apply(new BehaviorKillTarget(enemy));
                }

            PlayerCurio.MoveTo(Map.GetTile(start.Value.X, start.Value.Y));
            CameraCurio.MoveTo(Map.GetTile(start.Value.X, start.Value.Y));
        }

        private void DestroyMap()
        {
            foreach (var curio in Map.Curios)
                curio.Destroy();
            foreach (var tile in Map.EnumerateTiles())
                tile.ClearBehaviors();
            VisualEffects.Clear();
        }

        private Wait WaitForInput()
        {
            if (WaitBetweenLevel?.Done ?? true)
                WaitBetweenLevel = new WaitForInput();
            return WaitBetweenLevel;
        }

        private Vector2 GetOutsidePosition(Vector2 pos, Point direction, int outsideDistance)
        {
            var offset = Vector2.Normalize(direction.ToVector2());
            BoundingBox box = new BoundingBox(new Vector3(0, 0, 0), new Vector3(Map.Width * 16, Map.Height * 16, 0));
            Ray ray = new Ray(new Vector3(pos, 0), new Vector3(offset, 0));
            if(box.Contains(ray.Position) == ContainmentType.Contains)
                ray = new Ray(new Vector3(pos + offset * Vector3.Distance(box.Min, box.Max), 0), new Vector3(-offset, 0));            

            var distance = ray.Intersects(box);

            if (distance.HasValue)
            {
                var end = ray.Position + ray.Direction * distance.Value;
                return new Vector2(end.X, end.Y) + offset * outsideDistance; 
            }
            return pos + offset * outsideDistance;
        }

        public IEnumerable<Wait> RoutineStartLevel()
        {
            var tile = PlayerCurio.GetMainTile();
            var movable = PlayerCurio.GetBehavior<BehaviorMovable>();
            var orientation = PlayerCurio.GetBehavior<BehaviorOrientable>();
            var start = tile.GetBehavior<BehaviorLevelStart>();
            var player = PlayerCurio.GetBehavior<BehaviorPlayer>();
            player.LevelTransition = true;
            orientation.OrientTo(Util.PointToAngle(start.Direction) + MathHelper.Pi);
            var pos = tile.VisualPosition;
            CameraCurio.TeleportVisual(GetOutsidePosition(pos, start.Direction, 150));
            PlayerCurio.TeleportVisual(GetOutsidePosition(pos, start.Direction, 200));
            player.Fade.Set(0);
            yield return new WaitTime(100);
            CameraCurio.MoveVisual(tile.VisualPosition, LerpHelper.QuadraticOut, new SliderScene(this, 150));
            yield return new WaitTime(100);
            CurrentTheme?.Stop();
            CurrentTheme = new MusicEffect(Theme);
            CurrentTheme.Volume.Set(0, 1, LerpHelper.QuadraticIn, 20);
            CurrentTheme.Play();
            SoundIngress.Play(1f, 0f, 0f);
            player.Fade.Set(1, LerpHelper.QuarticOut, 70);
            PlayerCurio.MoveVisual(tile.VisualPosition, LerpHelper.QuarticOut, new SliderScene(this, 100));
            yield return new WaitTime(70);
            player.LevelTransition = false;
            yield return new WaitTime(30);
           
        }

        public IEnumerable<Wait> RoutineEndLevel()
        {
            var tile = PlayerCurio.GetMainTile();
            var movable = PlayerCurio.GetBehavior<BehaviorMovable>();
            var orientation = PlayerCurio.GetBehavior<BehaviorOrientable>();
            var end = tile.GetBehavior<BehaviorLevelEnd>();
            var player = PlayerCurio.GetBehavior<BehaviorPlayer>();
            int stains = 0;
            float pitchSlide = 0;
           
            foreach (var bloodStain in VisualEffects.OfType<BloodStain>())
            {
                RunStats.Splats += 1;
                stains += 1;
                var center = PlayerCurio.GetVisualTarget();
                var emit = bloodStain.WorldPosition;
                var delta = Vector2.Normalize(emit - center);
                var offset = delta * Random.NextFloat(40, 80);
                int score;
                if (stains > 20)
                    score = 1000;
                else if (stains > 10)
                    score = 500;
                else
                    score = 50;
                pitchSlide = (float)LerpHelper.QuadraticIn(-1, 1, Math.Min(0, stains / 50f));

                var blob = new ScoreBlood(this, emit, center + delta * Random.NextFloat(4, 8), offset, Util.AngleToVector(Random.NextAngle()) * Random.NextFloat(80, 200), score, 20 + stains * 2);
                if(stains % 3 == 0)
                {
                    blob.Sound = SoundBloodstainImpact.CreateInstance();
                    blob.Sound.Pitch = pitchSlide;
                }
                bloodStain.Destroy();
            }
            SoundBloodstain.Play(1, pitchSlide, 0);
            yield return new WaitTime(30 + stains * 2);
            orientation.OrientTo(Util.PointToAngle(end.Direction), LerpHelper.QuadraticIn, new SliderScene(this, 10));
            player.Fade.Set(0, LerpHelper.QuadraticIn, 70);
            var pos = tile.VisualPosition;
            PlayerCurio.MoveVisual(GetOutsidePosition(pos, end.Direction, 200), LerpHelper.QuadraticIn, new SliderScene(this, 100));
            CameraCurio.MoveVisual(GetOutsidePosition(pos, end.Direction, 150), LerpHelper.QuadraticIn, new SliderScene(this, 100));
            SoundEgress.Play(1f, 0f, 0f);
            CurrentTheme.Volume.Set(0, LerpHelper.QuadraticIn, 90);
            player.LevelTransition = true;
            yield return new WaitTime(100);
            CurrentTheme.Stop(false);
            PlayerCurio.MoveTo(null);
            CameraCurio.MoveTo(null);
            DestroyMap();
            Menu.SubMenu.Open(new BetweenLevelMenu(this));
            yield return WaitForInput();
            GenerateMap();
            yield return Scheduler.Instance.RunAndWait(RoutineStartLevel());
        }

        public void ReturnToTitle()
        {
            DestroyMap();
            Game.Scene = new SceneTitle(Game);
            CurrentTheme?.Stop();
            CurrentGameOver?.Stop();
            Manager.Reset();
        }

        public void Restart()
        {
            DestroyMap();
            Game.Scene = new SceneLoading(Game);
            CurrentTheme?.Stop();
            CurrentGameOver?.Stop();
            Manager.Reset();
        }

        public void GameOver(GameOverType type)
        {
            if (IsGameOver)
                return;
            RunStats.GameOverType = type;
            CurrentTheme.Pitch.Set(-1, LerpHelper.QuadraticIn, 100);
            CurrentTheme.Volume.Set(0, LerpHelper.QuadraticIn, 100);
            if (RunStats.GameOverType.VictoryType != VictoryType.Win)
            {
                CurrentGameOver?.Stop();
                CurrentGameOver = new MusicEffect(ThemeGameOver);
                CurrentGameOver.Volume.Set(0, 1, LerpHelper.QuadraticIn, 240);
                CurrentGameOver.Play();
            }

            HighscoreRunFile.Flush(); //Write score to disk
        }

        private Matrix CreateViewMatrix()
        {
            return Matrix.Identity
                * Matrix.CreateTranslation(new Vector3(-CameraPosition, 0))
                * Matrix.CreateTranslation(new Vector3(-CameraSize / 2, 0)) //These two lines center the character on (0,0)
                * Matrix.CreateScale(ViewScale, ViewScale, 1) //Scale it up by 2
                * Matrix.CreateTranslation(Viewport.Width / 2, Viewport.Height / 2, 0); //Translate the character to the middle of the viewport
        }

        private void DrawTextures()
        {
            Menu.PreDraw(this);

            //var curios = Manager.GetCurios();
            var preDrawables = Manager.PreDrawable.GetPreDrawables();
            foreach (var behavior in preDrawables)
            {
                behavior.PreDraw(this);
            }
            foreach (var wave in VisualEffects.OfType<IPreDrawable>())
            {
                wave.PreDraw(this);
            }
            SetRenderTarget(BloodMapAdditive);
            GraphicsDevice.Clear(Color.Black);
            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            foreach (var bloodstain in VisualEffects.OfType<BloodStain>())
            {
                bloodstain.Draw(this, DrawPass.BloodAdditive);
            }
            PopSpriteBatch();
            SetRenderTarget(BloodMapMultiply);
            GraphicsDevice.Clear(Color.White);
            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            foreach (var bloodstain in VisualEffects.OfType<BloodStain>())
            {
                bloodstain.Draw(this, DrawPass.BloodMultiply);
            }
            PopSpriteBatch();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimeModifier();

            CurrentTheme?.Update();
            CurrentGameOver?.Update();

            UpdateProtoEffects();
            UpdateVisualEffects();

            foreach (var timer in Timers)
            {
                timer.Update();
            }
            Timers.RemoveAll(x => x.Slide >= 1);

            UpdateInput();

            var tickables = Manager.Tickable.GetTickables();
            foreach (var tickable in tickables.ToList())
                tickable.Tick(this);

            Vector2 worldPos = Vector2.Transform(new Vector2(InputState.MouseX, InputState.MouseY), Matrix.Invert(WorldTransform));
            int tileX = Util.FloorDiv((int)worldPos.X, 16);
            int tileY = Util.FloorDiv((int)worldPos.Y, 16);

            TileCursor = new Point(tileX, tileY);
            MenuCursor = Menu.GetMouseOver(InputState.MouseX, InputState.MouseY);
            if (MenuCursor != null)
                TileCursor = null;

            TooltipText = new TextBuilder(float.PositiveInfinity, float.PositiveInfinity);
            if (MenuCursor != null)
            {
                MenuCursor.GenerateTooltip(TooltipText);
            }
            else if (Map != null && TileCursor.HasValue)
            {
                MapTile tile = Map.GetTile(TileCursor.Value.X, TileCursor.Value.Y);
                if (tile != null)
                    tile.AddTooltip(TooltipText);
            }
            TooltipText.EndContainer();
            TooltipText.Finish();
        }

        private void UpdateInput()
        {
            InputTwinState state = Game.InputState;
            Menu.Update(this);
            Menu.HandleInput(this);
        }

        private void UpdateVisualEffects()
        {
            VisualEffects.Update();
            foreach (var visualEffect in VisualEffects)
            {
                visualEffect.Update();
            }
            VisualEffects.RemoveAll(x => x.Destroyed);
        }

        public override void Draw(GameTime gameTime)
        {
            CameraTarget.Setup(this, Viewport.Width, Viewport.Height);
            Util.SetupRenderTarget(this, ref DistortionMap, Viewport.Width, Viewport.Height);
            Util.SetupRenderTarget(this, ref BloodMapAdditive, Viewport.Width, Viewport.Height);
            Util.SetupRenderTarget(this, ref BloodMapMultiply, Viewport.Width, Viewport.Height);

            Projection = Matrix.CreateOrthographicOffCenter(0, Viewport.Width, Viewport.Height, 0, 0, -1);
            WorldTransform = CreateViewMatrix();
            ApplyScreenShake(ref WorldTransform);

            DrawTextures();

            SetRenderTarget(null);

            var cameraTile = CameraCurio.GetMainTile();
            //var tiles = cameraTile?.GetNearby(15) ?? Enumerable.Empty<MapTile>();
            //var curios = Manager.GetCurios().Where(x => !(x is MapTile)).Concat(tiles);
            //var gameObjects = curios.SelectMany(curio => curio.GetDrawables());
            var globalDrawables = Manager.Drawable.GetDrawables();
            var cameraPos = CameraCurio.GetVisualTarget();
            var drawPasses = globalDrawables
                .Concat(VisualEffects)
                .Concat(Menu.GetAllMenus().OfType<IDrawable>())
                .Where(x => x.ShouldDraw(this, cameraPos))
                .ToMultiLookup(x => x.GetDrawPasses());

            SetRenderTarget(CameraTarget.A);

            int width = 19 * 32;
            int height = 19 * 32;
            GraphicsDevice.ScissorRectangle = new Rectangle((Viewport.Width - width) / 2, (Viewport.Height - height) / 2, width, height);
            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, transform: WorldTransform, projection: Projection);
            PushSpriteBatch(transform: WorldTransform * Matrix.CreateTranslation(new Vector3(-CameraSize, 0)) * Matrix.CreateScale(0.80f) * Matrix.CreateTranslation(new Vector3(CameraSize, 0)));
            drawPasses.DrawPass(this, DrawPass.ChasmBottom);
            PopSpriteBatch();
            PushSpriteBatch(transform: WorldTransform * Matrix.CreateTranslation(new Vector3(-CameraSize, 0)) * Matrix.CreateScale(0.85f) * Matrix.CreateTranslation(new Vector3(CameraSize, 0)));
            drawPasses.DrawPass(this, DrawPass.Chasm1);
            PopSpriteBatch();
            PushSpriteBatch(transform: WorldTransform * Matrix.CreateTranslation(new Vector3(-CameraSize, 0)) * Matrix.CreateScale(0.90f) * Matrix.CreateTranslation(new Vector3(CameraSize, 0)));
            drawPasses.DrawPass(this, DrawPass.Chasm2);
            PopSpriteBatch();
            PushSpriteBatch(transform: WorldTransform * Matrix.CreateTranslation(new Vector3(-CameraSize, 0)) * Matrix.CreateScale(0.95f) * Matrix.CreateTranslation(new Vector3(CameraSize, 0)));
            drawPasses.DrawPass(this, DrawPass.Chasm3);
            PopSpriteBatch();
            drawPasses.DrawPass(this, DrawPass.Tile);
            drawPasses.DrawPass(this, DrawPass.Item);
            drawPasses.DrawPass(this, DrawPass.EffectLow);
            PushSpriteBatch(blendState: BlendState.Additive);
            drawPasses.DrawPass(this, DrawPass.EffectLowAdditive);
            PopSpriteBatch();
            drawPasses.DrawPass(this, DrawPass.WallBottom);
            drawPasses.DrawPass(this, DrawPass.Creature);
            PushSpriteBatch(blendState: BlendState.Additive);
            drawPasses.DrawPass(this, DrawPass.EffectCreatureAdditive);
            PopSpriteBatch();
            drawPasses.DrawPass(this, DrawPass.WallTop);
            drawPasses.DrawPass(this, DrawPass.Effect);
            PushSpriteBatch(blendState: BlendState.Additive);
            drawPasses.DrawPass(this, DrawPass.EffectAdditive);
            PopSpriteBatch();
            PopSpriteBatch();

            SetRenderTarget(CameraTarget.B);
            CameraTarget.Swap();

            //Draw screenflashes
            ColorMatrix color = ColorMatrix.Identity;
            ApplyScreenFlash(ref color);

            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(color, Matrix.Identity, Projection);
            });
            SpriteBatch.Draw(CameraTarget.B, CameraTarget.B.Bounds, Color.White);
            PopSpriteBatch();

            SetRenderTarget(CameraTarget.B);
            CameraTarget.Swap();

            //Draw glitches
            RenderGlitches(CameraTarget);

            //Draw to screen
            SetRenderTarget(null);

            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: NonPremultiplied, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            SpriteBatch.Draw(CameraTarget.B, CameraTarget.B.Bounds, Color.White);
            PopSpriteBatch();


            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: BlendState.Additive, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            SpriteBatch.Draw(BloodMapAdditive, BloodMapAdditive.Bounds, Color.White);
            PopSpriteBatch();
            PushSpriteBatch(samplerState: SamplerState.PointWrap, blendState: MultiplyBoth, shader: Shader, shaderSetup: (transform, projection) =>
            {
                SetupColorMatrix(ColorMatrix.Identity, Matrix.Identity, Projection);
            });
            SpriteBatch.Draw(BloodMapMultiply, BloodMapMultiply.Bounds, Color.White);
            PopSpriteBatch();

            //Draw cursor and tooltip
            SpriteReference cursor_tile = SpriteLoader.Instance.AddSprite("content/cursor_tile");

            SetupNormal(Matrix.Identity, Projection);
            //SpriteBatch.Begin(blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, samplerState: SamplerState.PointWrap, transformMatrix: WorldTransform);
            PushSpriteBatch(blendState: NonPremultiplied, samplerState: SamplerState.PointWrap, transform: WorldTransform, projection: Projection);

            if (TileCursor.HasValue)
            {
                DrawSprite(cursor_tile, Frame / 8, new Vector2(TileCursor.Value.X * 16, TileCursor.Value.Y * 16), SpriteEffects.None, 0);
            }

            drawPasses.DrawPass(this, DrawPass.UIWorld);

            //SpriteBatch.End();
            PopSpriteBatch();

            //SetupNormal(Matrix.Identity);
            //SpriteBatch.Begin(blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, samplerState: SamplerState.PointWrap);

            PushSpriteBatch(blendState: NonPremultiplied, samplerState: SamplerState.PointWrap, projection: Projection);

            drawPasses.DrawPass(this, DrawPass.UI);

            Menu.Draw(this);

            var mousePos = new Vector2(InputState.MouseX, InputState.MouseY);
            var mouseCursor = SpriteLoader.Instance.AddSprite("content/ui_mouse_cursor");
            DrawSprite(mouseCursor, 0, mousePos, SpriteEffects.None, 0);

            DrawTooltip();

            PopSpriteBatch();
        }

        private void DrawTooltip()
        {
            if (TooltipText != null && !TooltipText.IsEmpty())
            {
                SpriteReference ui_tooltip = SpriteLoader.Instance.AddSprite("content/ui_box");
                int tooltipWidth = (int)TooltipText.GetContentWidth();
                int screenWidth = Viewport.Width - 8 - InputState.MouseX + 4;
                bool invert = false;
                if (tooltipWidth > screenWidth)
                {
                    screenWidth = Viewport.Width - screenWidth;
                    invert = true;
                }
                int tooltipHeight = (int)TooltipText.GetContentHeight();
                int tooltipX = InputState.MouseX + 4;
                int tooltipY = Math.Max(0, InputState.MouseY - 4 - tooltipHeight);
                if (invert)
                    tooltipX -= tooltipWidth;
                DrawUI(ui_tooltip, new Rectangle(tooltipX-2, tooltipY-2, tooltipWidth+4, tooltipHeight+4), Color.White);
                TooltipText.Draw(new Vector2(tooltipX, tooltipY), FontRenderer);
            }
        }

        public void AddWorldScore(int score, Vector2 position, ScoreType type)
        {
            RunStats.Score += score;
            switch (type)
            {
                case ScoreType.Small:
                    new Score(this, position, score, false, 80).FromWorld();
                    break;
                case ScoreType.Big:
                    new Score(this, position, score, true, 100).FromWorld();
                    break;
                case ScoreType.Silent:
                    break;
            }
        }

        public void AddUIScore(int score, Vector2 position, ScoreType type)
        {
            RunStats.Score += score;
            switch (type)
            {
                case ScoreType.Small:
                    new Score(this, position, score, false, 80);
                    break;
                case ScoreType.Big:
                    new Score(this, position, score, true, 100);
                    break;
                case ScoreType.Silent:
                    break;
            }
        }

        public void DrawGrappleLine(Vector2 start, Vector2 end, float waveAmplitude, float waveOffset, float waveFrequency, int precision, LerpHelper.Delegate lerp, Color color, BlendState blend)
        {
            SetupColorMatrix(ColorMatrix.Identity, WorldTransform, Projection);
            PrimitiveBatch.Begin(PrimitiveType.LineStrip, texture: Pixel, blendState: blend, rasterizerState: RasterizerState, samplerState: SamplerState.PointWrap, transform: WorldTransform, projection: Projection, effect: Shader);

            var delta = end - start;
            var lateral = Vector2.Normalize(new Vector2(-delta.Y, delta.X));

            for (int i = 0; i < precision; i++)
            {

                float slide = (float)i / (precision - 1);
                float posSlide = (float)lerp(0, 1, slide);
                var offset = lateral * (float)Math.Sin((slide * waveFrequency + waveOffset) * MathHelper.TwoPi) * (float)Math.Sin(MathHelper.Pi * slide) * waveAmplitude;
                var pos = start + posSlide * delta + offset;

                PrimitiveBatch.AddVertex(new VertexPositionColorTexture(new Vector3(pos, 0), color, new Vector2(0, 0)));
            }

            PrimitiveBatch.End();
        }
    }

    enum ScoreType
    {
        Small,
        Big,
        Silent,
    }
}
