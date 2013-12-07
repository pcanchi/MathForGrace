using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualBasic;
//TODO: TEST kinect hand input more, change black text to white.
namespace MathGame_
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        // These variables are used to display text and images on the screen.
        SpriteBatch spriteBatch;
        SpriteBatch sp2;
        private SpriteFont number_font;
        private SpriteFont end_round_font;
        private SpriteFont eq, p_font;
        private SpriteFont countdown_font;
        Texture2D card, art, red, green, select, option, logo, felt, menu, options, arrow, inst;

        // These variables are used to handle kinect sensor data.
        KinectSensor kinect;
        Skeleton[] skeletonData;
        
        // Thest variables are used to load and play audio durin the course of the game.
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        SoundEffect soundEngine, soundEngine2, soundEngine3, soundEngine4, soundEngine5, soundEngine6;
        SoundEffectInstance click, qright, qwrong, pass_r, fail_r, reset_m;
        
        // These variables deal with calibration and tracking of the user's right wrist.
        float currHandx = 0;
        float currHandy = 0;
        float currHandz = 0;
        float yLow, yHigh;
        float cali = 99999;
        
        float timer = 5;
        const float TIMERCLOCK1 = 3;
        const float TIMERCLOCK2 = 3;
        
        bool time_flag1 = true; //initialized to true, will work like an on/off switch
        bool answer_flag = false; //initialized to false, will work like an on/off switch
        bool game_over_flag = false;
        bool answer_chosen = false;
        
        //These variables handle the math logic of the game.
        int correct_answer; //set this global value everytime equation generated
        int correct_answer_slot; //this tells us which choice will be the correct answer
        int selected_answer = -1; //we need to set this value once we detect a hand raise
        int current_answer_displayed = -1;
        int question_count = 0;
        int correct_answer_count = 0;
        int ans1, ans2, ans3, ans4;
        
        // The following strings are used to track the current state of the game.
        string firstnumstring = "";
        string secondnumstring = "";
        string choice = "";
        string lost = "e";
        String cat = "none";
        String cur = "none";
        string menuop = "none";
        string sop = "none";
        string hand = "";
        
        // The following integers are used to track and maintain round logic.
        int num_questions = 10;
        int pass = 7;
        int roundNum = 1;
        int addnum, subnum, mulnum, divnum;

        // The following list of booleans are used to transition through the different states of the game.
        bool start = true;
        bool restart = false;
        bool decision = false;
        bool answer_key_chosen = false;
        bool once = false;
        bool hdown = true;
        bool roundcomplete = false;
        bool calibrated = false;
        bool gomain = false;
        bool settings = false;
        bool up = false;
        bool scorescreen = false;
        bool catchosen = false;
        bool instdone = false;
        bool inputdone = false;

        int count = 20;

        //form to allow user to change threshold, and # questions per round
        public Form form1 = new Form();
        public TextBox text1 = new TextBox();
        public Button ok1 = new Button();
        public Button cancel1 = new Button();
        bool done1 = false;

        //form to allow the user to choose which hand will control the game.
        public Form form2 = new Form();
        public TextBox text2 = new TextBox();
        public Button rightHand = new Button();
        public Button leftHand = new Button();
        bool done2 = false;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            IsMouseVisible = true;
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
            currHandz = positionFrom.Z;
            Console.WriteLine(currHandz);
            if (currHandy < cali)
            {
                hdown = true;
            }
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
            if(hand == "r")
                TrackBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);
            
            else if(hand == "l")
                TrackBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);
        }

        private void TrackSkeletons()
        {
            float minZ = 9999;
            Skeleton player = new Skeleton();
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked && skeleton.Joints[JointType.WristRight].Position.Z < minZ)
                {
                    minZ = skeleton.Joints[JointType.WristRight].Position.Z;
                    player = skeleton;
                    //TrackedSkeletonJoints(skeleton.Joints);
                }
            }

            TrackedSkeletonJoints(player.Joints);
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                    TrackSkeletons();
                }
            }

        }

        public void rClick(object sender, EventArgs e)
        {
            hand = "r";
            form2.Hide();
            graphics.ToggleFullScreen();
            inputdone = true;
        }

        public void lClick(object sender, EventArgs e)
        {
            hand = "l";
            form2.Hide();
            graphics.ToggleFullScreen();
            inputdone = true;
        }

        public void c1Click(object sender, EventArgs e)
        {
            cali = (yLow + yHigh) / (float)2.0;
            graphics.ToggleFullScreen();
            form1.Hide();

        }

        public void ok1Click(object sender, EventArgs e)
        {
            string newval = text1.Text;
            bool success = true;
            int val = -1;
            try
            {
                val = Convert.ToInt32(newval);
            }

            catch (FormatException ex)
            {
                success = false;
                form1.Text = "Invalid entry... Please enter an integer between [5,10]";
            }

            if (success && val >= (num_questions/2) && val <= num_questions)
            {
                done1 = true;
                pass = val;
                form1.Hide();
                graphics.ToggleFullScreen();
                cali = (yLow + yHigh) / (float)2.0;
            }

            else
            {
                //done = false;
                form1.Text = "Invalid entry... Please enter an integer between [5,10]";
            }

        }

        public void showForm1()
        {
            cali = 9999;
            
            if (graphics.IsFullScreen)
                graphics.ToggleFullScreen();

            form1.Show();
        }

        public void showForm2()
        {
            if (graphics.IsFullScreen)
                graphics.ToggleFullScreen();

            form2.Show();
            form2.BringToFront();
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            try
            {
                kinect = KinectSensor.KinectSensors[0];
                kinect.SkeletonStream.Enable();
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
                kinect.Start();
                kinect.ElevationAngle = 0;
           }

            catch(ArgumentOutOfRangeException e)
            {
               this.Exit();
            }
            
            if (!(System.IO.File.Exists("state.txt")))
            {
                var v = System.IO.File.Create("state.txt");
                v.Close();
                addnum = subnum = mulnum = divnum = 1;

            }
            else
            {
                using (StreamReader sr = new StreamReader("state.txt"))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        string line = sr.ReadLine();
                        if (i == 0)
                            addnum = Convert.ToInt32(line);
                        else if (i == 1)
                            subnum = Convert.ToInt32(line);
                        else if (i == 2)
                            mulnum = Convert.ToInt32(line);
                        else
                            divnum = Convert.ToInt32(line);
                    }

                }
            }
            
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
            
            card = Content.Load<Texture2D>("card");
            art = Content.Load<Texture2D>("icon");
            eq = Content.Load<SpriteFont>("SpriteFont3");
            countdown_font = Content.Load<SpriteFont>("SpriteFont4");
            p_font = Content.Load<SpriteFont>("SpriteFont5");
            red = Content.Load<Texture2D>("red");
            green = Content.Load<Texture2D>("green");
            logo = Content.Load<Texture2D>("logo");
            option = Content.Load<Texture2D>("art");
            select = Content.Load<Texture2D>("please");
            felt = Content.Load<Texture2D>("felt");
            menu = Content.Load<Texture2D>("menu");
            options = Content.Load<Texture2D>("options");
            inst = Content.Load<Texture2D>("inst");
            
            soundEngine = Content.Load<SoundEffect>("click1");
            soundEngine2 = Content.Load<SoundEffect>("question_correct");
            soundEngine3 = Content.Load<SoundEffect>("question_wrong");
            soundEngine4 = Content.Load<SoundEffect>("round_fail");
            soundEngine5 = Content.Load<SoundEffect>("round_pass");
            soundEngine6 = Content.Load<SoundEffect>("reset");
            click = soundEngine.CreateInstance();
            qright = soundEngine2.CreateInstance();
            qwrong = soundEngine3.CreateInstance();
            fail_r = soundEngine4.CreateInstance();
            pass_r = soundEngine5.CreateInstance();
            reset_m = soundEngine6.CreateInstance();
            // TODO: use this.Content to load your game content here

            form1.AcceptButton = ok1;
            form1.CancelButton = cancel1;
            ok1.Text =  "OK";
            cancel1.Text = "Cancel";
            ok1.Location = new System.Drawing.Point(0, 100);
            cancel1.Location = new System.Drawing.Point(100, 100);
            text1.Location = new System.Drawing.Point(0, 0);
            text1.Width = 500;
            text1.Height = 1000;
            form1.Text = "Please enter a value between [5,10]";
            form1.Width = 500;
            form1.Height = 500;
            form1.Controls.Add(ok1);
            form1.Controls.Add(cancel1);
            form1.Controls.Add(text1);
            form1.ControlBox = false;
            ok1.Click += new EventHandler(ok1Click);
            cancel1.Click += new EventHandler(c1Click);

            rightHand.Text = "Right Hand";
            leftHand.Text = "Left Hand";
            leftHand.Location = new System.Drawing.Point(50, 25);
            rightHand.Location = new System.Drawing.Point(250, 25);
            form2.Text = "Please choose which hand controls the game";
            form2.BackColor = System.Drawing.Color.CornflowerBlue;
            rightHand.BackColor = System.Drawing.Color.LightGray;
            leftHand.BackColor = System.Drawing.Color.LightGray;
            form2.Width = 450;
            form2.Height = 100;
            form2.StartPosition = FormStartPosition.CenterScreen;
            form2.Controls.Add(rightHand);
            form2.Controls.Add(leftHand);
            rightHand.Click += new EventHandler(rClick);
            leftHand.Click += new EventHandler(lClick);
            form2.ControlBox = false;

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

            if (!inputdone)
            {
                showForm2();
            }
            
            else if (count == 0 && !instdone)
            {
                instdone = true;
                count = 10;
            }
            
            else if (count == 0 && !up && !calibrated)
            {
                yLow = currHandy;
                up = true;
                count = 5;
            }

            else if (count == 0 && up && !calibrated)
            {
                yHigh = currHandy;
                calibrated = true;
                gomain = true;
                cali = (yLow + yHigh) / (float)2.0;
                
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                if (cat == "Addition")
                {
                    addnum = roundNum;
                }
                else if (cat == "Subtraction")
                {
                    subnum = roundNum;
                }
                else if (cat == "Multiplication")
                {
                    mulnum = roundNum;
                }
                else
                {
                    divnum = roundNum;
                }
                string[] lines = { addnum.ToString(), subnum.ToString(), mulnum.ToString(), divnum.ToString() };
                System.IO.File.WriteAllLines("state.txt", lines);
                kinect.Dispose();
                this.Exit();
            }
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

            if (currHandy >= cali && cat != "none" && current_answer_displayed != -1 && hdown && calibrated && !gomain)
            {
                //click.Play();
                hdown = false;
                selected_answer = current_answer_displayed;
                answer_chosen = true;
                //answer_choice_count = 0;
                answer_flag = false; //on-off switch set
                answer_key_chosen = false;
                question_count++;
                if (selected_answer == correct_answer && selected_answer != -1)
                {
                    correct_answer_count++;
                } 

                choice = "";
                current_answer_displayed = -1;
            }

            else if (currHandy >= cali && decision == true && hdown && calibrated && !gomain && catchosen)
            {
                click.Play();
                fail_r.Stop();
                hdown = false;
                if (lost == "m")
                {
                    restart = true;
                    gomain = true;
                    catchosen = false;
                    calibrated = true;
                    decision = false;
                    question_count = 0;
                    correct_answer_count = 0;
                    start = true;
                    choice = "";
                    firstnumstring = "";
                    if (cat == "Addition")
                        addnum = roundNum;
                    else if (cat == "Subtraction")
                        subnum = roundNum;
                    else if (cat == "Multiplication")
                        mulnum = roundNum;
                    else
                        divnum = roundNum;
                    cat = "none";

                }

                else if (lost == "e")
                {
                    if (cat == "Addition")
                    {
                        addnum = roundNum;
                    }
                    else if (cat == "Subtraction")
                    {
                        subnum = roundNum;
                    }
                    else if (cat == "Multiplication")
                    {
                        mulnum = roundNum;
                    }
                    else if (cat == "Division")
                    {
                        divnum = roundNum;
                    }
                    string[] lines = { addnum.ToString(), subnum.ToString(), mulnum.ToString(), divnum.ToString() };
                    System.IO.File.WriteAllLines("state.txt", lines);
                    kinect.Dispose();
                    this.Exit();
                }

                else if (lost == "r")
                {
                    question_count = 0;
                    correct_answer_count = 0;
                    start = true;
                    choice = "";
                    firstnumstring = "";
                }

                lost = "e";
            }

            else if (currHandy >= cali && hdown && calibrated && !gomain && !catchosen)
            {
                click.Play();
                hdown = false;
                catchosen = true;
                restart = false;
                once = false;
                cat = cur;
                cur = "Division";
                if (cat == "Addition")
                    roundNum = addnum;
                else if (cat == "Subtraction")
                    roundNum = subnum;
                else if (cat == "Multiplication")
                    roundNum = mulnum;
                else
                    roundNum = divnum;
            }

            else if (currHandy >= cali && gomain && hdown && !settings && !scorescreen && calibrated)
            {
                hdown = false;
                click.Play();
                if (menuop == "s")
                    gomain = false;

                else if (menuop == "e")
                {
                    if (cat == "Addition")
                        addnum = roundNum;
                    else if (cat == "Division")
                        subnum = roundNum;
                    else if (cat == "Multiplication")
                        mulnum = roundNum;
                    else if (cat == "Division")
                        divnum = roundNum;

                    string[] lines = { addnum.ToString(), subnum.ToString(), mulnum.ToString(), divnum.ToString() };
                    System.IO.File.WriteAllLines("state.txt", lines);

                    kinect.Dispose();
                    this.Exit();

                }

                else if (menuop == "o")
                {
                    settings = true;
                    
                }
                menuop = "e";
            }

            else if (currHandy >= cali && gomain && hdown && settings && !scorescreen && calibrated)
            {
                click.Play();
                hdown = false;
                if (sop == "b")
                {
                    settings = false;
                }

                else if (sop == "r")
                {
                    mulnum = addnum = divnum = subnum = roundNum = 1;
                    reset_m.Play();
                    settings = false;
                }

                else if (sop == "v")
                {
                    settings = false;
                    scorescreen = true;

                }
                else if (sop == "t")
                {
                    showForm1();
                }

                else if (sop == "n")
                {
                }

                sop = "b";
            }

            else if (currHandy >= cali && gomain && hdown && !settings && scorescreen && calibrated)
            {
                settings = true;
                scorescreen = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           // GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Aqua);
            spriteBatch.Begin();
            spriteBatch.Draw(felt, new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight), Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();

            if (!calibrated)
            {
                if (!instdone)
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(end_round_font, "Game Loading...", new Vector2(0, 0), Microsoft.Xna.Framework.Color.Yellow);
                    spriteBatch.Draw(inst, new Microsoft.Xna.Framework.Rectangle(100, 100, 900, 600), Microsoft.Xna.Framework.Color.White);
                    count = count - 1;
                    spriteBatch.End();
                }
                
                else if (!up)
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(end_round_font, "Calibrating, please keep your hand", new Vector2(50, 100), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "at a rest position..", new Vector2(50, 150), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(countdown_font, count.ToString(), new Vector2(500, 150), Microsoft.Xna.Framework.Color.Black);
                    count = count - 1;
                    spriteBatch.End();
                }
                else
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(end_round_font, "Now, please raise your hand..", new Vector2(50, 100), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(countdown_font, count.ToString(), new Vector2(500, 150), Microsoft.Xna.Framework.Color.Black);
                    count = count - 1;
                    spriteBatch.End();

                }
                this.TargetElapsedTime = TimeSpan.FromSeconds(100.0f / 100.0f);
            }

            else if (calibrated && gomain)
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                if (!settings && !scorescreen)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(menu, new Vector2(450, 0), Microsoft.Xna.Framework.Color.White);
                    spriteBatch.Draw(option, new Vector2(450, 150), Microsoft.Xna.Framework.Color.OrangeRed);
                    spriteBatch.Draw(option, new Vector2(450, 350), Microsoft.Xna.Framework.Color.MediumPurple);
                    spriteBatch.Draw(option, new Vector2(450, 550), Microsoft.Xna.Framework.Color.White);
                    
                    if (menuop == "none" || menuop == "e")
                    {
                        menuop = "s";
                        spriteBatch.DrawString(end_round_font, "Start Game", new Vector2(450, 170), Microsoft.Xna.Framework.Color.Yellow);
                        spriteBatch.DrawString(end_round_font, "Options", new Vector2(450, 370), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Exit", new Vector2(450, 570), Microsoft.Xna.Framework.Color.Black);
                    }

                    else if (menuop == "s")
                    {
                        menuop = "o";
                        spriteBatch.DrawString(end_round_font, "Start Game", new Vector2(450, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Options", new Vector2(450, 370), Microsoft.Xna.Framework.Color.Yellow);
                        spriteBatch.DrawString(end_round_font, "Exit", new Vector2(450, 570), Microsoft.Xna.Framework.Color.Black);
                    }

                    else if (menuop == "o")
                    {
                        menuop = "e";
                        spriteBatch.DrawString(end_round_font, "Start Game", new Vector2(450, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Options", new Vector2(450, 370), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Exit", new Vector2(450, 570), Microsoft.Xna.Framework.Color.Yellow);
                    }

                    spriteBatch.End();
                }

                else if (settings)
                {
                    this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                    spriteBatch.Begin();
                    spriteBatch.Draw(options, new Microsoft.Xna.Framework.Rectangle(550, 0, 200, 100), Microsoft.Xna.Framework.Color.White);
                    spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(150, 150, 400, 100), Microsoft.Xna.Framework.Color.OrangeRed);
                    spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(750, 150, 400, 100), Microsoft.Xna.Framework.Color.LightPink);
                    spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(150, 350, 400, 100), Microsoft.Xna.Framework.Color.CornflowerBlue);
                    spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(750, 350, 400, 100), Microsoft.Xna.Framework.Color.MediumPurple);
                    //spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(450, 550, 420, 100), Microsoft.Xna.Framework.Color.Navy);
                    if (sop == "none" || sop == "b")
                    {
                        sop = "r";
                        spriteBatch.DrawString(end_round_font, "Reset Game", new Vector2(150, 170), Microsoft.Xna.Framework.Color.Yellow);
                        spriteBatch.DrawString(end_round_font, "View Progress", new Vector2(750, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Edit Threshold", new Vector2(150, 370), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Go Back", new Vector2(750, 370), Microsoft.Xna.Framework.Color.Black);
                        
                    }

                    else if (sop == "r")
                    {
                        sop = "v";
                        spriteBatch.DrawString(end_round_font, "Reset Game", new Vector2(150, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "View Progress", new Vector2(750, 170), Microsoft.Xna.Framework.Color.Yellow);
                        spriteBatch.DrawString(end_round_font, "Edit Threshold", new Vector2(150, 370), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Go Back", new Vector2(750, 370), Microsoft.Xna.Framework.Color.Black);
                        
                    }

                    else if (sop == "v")
                    {
                        sop = "t";
                        spriteBatch.DrawString(end_round_font, "Reset Game", new Vector2(150, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "View Progress", new Vector2(750, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Edit Threshold", new Vector2(150, 370), Microsoft.Xna.Framework.Color.Yellow);
                        spriteBatch.DrawString(end_round_font, "Go Back", new Vector2(750, 370), Microsoft.Xna.Framework.Color.Black);
                        
                    }

                    else if (sop == "t")
                    {
                        sop = "b";
                        spriteBatch.DrawString(end_round_font, "Reset Game", new Vector2(150, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "View Progress", new Vector2(750, 170), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Edit Threshold", new Vector2(150, 370), Microsoft.Xna.Framework.Color.Black);
                        spriteBatch.DrawString(end_round_font, "Go Back", new Vector2(750, 370), Microsoft.Xna.Framework.Color.Yellow);
                        
                    }

                     
                        
                    spriteBatch.End();

                    
                }

                else if (scorescreen)
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(end_round_font, "Current Game Progress", new Vector2(400, 0), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "Category", new Vector2(250, 100), Microsoft.Xna.Framework.Color.Navy);
                    spriteBatch.DrawString(end_round_font, "Round/Value", new Vector2(750, 100), Microsoft.Xna.Framework.Color.Navy);
                    spriteBatch.DrawString(end_round_font, "Addition:", new Vector2(250, 200), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "Subtraction:", new Vector2(250, 250), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "Multiplication:", new Vector2(250, 300), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "Division:", new Vector2(250, 350), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, addnum.ToString(), new Vector2(800, 200), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, subnum.ToString(), new Vector2(800, 250), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, mulnum.ToString(), new Vector2(800, 300), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, divnum.ToString(), new Vector2(800, 350), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "Threshold: ", new Vector2(250, 450), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, pass.ToString(), new Vector2(800, 450), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, "# Questions: ", new Vector2(250, 500), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.DrawString(end_round_font, num_questions.ToString(), new Vector2(800, 500), Microsoft.Xna.Framework.Color.Black);
                    spriteBatch.Draw(option, new Microsoft.Xna.Framework.Rectangle(400, 600, 420, 70), Microsoft.Xna.Framework.Color.CornflowerBlue);
                    spriteBatch.DrawString(end_round_font, "Back to Main Menu", new Vector2(400, 600), Microsoft.Xna.Framework.Color.Yellow);
                    spriteBatch.End();
                }
               
            }

            else
            {
                if (answer_chosen == true)
                {
                    answer_chosen = false;
                    sp2.Begin();
                    sp2.Draw(art, new Vector2(700, 10), Microsoft.Xna.Framework.Color.Green);
                    int pos;
                    String sa = selected_answer.ToString();
                    if (sa.Length == 1)
                        pos = 775;
                    else
                        pos = 700;
                    if (sa.Length < 3)
                        sp2.DrawString(number_font, selected_answer.ToString(), new Vector2(pos, 0), Microsoft.Xna.Framework.Color.Yellow);
                    else
                        sp2.DrawString(eq, selected_answer.ToString(), new Vector2(pos, 0), Microsoft.Xna.Framework.Color.Yellow);
                    if (selected_answer == correct_answer && selected_answer != -1)
                    {
                        sp2.Draw(green, new Vector2(600, 150), Microsoft.Xna.Framework.Color.Green);
                        qright.Play();
                        if (question_count == num_questions)
                        {
                            roundcomplete = true;
                        }
                    }
                    else if (selected_answer != -1)
                    {
                        sp2.Draw(red, new Vector2(650, 300), Microsoft.Xna.Framework.Color.Red);
                        qwrong.Play();
                        if (question_count == num_questions)
                        {
                            roundcomplete = true;
                        }
                    }
                    sp2.End();
                    this.TargetElapsedTime = TimeSpan.FromSeconds(200.0f / 100.0f);
                }

                if ((cat == "none") || (restart == true))
                {
                    

                    sp2.Begin();
                    //sp2.Draw(select, new Vector2(200, 10), Microsoft.Xna.Framework.Color.White);
                    sp2.DrawString(p_font, "Please Select a Category", new Vector2(180, 50), Microsoft.Xna.Framework.Color.Black);
                    sp2.Draw(option, new Vector2(215, 200), Microsoft.Xna.Framework.Color.OrangeRed);
                    sp2.Draw(option, new Vector2(660, 200), Microsoft.Xna.Framework.Color.LightPink);
                    sp2.Draw(option, new Vector2(215, 400), Microsoft.Xna.Framework.Color.CornflowerBlue);
                    sp2.Draw(option, new Vector2(660, 400), Microsoft.Xna.Framework.Color.MediumPurple);
                    sp2.Draw(logo, new Vector2(450, 600), Microsoft.Xna.Framework.Color.White);
                    if (cur == "Division" || cur == "none")
                    {
                        cur = "Addition";
                        sp2.DrawString(end_round_font, "Addition", new Vector2(320, 225), Microsoft.Xna.Framework.Color.Yellow);
                        sp2.DrawString(end_round_font, "Subtraction", new Vector2(735, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Multiplication", new Vector2(250, 425), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Division", new Vector2(760, 425), Microsoft.Xna.Framework.Color.Black);
                    }

                    else if (cur == "Addition")
                    {
                        cur = "Subtraction";
                        sp2.DrawString(end_round_font, "Addition", new Vector2(320, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Subtraction", new Vector2(735, 225), Microsoft.Xna.Framework.Color.Yellow);
                        sp2.DrawString(end_round_font, "Multiplication", new Vector2(250, 425), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Division", new Vector2(760, 425), Microsoft.Xna.Framework.Color.Black);

                    }

                    else if (cur == "Subtraction")
                    {
                        cur = "Multiplication";
                        sp2.DrawString(end_round_font, "Addition", new Vector2(320, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Subtraction", new Vector2(735, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Multiplication", new Vector2(250, 425), Microsoft.Xna.Framework.Color.Yellow);
                        sp2.DrawString(end_round_font, "Division", new Vector2(760, 425), Microsoft.Xna.Framework.Color.Black);

                    }

                    else if (cur == "Multiplication")
                    {
                        cur = "Division";
                        sp2.DrawString(end_round_font, "Addition", new Vector2(320, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Subtraction", new Vector2(735, 225), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Multiplication", new Vector2(250, 425), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(end_round_font, "Division", new Vector2(760, 425), Microsoft.Xna.Framework.Color.Yellow);

                    }
                    sp2.End();
                    this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                }

                else if (start && roundNum <= 10)
                {

                    sp2.Begin();
                    if (cat != "none" && !once)
                    {
                        sp2.DrawString(end_round_font, "You Chose " + cat, new Vector2(400, 200), Microsoft.Xna.Framework.Color.Black);
                    }
                    sp2.DrawString(end_round_font, "Starting Round " + roundNum, new Vector2(400, 300), Microsoft.Xna.Framework.Color.Black);
                    sp2.End();
                    start = false;
                    once = true;
                    this.TargetElapsedTime = TimeSpan.FromSeconds(200.0f / 100.0f);
                }

                else if (question_count < num_questions && firstnumstring != "" && roundNum <= 10)
                {
                    int x1, x2;
                    bool symchange = false;
                    if (firstnumstring.Length == 1)
                        x1 = 375;
                    else if (firstnumstring.Length == 2)
                        x1 = 250;
                    else
                        x1 = 150;

                    if (secondnumstring.Length == 1)
                        x2 = 375;
                    else if (secondnumstring.Length == 2)
                        x2 = 250;
                    else
                    {
                        x2 = 150;
                        symchange = true;
                    }
                    sp2.Begin();
                    sp2.Draw(card, new Vector2(100, 100), Microsoft.Xna.Framework.Color.White);
                    if (cat == "Addition")
                    {
                        sp2.DrawString(number_font, firstnumstring, new Vector2(x1, 100), Microsoft.Xna.Framework.Color.Black);
                        if (!symchange)
                            sp2.DrawString(number_font, "+", new Vector2(125, 300), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, "+", new Vector2(100, 250), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(number_font, secondnumstring, new Vector2(x2, 300), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(eq, "?", new Vector2(275, 525), Microsoft.Xna.Framework.Color.Black);
                    }
                    else if (cat == "Subtraction")
                    {
                        sp2.DrawString(number_font, firstnumstring, new Vector2(x1, 100), Microsoft.Xna.Framework.Color.Black);
                        if (!symchange)
                            sp2.DrawString(number_font, "-", new Vector2(125, 300), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, "-", new Vector2(100, 250), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(number_font, secondnumstring, new Vector2(x2, 300), Microsoft.Xna.Framework.Color.Black);
                        
                        sp2.DrawString(eq, "?", new Vector2(275, 525), Microsoft.Xna.Framework.Color.Black);
                    }
                    else if (cat == "Multiplication")
                    {
                        sp2.DrawString(number_font, firstnumstring, new Vector2(x1, 100), Microsoft.Xna.Framework.Color.Black);
                        if (!symchange)
                            sp2.DrawString(number_font, "x", new Vector2(125, 300), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, "x", new Vector2(100, 250), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(number_font, secondnumstring, new Vector2(x2, 300), Microsoft.Xna.Framework.Color.Black);
                        
                        sp2.DrawString(eq, "?", new Vector2(275, 525), Microsoft.Xna.Framework.Color.Black);
                    }
                    else if (cat == "Division")
                    {
                        sp2.DrawString(number_font, firstnumstring, new Vector2(x1, 100), Microsoft.Xna.Framework.Color.Black);
                        if (!symchange)
                            sp2.DrawString(number_font, "/", new Vector2(125, 300), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, "/", new Vector2(100, 250), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(number_font, secondnumstring, new Vector2(x2, 300), Microsoft.Xna.Framework.Color.Black);
                        //  sp2.DrawString(number_font, "=", new Vector2(900, 10), Microsoft.Xna.Framework.Color.Black);
                        sp2.DrawString(eq, "?", new Vector2(275, 525), Microsoft.Xna.Framework.Color.Black);
                    }

                    sp2.End();
                }

                else if (roundNum > 10)
                {
                    sp2.Begin();
                    sp2.DrawString(end_round_font, "Category Mastered!", new Vector2(450, 450), Microsoft.Xna.Framework.Color.Black);
                    sp2.End();
                }

                if ((answer_flag == false) && cat != "none")
                {
                    if (question_count < num_questions)
                    {
                        answer_chosen = false;
                        //Random number generation for equations
                        Random r1 = new Random();
                        Random r2 = new Random();
                        int firstnum;
                        int secondnum;

                        if (cat == "Addition" || cat == "Subtraction")
                        {
                            firstnum = r1.Next(11 + roundNum * 10, 20 + roundNum * 10);
                            secondnum = r1.Next(1 + roundNum * 10, 10 + roundNum * 10);
                            correct_answer_slot = r2.Next(0, 4);
                            if (cat == "Addition")
                            {
                                correct_answer = firstnum + secondnum;
                            }
                            else
                            {
                                correct_answer = firstnum - secondnum;
                            }
                        }
                        else
                        {
                            firstnum = r1.Next(1 + roundNum, 8 + roundNum * 2);
                            secondnum = r1.Next(1 + roundNum, 8 + roundNum * 2);
                            correct_answer_slot = r2.Next(0, 4);

                            if (cat == "Multiplication")
                            {
                                correct_answer = firstnum * secondnum;
                            }
                            else
                            {
                                correct_answer = firstnum;
                                firstnum = firstnum * secondnum;
                            }
                        }
                        answer_flag = true; //setting switch
                        firstnumstring = firstnum.ToString();
                        secondnumstring = secondnum.ToString();
                        //base.Draw(gameTime);ru
                    }
                    else
                    {

                        if (roundcomplete)
                        {
                            roundcomplete = false;
                            this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                        }
                        //Signal end of round
                        //calculate and display score!
                        else
                        {
                            string pass_fail_str;
                            string correct_str = correct_answer_count.ToString();
                            string total_str = question_count.ToString();
                            string results_str = "You got " + correct_str + "/" + total_str + " questions right";
                            if (correct_answer_count >= pass)
                            {
                                pass_r.Play();
                                spriteBatch.Begin();
                                spriteBatch.DrawString(end_round_font, "Round " + roundNum + " Complete", new Vector2(100, 100), Microsoft.Xna.Framework.Color.Black);
                                pass_fail_str = "You Won :)";
                                roundNum = roundNum + 1;
                                question_count = 0;
                                correct_answer_count = 0;
                                start = true;
                                choice = "";
                                firstnumstring = "";
                                spriteBatch.DrawString(end_round_font, pass_fail_str, new Vector2(100, 200), Microsoft.Xna.Framework.Color.Black);
                                spriteBatch.DrawString(end_round_font, results_str, new Vector2(100, 300), Microsoft.Xna.Framework.Color.Black);
                                spriteBatch.End();
                                this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                            }
                            else
                            {

                                if (fail_r.State == SoundState.Stopped)
                                    fail_r.Play();
                                else if (fail_r.State == SoundState.Playing)
                                    fail_r.Pause();

                                spriteBatch.Begin();
                                spriteBatch.DrawString(end_round_font, "Round " + roundNum + " Complete", new Vector2(100, 100), Microsoft.Xna.Framework.Color.Black);
                                decision = true;
                                pass_fail_str = "You Lost :(";
                                //  game_over_flag = true;
                                spriteBatch.Draw(art, new Vector2(150, 450), Microsoft.Xna.Framework.Color.White);
                                spriteBatch.Draw(art, new Vector2(450, 450), Microsoft.Xna.Framework.Color.White);
                                spriteBatch.Draw(art, new Vector2(750, 450), Microsoft.Xna.Framework.Color.White);
                                if (lost == "e")
                                {
                                    lost = "r";
                                    spriteBatch.DrawString(end_round_font, "Restart", new Vector2(200, 550), Microsoft.Xna.Framework.Color.Yellow);
                                    spriteBatch.DrawString(end_round_font, "Round", new Vector2(200, 600), Microsoft.Xna.Framework.Color.Yellow);
                                    spriteBatch.DrawString(end_round_font, "Main", new Vector2(500, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Menu", new Vector2(500, 600), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Exit", new Vector2(800, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Game", new Vector2(800, 600), Microsoft.Xna.Framework.Color.Black);
                                }
                                else if (lost == "r")
                                {
                                    lost = "m";
                                    spriteBatch.DrawString(end_round_font, "Restart", new Vector2(200, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Round", new Vector2(200, 600), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Main", new Vector2(500, 550), Microsoft.Xna.Framework.Color.Yellow);
                                    spriteBatch.DrawString(end_round_font, "Menu", new Vector2(500, 600), Microsoft.Xna.Framework.Color.Yellow);
                                    spriteBatch.DrawString(end_round_font, "Exit", new Vector2(800, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Game", new Vector2(800, 600), Microsoft.Xna.Framework.Color.Black);
                                }
                                else if (lost == "m")
                                {
                                    lost = "e";
                                    spriteBatch.DrawString(end_round_font, "Restart", new Vector2(200, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Round", new Vector2(200, 600), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Main", new Vector2(500, 550), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Menu", new Vector2(500, 600), Microsoft.Xna.Framework.Color.Black);
                                    spriteBatch.DrawString(end_round_font, "Exit", new Vector2(800, 550), Microsoft.Xna.Framework.Color.Yellow);
                                    spriteBatch.DrawString(end_round_font, "Game", new Vector2(800, 600), Microsoft.Xna.Framework.Color.Yellow);
                                }
                                spriteBatch.DrawString(end_round_font, pass_fail_str, new Vector2(100, 200), Microsoft.Xna.Framework.Color.Black);
                                spriteBatch.DrawString(end_round_font, results_str, new Vector2(100, 300), Microsoft.Xna.Framework.Color.Black);
                                spriteBatch.End();
                                this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                            }
                        }

                        //base.Draw(gameTime);
                        //-----------------------------
                    }

                }
                else if ((answer_flag == true) && cat != "none" && roundNum <= 10)
                {

                    Random random_answer_seed = new Random();

                    if (!answer_key_chosen)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int random_answer_num = correct_answer;
                            bool unique = false;

                            if (i == correct_answer_slot)
                            {
                                if (i == 0)
                                    ans1 = correct_answer;
                                else if (i == 1)
                                    ans2 = correct_answer;
                                else if (i == 2)
                                    ans3 = correct_answer;
                                else
                                    ans4 = correct_answer;
                            }

                            else
                            {
                                while ((random_answer_num == correct_answer || random_answer_num < 0) || !unique)
                                {
                                    unique = true;
                                    random_answer_num = random_answer_seed.Next(correct_answer - 20, correct_answer + 20);

                                    if (i == 0)
                                    {
                                        ans1 = random_answer_num;
                                    }
                                    if (i == 1)
                                    {
                                        ans2 = random_answer_num;
                                        if (ans1 == ans2)
                                        {
                                            unique = false;
                                        }
                                    }
                                    if (i == 2)
                                    {
                                        ans3 = random_answer_num;
                                        if (ans1 == ans3 || ans2 == ans3)
                                        {
                                            unique = false;
                                        }
                                    }
                                    if (i == 3)
                                    {
                                        ans4 = random_answer_num;
                                        if (ans1 == ans4 || ans2 == ans4 || ans3 == ans4)
                                        {
                                            unique = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    answer_key_chosen = true;
                    int a, b, c, d;
                    String a1 = ans1.ToString();
                    String a2 = ans2.ToString();
                    String a3 = ans3.ToString();
                    String a4 = ans4.ToString();
                    if (a1.Length == 1)
                        a = 675;
                    else
                        a = 600;

                    if (a2.Length == 1)
                        b = 975;
                    else
                        b = 900;

                    if (a3.Length == 1)
                        c = 675;
                    else
                        c = 600;

                    if (a4.Length == 1)
                        d = 975;
                    else
                        d = 900;

                    sp2.Begin();
                    sp2.DrawString(end_round_font, "Round: ", new Vector2(100, 0), Microsoft.Xna.Framework.Color.White);
                    sp2.DrawString(end_round_font, roundNum.ToString(), new Vector2(250, 0), Microsoft.Xna.Framework.Color.Yellow);
                    sp2.DrawString(end_round_font, "Question: ", new Vector2(400, 0), Microsoft.Xna.Framework.Color.White);
                    sp2.DrawString(end_round_font, (question_count + 1).ToString(), new Vector2(625, 0), Microsoft.Xna.Framework.Color.Yellow);
                    sp2.DrawString(end_round_font, "# Correct: ", new Vector2(775, 0), Microsoft.Xna.Framework.Color.White);
                    sp2.DrawString(end_round_font, correct_answer_count.ToString(), new Vector2(1025, 0), Microsoft.Xna.Framework.Color.Yellow);
                    sp2.Draw(art, new Vector2(600, 100), Microsoft.Xna.Framework.Color.Green);
                    sp2.Draw(art, new Vector2(900, 100), Microsoft.Xna.Framework.Color.Green);
                    sp2.Draw(art, new Vector2(600, 400), Microsoft.Xna.Framework.Color.Green);
                    sp2.Draw(art, new Vector2(900, 400), Microsoft.Xna.Framework.Color.Green);

                    if (choice == ans1.ToString() || choice == "")
                    {
                        choice = ans2.ToString();
                        current_answer_displayed = ans1;
                        if (a1.Length < 3)
                            sp2.DrawString(number_font, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Yellow);
                        else
                            sp2.DrawString(eq, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Yellow);

                        if (a2.Length < 3)
                            sp2.DrawString(number_font, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a3.Length < 3)
                            sp2.DrawString(number_font, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);

                        if (a4.Length < 3)
                            sp2.DrawString(number_font, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);
                    }

                    else if (choice == ans2.ToString())
                    {
                        choice = ans3.ToString();
                        current_answer_displayed = ans2;
                        if (a1.Length < 3)
                            sp2.DrawString(number_font, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a2.Length < 3)
                            sp2.DrawString(number_font, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Yellow);
                        else
                            sp2.DrawString(eq, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Yellow);

                        if (a3.Length < 3)
                            sp2.DrawString(number_font, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);

                        if (a4.Length < 3)
                            sp2.DrawString(number_font, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);

                    }

                    else if (choice == ans3.ToString())
                    {
                        choice = ans4.ToString();
                        current_answer_displayed = ans3;
                        if (a1.Length < 3)
                            sp2.DrawString(number_font, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a2.Length < 3)
                            sp2.DrawString(number_font, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a3.Length < 3)
                            sp2.DrawString(number_font, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Yellow);
                        else
                            sp2.DrawString(eq, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Yellow);

                        if (a4.Length < 3)
                            sp2.DrawString(number_font, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Black);
                    }

                    else if (choice == ans4.ToString())
                    {
                        choice = ans1.ToString();
                        current_answer_displayed = ans4;
                        if (a1.Length < 3)
                            sp2.DrawString(number_font, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans1.ToString(), new Vector2(a, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a2.Length < 3)
                            sp2.DrawString(number_font, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans2.ToString(), new Vector2(b, 90), Microsoft.Xna.Framework.Color.Black);

                        if (a3.Length < 3)
                            sp2.DrawString(number_font, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);
                        else
                            sp2.DrawString(eq, ans3.ToString(), new Vector2(c, 390), Microsoft.Xna.Framework.Color.Black);

                        if (a4.Length < 3)
                            sp2.DrawString(number_font, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Yellow);
                        else
                            sp2.DrawString(eq, ans4.ToString(), new Vector2(d, 390), Microsoft.Xna.Framework.Color.Yellow);

                    }
                    sp2.End();
                    this.TargetElapsedTime = TimeSpan.FromSeconds(300.0f / 100.0f);
                }
            }
        }
    }
}