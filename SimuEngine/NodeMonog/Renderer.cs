using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimuEngine;
using Core;
using System.Linq;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;
using SharpDX.Direct2D1.Effects;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NodeMonog
{
    enum Status {
        Running,
        IterationCap,
        MinimaReached
    }

    class StatusWrapper {
        public Status inner;
        public StatusWrapper(Status init) {
            inner = init;
        }
    }

    class TaskStatus {
        private StatusWrapper status;
        public Status Status {
            get {
                lock (status) {
                    return status.inner;
                }
            }
            set {
                lock (status) {
                    status.inner = value;
                }
            }
        }

        public TaskStatus(Status status) {
            this.status = new StatusWrapper(status);
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Renderer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState oms = Mouse.GetState();

        // hud elements:
        Texture2D circle, pixel, topCurve, tickButton, square, arrow;
        SpriteFont arial;
        int selectedTab = 0;

        int dragtimer = 0;
        Point cameraPosition = Point.Zero, cameraGoal = Point.Zero;
        Vector2 cameraVelocity = Vector2.Zero;
        double zoomlevel = 1f;

        const int zwoomTime = 200;
        int frameRate = 0;

        int animation = 0;
        const int animThreshold = short.MaxValue;
        const int animationRepeat = short.MaxValue;
        int transitionAnimation = animThreshold;

        const int circleDiameter = 64;


        Random r = new Random();



        ShittyAssNode selectedNode;
        ShittyAssNode hoverNode;
        int hoverTime;

        TaskStatus simulationStatus;

        Graph graph;

        //List<TickInfo> 



        public Renderer(Graph graph)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graph = graph;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            //AA boy
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 32;
            graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            //RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };

            graphics.ApplyChanges();




            selectedNode = (ShittyAssNode)graph.GetNodes()[0];

            // cameraGoal = new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);



            ShittyAssNode.simulation = new Core.Physics.System(graph, 0.8f, 0.5f, 0.3f, 0.4f);
            simulationStatus = new TaskStatus(Status.Running);
            // remove this line if you wanna stop the async hack stuff, and advance the simulation elsewhere
            RunSimulation();

            base.Initialize();
        }

        async Task RunSimulation() {
            await Task.Run(() => {
                for (int i = 0; i < 1000; i++) {
                    ShittyAssNode.simulation.Advance(1.0f / (i > 500 ? 2 : 1));
                    if (ShittyAssNode.simulation.WithinThreshold) {
                        simulationStatus.Status = Status.MinimaReached;
                        return;
                    }
                }
                simulationStatus.Status = Status.IterationCap;
            });
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            arial = Content.Load<SpriteFont>(@"Arial");
            circle = Content.Load<Texture2D>(@"circle");
            pixel = Content.Load<Texture2D>(@"pixel");
            topCurve = Content.Load<Texture2D>(@"topCurve");
            tickButton = Content.Load<Texture2D>(@"TickButton");
            square = Content.Load<Texture2D>(@"transparantSquare");
            arrow = Content.Load<Texture2D>(@"Arrow");


            // TODO: use this.Content to load your game content here
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            graphics.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param NName="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var timeFactor = 100f;
            if (Keyboard.GetState().IsKeyDown(Keys.Space)) timeFactor = 25f;
            //ShittyAssNode.simulation.Advance(gameTime.ElapsedGameTime.Milliseconds / timeFactor);

            Rectangle r = Window.ClientBounds;
            int x = r.Width / 3;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            MouseState nms = Mouse.GetState();

            if (nms != oms)
            {

                if (nms.LeftButton == ButtonState.Pressed && oms.LeftButton == ButtonState.Released)
                {


                    //Högra hud klick
                    if (new Rectangle(x * 2, 0, x, r.Height).Contains(nms.Position))
                    {
                        if (new Rectangle(x * 2, 0, r.Width / 12, 16).Contains(nms.Position)) selectedTab = 0;
                        else if (new Rectangle((int)(x * 2.2f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 1;
                        else if (new Rectangle((int)(x * 2.4f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 2;
                        else if (new Rectangle((int)(x * 2.6f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 3;
                        else if (new Rectangle((int)(x * 2.8f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 4;
                    }
                    //Vänsta hud klick
                    else
                    {


                        for (int i = 0; i < graph.GetNodes().Count; i++)
                        {
                            ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];


                            if (new Rectangle(cameraTransform(currentNode.Position).ToPoint(), new Point(
                                (int)(64 * zoomlevel),
                                (int)(64 * zoomlevel))).Contains(nms.Position))
                            {
                                selectedNode = currentNode;
                            };


                        }
                    }
                }
                if (nms.LeftButton == ButtonState.Pressed)
                {
                    if (!new Rectangle(x * 2, 0, x, r.Height).Contains(nms.Position))
                    {
                        if (dragtimer > 20)
                        {
                            cameraPosition -=  ((nms.Position - oms.Position).ToVector2() / (float)zoomlevel).ToPoint();
                            cameraVelocity = Vector2.Zero;
                        }
                        else dragtimer += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                else if (oms.LeftButton == ButtonState.Pressed && nms.LeftButton == ButtonState.Released)
                {
                    dragtimer = 0;
                }
                if (nms.ScrollWheelValue != oms.ScrollWheelValue)
                {
                    zoomlevel *= ((nms.ScrollWheelValue - oms.ScrollWheelValue) / 2000f) + 1f;

                }

            }

           
            cameraGoal = new Vector2(
                selectedNode.Position.X + circleDiameter / 4 * 3,
                selectedNode.Position.Y + circleDiameter / 4 * 3).ToPoint();


            if (dragtimer == 0)
            {
                cameraVelocity = ((cameraGoal - cameraPosition).ToVector2() / zwoomTime);
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.S))
            {
                cameraPosition += (cameraVelocity * gameTime.ElapsedGameTime.Milliseconds).ToPoint();
            }

            if (gameTime.ElapsedGameTime.Milliseconds != 0) frameRate = 1000 / gameTime.ElapsedGameTime.Milliseconds;

            if (transitionAnimation < animThreshold)
            {
                transitionAnimation += gameTime.ElapsedGameTime.Milliseconds * 25;
            }
            if (transitionAnimation > animThreshold) transitionAnimation = animThreshold;

            animation += gameTime.ElapsedGameTime.Milliseconds;
            if (animation > animationRepeat) animation = 0;


            oms = nms;


            // TODO: Add your update logic here

            base.Update(gameTime);
        }




        //Methods to generalise and make more readable, aka make Theo happy

        public Point cameraTransform(Point p)
        {
            return new Point((int)((p.X - cameraPosition.X) * zoomlevel), (int)((p.Y - cameraPosition.Y) * zoomlevel)) +
                new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }

        public Vector2 cameraTransform(Vector2 v)
        {
            return (v - cameraPosition.ToVector2()) * (float)zoomlevel +
                new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }

        public Rectangle cameraTransform(Rectangle r) {
            var newPoint = cameraTransform(r.Location);
            var newScale = (r.Size.ToVector2() * (float)zoomlevel).ToPoint();
            return new Rectangle(newPoint, newScale);
        }

        public Rectangle cameraTransform(Vector2 location, Vector2 size) {
            var newLocation = cameraTransform(location);
            var newSize = size * (float)zoomlevel;
            return new Rectangle(newLocation.ToPoint(), newSize.ToPoint());
        }

        public int cameraTransform(int i, bool xAxis)
        {
            if (xAxis) return (int)((i + cameraPosition.X) * zoomlevel);
            else return (int)((i + cameraPosition.Y) * zoomlevel);
        }







        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param NName="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            Rectangle r = Window.ClientBounds;

            spriteBatch.Begin(SpriteSortMode.BackToFront);


            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);
            spriteBatch.DrawString(arial, "Tick", new Vector2(16, r.Height - 48), Color.Black);


            int centerX = r.Width / 3;



            spriteBatch.DrawString(arial, (animation % 1000).ToString() + "   :   " + transitionAnimation, Vector2.Zero, Color.Black);

            spriteBatch.DrawString(arial, frameRate.ToString() + "fps", new Vector2(0, 32), Color.Black);
            string simStatusString = simulationStatus.Status switch
            {
                Status.Running => $"Running\ntotal energy: {ShittyAssNode.simulation.GetTotalEnergy()}",
                Status.IterationCap => "Iteration cap reached",
                Status.MinimaReached => "Local minima reached",
                _ => "This should never happen"
            };

            spriteBatch.DrawString(arial, simStatusString, new Vector2(0, 48), Color.Black);



            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];

                Color selectcolour;
                float depth = 0.5f;
                if (selectedNode == currentNode)
                {
                    selectcolour = Color.Black;
                    depth = 0.2f;
                }
                else selectcolour = new Color(0, 0, 0, 15);

                foreach ((Connection c, ShittyAssNode n) in graph.GetConnections(currentNode).Select(parent => (parent.Item1, (ShittyAssNode)parent.Item2)))
                {
                    Vector2 arrowVector = (n.Position - currentNode.Position);
                    double rotation = Math.Atan(arrowVector.Y / arrowVector.X);
                    if (arrowVector.X < 0) rotation += Math.PI;

                    Vector2 offsetPoint = new Vector2(
                        (float)(circleDiameter / 2 + circleDiameter / 2 * Math.Cos(rotation)),
                        (float)(circleDiameter / 2 + circleDiameter / 2 * Math.Sin(rotation)));


                    spriteBatch.Draw(pixel,
                        destinationRectangle: new Rectangle(cameraTransform((currentNode.Position + offsetPoint).ToPoint()),
                        new Point(
                        (int)((arrowVector.Length() - circleDiameter) * zoomlevel),
                         (int)(8 * zoomlevel))),
                        sourceRectangle: null,
                        color: selectcolour,
                        rotation: (float)rotation,
                        origin: new Vector2(0, 0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: depth
                        );

                }

                spriteBatch.Draw(circle,
                    destinationRectangle: new Rectangle(cameraTransform(currentNode.Position).ToPoint(),
                    new Point(
                    (int)(circleDiameter * zoomlevel),
                    (int)(circleDiameter * zoomlevel))),
                    sourceRectangle: null,
                    color: Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.25f);

                
                if(zoomlevel > 0.35f)
                {
                    Color fadeColour = Color.Black;
                    if (zoomlevel < 0.8f) fadeColour = new Color(0, 0, 0, (int)((zoomlevel - 0.35f) * 255 * 4));
                    spriteBatch.DrawString(arial,
                        currentNode.NName,
                        cameraTransform(currentNode.Position),
                        fadeColour,
                        0,
                        Vector2.Zero,
                        (float)(1 / zoomlevel / 32 + 1.2f),
                        SpriteEffects.None,
                        0.1f);
                }
            }

            //Theos lab colours
            var startColor = LabColor.RgbToLab(new Color(0xA5, 0xD7, 0xC8));
            Console.WriteLine(startColor);
            var endColor = LabColor.RgbToLab(new Color(0x48, 0x73, 0x66));
            float time = transitionAnimation / (float)animThreshold;
            time = 1 - (float)Math.Pow(1 - time, 3);
            var color = LabColor.LabToRgb(LabColor.LinearGradient(startColor, endColor, time));



            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);


            spriteBatch.Draw(pixel, new Rectangle(centerX * 2, 16, centerX + 1, r.Height - 16), null,Color.DarkGray,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.08f);
            spriteBatch.Draw(topCurve, new Rectangle(centerX * 2 + centerX / 5 * selectedTab, 0, centerX / 5, 16), null,Color.DarkGray,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.08f);



            spriteBatch.DrawString(arial, "Global", new Vector2(centerX * 2 + 2, 0), Color.Black,
                    0,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.05f);
            spriteBatch.DrawString(arial, "Group", new Vector2(centerX * 2.2f + 2, 0), Color.Black,
                    0,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.05f);
            spriteBatch.DrawString(arial, "Person", new Vector2(centerX * 2.4f + 2, 0), Color.Black,
                    0,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.05f);
            spriteBatch.DrawString(arial, "Stats", new Vector2(centerX * 2.6f + 2, 0), Color.Black,
                    0,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.05f);

            spriteBatch.DrawString(arial, "Options", new Vector2(centerX * 2.80f + 2, 0), Color.Black,
                    0,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0.05f);


            switch (selectedTab)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    int o = 0;
                    foreach (KeyValuePair<string, int> kv in selectedNode.Traits)
                    {
                        spriteBatch.DrawString(arial, kv.Key + ":   " + kv.Value, new Vector2(2 * centerX + 16, 64 + 32 * o++), Color.Black);
                     }

                    spriteBatch.DrawString(arial,
                            "Connections:",
                            new Vector2(centerX * 2.15f, r.Height / 2 - 32),
                            Color.Black);

                    List<(Connection, ShittyAssNode)> d = graph.GetConnections(selectedNode).Select(parent => (parent.Item1, (ShittyAssNode)parent.Item2)).ToList(); ;
                    for (int i = 0; i < d.Count; i++)
                    {
                        spriteBatch.DrawString(arial,
                            d[i].Item2.NName, 
                            new Vector2(centerX * 2.1f, r.Height / 2 + i * 32),
                            Color.Black);
                    }

                    break;
                case 3:
                    if (r.Width > 720)
                    {
                        for (int i = 0; i < (centerX / 32) - 1; i++)
                        {
                            for (int j = 0; j < (r.Height / 3) / 32; j++)
                            {
                                spriteBatch.Draw(square, new Rectangle(centerX * 2 + ((centerX % 32) + 32) / 2 + i * 32, r.Height / 3 * 2 + j * 32 - 64, 32, 32), Color.Black);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
