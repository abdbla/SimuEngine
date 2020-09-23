using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimuEngine;
using Core;
using System.Linq;
using System.Collections.Generic;
using System;

namespace NodeMonog
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MegaLoop : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont arial;
        // hud elements:
        Texture2D circle, pixel, topCurve, tickButton, square, arrow;
        int selectedTab = 0;
        MouseState oms = Mouse.GetState();
        Random r = new Random();

        int dragtimer = 0;
        Point cameraPosition = Point.Zero, cameraGoal = Point.Zero,  cameraPress = Point.Zero;
        Vector2 cameraVelocity = Vector2.Zero;

        const int zwoomTime = 200;
        const int animThreshold = short.MaxValue;
        int frameRate = 0;

        int animation = 0;
        const int animationRepeat = short.MaxValue;

        int transitionAnimation = animThreshold;

        double zoomlevel = 1f;

       
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
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            //RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };

            graphics.ApplyChanges();



            testNode = new ShittyAssNode(new Point(r.Next(0,64)));
            testNode2 = new ShittyAssNode(new Point( 72 + r.Next(0, 64), r.Next(0,128)));
            testNode3 = new ShittyAssNode(new Point(156 + r.Next(0, 64), r.Next(0,128)));
            testNode4 = new ShittyAssNode(new Point(256 + r.Next(0, 64), r.Next(0,128)));
            testNode5 = new ShittyAssNode(new Point( 326 + r.Next(0, 64), r.Next(0,128)));


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
            graph.AddConnection(testNode, testNode2, new ShittyAssKnect (2000, 1000));
            graph.AddConnection(testNode, testNode3, new ShittyAssKnect (2000, 1000));
            graph.AddConnection(testNode, testNode5, new ShittyAssKnect (2000, 1000));
            graph.AddConnection(testNode, testNode4, new ShittyAssKnect (1000, 500));
            graph.AddConnection(testNode2, testNode3, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode2, testNode, new ShittyAssKnect (2000, 1000));
            graph.AddConnection(testNode4, testNode3, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode4, testNode2, new ShittyAssKnect(2000, 1000));
            graph.AddConnection(testNode4, testNode, new ShittyAssKnect (2000, 1000));

            selectedNode = testNode;
            previosNode = testNode;
            //selectedNode.connections.Add(new ShittyAssKnect(100, 20));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));
            //selectedNode.connections.Add(new ShittyAssKnect(4, 2));

           // cameraGoal = new Point(Window.ClientBounds.Width / 3, Window.ClientBounds.Height / 2);
    

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

                        //if (new Rectangle(r.Height - 64, 0, 256, 64).Contains(nms.Position)) ; //Call Tick
                        //else
                        //{
                        //    //float spinInterval = (float)Math.PI / selectedNode.connections.Count;
                        //    //for (int i = 0; i < selectedNode.connections.Count; i++)
                        //    //{
                        //    //    if (new Rectangle((int)(x + Math.Cos(spinInterval * i) * x / 2) - x / 8, (int)(r.Height / 2 + Math.Sin(spinInterval * i) * x / 2) - x / 8, x / 4, x / 4)
                        //    //        .Contains(nms.Position)) /*selectedNode = selectedNode.connections[i]*/;
                        //    //
                        //    //}
                        //}

                        for (int i = 0; i < graph.GetNodes().Count; i++)
                        {
                            ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];


                            if (new Rectangle(
                                x: (int)(currentNode.position.X * zoomlevel) + cameraPosition.X,
                                y: (int)(currentNode.position.Y * zoomlevel) + cameraPosition.Y,
                                width:  (int)(64 * zoomlevel),
                                height: (int)(64 * zoomlevel)).Contains(nms.Position))
                            {
                                selectedNode = currentNode;
                                cameraGoal = new Point(x - selectedNode.position.X, r.Height / 2 - selectedNode.position.Y);
                                //cameraPosition += new Point((int)((((x + Math.Cos(spinInterval * i) * x / 2f) - x / 8) + cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * i) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel)) - new Point((int)((x - x / 6 + cameraPosition.X) * zoomlevel), (int)((r.Height / 2 - x / 6 + cameraPosition.Y) * zoomlevel));


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
                //if(nms.ScrollWheelValue != oms.ScrollWheelValue) 
                    zoomlevel *= ( (oms.ScrollWheelValue - nms.ScrollWheelValue )/ 2000f) + 1f;


            }
            
            if(dragtimer == 0)
            {
cameraVelocity = ((cameraGoal - cameraPosition).ToVector2() / zwoomTime);
            }

            

            cameraPosition += (cameraVelocity * gameTime.ElapsedGameTime.Milliseconds).ToPoint();

            if(gameTime.ElapsedGameTime.Milliseconds != 0)frameRate = 1000 / gameTime.ElapsedGameTime.Milliseconds; 

            if(transitionAnimation < animThreshold)
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


            int x = r.Width / 3;
            


            spriteBatch.DrawString(arial, (animation % 1000).ToString() + "   :   " + transitionAnimation,  Vector2.Zero,Color.Black);

            spriteBatch.DrawString(arial, frameRate.ToString() + "fps", new Vector2(0, 32), Color.Black);


            //float spinInterval = (float)MathHelper.Pi / selectedNode.connections.Count * 2;

            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];

                Color selectcolour;
                if (selectedNode == currentNode) selectcolour = Color.Black;
                else selectcolour = new Color(0, 0, 0, 15);

                foreach ((Connection c, Node n) in graph.GetConnections(currentNode))
                {
                    ShittyAssNode nConvert = (ShittyAssNode)n;
                    Vector2 pointerKinda = (new Vector2(
                        x: nConvert.position.X - currentNode.position.X,
                        y: nConvert.position.Y - currentNode.position.Y));
                    double rotation = Math.Atan(pointerKinda.Y / pointerKinda.X);
                    if (pointerKinda.X < 0) rotation += Math.PI;
                    Point offsetPoint = new Point((int)(32 + 16 * Math.Cos(rotation)), (int)(32 + 16 * Math.Sin(rotation)));
                    

                    spriteBatch.Draw(pixel, 
                        destinationRectangle: new Rectangle(
                        x: (int)((currentNode.position.X + offsetPoint.X) * zoomlevel + cameraPosition.X),
                        y: (int)((currentNode.position.Y + offsetPoint.Y) * zoomlevel + cameraPosition.Y),
                        width: (int)(pointerKinda.Length() * zoomlevel),
                        height: (int)(8 * zoomlevel)),
                        color: selectcolour,
                        rotation: (float)rotation,
                        origin: new Vector2(0,0.5f),
                        effects: SpriteEffects.None,
                        layerDepth: 0.5f
                        );

                    spriteBatch.Draw(pixel,
                        destinationRectangle: new Rectangle(
                        x: (int)((currentNode.position.X + offsetPoint.X) * zoomlevel + cameraPosition.X),
                        y: (int)((currentNode.position.Y + offsetPoint.Y) * zoomlevel + cameraPosition.Y),
                        width: (int)(4 * zoomlevel),
                        height: (int)(8 * zoomlevel)),
                        color: Color.Red,
                        rotation: (float)rotation,
                       /* origin: new Vector2(0, (int)(4 * zoomlevel)),*/
                        effects: SpriteEffects.None,
                        layerDepth: 0.5f
                        );
                }




            }


            //A repeat becaue of the bitch ass alyers not working
            for (int i = 0; i < graph.GetNodes().Count; i++)
            {
                ShittyAssNode currentNode = (ShittyAssNode)graph.GetNodes()[i];

                spriteBatch.Draw(circle, destinationRectangle: new Rectangle(
                    x: (int)(currentNode.position.X * zoomlevel) + cameraPosition.X,
                    y: (int)(currentNode.position.Y * zoomlevel) + cameraPosition.Y,
                    width: (int)(64 * zoomlevel),
                    height: (int)(64 * zoomlevel)),
                    color: Color.White,
                    layerDepth: 0.75f);

                spriteBatch.DrawString(arial, currentNode.NName, (currentNode.position ).ToVector2() * (float)zoomlevel + cameraPosition.ToVector2(), Color.Black);

            }


            var startColor = LabColor.RgbToLab(new Color(0xA5, 0xD7, 0xC8));
            Console.WriteLine(startColor);
            var endColor = LabColor.RgbToLab(new Color(0x48, 0x73, 0x66));
            float time = transitionAnimation / (float)animThreshold;
            time = 1 - (float)Math.Pow(1 - time, 3);
            var color = LabColor.LabToRgb(LabColor.LinearGradient(startColor, endColor, time));

           // spriteBatch.Draw(circle, new Rectangle(
           //         x: (int)((x - x / (8 - time * 2) + cameraPosition.X) * zoomlevel),
           //         y: (int)((r.Height / 2 - x / (8 - time * 2) + cameraPosition.Y) * zoomlevel),
           //         width: (int)(x / (4 - time) * zoomlevel),
           //         height: (int)(x / (4 - time) * zoomlevel)
           //     ),
           //    color);

                //new Color(
                //    transitionAnimation / (float)animThreshold, 0, 1.0f - transitionAnimation / (float)animThreshold)
                //);
            
            
            
            spriteBatch.Draw(tickButton, new Rectangle(0, r.Height - 64, 256, 64), Color.DarkGray);
           

            spriteBatch.Draw(pixel, new Rectangle(x * 2, 16, x + 1, r.Height - 16), Color.DarkGray);
            spriteBatch.Draw(topCurve, new Rectangle(x * 2 + x / 4 * selectedTab, 0, x / 4, 16), Color.DarkGray);
            spriteBatch.DrawString(arial, "Global", new Vector2(x * 2 + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Group", new Vector2(x * 2.25f + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Person", new Vector2(x * 2.5f + 2, 0), Color.Black);
            spriteBatch.DrawString(arial, "Stats", new Vector2(x * 2.75f + 2, 0), Color.Black);


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
                        spriteBatch.DrawString(arial, kv.Key + ":   " + kv.Value, new Vector2(2 * x + 16, 64 + 32 * o++), Color.Black);

                    }

                    //for (int i = 0; i < selectedNode.traits.Count; i++)
                    //{
                    //    spriteBatch.DrawString(arial, selectedNode.traits[].ToString(), new Vector2(x + 16,64 + 32 * i), Color.Black);
                    //}

                    break;
                case 3:
                    if (r.Width > 720)
                    {
                        for (int i = 0; i < (x / 32) - 1; i++)
                        {
                            for (int j = 0; j < (r.Height / 3) / 32; j++) {
                                spriteBatch.Draw(square, new Rectangle(x * 2 + ((x % 32) + 32)/ 2 + i * 32, r.Height / 3 * 2 + j * 32 - 64, 32, 32), Color.Black);
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

    //class TickInfo{
    //    int dead, alive, sick;
    //
    //    public TickInfo(int dead, int alive, int sick)
    //    {
    //        this.dead = dead;
    //        this.alive = alive;
    //        this.sick = sick;
    //    }
    //}
}
