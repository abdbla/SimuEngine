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
        Texture2D circle, pixel, topCurve, tickButton, square;
        int selectedTab = 0;
        MouseState oms = Mouse.GetState();

        int dragtimer = 0;
        Point cameraPosition = Point.Zero, cameraGoal = Point.Zero,  cameraPress = Point.Zero;
        Vector2 cameraVelocity = Vector2.Zero;

        const int zwoomTime = 200;
        const int animThreshold = 10000;
        int frameRate = 0;

        int transitionAnimation = animThreshold;

        double zoomlevel = 1f;

        Node testNode; 
        Node testNode2;
        Node testNode3;
        Node testNode4;
        Node testNode5;

        Node selectedNode;
        Node previosNode;



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
            testNode = new ShittyAssNode();
            testNode2 = new ShittyAssNode();
            testNode3 = new ShittyAssNode();
            testNode4 = new ShittyAssNode();
            testNode5 = new ShittyAssNode();


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

            testNode.name = "billy";
            testNode2.name = "Steve";
            testNode3.name = "Felix";
            testNode4.name = "Felix But good";
            testNode5.name = "Felix 2";

            //Doesn't work btw
            graph.AddConnection(testNode, testNode2, new ShittyAssKnect(200,100));
            graph.AddConnection(testNode, testNode3, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode, testNode5, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode, testNode4, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode2, testNode3, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode2, testNode, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode4, testNode3, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode4, testNode2, new ShittyAssKnect(200, 100));
            graph.AddConnection(testNode4, testNode, new ShittyAssKnect(200, 100));

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

            
            // TODO: use this.Content to load your game content here
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
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
                    //screen click
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

                        float spinInterval = (float)MathHelper.Pi / graph.GetConnections(selectedNode).Count * 2;
                        int i = 0;
                        foreach ((Connection c, Node n) in graph.GetConnections(selectedNode))
                        {
                            //Changing selected Node
                            if (new Rectangle((int)((((x + Math.Cos(spinInterval * i) * x / 2f) - x / 8) + cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * i) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel), (int)(x / 4f * zoomlevel), (int)(x / 4 * zoomlevel))
                                .Contains(nms.Position))
                            {
                                previosNode = selectedNode;
                                selectedNode = n;
                                cameraPosition += new Point((int)((((x + Math.Cos(spinInterval * i) * x / 2f) - x / 8) + cameraPosition.X) * zoomlevel),(int)(((r.Height / 2 + Math.Sin(spinInterval * i) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel)) - new Point((int)((x - x / 6 + cameraPosition.X) * zoomlevel), (int)((r.Height / 2 - x / 6 + cameraPosition.Y) * zoomlevel));
                                //cameraGoal -= cameraGoal + cameraGoal;
                                //cameraPosition -= cameraPosition + cameraPosition;
                                transitionAnimation = 0;
                            }
                            
                            i++;
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


            oms = nms;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            Rectangle r = Window.ClientBounds;

            spriteBatch.Begin();

            


            spriteBatch.DrawString(arial, Mouse.GetState().LeftButton.ToString() + "   :   " + transitionAnimation,  Vector2.Zero,Color.Black);

            spriteBatch.DrawString(arial, frameRate.ToString(), new Vector2(0, 32), Color.Black);

            int x = r.Width / 3;

            //float spinInterval = (float)MathHelper.Pi / selectedNode.connections.Count * 2;
            float spinInterval = (float)MathHelper.Pi / graph.GetConnections(selectedNode).Count * 2;
            int ii = 0;
            foreach ((Connection c, Node n) in graph.GetConnections(selectedNode))
            {
                
                float spiiinInterval = (float)MathHelper.Pi / (graph.GetConnections(n).Count + 1) * 2;
                int iii = 0;

                foreach ((Connection cc, Node nn) in graph.GetConnections(n))
                {
                    spriteBatch.Draw(pixel, new Rectangle((int)((x + Math.Cos(spinInterval * ii) * x / 2f + cameraPosition.X) * zoomlevel), (int)((r.Height / 2 + Math.Sin(spinInterval * ii) * x / 2 + cameraPosition.Y) * zoomlevel), (int)(x / 4 * zoomlevel), (int)(2 * zoomlevel)), sourceRectangle: new Rectangle(0, 0, 1, 1), color: new Color(0,0,0,200), rotation: spinInterval * iii, origin: Vector2.Zero, SpriteEffects.None, 1);

                    spriteBatch.Draw(circle, new Rectangle((int)((((x + Math.Cos(spinInterval * ii) * x / 2f + Math.Cos(spinInterval * iii) * x / 4f) - x / 16) + cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * ii) * x / 2 + Math.Sin(spinInterval * iii) * x / 4) - x / 16 + cameraPosition.Y) * zoomlevel), (int)(x / 8f * zoomlevel), (int)(x / 8 * zoomlevel)), new Color(0,0,255,150));
                    iii++;
                }


                spriteBatch.Draw(pixel, new Rectangle((int)((x + cameraPosition.X) * zoomlevel),(int)(( r.Height / 2 + cameraPosition.Y) * zoomlevel),(int)( x / 2  * zoomlevel), (int)(4 * zoomlevel)), sourceRectangle: new Rectangle(0,0,1,1), color:  Color.Black, rotation: spinInterval * ii, origin: Vector2.Zero,  SpriteEffects.None, 1);
                if (n == previosNode)
                {
                    //  spriteBatch.Draw(circle, new Rectangle(
                    // x: (int)((x + Math.Cos(spinInterval * iiii) * x / 2f - x / (7 - transitionAnimation / (double)animThreshold * 2) + cameraPosition.X) * zoomlevel),
                    // y: (int)((r.Height / 2 + Math.Sin(spinInterval * iiii) * x / 2) - (x / (8 - transitionAnimation / (double)animThreshold * 2) + cameraPosition.Y) * zoomlevel),
                    // width: (int)(x / (3 + transitionAnimation / (double)animThreshold) * zoomlevel),
                    // height: (int)(x / (3 + transitionAnimation / (double)animThreshold) * zoomlevel)),
                    // new Color(1.0f - transitionAnimation / (float)animThreshold, 0, transitionAnimation / (float)animThreshold));
                    spriteBatch.Draw(circle, new Rectangle((int)((((x + Math.Cos(spinInterval * ii) * x / 2f) - x / 8) + cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * ii) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel), (int)(x / 4f * zoomlevel), (int)(x / 4 * zoomlevel)), Color.Blue);


                }
                else spriteBatch.Draw(circle, new Rectangle((int)((((x + Math.Cos(spinInterval * ii) * x / 2f) - x / 8) + cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * ii) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel), (int)(x / 4f * zoomlevel), (int)(x / 4 * zoomlevel)), Color.Blue);

                spriteBatch.DrawString(arial, n.name, new Vector2((int)((((x + Math.Cos(spinInterval * ii) * x / 2f) - x / 8)+ cameraPosition.X) * zoomlevel), (int)(((r.Height / 2 + Math.Sin(spinInterval * ii) * x / 2) - x / 8 + cameraPosition.Y) * zoomlevel)),Color.Black);
                            
                ii++;
            


            }

            spriteBatch.Draw(circle, new Rectangle(
                    x: (int)((x - x / (8 - transitionAnimation / (double)animThreshold * 2) + cameraPosition.X) * zoomlevel),
                    y: (int)(( r.Height / 2 - x / (8 - transitionAnimation / (double)animThreshold * 2) + cameraPosition.Y) * zoomlevel),
                    width: (int)(x / (4 - transitionAnimation /  (double)animThreshold) * zoomlevel),
                    height: (int)(x / (4 - transitionAnimation / (double)animThreshold) * zoomlevel)
                    
                ),
                new Color(transitionAnimation / (float)animThreshold, 0, 1.0f - transitionAnimation / (float)animThreshold)
                );
            spriteBatch.DrawString(arial, selectedNode.name, new Vector2((float)((x - x / 6 + cameraPosition.X * zoomlevel)), (float)( (r.Height / 2 - x / 6 + cameraPosition.Y) * zoomlevel)),Color.Black);

            
            
            
            
            
            
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
