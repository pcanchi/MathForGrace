using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace MathGame_
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch sp2;
        Texture2D arrow;
        Vector2 arrowpos = Vector2.Zero;
        private SpriteFont number_font;
        private SpriteFont end_round_font;
        KinectSensor kinect;
        Skeleton[] skeletonData;
        Skeleton skeleton;

        String cat = "";
        String cur = "divide";
        float currHandx = 0;
        float currHandy = 0;
        bool start = true;
        float timer = 5;
        const float TIMERCLOCK1 = 3;
        const float TIMERCLOCK2 = 3;
        
        bool time_flag1 = true; //initialized to true, will work like an on/off switch
        bool answer_flag = false; //initialized to false, will work like an on/off switch
        int correct_answer; //set this global value everytime equation generated
        int recorded_answer; //connect this up to kinect motion tracking
        int answer_choice_count = 0; //counter for displaying possible answers
        int correct_answer_slot; //this tells us which choice will be the correct answer
        int selected_answer; //we need to set this value once we detect a hand raise
        
        int current_answer_displayed;
        int question_count = 0;
        int correct_answer_count = 0;
        bool game_over_flag = false;
        bool answer_chosen = false;
        string firstnumstring = "";
        string secondnumstring = "";
        string choice = "(A)";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(100.0f / 100.0f);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        private void TrackedBoneLine(SkeletonPoint positionFrom, SkeletonPoint positionTo)
        {
            currHandx = positionFrom.X;
            currHandy = positionFrom.Y;
            Console.WriteLine(currHandy);
        }

        private void TrackBone(Joint jointFrom, Joint jointTo)
        {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked || jointTo.TrackingState == JointTrackingState.NotTracked)
            {
                return; // nothing to draw, one of the joints is not tracked
            }

            else
            {
                TrackedBoneLine(jointFrom.Position, jointTo.Position);
            }
        }

        private void TrackedSkeletonJoints(JointCollection jointCollection)
        {
            TrackBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);
        }   

        private void TrackSkeletons()
        {
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    TrackedSkeletonJoints(skeleton.Joints);
                }
            }
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                // Console.WriteLine("here");
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    // Console.WriteLine("inside");
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                    TrackSkeletons();
                }
            }

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            kinect = KinectSensor.KinectSensors[0];
            kinect.SkeletonStream.Enable();
            kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            kinect.Start();
            kinect.ElevationAngle = 0;
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
            sp2 = new SpriteBatch(GraphicsDevice);
            number_font = Content.Load<SpriteFont>("SpriteFont1");
            end_round_font = Content.Load<SpriteFont>("SpriteFont2");
            arrow = Content.Load<Texture2D>("arrow");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Pause screen for now is a 
            //simple flag that supresses Update()
            //from calling Draw(), so the text stays
            //on the screen
            if (game_over_flag)
            {
                this.SuppressDraw(); //TODO: make this more robust.
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time_flag1 = false;

            // TODO: Add your update logic here
            timer -= elapsed;
            if (timer < 0)
            {
                timer = elapsed + TIMERCLOCK1; //resetting timer
                time_flag1 = true;
            }
            if (currHandy >= 0.15)
                selected_answer = current_answer_displayed;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (cat == "")
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                sp2.Begin();
                sp2.DrawString(end_round_font, "Please select a category", new Vector2(100, 0), Color.DarkGreen);
                if (cur == "divide")
                {
                    cur = "add";
                    sp2.DrawString(end_round_font, "Addition", new Vector2(100, 100), Color.Yellow);
                    sp2.DrawString(end_round_font, "Subtraction", new Vector2(500, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Multiplication", new Vector2(100, 300), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Division", new Vector2(500, 300), Color.DarkGreen);
                    if (currHandy >= 0.15)
                    {
                        cat = cur;
                    }
                }

                else if (cur == "add")
                {
                    cur = "subtract";
                    sp2.DrawString(end_round_font, "Addition", new Vector2(100, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Subtraction", new Vector2(500, 100), Color.Yellow);
                    sp2.DrawString(end_round_font, "Multiplication", new Vector2(100, 300), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Division", new Vector2(500, 300), Color.DarkGreen);
                    if (currHandy >= 0.15)
                    {
                        cat = cur;
                    }
                }

                else if (cur == "subtract")
                {
                    cur = "multiply";
                    sp2.DrawString(end_round_font, "Addition", new Vector2(100, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Subtraction", new Vector2(500, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Multiplication", new Vector2(100, 300), Color.Yellow);
                    sp2.DrawString(end_round_font, "Division", new Vector2(500, 300), Color.DarkGreen);
                    if (currHandy >= 0.15)
                    {
                        cat = cur;
                    }
                }

                else if (cur == "multiply")
                {
                    cur = "divide";
                    sp2.DrawString(end_round_font, "Addition", new Vector2(100, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Subtraction", new Vector2(500, 100), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Multiplication", new Vector2(100, 300), Color.DarkGreen);
                    sp2.DrawString(end_round_font, "Division", new Vector2(500, 300), Color.Yellow);
                    if (currHandy >= 0.15)
                    {
                        cat = cur;
                    }
                }
                sp2.End();
            }

            else if (start)
            {
                sp2.Begin();
                sp2.DrawString(end_round_font, "You Chose " + cat, new Vector2(0, 10), Color.Yellow);
                sp2.DrawString(end_round_font, "Starting Round 1", new Vector2(100, 100), Color.DarkGreen);
                sp2.End();
                start = false;
                //this.TargetElapsedTime = TimeSpan.FromSeconds(100.0f / 100.0f);
            }

            else if (question_count < 1 && firstnumstring != "")
            {
                sp2.Begin();
                sp2.DrawString(number_font, firstnumstring, new Vector2(100, 10), Color.Black);
                sp2.DrawString(number_font, "+", new Vector2(300, 10), Color.Black);
                sp2.DrawString(number_font, secondnumstring, new Vector2(400, 10), Color.Black);
                sp2.DrawString(number_font, "=", new Vector2(600, 10), Color.Black);
                sp2.DrawString(number_font, "?", new Vector2(700, 10), Color.Black);
                sp2.End();
            }
            
            if((time_flag1 == true) && (answer_flag == false) && cat != "")
            {
                if (question_count < 1)
                {
                    //Random number generation for equations
                    Random r1 = new Random();
                    Random r2 = new Random();
                    Random r3 = new Random();

                    int firstnum = r1.Next(0, 50);
                    int secondnum = r2.Next(51, 100);
                    correct_answer_slot = r3.Next(0, 3);
                    correct_answer = firstnum + secondnum;
                    answer_flag = true; //setting switch
                    firstnumstring = firstnum.ToString();
                    secondnumstring = secondnum.ToString();
                    
                    base.Draw(gameTime);
                }
                else
                {

                    //Signal end of round
                    //calculate and display score!
                    spriteBatch.Begin();
                    spriteBatch.DrawString(end_round_font, "Round 1 Complete", new Vector2(100, 100), Color.Yellow);
                    string pass_fail_str;
                    string correct_str = correct_answer_count.ToString();
                    string total_str = question_count.ToString();
                    string results_str = "You got " + correct_str + "/" + total_str + " questions right";
                    if (correct_answer_count >= 2)
                    {
                        pass_fail_str = "You Won :)";
                    }
                    else
                    {
                        pass_fail_str = "You Lost :(";
                    }
                    spriteBatch.DrawString(end_round_font, pass_fail_str, new Vector2(100, 200), Color.Yellow);
                    spriteBatch.DrawString(end_round_font, results_str, new Vector2(100, 300), Color.Yellow);
                    //spriteBatch.DrawString(number_font, "A", new Vector2(Handx, Handy), Color.Black);
                    spriteBatch.End();
                    base.Draw(gameTime);
                    //------------------------------

                    //Setting and resetting flags/counters to pause
                    game_over_flag = true;
                    correct_answer_count = 0;
                    question_count = 0;
                }
            }
            else if ((time_flag1 == true) && (answer_flag == true) && cat != "")
            {
                if (answer_choice_count == 4)
                {
                    //break logic to display next random equation
                    answer_choice_count = 0;
                    answer_flag = false; //on-off switch set
                    question_count++;
                    if (selected_answer == correct_answer)
                    {
                        correct_answer_count++;
                    }
                }
                else
                {
                    if (answer_choice_count == correct_answer_slot)
                    {
                        //this means we must display the correct answer
                        string correct_answer_string = correct_answer.ToString();
                        spriteBatch.Begin();
                        spriteBatch.DrawString(number_font, choice, new Vector2(150, 200), Color.Yellow);
                        spriteBatch.DrawString(number_font, correct_answer_string, new Vector2(375, 200), Color.Yellow);
                        spriteBatch.End();
                        base.Draw(gameTime);
                        current_answer_displayed = correct_answer; //setting cur_answer for kinext reference
                    }
                    else
                    {
                        //display random numbers/answers
                        spriteBatch.Begin();
                        Random random_answer_seed = new Random();
                        int random_answer_num = random_answer_seed.Next(correct_answer - 20, correct_answer + 20);
                        string random_answer_str = random_answer_num.ToString();

                        spriteBatch.DrawString(number_font, choice, new Vector2(150, 200), Color.Yellow);
                        spriteBatch.DrawString(number_font, random_answer_str, new Vector2(375, 200), Color.Yellow);
                        spriteBatch.End();
                        base.Draw(gameTime);
                        current_answer_displayed = random_answer_num; //setting cur_answer for kinect to reference
                    }
                    answer_choice_count++;
                    if (choice == "(A)")
                        choice = "(B)";
                    else if (choice == "(B)")
                        choice = "(C)";
                    else if (choice == "(C)")
                        choice = "(D)";
                    else
                        choice = "(A)";
                }
            }

            base.Draw(gameTime);
        }
    }
}
