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
using GeonBit.UI;
using GeonBit.UI.Entities;

namespace NodeMonog
{
    enum Status {
        Running,
        IterationCap,
        MinimaReached
    }

    class StatusWrapper {
        public Status inner;
        public float timestep;
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
        public float TimeStep {
            get {
                lock (status) {
                    return status.timestep;
                }
            }
            set {
                lock (status) {
                    status.timestep = value;
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


        DrawNode selectedNode;
        List<DrawNode> drawNodes = new List<DrawNode>();


        DrawNode hoverNode;
        int hoverTime = 0;

        const int hoverLimit = 1000;

        TaskStatus simulationStatus;
        List<gameState> history = new List<gameState>();

        Graph graph;
        Engine engine;


        PanelTabs tabs;
        Panel outsidePanel;

        List<TabData> allTabs;

        TabData global;
        TabData group;
        TabData person;
        TabData stats;
        TabData options;


        public Renderer(Graph graph, Engine engine)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graph = graph;
            this.engine = engine;
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

            UserInterface.Initialize(Content, theme: "editorSourceCodePro");



            outsidePanel = new Panel(new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height));
            outsidePanel.Anchor = Anchor.TopRight;

            foreach (Node item in graph.GetNodes())
            {
                drawNodes.Add(new DrawNode(new Vector2(), item));
            }

            tabs = new PanelTabs();

            global = tabs.AddTab("Global");
            group = tabs.AddTab("Group");
            person = tabs.AddTab("Person");
            options = tabs.AddTab("Options");
            stats = tabs.AddTab("Stats");

            


            allTabs = new List<TabData>() { global, group, person, options, stats };

            foreach (var tab in allTabs)
            {
                tab.button.ButtonParagraph.WrapWords = false;
            }

            selectedNode =  new DrawNode(Vector2.Zero,graph.GetNodes()[0]);


            engine.player.SelectNode(selectedNode.node);

            history.Add(new gameState(
                                alive: graph.FindAllNodes(x => x.statuses.Contains("Healthy")).Count,
                                dead: graph.FindAllNodes(x => x.statuses.Contains("Dead")).Count,
                                recovered: graph.FindAllNodes(x => x.statuses.Contains("Recovered")).Count,
                                infected: graph.FindAllNodes(x => x.statuses.Contains("Infected")).Count));



            InitializeHud();


            outsidePanel.AddChild(tabs);

            

            
            UserInterface.Active.AddEntity(outsidePanel);
            UserInterface.Active.ShowCursor = false;


            Window.ClientSizeChanged += resizeMenu;


            
            



            DrawNode.simulation = new Core.Physics.System(graph, 0.8f, 0.5f, 0.3f, 0.4f);
            simulationStatus = new TaskStatus(Status.Running);
            // remove this line if you wanna stop the async hack stuff, and advance the simulation elsewhere
            _ = RunSimulation();

            base.Initialize();
        }

        public void InitializeHud()
        {
            

            Panel currentPanel;



            currentPanel = global.panel;
            currentPanel.ClearChildren();
            SelectList eventList = new SelectList(Anchor.TopCenter);
            foreach ((string, Event) e in engine.player.Actions)
            {
                eventList.AddItem(e.Item1);
            }
            eventList.OnValueChange += delegate (Entity target)
            {
                engine.player.ActivateAction(engine.player.Actions[eventList.SelectedIndex].Item2);
                Console.WriteLine();
                InitializeHud();
                return;
            };

            currentPanel.AddChild(eventList);



            currentPanel = group.panel;
            currentPanel.ClearChildren();
            currentPanel.AddChild(new Paragraph("Not implemented yet"));


            currentPanel = stats.panel;
            stats.panel.ClearChildren();

            currentPanel = person.panel;
            currentPanel.ClearChildren();
            currentPanel.AddChild(new Header("Person"));
            SelectList connectionList = new SelectList();
            foreach ((Connection c, Node n) in graph.GetConnections(engine.player.selectedNode))
            {
                connectionList.AddItem(n.Name);
            }
            connectionList.OnValueChange += delegate (Entity target)
            {
                Node clickedNode = graph.GetNodes().Find(x => x.Name == connectionList.SelectedValue);
                engine.player.SelectNode(clickedNode);
                selectedNode = drawNodes.Find(x => x.node == clickedNode);
                Console.WriteLine();
                InitializeHud();
                return;
            };
            currentPanel.AddChild(new HorizontalLine());
            currentPanel.AddChild(connectionList);

            SelectList traitList = new SelectList();
            foreach (KeyValuePair<string, int> kv in selectedNode.node.Traits)
            {
                traitList.AddItem(kv.Key + ":   " + kv.Value);
            }
            foreach (string status in selectedNode.node.Statuses)
            {
                traitList.AddItem(status);
            }

            currentPanel.AddChild(traitList);


            currentPanel = options.panel;
            currentPanel.ClearChildren();
            currentPanel.AddChild(new Header("Choices"));
            


            currentPanel = stats.panel;
            currentPanel.ClearChildren();
            currentPanel.AddChild(new Paragraph($"Ticks: {history.Count}"));
            currentPanel.AddChild(new Paragraph($"Alive people {history.Last().alive}"));
            currentPanel.AddChild(new Paragraph($"Infected {history.Last().infected}"));
            currentPanel.AddChild(new Paragraph($"Dead people {history.Last().dead}"));
            currentPanel.AddChild(new Paragraph($"Recovered people {history.Last().recovered}"));

        }



        void resizeMenu(object sender, EventArgs e)
        {
            outsidePanel.Size = new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height);
            outsidePanel.Anchor = Anchor.TopRight;
        }

