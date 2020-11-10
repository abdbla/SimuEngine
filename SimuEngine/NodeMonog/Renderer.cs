using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimuEngine;
using Core;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Simulation = Core.Physics.Simulation;
using SimulationParams = Core.Physics.SimulationParams;
using System.Xml.Schema;

namespace NodeMonog
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Renderer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState oms = Mouse.GetState();
        KeyboardState okbs = Keyboard.GetState();

        Random rng = new Random();

        // hud elements:
        Texture2D circle, pixel, tickButton, square, arrow;
        SpriteFont arial;

        const int SEPARATION = 4;
        readonly static SimulationParams SIMULATION_PARAMS = new SimulationParams(0.8f, 0.5f, 0.3f, 0.4f);

        int dragtimer = 0;
        Point cameraPosition = Point.Zero, cameraGoal = Point.Zero;
        Vector2 cameraVelocity = Vector2.Zero;
        double zoomlevel = 1f;

        const int zwoomTime = 200;
        int frameRate = 0;

        int animation = 0;

        const int circleDiameter = 64;


        DrawNode selectedNode { get => currentSimulation.SelectedDrawNode; set => currentSimulation.SelectedDrawNode = value; }
        List<DrawNode> drawNodes { get => currentSimulation.DrawNodes.ToList(); set { } }

        List<GameState> history = new List<GameState>();

        Engine engine;

        Task updateTask;

        Graph masterGraph;
        List<(PhysicsWrapper, string)> visitedGraphs = new List<(PhysicsWrapper, string)>();
        List<string> allActiveStatuses = new List<string>();

        int historyIndex = 0;
        List<PhysicsWrapper> ranSimulations = new List<PhysicsWrapper>();
        PhysicsWrapper currentSimulation { get => visitedGraphs[historyIndex].Item1; }
        Graph currentGraph { get => visitedGraphs[historyIndex].Item1.Graph; }


        PanelTabs tabs;
        Panel outsidePanel;

        List<TabData> allTabs;

        TabData actions;
        TabData group;
        TabData person;
        TabData stats;
        TabData options;

        //Options
        bool cameraLock = true;
        bool showGraph = false;
        bool animations = false;
        bool showBoundingBox = false;

        public Renderer(Engine engine)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.engine = engine;
            masterGraph = engine.system.graph;

            visitedGraphs.Add((new PhysicsWrapper(masterGraph,
                                                  masterGraph.Nodes[0],
                                                  SEPARATION,
                                                  SIMULATION_PARAMS),
                               "master"));
            // alt: 0.8f, 0.5f, 0.3f, 0.1f
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

            //Gui initial Initialisation
            UserInterface.Initialize(Content, theme: "editorSourceCodePro");
            UserInterface.Active.UseRenderTarget = true;


            outsidePanel = new Panel(new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height));
            outsidePanel.Anchor = Anchor.TopRight;

            tabs = new PanelTabs();


            actions = tabs.AddTab("Global");
            Vector2 offset = new Vector2(0, -actions.button.CalcDestRect().Height / 3);
            Vector2 size =   new Vector2(actions.button.CalcDestRect().Height / 3, actions.button.CalcDestRect().Height / 3);
            Image globalI = new Image(Content.Load<Texture2D>(@"GlobeIcon"), size: size * 2.25f, offset: offset, anchor: Anchor.TopCenter);
            globalI.ClickThrough = true;
            actions.button.AddChild(globalI);

            group = tabs.AddTab("Group");
            Image groupI = new Image(Content.Load<Texture2D>(@"GroupIcon"), size: size * 2.25f, offset: offset, anchor: Anchor.TopCenter);
            groupI.ClickThrough = true;
            group.button.AddChild(groupI);
            //group.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

            person = tabs.AddTab("Person");
            Image personI = new Image(Content.Load<Texture2D>(@"PersonIcon") , size: size * 2.25f,offset: offset,anchor: Anchor.TopCenter);
            personI.ClickThrough = true;
            person.button.AddChild(personI);
            //person.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

            options = tabs.AddTab("Options");
            Image optI = new Image(Content.Load<Texture2D>(@"GearIcon"), size: size * 2.25f, anchor: Anchor.TopCenter, offset: offset);
            optI.ClickThrough = true;
            options.button.AddChild(optI);
            //options.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

            stats = tabs.AddTab("Stats");
            Image satsI = new Image(Content.Load<Texture2D>(@"StatsIcon"), size: size * 2.25f, anchor: Anchor.TopCenter, offset: offset);
            satsI.ClickThrough = true;
            stats.button.AddChild(satsI);
            //stats.panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

            allTabs = new List<TabData>() { actions, group, person, options, stats };
            foreach (var tab in allTabs)
            {
                tab.button.ButtonParagraph.Visible = false;
                tab.button.Children.First(x => x.GetType() == typeof(Image) || x.GetType() == typeof(Icon)).Visible = true;
            }

            engine.player.SelectNode(selectedNode.node);


            history.Add(new GameState(masterGraph));

            resizeMenu(new object(), new EventArgs());



            UpdateHud();
            outsidePanel.AddChild(tabs);

            
            
            UserInterface.Active.AddEntity(outsidePanel);
            UserInterface.Active.ShowCursor = false;


            Window.ClientSizeChanged += resizeMenu;



            // remove this line if you wanna stop the async hack stuff, and advance the simulation elsewhere
            currentSimulation.StartSimulation();

            base.Initialize();
        }


        public void UpdateHud() {


            actions.panel.ClearChildren();
            SelectList eventList = new SelectList(Anchor.TopCenter);
            foreach ((string, Event) e in engine.player.Actions)
            {
                eventList.AddItem(e.Item1);
            }
            eventList.OnValueChange += delegate (Entity target)
            {
                engine.player.ActivateAction(engine.player.Actions[eventList.SelectedIndex].Item2);
                Console.WriteLine();
                UpdateHud();
                return;
            };

            actions.panel.AddChild(eventList);
            
            if (selectedNode.node.SubGraph.Nodes.Count != 0) { 
            Button subGraphButton = new Button("Enter Subgraph");
                subGraphButton.OnClick += x =>
                {
                    GoIntoAGraph(selectedNode.node);
                };
                actions.panel.AddChild(subGraphButton);
            }


            group.panel.ClearChildren();
            SelectList fittingPeople = new SelectList();
            fittingPeople.Size = new Vector2(fittingPeople.Size.X, Window.ClientBounds.Height - 200);
            fittingPeople.Items = currentGraph.Nodes.Select(x => x.Name).ToArray();
            DropDown drop = new DropDown(new Vector2(0, 280));
            drop.AddItem("All");
            foreach (KeyValuePair<string, int> entry in new GameState(masterGraph).allTraits)
            {
                drop.AddItem(entry.Key);
            }
            drop.OnValueChange += _ => {
                if (drop.SelectedValue == "All")
                {
                    fittingPeople.Items = currentGraph.Nodes.Select(x => x.Name).ToArray();
                }
                else
                {
                    fittingPeople.Items = currentGraph.FindAllNodes(x => x.statuses.Contains(drop.SelectedValue)).Select(x => x.Name).ToArray();
                }
            };
            fittingPeople.OnValueChange += _ =>
            {
                    currentSimulation.SelectedNode = currentGraph.FindNode(x => x.Name == fittingPeople.SelectedValue);
                
                //catch (Exception)
                //{
                //    selectedNode = new DrawNode(currentGraph.FindNode(x => x.Name == fittingPeople.SelectedValue), currentSimulation.Simulation);
                //}
                
            };
            group.panel.AddChild(drop);
            group.panel.AddChild(fittingPeople);

            person.panel.ClearChildren();

            person.panel.AddChild(new Header(engine.player.selectedNode.Name));
            SelectList connectionList = new SelectList();
            foreach ((Connection c, Node n) in currentGraph.GetOutgoingConnections(engine.player.selectedNode))
            {
                connectionList.AddItem(n.Name + ": " + c.Traits["Proximity"]);
            }
            connectionList.OnValueChange += delegate (Entity target)
            {
                Node clickedNode = currentGraph.GetOutgoingConnections(selectedNode.node)[connectionList.SelectedIndex].Item2;
                engine.player.SelectNode(clickedNode);
                currentSimulation.SelectedNode = clickedNode;
                Console.WriteLine();
                UpdateHud();
                return;
            };

            person.panel.AddChild(new HorizontalLine());
            Paragraph p = new Paragraph("Connections:", scale: 1.20f);
            p.WrapWords = false;
            person.panel.AddChild(p);
            person.panel.AddChild(connectionList);



            Paragraph traitHeader = new Paragraph("Traits:", scale: 1.20f);
            traitHeader.WrapWords = false;
            person.panel.AddChild(traitHeader);
            SelectList traitList = new SelectList();
            List<string> traitNames = new List<string>();
            List<string> traitValues = new List<string>();
            int nameMaxWidth = 0;
            int valMaxWidth = 0;
            foreach (var kv in selectedNode.node.Traits) {
                var traitVal = kv.Value.ToString();
                traitNames.Add(kv.Key);
                traitValues.Add(traitVal);
                nameMaxWidth = Math.Max(nameMaxWidth, kv.Key.Length);
                valMaxWidth = Math.Max(valMaxWidth, traitVal.Length);
            }
            foreach ((var name, var val) in traitNames.Zip(traitValues, (x, y) => (x, y))) {
                traitList.AddItem(name.PadRight(nameMaxWidth + 1) + val.PadLeft(valMaxWidth));
            }
            foreach (string status in selectedNode.node.Statuses) {
                traitList.AddItem(status);
            }

            person.panel.AddChild(traitList);

            //SCrolbar, if only
            //VerticalScrollbar scrollbar = new VerticalScrollbar(0, 10, Anchor.CenterRight);
            //scrollbar.
            //person.panel.AddChild(scrollbar);



            options.panel.ClearChildren();
            //currentPanel.AddChild(new VerticalScrollbar(1,10));
            CheckBox graphBox = new CheckBox("Show time step graph");
            graphBox.Checked = showGraph;
            graphBox.OnValueChange += _ => showGraph = graphBox.Checked;

            CheckBox animationBox = new CheckBox("Arrow Animations");
            animationBox.Checked = animations;
            animationBox.OnValueChange += _ => animations = animationBox.Checked;

            CheckBox cameraBox = new CheckBox("Camera Lock");
            cameraBox.Checked = cameraLock;
            cameraBox.OnValueChange += _ => cameraLock = cameraBox.Checked;

            CheckBox boundingBox = new CheckBox("Show bounding box");
            boundingBox.Checked = false;
            boundingBox.OnValueChange += _ => showBoundingBox = boundingBox.Checked;

            
            options.panel.AddChild(graphBox);
            options.panel.AddChild(animationBox);
            options.panel.AddChild(cameraBox);
            options.panel.AddChild(boundingBox);


            Slider sliderDamp = new Slider(0, 500);
            float damping = currentSimulation.Simulation.Damping;
            Paragraph sDampParagraph = new Paragraph("Physics Damp: " + damping);
            options.panel.AddChild(sDampParagraph);
            sliderDamp.Value = (int)((Math.Log10(damping) + 3) * 100);
            sliderDamp.OnValueChange += _ => 
            {
                float damping = (float) Math.Pow(10f, ((float)sliderDamp.Value / 100) - 3);
                currentSimulation.Simulation.Damping = damping;
                sDampParagraph.Text = "Physics Damp: " + damping;
            };
            options.panel.AddChild(sliderDamp);




            Slider sliderRepulsion = new Slider(0, 500);
            float repulsion = currentSimulation.Simulation.Repulsion;
            Paragraph sRepulsionParagraph = new Paragraph("Physics Rep: " + repulsion);
            options.panel.AddChild(sRepulsionParagraph);
            sliderRepulsion.Value = (int)((Math.Log10(repulsion) + 3) * 100);
            sliderRepulsion.OnValueChange += _ =>
            {
                float repulsion = (float)Math.Pow(10f, ((float)sliderRepulsion.Value / 100) - 3);
                currentSimulation.Simulation.Repulsion = repulsion;
                sRepulsionParagraph.Text = "Physics Rep: " + repulsion;
            };
            options.panel.AddChild(sliderRepulsion);

            Slider sliderGravity = new Slider(0, 500);
            float gravity = currentSimulation.Simulation.Gravity;
            Paragraph sGravityParagraph = new Paragraph("Physics Grav: " + gravity);
            options.panel.AddChild(sGravityParagraph);
            sliderGravity.Value = (int)((Math.Log10(gravity) + 3) * 100);
            sliderGravity.OnValueChange += _ =>
            {
                float gravity = (float)Math.Pow(10f, ((float)sliderGravity.Value / 100) - 3);
                currentSimulation.Simulation.Gravity = gravity;
                sGravityParagraph.Text = "Physics Grav: " + gravity;
            };
            options.panel.AddChild(sliderGravity);

            Slider sliderStiffness = new Slider(0, 500);
            float stiffness = currentSimulation.Simulation.Stiffness;
            Paragraph sStiffParagraph = new Paragraph("Physics Stiff: " + stiffness);
            options.panel.AddChild(sStiffParagraph);
            sliderStiffness.Value = (int)((Math.Log10(stiffness) + 3) * 100);
            sliderStiffness.OnValueChange += _ =>
            {
                float stiffness = (float)Math.Pow(10f, ((float)sliderStiffness.Value / 100) - 3);
                currentSimulation.Simulation.Stiffness = stiffness;
                sStiffParagraph.Text = "Physics Stiff: " + stiffness;
            };
            options.panel.AddChild(sliderStiffness);


            stats.panel.ClearChildren();
            stats.panel.AddChild(new Paragraph($"Ticks: {history.Count}"));
            foreach (KeyValuePair<string, int> entry in new GameState(masterGraph).allTraits)
            {
                stats.panel.AddChild(new Paragraph(entry.Key + ": " + entry.Value));
            }
            stats.panel.AddChild(new Paragraph());
            foreach (KeyValuePair<string, List<int>> entry in new GameState(masterGraph).allStatuses)
            {
                stats.panel.AddChild(new Paragraph(entry.Key + " average: " + entry.Value.Average()));
            }

        }



        void resizeMenu(object sender, EventArgs e)
        {
            outsidePanel.Size = new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height);
            outsidePanel.Anchor = Anchor.TopRight;
            UpdateHud();
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
        protected override void Update(GameTime gameTime) {
            Rectangle r = Window.ClientBounds;
            int x = r.Width / 3;

            MouseState nms = Mouse.GetState();
            KeyboardState nkbs = Keyboard.GetState();

            if (nkbs.IsKeyDown(Keys.Space) && okbs.IsKeyUp(Keys.Space)) {
                currentSimulation.AdvanceOnce();
            }
            

            //Mouse is moved/pressed/Scrolled
            if (nms != oms) {
                //Vänsta hud
                if (new Rectangle(0, 0, x * 2, r.Height).Contains(nms.Position)) {
                    if (nms.ScrollWheelValue != oms.ScrollWheelValue) {
                        zoomlevel *= ((nms.ScrollWheelValue - oms.ScrollWheelValue) / 2000f) + 1f;
                    }

                    if (nms.LeftButton == ButtonState.Pressed && oms.LeftButton == ButtonState.Released) {
                        foreach (var currentNode in currentSimulation.DrawNodes) {
                            if (new Rectangle(CameraTransform(currentNode.Position).ToPoint(), new Point(
                                (int)(64 * zoomlevel),
                                (int)(64 * zoomlevel))).Contains(nms.Position))
                            {
                                engine.player.SelectNode(currentNode.node);
                                selectedNode = currentNode;
                                UpdateHud();
                                break;
                            }
                        }

                        
                        if (new Rectangle(0, r.Height - 128, 256, 128).Contains(nms.Position) && (updateTask?.IsCompleted ?? true)) {
                            //The tickbutton is pressed
                            Action action = ()  =>
                            {
                                engine.handler.Tick(visitedGraphs[historyIndex].Item1.Graph);
                                history.Add(new GameState(masterGraph));
                                UpdateHud();
                            };
                            updateTask = Task.Run(action);
                        }
                    }

                    if (nms.LeftButton == ButtonState.Pressed) {
                        if (!new Rectangle(x * 2, 0, x, r.Height).Contains(nms.Position)) {
                            if (dragtimer > 50) {
                                cameraPosition -= ((nms.Position - oms.Position).ToVector2() / (float)zoomlevel).ToPoint();
                                cameraVelocity = Vector2.Zero;
                            }
                            else dragtimer += gameTime.ElapsedGameTime.Milliseconds;
                        }
                    }
                    else if (oms.LeftButton == ButtonState.Pressed && nms.LeftButton == ButtonState.Released) {
                        dragtimer = 0;
                    }
                }

                if (!new Rectangle(0, 0, Window.ClientBounds.Width / 3 * 2, Window.ClientBounds.Height).Contains(nms.Position)) {
                    dragtimer = 0;
                };
            }

           
            cameraGoal = new Vector2(
                selectedNode.Position.X + circleDiameter / 4 * 3,
                selectedNode.Position.Y + circleDiameter / 4 * 3).ToPoint();
            
            if (nkbs.IsKeyDown(Keys.Z) && !okbs.IsKeyDown(Keys.Z)) GoOutAGraph();
            

            if (dragtimer == 0) {
                cameraVelocity = ((cameraGoal - cameraPosition).ToVector2() / zwoomTime);
            }
            if (cameraLock) {
                cameraPosition += (cameraVelocity * gameTime.ElapsedGameTime.Milliseconds).ToPoint();
            }

            if (nkbs.IsKeyDown(Keys.LeftControl) && nkbs.IsKeyDown(Keys.R) && !okbs.IsKeyDown(Keys.R)) {
                currentSimulation.CompleteReset();
            }
            else if (nkbs.IsKeyDown(Keys.R) && !okbs.IsKeyDown(Keys.R))
            {
                currentSimulation.Restart();
            }

            if (nkbs.IsKeyDown(Keys.C)) cameraPosition = cameraGoal;


            if (gameTime.ElapsedGameTime.Milliseconds != 0) frameRate = 1000 / gameTime.ElapsedGameTime.Milliseconds;

            animation += gameTime.ElapsedGameTime.Milliseconds;

            UserInterface.Active.Update(gameTime);

            oms = nms;
            okbs = nkbs;

            // TODO: Add your update logic here


            base.Update(gameTime);
        }

        public void GoIntoAGraph(Node enteredNode) {
            Graph g = enteredNode.SubGraph;

            PhysicsWrapper s = ranSimulations.Find(x => x.Graph == g) ?? new PhysicsWrapper(g, g.Nodes[0], degrees: SEPARATION, SIMULATION_PARAMS);

            visitedGraphs.Add((s, enteredNode.Name));
            historyIndex++;

            selectedNode = s.SelectedDrawNode;
            engine.player.SelectNode(g.Nodes[0]);

            cameraPosition = cameraGoal;

            UpdateHud();
        }

        public void GoOutAGraph() {
            if (historyIndex > 0) {
                historyIndex--;
                engine.player.SelectNode(currentSimulation.SelectedNode);

                visitedGraphs.RemoveAt(historyIndex + 1);

                cameraPosition = selectedNode.Position.ToPoint();

                UpdateHud();

            }
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

        public int CameraTransform(int i)
        {
            return (int)(i * zoomlevel);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param NName="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            UserInterface.Active.Draw(spriteBatch);

            GraphicsDevice.Clear(Color.LightGray);

            Rectangle r = Window.ClientBounds;

            spriteBatch.Begin(SpriteSortMode.BackToFront);

            if (showBoundingBox) {
                (var bl_1, var bl_2) = currentSimulation.Simulation.GetBoundingBox();
                var bottomleft = new Vector2(bl_1.X, bl_1.Y);
                var topright = new Vector2(bl_2.X, bl_2.Y);

                bottomleft = bottomleft * 50 + new Vector2(300, 300);
                topright = topright * 50 + new Vector2(300, 300);

                bottomleft = CameraTransform(bottomleft);
                topright = CameraTransform(topright);

                spriteBatch.Draw(square, new Rectangle(bottomleft.ToPoint(), (topright - bottomleft).ToPoint()), Color.Red);
            }

            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);
            spriteBatch.DrawString(arial, "Tick", new Vector2(16, r.Height - 48), Color.Black);


            int centerX = r.Width / 3;

            for (int k = 0; k < visitedGraphs.Count; k++) {
                spriteBatch.DrawString(arial, visitedGraphs[k].Item2, new Vector2(centerX, k * 32), Color.Black);
            }

            spriteBatch.DrawString(arial, r.ToString() + "   :   " + engine.player.selectedNode.ToString(), Vector2.Zero, Color.Black);

            spriteBatch.DrawString(arial, (animation).ToString(), new Vector2(0, 32), Color.Black);
            string simStatusString = currentSimulation.SimulationStatus.Status switch
            {
                Status.Running => $"Running\ntotal energy: {currentSimulation.GetTotalEnergy()}" +
                $"\ntimestep: {currentSimulation.SimulationStatus.TimeStep}",
                Status.IterationCap => "Iteration cap reached",
                Status.MinimaReached => "Local minima reached",
                Status.Idle => "idle",
                Status.Cancelled => "Cancelled",
                // _ => "This should never happen"
            };
            try {
                spriteBatch.DrawString(arial, simStatusString, new Vector2(0, 48), Color.Black);
            } catch {
            }
            if (showGraph) {
                float maxTime = Math.Max(1, currentSimulation.SimulationStatus.TimeStepHistory.Select(t => t.Item2).Max());
                float maxIter = Math.Min(1000, currentSimulation.SimulationStatus.TimeStepHistory.Last().Item1); // simulationStatus.TimeStepHistory.Select(t => t.Item1).Max();

                float graphWidth = Window.ClientBounds.Width / 5f;
                float graphHeight = Window.ClientBounds.Height / 4f;

                foreach ((int iter, float timeStep) in currentSimulation.SimulationStatus.TimeStepHistory.Skip(
                    Math.Min(0, currentSimulation.SimulationStatus.TimeStepHistory.Last().Item1 - 1000)))
                {
                    var y = (Math.Log(timeStep) - Math.Log(0.01)) / (Math.Log(maxTime) - Math.Log(0.01)) * graphHeight;
                    var x = iter / maxIter * graphWidth;
                    spriteBatch.Draw(pixel,
                        new Rectangle(new Point((int)x, (int)(graphHeight - y) + 128),
                                      new Point(1, 1)),
                        new Color(timeStep / maxTime, 0, 0));
                }
            }



            //for (int i = 0; i < visitedGraphs[historyIndex].Item1.Graph.Nodes.Count; i++)
            foreach (DrawNode currentDrawNode in currentSimulation.DrawNodes)
            {
                //Node currentNode = visitedGraphs[historyIndex].Item1.Graph.Nodes[i];
                //Vector2 currentNodePosition = currentSimulation.DrawNodes.Find(x => x.node == currentNode).Position;

                Node currentNode = currentDrawNode.node;
                Vector2 currentNodePosition = currentDrawNode.Position;

                Color selectcolour;
                float depth = 0.5f;
                if (selectedNode.node == currentNode) {
                    selectcolour = Color.Black;
                    depth = 0.2f;
                }
                else selectcolour = new Color(
                    0.7f, // (float)rng.NextDouble(),
                    0.7f, // (float)rng.NextDouble(),
                    0.7f, // (float)rng.NextDouble(),
                    1.0f
                );
                
                foreach ((Connection c, Node n) in currentGraph.GetOutgoingConnections(currentNode)) {
                    DrawNode otherDrawNode = currentSimulation.LookupDrawNode(n);
                    if (otherDrawNode == null) continue;
                    Vector2 arrowVector = otherDrawNode.Position - currentNodePosition;
                    double rotation = Math.Atan(arrowVector.Y / arrowVector.X);
                    if (arrowVector.X < 0) rotation += Math.PI;

                    Vector2 offsetPoint = new Vector2(
                        circleDiameter / 2,
                        circleDiameter / 2);

                    spriteBatch.Draw(pixel,
                        destinationRectangle: new Rectangle(CameraTransform((currentNodePosition + offsetPoint).ToPoint()),
                        new Point(
                        (int)(arrowVector.Length() * zoomlevel),
                         CameraTransform(6) + 2)),
                        sourceRectangle: null,
                        color: selectcolour,
                        rotation: (float)rotation,
                        origin: new Vector2(0, 0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: depth
                    );

                    if (animations)
                    {

                        Vector2 relativeMovement = 
                            (arrowVector + offsetPoint) * (animation % (c.Strength() * 1000)) / (c.Strength() * 1000);
                        spriteBatch.Draw(
                            texture: arrow,
                            destinationRectangle:
                                CameraTransform(
                                    new Rectangle(
                                        location: (currentNodePosition + relativeMovement + offsetPoint).ToPoint(),
                                        size: new Point(32, 16))),
                            sourceRectangle: null,
                            color: selectcolour,
                            rotation: (float)rotation,
                            origin: new Vector2(0.5f, 0.5f),
                            effects: SpriteEffects.None,
                            layerDepth: depth);
                    }
                }


                if (currentGraph.GetOutgoingConnections(selectedNode.node).Exists(x => x.Item2 == currentNode)) depth = 0.2f;
                //Draws circles
                var _color = Color.White;
                if (currentNode.Statuses.Contains("Infected")) _color = Color.Red;
                if (currentNode.Statuses.Contains("Dead")) _color = Color.Black;
                if (currentNode.Statuses.Contains("Recovered")) _color = new Color(81, 182, 74);
                spriteBatch.Draw(circle,
                    destinationRectangle: new Rectangle(CameraTransform(currentNodePosition).ToPoint(),
                                                        new Point(
                                                            (int)(circleDiameter * zoomlevel),
                                                            (int)(circleDiameter * zoomlevel))),
                    sourceRectangle: null,
                    color: _color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    SpriteEffects.None,
                    depth / 2 + 0.01f);

                spriteBatch.Draw(circle,
                   destinationRectangle: new Rectangle(CameraTransform(currentNodePosition - new Vector2(6)).ToPoint(),
                                                       new Point(
                                                           (int)(circleDiameter * zoomlevel + CameraTransform(12)),
                                                           (int)(circleDiameter * zoomlevel + CameraTransform(12)))),
                   sourceRectangle: null,
                   color: Color.LightGray,
                   0,
                   Vector2.Zero,
                   SpriteEffects.None,
                   depth / 2 + 0.02f);

                //Draws node text
                if (zoomlevel > 0.35f)
                {
                    Color fadeColour = Color.Black;
                    if (zoomlevel < 0.8f) fadeColour = new Color(0, 0, 0, (int)((zoomlevel - 0.35f) * 255 * 4));
                    spriteBatch.DrawString(arial,
                        currentNode.Name,
                        CameraTransform(currentNodePosition + new Vector2(32 * (float)zoomlevel - (currentNode.Name.Length) * 8, 16 * (float)zoomlevel)),
                        fadeColour,
                        0,
                        Vector2.Zero,
                        (float)(1 / zoomlevel / 32 + 1f),
                        SpriteEffects.None,
                        0.1f);
                }
            }

            //Theos lab colours
            //var startColor = LabColor.RgbToLab(new Color(0xA5, 0xD7, 0xC8));
            //Console.WriteLine(startColor);
            //var endColor = LabColor.RgbToLab(new Color(0x48, 0x73, 0x66));
            //float time = transitionAnimation / (float)animThreshold;
            //time = 1 - (float)Math.Pow(1 - time, 3);
            //var color = LabColor.LabToRgb(LabColor.LinearGradient(startColor, endColor, time));




            spriteBatch.End();

            UserInterface.Active.DrawMainRenderTarget(spriteBatch);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

       
    }
    public class GameState
    {

        public Dictionary<string, int> allTraits;
        public Dictionary<string, List<int>> allStatuses;

        public GameState(Graph master)
        {
            List<Node> allNodes = new List<Node>();
            allNodes.AddRange(master.Nodes);

            for (int i = 0; i < allNodes.Count; i++)
            {

                if (allNodes[i].SubGraph != null) allNodes.AddRange(allNodes[i].SubGraph.Nodes);
            }
            

            Dictionary<string, int> allTraits = new Dictionary<string, int>();
            Dictionary<string, List<int>> allStatuses = new Dictionary<string, List<int>>();


            foreach (Node n in allNodes)
               {
                   foreach (string s in n.Statuses)
                   {
                       if (allTraits.Keys.Contains(s)) allTraits[s]++;
                       else allTraits.Add(s, 1);
                   }
                   foreach(KeyValuePair<string,int> kvp in n.Traits)
                {
                    if (allStatuses.Keys.Contains(kvp.Key)) allStatuses[kvp.Key].Add(kvp.Value);
                    else {
                        List<int> x = new List<int>  { kvp.Value };
                        allStatuses.Add(kvp.Key, x);
                    }
                }
               }
            
            this.allTraits = allTraits;
            this.allStatuses = allStatuses;
        }
    }
    
}
