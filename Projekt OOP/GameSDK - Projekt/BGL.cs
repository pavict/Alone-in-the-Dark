using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Pmfst_GameSDK
{
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";

        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {                
                foreach (Sprite sprite in allSprites)
                {                    
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                //MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.Black);
            string text = ISPIS;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Arial", 14);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(0, 0, stringSize.Width, stringSize.Height);
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                e.Graphics.DrawString(text, font1, Brushes.White, rectF1);
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;            
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }

        //private int SlucajanBroj(int min, int max)
        //{
        //    Random r = new Random();
        //    int br = r.Next(min, max + 1);
        //    return br;
        //}

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */

        /* ------------ GAME CODE START ------------ */

        /* Game variables */

        // deklaracija objekata
        //likovi ode dodaj jos zombije
        Heroj GlavniLik;
        Bullet Metak;
        Bullet Zivot;
        Sprite Maska;
        Sprite TextBbl;
        Sprite SRCA;
        Zombie BrziZombi;
        Zombie VelikiZombi;
        Zombie ObicniZombie;
        /* Initialization */
        int SCORE = 0;

        private void SetupGame()
        {
            //1. setup stage
            SetStageTitle("PMF");
            //setBackgroundColor(Color.WhiteSmoke);        
            setBackgroundPicture("backgrounds\\Sand.jpg");
            //none, tile, stretch, center, zoom
            setPictureLayout("stretch");


            //none, tile, stretch, center, zoom


            //2. add sprites
            //-------------------------------------------------
            GlavniLik = new Heroj("sprites\\bibleThump.png", GameOptions.RightEdge / 2-20, GameOptions.DownEdge / 2-39);//nediraj ove brojeve
            GlavniLik.RotationStyle="AllAround";//omogucava liku da se rotira nemoj zaboravit OVO!
            GlavniLik.AddCostumes("sprites\\Isaac_Fate.png");//Dodatni kostimi za glavnog lika
            GlavniLik.AddCostumes("sprites\\IsaacDescent.png");//dodava jedan po jedan ne vise od jednom jer zna nekad zamrznit igricu kad promini se vise kostima odjednom
            //-------------------------------------------------

            //-------------------------------------------------
            Metak = new Bullet("sprites\\Metak.png", GlavniLik.X+30,GlavniLik.Y+20);//metak netriba se rotirat
            //-------------------------------------------------

            //-------------------------------------------------     
            Maska = new Sprite("sprites\\DarkTest.png", 0, 0);//ovo ti je maska koja zamraci sve
            Maska.SetTransparentColor(Color.White);//nepotrebno 
            
            //-------------------------------------------------

            //-------------------------------------------------
            Zivot = new Bullet("sprites\\Zivot.png", 0, 0);//Zivot je kodiran ka metak jer mi se nije dalo radit jos jednu klasu
            Zivot.AddCostumes("sprites\\SrceUvaceno.png");
            //-------------------------------------------------

            //-------------------------------------------------
            TextBbl = new Sprite("sprites\\TextIntro.png", 180, GameOptions.DownEdge - 115);// ovo ti je onaj text na dnu
            TextBbl.AddCostumes("sprites\\TextInfo.png", "sprites\\TextWin.png", "sprites\\TextLose.png");//svi textovi
            //-------------------------------------------------

            //-------------------------------------------------
            SRCA = new Sprite("sprites\\Hp0.png", 20, 450);//Ovo su ti ona srca na dnu 
            SRCA.AddCostumes("sprites\\Hp1.png", "sprites\\Hp2.png", "sprites\\Hp3.png");
            //-------------------------------------------------
            BrziZombi = new Zombie("sprites\\Zombi6.png", 20, 20,7);
            BrziZombi.RotationStyle="AllAround";
            VelikiZombi = new Zombie("sprites\\Zombi3.png", 0, 350, 3);
            VelikiZombi.RotationStyle = "AllAround";
            ObicniZombie = new Zombie("sprites\\Zombi1.png", 400, 20, 3);
            ObicniZombie.RotationStyle = "AllAround";
            //-------------------------------------------------
            //dodaj Zombije u Game
            Game.AddSprite(GlavniLik);
            Game.AddSprite(Metak);
            Game.AddSprite(Zivot);
            Game.AddSprite(VelikiZombi);
            Game.AddSprite(ObicniZombie);
            Game.AddSprite(BrziZombi);
            Game.AddSprite(Maska);
            Game.AddSprite(TextBbl);
            Game.AddSprite(SRCA);
            
            
            

            //dodati event handlers ovdje
            //napomena: prije metoda u kojima se pozivaju
            //eventi Za dobivanje zivota i kraj igrice
            HpUpEvent += BGL_HpUp;
            HpDownEvent += BGL_HpDown;
            IsaacWon += BGL_GG;
            IsaacLost += BGL_Lose;
            //ovaj primjer pozivamo: testEvent.Invoke(1234);
            //tada se poziva izvršavanje metode BGL_testEvent(1234)

            //3. scripts that start
            //Game.StartScript(Metoda);
            //napravi skriptnu ZombieMove
            Game.StartScript(Metoda);
            Game.StartScript(ZivotMove);
            Game.StartScript(Text123);
            Game.StartScript(BrziZombieMove);
            Game.StartScript(VelikiZombieMove);
            Game.StartScript(ObicniZombieMove);
            

        }

        //možemo slati i parametre kod poziva događaja
        //Hp gori doli eventi
        public delegate void HPDelegat(int broj);
        public event HPDelegat HpUpEvent;
        public event HPDelegat HpDownEvent;//HpDownEvent.Invoke(Koliko zivota zelis da izgubi);
        

        private void BGL_HpUp(int broj)
        {
            if(GlavniLik.HP<3)
            {
                GlavniLik.HP += broj;
                while (GlavniLik.HP!=SRCA.CostumeIndex)
                {
                    SRCA.NextCostume();
                    Wait(0.1);
                }
            }
                      
        }
        private void BGL_HpDown(int broj)//koristi ovo za smanjivanje zivota kad te udre zombi 
        {
            GlavniLik.HP -= broj;
            if (GlavniLik.HP >= 0)
            { 
                while (GlavniLik.HP != SRCA.CostumeIndex)
                {
                    SRCA.NextCostume();
                    Wait(0.1);
                }
            }
            if(GlavniLik.HP==0)
            {
                IsaacLost.Invoke(SCORE);//ovo 3 zaminit sa score;
            }
        }
        //eventi za kraj igre
        public delegate void GameEnd(int broj);//Pobjeda i Poraz
        public event GameEnd IsaacWon;
        public event GameEnd IsaacLost;


        public void BGL_GG(int broj)//pobjeda poziva se pomovu IsaacWon.Invoke(Score)
        {
            //Teski spageti ode nediraj nista samo pozovi
            //nije stabilno more zamrznit igricu
            //divna je rekla da ce pogledat i popravit hahahahaha
            GlavniLik.Y -=25;
            GlavniLik.X -= 66;

            TextBbl.Y = 0;
            TextBbl.X -= 17;
            
            GlavniLik.Width = 200;
            GlavniLik.Height = 100;
            GlavniLik.NextCostume();
            
            
            Zivot.SetVisible(false);
            Metak.SetVisible(false);
            BrziZombi.SetVisible(false);
            VelikiZombi.SetVisible(false);
            ObicniZombie.SetVisible(false);

            TextBbl.NextCostume();
            START = false;
            ISPIS = "POBJEDA Score: " + broj;
            
            Wait(15);//nepotrebno al nediraj
        }
        public void BGL_Lose(int broj)//poraz
        {
            //ova je malo pouzdanija radi skoro svaki put
            GlavniLik.Width = 150;
            GlavniLik.Height = 109;
            GlavniLik.NextCostume();//bilo je nesto sa ovin nextCoustume nebi prominilo kostim nego bi zamrzlo igricu 
            GlavniLik.SetVisible(false);
            Wait(0.1);//pauza od 0.1 i dodavanje kostima posebno,ne sve od jednom popravi
            GlavniLik.NextCostume();
            GlavniLik.SetVisible(true);
            BrziZombi.SetVisible(false);
            VelikiZombi.SetVisible(false);
            ObicniZombie.SetVisible(false);
            TextBbl.NextCostume();
            TextBbl.NextCostume();
            GlavniLik.X -= 50;
            GlavniLik.Y -= 15;
            Zivot.SetVisible(false);
            Metak.SetVisible(false);
            START = false;
            
            ISPIS = "DEFEAT! Score: " + broj;
            
            Wait(15);//nepotrebno al nediraj

            //nezz jel bi dodava kad pritisne npr R da se restartira igrica
        }
        /* Event handlers - metode*/



        /* Scripts */
        public void ResetBullet()
        {
            Metak.X = GlavniLik.X + 30;
            Metak.Y = GlavniLik.Y + 20;
            Metak.SetVisible(false);
            Metak.Active = false;
        }
        private int Metoda()//glavna metoda spojeno metak i glavni lik u jednu skriptu
        {
            SetHpImageto(GlavniLik.HP);
            while(START) //ili neki drugi uvjet
            {
                GlavniLik.PointToMouse(sensing.Mouse);

                if (sensing.MouseDown)
                {
                    if (!Metak.Active)
                    {
                        Metak.X = GlavniLik.X+30;
                        Metak.Y = GlavniLik.Y+20;
                        Metak.SetDirection(GlavniLik.GetDirection());
                        Metak.SetVisible(true);
                        Metak.Active = true;
                    }
                }
                try//mora san priko exception jer overide nije radija
                {
                    if (Metak.Active)
                    {
                        
                        Metak.MoveSteps(Metak.Brzina);
                    }
                }
                catch
                {
                    ResetBullet();
                }

                ISPIS = "Score : " + SCORE;
                Wait(0.02);
                if(SCORE>=10&&GlavniLik.HP>=2)
                {
                    IsaacWon.Invoke(SCORE);
                }
            }
            return 0;
        }

        private int ZivotMove()//skripta da zivot leta okolo
        {
            Zivot.Brzina = 5;//moglo se u konstruktoru
            Random Rnd = new Random();//RNG
            while(START)
            {
                if(!Zivot.Active)//Ako Vec nije stvoren zivot
                {
                    int kantun = Rnd.Next(1, 5);//Odaberi Jedan od cetri kantuna
                    if (kantun == 1)//Ovo je da odabere stranu s koje ce se zivot stvorit i di ce krenit
                    {
                        Zivot.Y = 1;
                        Zivot.X = Rnd.Next(200, 500);
                        Zivot.SetDirection(180);
                    }
                    if (kantun == 2)//livo
                    {
                        Zivot.Y = 400;
                        Zivot.X = Rnd.Next(200, 500);
                        Zivot.SetDirection(0);
                    }
                    if (kantun == 3)//desno
                    {
                        Zivot.Y = Rnd.Next(150, 380); ;
                        Zivot.X =1;
                        Zivot.SetDirection(90);
                    }
                    if (kantun == 4)//doli
                    {
                        Zivot.Y = Rnd.Next(150, 380);
                        Zivot.X = 550;
                        Zivot.SetDirection(270);
                    }
                    Zivot.Active = true;
                    Zivot.SetVisible(true);
                }
                else//Ako zivot postoji 
                    try
                    {
                        Zivot.MoveSteps(Zivot.Brzina-3);//Mici zivot naprid nekon brzinon
                        if(Zivot.TouchingSprite(Metak))//ako se sudari sa Metkon
                        {
                            HpUpEvent.Invoke(1);//Event koji povecava zivot za 1
                            Zivot.Brzina+=2;//ubzaj zivot
                            ResetBullet();
                            Zivot.NextCostume();//promini kostim u onaj sta ima +1 na sebi
                            Wait(1);
                            Zivot.NextCostume();
                            throw new ArgumentException();//pribaci na catch
                        }
                        Wait(0.02);
                        
                    }
                    catch (Exception)//ako je zivot udrija u rub ili metak
                    {
                        Zivot.X = 0;//vjerojatno netriba ovo al zna se zbagat bez ovoga doda po 2 zivota nezz kako
                        Zivot.Y = 0;
                        Zivot.Active = false;
                        Zivot.SetVisible(false);
                        Wait(10);//zivot se stvara svako 10 sec 
                        
                    }
            }
            return 0;
        }
        public int Text123()//pribaci sa intro texta na info text
        {
            Wait(5);
            TextBbl.NextCostume();

            return 0;
        }
        public void SetHpImageto(int broj)//namista ona animirana srca na broj
        {

            while (broj != SRCA.CostumeIndex)//costune index pocinje od 0.1hp je index 1.0hp je index 1.
            {
                SRCA.NextCostume();
                Wait(0.3);
            }
            
        }
        public void SelectSpawn(string KojiZombi)
        {
            Random Rnd = new Random();
            if(KojiZombi=="brzi")
            {
                int Strana = Rnd.Next(0, 2);
                if(Strana==0)
                {
                    BrziZombi.Y = Rnd.Next(0, 449);
                    BrziZombi.X = 1;
                }
                else
                {
                    BrziZombi.Y = Rnd.Next(0, 449);
                    BrziZombi.X = 649;
                }
            }
            else if(KojiZombi=="veliki")
            {
                int Strana = Rnd.Next(0, 2);
                if (Strana == 0)
                {
                    VelikiZombi.Y = Rnd.Next(0, 399);
                    VelikiZombi.X = 1;
                }
                else
                {
                    VelikiZombi.Y = Rnd.Next(0, 399);
                    VelikiZombi.X = 599;
                }
            }
            else if (KojiZombi == "obicni")
            {
                int Strana = Rnd.Next(0, 2);
                if (Strana == 0)
                {
                    ObicniZombie.Y = Rnd.Next(0, 419);
                    ObicniZombie.X = 1;
                }
                else
                {
                    ObicniZombie.Y = Rnd.Next(0, 419);
                    ObicniZombie.X = 619;
                }
            }
        }
        public int BrziZombieMove()
        {
            Wait(2);
            while(START)
            {
                
                BrziZombi.PointToSprite(GlavniLik);
                BrziZombi.MoveSteps(BrziZombi.Brzina);
                
                if (BrziZombi.TouchingSprite(GlavniLik))
                {
                    HpDownEvent(1);
                    SelectSpawn("brzi");
                }
                if (BrziZombi.TouchingSprite(Metak))
                {
                    ResetBullet();
                    SCORE++;
                    SelectSpawn("brzi");
                    Wait(4);
                }
                Wait(0.05);
            }
            return 0;
        }
        public int VelikiZombieMove()
        {
            Wait(10);
            while (START)
            {
                VelikiZombi.PointToSprite(GlavniLik);
                VelikiZombi.MoveSteps(VelikiZombi.Brzina);
                if (VelikiZombi.TouchingSprite(GlavniLik))
                {
                    HpDownEvent(2);
                    SelectSpawn("veliki");
                }
                if (VelikiZombi.TouchingSprite(Metak))
                {
                    ResetBullet();
                    SCORE++;
                    SelectSpawn("veliki");
                    Wait(6);
                }
                Wait(0.05);
            }
                return 0;
        }
        public int ObicniZombieMove()
        {
            
            while (START)
            {
                ObicniZombie.PointToSprite(GlavniLik);
                ObicniZombie.MoveSteps(ObicniZombie.Brzina);
                if (ObicniZombie.TouchingSprite(GlavniLik))
                {
                    HpDownEvent(1);
                    SelectSpawn("obicni");
                }
                if (ObicniZombie.TouchingSprite(Metak))
                {
                    ResetBullet();
                    SCORE++;
                    SelectSpawn("obicni");
                    Wait(6);
                }
                Wait(0.02);
            }
            return 0;
        }
        /* ------------ GAME CODE END ------------ */
    }
}