        async Task RunSimulation() {
            await Task.Run(() => {
                float timeStep = -1;
                for (int i = 0; i < 10000; i++) {
                    DrawNode.simulation.Advance((float)Math.Pow(10, timeStep));
                    if (DrawNode.simulation.WithinThreshold) {
                        simulationStatus.Status = Status.MinimaReached;
                        return;
                    }

                    var total = DrawNode.simulation.GetTotalEnergy();
                    timeStep = -((float)Math.Truncate(Math.Log10(DrawNode.simulation.GetTotalEnergy())) + 1);
                    simulationStatus.TimeStep = (float)Math.Pow(10, timeStep);
                }
                simulationStatus.Status = Status.IterationCap;
            });
        }

        public void testDelegate(object sender, EventArgs e)
        {
            Console.WriteLine("called");
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




                //Vänsta hud
                if (new Rectangle(0, 0, x * 2, r.Height).Contains(nms.Position))
                {
                    
                    if (nms.ScrollWheelValue != oms.ScrollWheelValue)
                    {
                        zoomlevel *= ((nms.ScrollWheelValue - oms.ScrollWheelValue) / 2000f) + 1f;
                    }

                    if (nms.LeftButton == ButtonState.Pressed && oms.LeftButton == ButtonState.Released)
                    {

                        for (int i = 0; i < graph.GetNodes().Count; i++)
                        {
                            DrawNode currentNode = new DrawNode(drawNodes.Find(x => x.node == graph.GetNodes()[i]).Position, graph.GetNodes()[i]);


                            if (new Rectangle(CameraTransform(currentNode.Position).ToPoint(), new Point(
                                (int)(64 * zoomlevel),
                                (int)(64 * zoomlevel))).Contains(nms.Position))
                            {
                                engine.player.SelectNode(currentNode.node);
                                selectedNode = currentNode;
                                InitializeHud();
                            };

                        }

                        if (new Rectangle(0, r.Height - 128, 128, 256).Contains(nms.Position))
                        {
                            engine.handler.Tick(graph);
                            history.Add(new gameState(
                                alive: graph.FindAllNodes(x => x.statuses.Contains("Healthy")).Count,
                                dead: graph.FindAllNodes(x => x.statuses.Contains("Dead")).Count,
                                recovered: graph.FindAllNodes(x => x.statuses.Contains("Recovered")).Count,
                                infected: graph.FindAllNodes(x => x.statuses.Contains("Infected")).Count));
                            InitializeHud();
                        }


                    }
                    //Vänsta hud klick
                    else
                    {



                    }
                }
                if (nms.LeftButton == ButtonState.Pressed)
                {
                    if (!new Rectangle(x * 2, 0, x, r.Height).Contains(nms.Position))
                    {
                        if (dragtimer > 50)
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

            UserInterface.Active.Update(gameTime);

            oms = nms;

            // TODO: Add your update logic here


            base.Update(gameTime);
        }

        //Methods to generalise and make more readable, aka make Theo happy

        public Point CameraTransform(Point p)
        {
            return new Point((int)((p.X - cameraPosition.X) * zoomlevel), (int)((p.Y - cameraPosition.Y) * zoomlevel)) +
                new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }

        public Vector2 CameraTransform(Vector2 v)
        {
            return (v - cameraPosition.ToVector2()) * (float)zoomlevel +
                new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }

        public Rectangle CameraTransform(Rectangle r) {
            var newPoint = CameraTransform(r.Location);
            var newScale = (r.Size.ToVector2() * (float)zoomlevel).ToPoint();
            return new Rectangle(newPoint, newScale);
        }

        public Rectangle CameraTransform(Vector2 location, Vector2 size) {
            var newLocation = CameraTransform(location);
            var newSize = size * (float)zoomlevel;
            return new Rectangle(newLocation.ToPoint(), newSize.ToPoint());
        }

        public int CameraTransform(int i, bool xAxis)
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

            string s;
            if (hoverNode == null) s = "Not hovering";
            else s = hoverNode.ToString();

            spriteBatch.DrawString(arial, s + "   :   " + engine.player.selectedNode.ToString(), Vector2.Zero, Color.Black);

            spriteBatch.DrawString(arial, frameRate.ToString() + "fps", new Vector2(0, 32), Color.Black);
            string simStatusString = simulationStatus.Status switch
            {
                Status.Running => $"Running\ntotal energy: {DrawNode.simulation.GetTotalEnergy()}" +
                $"\ntimestep: {simulationStatus.TimeStep}",
                Status.IterationCap => "Iteration cap reached",
                Status.MinimaReached => "Local minima reached",
                _ => "This should never happen"
            };

            spriteBatch.DrawString(arial, simStatusString, new Vector2(0, 48), Color.Black);



            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                Node currentNode = graph.GetNodes()[i];
                Vector2 currentNodePoistion = drawNodes.Find(x => x.node == currentNode).Position;

                Color selectcolour;
                float depth = 0.5f;
                if (selectedNode.node == currentNode)
                {
                    selectcolour = Color.Black;
                    depth = 0.2f;
                }
                //else if(hoverNode != null && hoverNode.node == currentNode)
                /*else if(hoverNode.node == currentNode)
                {
                    selectcolour = Color.Red;
                    depth = 0.2f;
                }*/
                else selectcolour = new Color(0, 0, 0, 15);

                foreach ((Connection c, Node n) in graph.GetConnections(currentNode))
                {
                    Vector2 arrowVector = (drawNodes.Find(x => x.node == n).Position - currentNodePoistion);
                    double rotation = Math.Atan(arrowVector.Y / arrowVector.X);
                    if (arrowVector.X < 0) rotation += Math.PI;

                    Vector2 offsetPoint = new Vector2(
                        (float)(circleDiameter / 2 ),
                        (float)(circleDiameter / 2 ));


                    spriteBatch.Draw(pixel,
                        destinationRectangle: new Rectangle(CameraTransform((currentNodePoistion + offsetPoint).ToPoint()),
                        new Point(
                        (int)((arrowVector.Length()) * zoomlevel),
                         (int)(8 * zoomlevel))),
                        sourceRectangle: null,
                        color: selectcolour,
                        rotation: (float)rotation,
                        origin: new Vector2(0, 0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: depth
                        );

                }


                if (graph.GetConnections(selectedNode.node).Exists(x => x.Item2 == currentNode)) depth = 0.2f;
                //Draws circles
                var _color = Color.White;
                if (currentNode.Statuses.Contains("Infected")) _color = Color.Red;
                if (currentNode.Statuses.Contains("Dead")) _color = Color.Black;
                if (currentNode.Statuses.Contains("Recovered")) _color = Color.Green;
                spriteBatch.Draw(circle,
                    destinationRectangle: new Rectangle(CameraTransform(currentNodePoistion).ToPoint(),
                    new Point(
                    (int)(circleDiameter * zoomlevel),
                    (int)(circleDiameter * zoomlevel))),
                    sourceRectangle: null,
                    color: _color,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    depth / 2 + 0.01f);

                //Draws node text
                if(zoomlevel > 0.35f)
                {
                    Color fadeColour = Color.Black;
                    if (zoomlevel < 0.8f) fadeColour = new Color(0, 0, 0, (int)((zoomlevel - 0.35f) * 255 * 4));
                    spriteBatch.DrawString(arial,
                        currentNode.Name,
                        CameraTransform(currentNodePoistion),
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
                    foreach (KeyValuePair<string, int> kv in selectedNode.node.Traits)
                    {
                        spriteBatch.DrawString(arial, kv.Key + ":   " + kv.Value, new Vector2(2 * centerX + 16, 64 + 32 * o++), Color.Black);
                     }
                    foreach (string status in selectedNode.node.Statuses)
                    {
                        spriteBatch.DrawString(arial, status, new Vector2(2 * centerX + 16, 64 + 32 * o++), Color.Black);
                    }

                    spriteBatch.DrawString(arial,
                            "Connections:",
                            new Vector2(centerX * 2 + 32, r.Height / 2 - 32),
                            Color.Black);

                    List<(Connection, Node)> d = graph.GetConnections(selectedNode.node).Select(parent => (parent.Item1, parent.Item2)).ToList(); ;
                    for (int i = 0; i < d.Count; i++)
                    {
                        spriteBatch.DrawString(arial,
                            d[i].Item2.Name, 
                            new Vector2(centerX * 2 + 16, r.Height / 2 + i * 32),
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

            UserInterface.Active.Draw(spriteBatch);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }


    public class gameState
    {
        public int alive;
        public int dead;
        public int recovered;
        public int infected;

        public gameState(int alive, int dead, int recovered, int infected)
        {
            this.alive = alive;
            this.dead = dead;
            this.recovered = recovered;
            this.infected = infected;
        }
    }


}
