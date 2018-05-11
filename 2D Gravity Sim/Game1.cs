using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2D_Gravity_Sim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D circleSprite;
        SpriteFont spriteFont;
        const int textHeight = 12;
        Color textColor = Color.White;

        public const int maxBodies = 50;
        private bool mergeBodies = true;
        private float minMass = 10;
        private float maxMass = 100;
        float maxPos = 100.0f;
        float maxMom = 50.0f;
        float angMomMean = 200;
        float angMomStdDev = 50;

        public bool IsRunning { get; set; }

        private GamePadState currentGamePadState;
        private MouseState currentMouseState;
        private KeyboardState currentKeyboardState;

        private GamePadState previousGamePadState;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;

        // Screen res
        public const int windowWidth = 1280;
        public const int windowHeight = 720;

        // Draw scale factor
        public float DrawScale { get; private set; }
        public const float maxZoomSpeed = 0.2f;
        public const float zoomIncrement = 0.1f;

        // Timescale factor
        public float TimeScale { get; private set; }
        public const float timeScaleIncrement = 0.25f;

        // Camera position, can be moved by the user
        public Vector2 CameraPos { get; set; }
        public const float maxCameraSpeed = 100;

        // Total elapsed system time
        public float TotalSystemTime { private set; get; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            DrawScale = 1.0f;
            TimeScale = 1.0f;
            
            // Set the window size
            graphics.IsFullScreen = false;
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();
            
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
            System.Initialise(maxBodies, minMass, maxMass, maxPos, maxMom, angMomMean, angMomStdDev);

            IsRunning = true;
            int seed = 10;
            RNG.Seed = seed;

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

            circleSprite = Content.Load<Texture2D>("circle");
            spriteFont = Content.Load<SpriteFont>("MonoSpatial");
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // Get inputs from the controller, mouse & keyboard
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();

            float gameTimeSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Get left stick input
            CameraPos += currentGamePadState.ThumbSticks.Left * maxCameraSpeed * gameTimeSeconds;

            // Get right stick state
            DrawScale += currentGamePadState.ThumbSticks.Right.Y * maxZoomSpeed * gameTimeSeconds;

            // Update time scale from bumpers
            if (currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed
                && previousGamePadState.Buttons.LeftShoulder != ButtonState.Pressed)
            {
                TimeScale -= timeScaleIncrement;
            }

            if (currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed
                && previousGamePadState.Buttons.RightShoulder != ButtonState.Pressed)
            {
                TimeScale += timeScaleIncrement;
            }

            // Pause/unpause game using start button
            if (currentGamePadState.Buttons.Start == ButtonState.Pressed
                && previousGamePadState.Buttons.Start != ButtonState.Pressed)
            {
                IsRunning = !IsRunning;
            }

            // Get camera (click and drag) input from mouse
            if (currentMouseState.LeftButton == ButtonState.Pressed
                && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 currentPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Vector2 previousPos = new Vector2(previousMouseState.X, previousMouseState.Y);

                Vector2 amountMoved = currentPos - previousPos;

                CameraPos += new Vector2(-1, 1) * amountMoved;
            }

            // Get camera zoom input from the scroll wheel
            if (currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
            {
                DrawScale += zoomIncrement;
            }
            else if (currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue
                && DrawScale > 2 * zoomIncrement)
            {
                DrawScale -= zoomIncrement;
            }

            // Pause game using key input
            if (CheckOneKeyPress(Keys.Space))
            {
                IsRunning = !IsRunning;
            }

            // Center camera using the "C" key
            if (CheckOneKeyPress(Keys.C))
            {
                CameraPos = Vector2.Zero;
            }

            // Change timescale value with left/right keys
            if (CheckOneKeyPress(Keys.Left))
            {
                TimeScale -= timeScaleIncrement;
            }

            if (CheckOneKeyPress(Keys.Right))
            {
                TimeScale += timeScaleIncrement;
            }

            // Record the current input states for the next loop
            previousGamePadState = currentGamePadState;
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;

            // Get the frame time
            float frameTime = TimeScale * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update system
            if (IsRunning)
            {
                System.Update(frameTime);
                TotalSystemTime += frameTime;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Draw the bodies
            foreach (Body body in System.Bodies)
            {
                // Calculate screen coordinates
                int X = (int)(DrawScale * body.Pos.X - body.Radius) + windowWidth / 2 - (int)CameraPos.X;
                int Y = - (int)(DrawScale * body.Pos.Y - body.Radius) + windowHeight / 2 + (int)CameraPos.Y;

                int size = (int)(DrawScale * 2 * body.Radius);

                Rectangle rect = new Rectangle(X, Y, size, size);

                spriteBatch.Draw(circleSprite, rect, Color.White);
            }



            // Draw the text info

            DrawText("Seed: " + RNG.Seed, 0);

            DrawText("Total time: " + TotalSystemTime, 2);
            DrawText("TimeScale: x" + TimeScale, 3);

            if (!IsRunning)
            {
                DrawText("PAUSED", 5);
            }

            DrawText("Camera: (" + CameraPos.X + ", " + CameraPos.Y + ")", 7);
            DrawText("Zoom: x" + DrawScale, 8);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draw a line of text in the top left corner
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="text"></param>
        /// <param name="row">Number of rows down from corner</param>
        private void DrawText(string text, int row)
        {
            spriteBatch.DrawString(spriteFont, text, new Vector2(0, row * textHeight), textColor);
            return;
        }

        private bool CheckOneKeyPress(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }
    }
}
