using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimuEngine;
using Core;
using System.Linq;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;

namespace NodeMonog
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MegaLoop : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState oms = Mouse.GetState();

        // hud elements:
        Texture2D circle, pixel, topCurve, tickButton, square, arrow;
        SpriteFont arial;
        int selectedTab = 0;

        int dragtimer = 0;
        Point cameraPosition = Point.Zero, cameraGoal = Point.Zero, cameraPress = Point.Zero;
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


        ShittyAssNode testNode;
        ShittyAssNode testNode2;
        ShittyAssNode testNode3;
        ShittyAssNode testNode4;
        ShittyAssNode testNode5;

        ShittyAssNode selectedNode;
        ShittyAssNode previosNode;



        Graph graph = new Graph();

        //List<TickInfo> 


        public MegaLoop()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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



            testNode = new ShittyAssNode(new Vector2(r.Next(0, 64)));
            testNode2 = new ShittyAssNode(new Vector2(72 + r.Next(0, 64), r.Next(0, 128)));
            testNode3 = new ShittyAssNode(new Vector2(156 + r.Next(0, 64), r.Next(0, 128)));
            testNode4 = new ShittyAssNode(new Vector2(256 + r.Next(0, 64), r.Next(0, 128)));
            testNode5 = new ShittyAssNode(new Vector2(326 + r.Next(0, 64), r.Next(0, 128)));


            testNode.traits.Add("Age", 500);
            testNode.traits.Add("Corona", 200);

            testNode2.traits.Add("Age", 100);
            testNode2.traits.Add("Corona", 300);


            testNode3.traits.Add("Age", 10);
            testNode3.traits.Add("Corona", 200);


            testNode4.traits.Add("Age", 10);
            testNode4.traits.Add("Corona", 200);


            testNode5.traits.Add("Age", 10);
            testNode5.traits.Add("Corona", 200);


            graph.Add(testNode);
            graph.Add(testNode2);
            graph.Add(testNode3);
            graph.Add(testNode4);
            graph.Add(testNode5);

            testNode.NName = "billy";
            testNode2.NName = "Steve";
            testNode3.NName = "Felix";
            testNode4.NName = "Felix But good";
            testNode5.NName = "Felix 2";

            //Doesn't work btw                                          
            graph.AddConnection(testNode, testNode2, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode, testNode3, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode, testNode5, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode, testNode4, new ShittyAssKnect(1000, 500));
            graph.AddConnection(testNode2, testNode3, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode2, testNode, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode4, testNode3, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode4, testNode2, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode4, testNode, new ShittyAssKnect(2000, 1000));

            selectedNode = testNode;
            previosNode = testNode;
            //selectedNode.connections.Add(new ShittyAssKnect(100, 20));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));

            // cameraGoal = new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);

            List<ShittyAssNode> more = new List<ShittyAssNode>();
            for (int i = 0; i < 50; i++) {
                var n = new ShittyAssNode();
                n.NName = i.ToString();
                more.Add(n);
                graph.Add(n);
            }

            Dictionary<ShittyAssNode, int> totalConns = new Dictionary<ShittyAssNode, int>();
            Dictionary<ShittyAssNode, int> curConns = new Dictionary<ShittyAssNode, int>();
            List<ShittyAssNode> remaining = new List<ShittyAssNode>();
            remaining.AddRange(more);

            var rng = new Random();
            for (int i = 0; i < more.Count; i++) {
                totalConns[more[i]] = rng.Next(2, 4);
                curConns[more[i]] = 0;
            }
            
            while (remaining.Count > 1) {
                var x1 = rng.Next(remaining.Count);
                var x2 = rng.Next(remaining.Count);
                if (x1 == x2) {
                    continue;
                }
                var node1 = remaining[x1];
                var node2 = remaining[x2];
                var strength = rng.Next(100, 400);
                graph.AddConnection(node1, node2, new ShittyAssKnect(strength, 500));
                graph.AddConnection(node2, node1, new ShittyAssKnect(strength, 500));
                curConns[node1] += 1;
                curConns[node2] += 1;
                if (curConns[node1] == totalConns[node1]) {
                    if (x1 < x2) {
                        x2 -= 1;
                    }
                    remaining.RemoveAt(x1);
                }
                if (curConns[node2] == totalConns[node2]) {
                    remaining.RemoveAt(x2);
                }
            }

            ShittyAssNode.simulation = new Core.Physics.System(graph, 0.8f, 0.5f, 0.3f, 0.4f);

            base.Initialize();
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
            ShittyAssNode.simulation.Advance(gameTime.ElapsedGameTime.Milliseconds / timeFactor);

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
                        else if (new Rectangle((int)(x * 2.25f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 1;
                        else if (new Rectangle((int)(x * 2.5f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 2;
                        else if (new Rectangle((int)(x * 2.75f), 0, x / 4, 16).Contains(nms.Position)) selectedTab = 3;
                    }
                    //Vänsta hud klick
                    else
                    {
                        cameraPress = nms.Position - cameraPosition;


                        for (int i = 0; i < graph.GetNodes().Count; i++)
                        {
                            ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];


                            if (new Rectangle(scale(currentNode.Position).ToPoint(), new Point(
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
                            cameraPosition = nms.Position - cameraPress;
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
                    zoomlevel *= ((oms.ScrollWheelValue - nms.ScrollWheelValue) / 2000f) + 1f;

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

            cameraGoal = new Vector2(x - selectedNode.Position.X, r.Height / 2 - selectedNode.Position.Y).ToPoint();


            // TODO: Add your update logic here

            base.Update(gameTime);
        }




        //Methods to generalise and make more readable, aka make Theo happy

        public Point scale(Point p)
        {
            return new Point((int)((p.X - cameraPosition.X) * zoomlevel), (int)((p.Y - cameraPosition.Y) * zoomlevel)) +
                new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }

        public Vector2 scale(Vector2 v)
        {
            return (v - cameraPosition.ToVector2()) * (float)zoomlevel +
                new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
        }


        public int scale(int i, bool xAxis)
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

            spriteBatch.Begin();


            //spriteBatch.Draw(circle, new Rectangle(r.Width / 3 - r.Width / 18, r.Height / 2 - r.Width / 18, r.Width / 9, r.Width / 9), Color.Red);
            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);
            spriteBatch.DrawString(arial, "Tick", new Vector2(16, r.Height - 48), Color.Black);


            int centerX = r.Width / 3;



            spriteBatch.DrawString(arial, (animation % 1000).ToString() + "   :   " + transitionAnimation, Vector2.Zero, Color.Black);

            spriteBatch.DrawString(arial, frameRate.ToString() + "fps", new Vector2(0, 32), Color.Black);


            //float spinInterval = (float)MathHelper.Pi / selectedNode.connections.Count * 2;

            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];

                Color selectcolour;
                if (selectedNode == currentNode) selectcolour = Color.Black;
                else selectcolour = new Color(0, 0, 0, 15);

                foreach ((Connection c, ShittyAssNode n) in graph.GetConnections(currentNode).Select(parent => (parent.Item1, (ShittyAssNode)parent.Item2)))
                {
                    Vector2 arrowVector = (n.Position - currentNode.Position);
                    double rotation = Math.Atan(arrowVector.Y / arrowVector.X);
                    if (arrowVector.X < 0) rotation += Math.PI;

                    Vector2 offsetPoint = new Vector2(
                        (float)(circleDiameter / 2 + circleDiameter / 4 * Math.Cos(rotation)),
                        (float)(circleDiameter / 2 + circleDiameter / 4 * Math.Sin(rotation)));


                    spriteBatch.Draw(pixel,
                        destinationRectangle: new Rectangle(scale((currentNode.Position + offsetPoint).ToPoint()),
                        new Point(
                        (int)(arrowVector.Length() * zoomlevel),
                         (int)(8 * zoomlevel))),
                        sourceRectangle: null,
                        color: selectcolour,
                        rotation: (float)rotation,
                        origin: new Vector2(0, 0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: 0.5f
                        );

                    spriteBatch.Draw(pixel,
                        destinationRectangle:
                        new Rectangle(scale(currentNode.Position + offsetPoint).ToPoint(),
                        new Point(
                         (int)(4 * zoomlevel),
                         (int)(8 * zoomlevel))),
                        sourceRectangle: null,
                        color: Color.Red,
                        rotation: (float)rotation,
                        origin: new Vector2(0, 0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: 0.5f
                        );
                }
            }


            //A repeat becaue of the bitch ass alyers not working
            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];

                spriteBatch.Draw(circle,
                    destinationRectangle: new Rectangle(scale(currentNode.Position).ToPoint(),
                    new Point(
                    (int)(circleDiameter * zoomlevel),
                    (int)(circleDiameter * zoomlevel))),
                    sourceRectangle: null,
                    color: Color.White);

                spriteBatch.DrawString(arial,
                    currentNode.NName,
                    scale(currentNode.Position), Color.Black);

            }


            var startColor = LabColor.RgbToLab(new Color(0xA5, 0xD7, 0xC8));
            Console.WriteLine(startColor);
            var endColor = LabColor.RgbToLab(new Color(0x48, 0x73, 0x66));
            float time = transitionAnimation / (float)animThreshold;
            time = 1 - (float)Math.Pow(1 - time, 3);
            var color = LabColor.LabToRgb(LabColor.LinearGradient(startColor, endColor, time));



            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);


            spriteBatch.Draw(pixel, new Rectangle(centerX * 2, 16, centerX + 1, r.Height - 16), Color.DarkGray);
            spriteBatch.Draw(topCurve, new Rectangle(centerX * 2 + centerX / 4 * selectedTab, 0, centerX / 4, 16), Color.DarkGray);
            spriteBatch.DrawString(arial, "Global", new Vector2(centerX * 2 + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Group", new Vector2(centerX * 2.25f + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Person", new Vector2(centerX * 2.5f + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Stats", new Vector2(centerX * 2.75f + 2, 0), Color.Black);


            switch (selectedTab)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    int o = 0;
                    foreach (KeyValuePair<string, int> kv in selectedNode.traits)
                    {
                        spriteBatch.DrawString(arial, kv.Key + ":   " + kv.Value, new Vector2(2 * centerX + 16, 64 + 32 * o++), Color.Black);

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
