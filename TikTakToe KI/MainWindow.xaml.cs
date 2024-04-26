using System;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TikTakToe_KI.assets;
using static System.Formats.Asn1.AsnWriter;

namespace TikTakToe_KI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread botThread;
        //Listen zum Lernen und Trainieren der KIs
        public List<Learning> XMemory = new List<Learning>();
        public List<Learning> OMemory = new List<Learning>();

        //bool zum sagen wer am zug ist
        public bool YourTurn;

        public bool fastmode = false;

        //Score Attribute
        public int ScoreX = 0;
        public int ScoreO = 0;

        //Liste der Knöpfe für den Computerspieler
        public List<Button> Buttons = new List<Button>();

        //Digitale ansicht des Boards für den Computer
        public int[] board = new int[9];

        Random random = new Random();
        public bool FieldEmpty = true;
        public MainWindow()
        {
            InitializeComponent();

            int i = 0;
            //Läde alle Buttons in eine Liste
            foreach (Button btn in GameGrid.Children)
            {
                btn.Tag = i;
                Buttons.Add(btn);
                i++;
            }

            XScore.Text = ScoreX.ToString();
            OScore.Text = ScoreO.ToString();

            NewGame();
            YourTurn = WhoBeginns();

            Thread botThread = new Thread(Bot);
            botThread.IsBackground = true; // Setzen des Threads als Hintergrundthread
            botThread.Start();

        }

        //Methode falls Fenster geschlossen wird dass auch der Bot Threat beendet wird
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Hier wird der Thread gestoppt, bevor das Fenster geschlossen wird
            if (botThread != null && botThread.IsAlive)
            {
                botThread.Abort(); // Beenden des Threads
            }
        }

        //Random auswahl wer beginnt
        public bool WhoBeginns()
        { 
            return (random.Next(0, 3) < 2);
        }

        //Platziere deine Form
        private void b_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Content == "")
            {
                int index = Buttons.IndexOf(btn);
                if (YourTurn)
                {
                    YourTurn = false;
                    PlaceMove(btn, 1, index);
                    CheckWin(1);
                    
                }
                else
                {
                    YourTurn = true;
                    PlaceMove(btn, 2, index);
                    CheckWin(2);
                    
                }
            }
        }

        //Methode um ein Feld zu platzieren
        private void PlaceMove(Button btn, int n, int index)
        {
            string p = "";
            if (n == 1)
            {
                p = "X";
            }
            else
            {
                p = "O";
            }
            //Frontend Visualisieren
            btn.Content = p;
            //ins Backend Speichern
            board[index] = n;

            //Objecte und Felderreihenfolge Speichern
            FieldEmpty = false;
        }

        //Button Reset Funktion 
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        //Starte ein neues Spiel
        public void NewGame()
        {
            foreach (Button btn in Buttons)
            {
                btn.Content = "";
            }

            //Board neu initialisieren
            board = new int[9];

            FieldEmpty = true;
        }

        //Startet Partie
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (fastmode)
            {
                Start.Background = Brushes.Green;
                Stop.Background = Brushes.DarkGray;
                fastmode = false;
            }
        }

        //Stopt Partie
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (fastmode == false)
            {
                Start.Background = Brushes.DarkGray;
                Stop.Background = Brushes.Red;
                fastmode = true;
            }
        }

        //Gewinnüberprüfung
        private void CheckWin(int p)
        {
            //Horizontal
            for (int i = 0; i <9; i+=3)
            {
                if (board[i] == p && board[i+1] == p && board[i+2] == p)
                {
                    LearnMove(p);
                    AddScore(p);
                    NewGame();
                    return;
                }
            }

            //Vertikal
            for (int i = 0;i <3;i++) 
            {
                if (board[i] ==p && board[i+3] ==p && board[i+6] == p)
                {
                    LearnMove(p);
                    AddScore(p);
                    NewGame();
                    return;
                }
            }

            //Diagonal

            if ((board[0] == p && board[4] == p && board[8] == p || board[2] == p && board[4] == p && board[6] == p))
            {
                LearnMove(p);
                AddScore(p);
                NewGame();
                return;
            }


            //Unentschieden erkennen
            int check = 0;
            for (int i = 0;i < 9 ;i+=3) 
            {
                if (board[i] != 0 && board[i+1] != 0 && board[i+2] !=0)
                {
                    check++;
                }
            }

            if(check == 3)
            {
                NewGame();
                return;
            }

        }

        private void AddScore(int p)
        {
            if (p == 1)
            {
                ScoreX++;
                XScore.Text = ScoreX.ToString();    
                C1Count.Text = XMemory.Count.ToString();
                C2Count.Text = OMemory.Count.ToString();
            }
            else
            {
                ScoreO++;
                OScore.Text = ScoreO.ToString();
                C2Count.Text = OMemory.Count.ToString();
                C1Count.Text = XMemory.Count.ToString();
            }
        }

        //Allgemeiner Botaufruf ??Platziert manchmal 2 Formen gleichzeitig
        private void Bot()
        {
            while (true)
            {
                //Thread.Sleep(10);
                Dispatcher.Invoke(() =>
                {
                    //X Bot
                    if (C1.IsChecked == true && YourTurn)
                    {
                        //Spiele mit KI
                        if (C1KI.IsChecked ==true)
                        {
                            if (KI(1))
                            {
                                YourTurn = false;
                                CheckWin(1);
                                return;
                            }
                        }

                        //Mit Spiellogikhilfe Spielen
                        if (C1Logic.IsChecked == true)
                        {
                            if (DetectWin(1, 1))
                            {
                                YourTurn = false;
                                CheckWin(1);
                                return;
                            }
                            else if (DetectWin(2, 1))
                            {
                                YourTurn = false;
                                CheckWin(1);
                                return;
                            }
                        }

                        RandomPick(1);
                        CheckWin(1);
                        YourTurn = false;

                    }
                    else if (C2.IsChecked == true && YourTurn == false) //O Bot
                    {
                        //Spiele mit KI
                        if (C2KI.IsChecked == true)
                        {
                            if (KI(2))
                            {
                                YourTurn = true;
                                CheckWin(2);
                                return;
                            }
                        }

                        //Mit Spiellogikhilfe Spielen
                        if (C2Logic.IsChecked == true)
                        {
                            if (DetectWin(2, 2))
                            {
                                YourTurn = true;
                                CheckWin(2);
                                return;
                            }
                            else if (DetectWin(1, 2))
                            {
                                YourTurn = true;
                                CheckWin(2);
                                return;
                            }
                        }

                        RandomPick(2);
                        CheckWin(2);
                        YourTurn = true;
                    }
                });

                if (fastmode == false)
                {
                    Thread.Sleep(100);
                }
                
            }
        }

        //Random Funktion zum auswählen eines leeren Feldes
        private void RandomPick(int p)
        {
            bool flag = false;
            while (flag == false)
            {
                int y = random.Next(9);
                if (board[y] == 0)
                {
                    flag = true;
                    Button btn = Buttons[y];

                    PlaceMove(btn, p, y);
                }
            }

        }

        //Lernfunkion für den Bot
        public void LearnMove(int player)
        {
            if (C1Learn.IsChecked == true)
            {
                //Speichern in X Memory (1)
                if (XMemory.Count > 0)
                {
                    CheckMemory(1, player);
                }
                else
                {
                    SaveMemory(board,1, player, XMemory);
                }
            }

            if (C2Learn.IsChecked == true)
            {
                //Speichern in O Memory (2)
                if (OMemory.Count > 0)
                {
                    CheckMemory(2, player);
                }
                else
                {
                    SaveMemory(board, 2, player, OMemory);
                }
            }
        }

        //Speichert die Stradegie in den Memory
        public void CheckMemory(int player, int winner)
        {
            //Prüfe um welchen spieler es sich handelt
            List<Learning> Brain = new List<Learning>();
            if (player == 1)
            {
                Brain = XMemory;
            }
            else
            {
                Brain = OMemory;
            }

            //Speicher das Board 
            int[] stage = new int[9];
            Array.Copy(board, stage, board.Length);

            //Falls der Aktuelle Spieler nicht der Gewinner ist wird das Board Invertiert
            if (player != winner)
            {
                stage = InvertBoard(stage);
            }

            //Prüfe ob dieses Muster schon existiert
            List<Learning> copy = new List<Learning>(Brain);

            bool exist = false;
            foreach (Learning l in copy)
            {
                //Prüfe ob diese Gewinnmöglichkeit exiastiert
                if (l.Strategy.SequenceEqual(stage))
                {
                    l.TimesWon++;
                    exist = true;
                }

            }

            //Falls es nicht existiert speicher es ab
            if (exist == false)
            {
                SaveMemory(stage, player, player, Brain);
            }
        }

        //Speicher in Memory
        public void SaveMemory(int[] stradegy, int player,int winner, List<Learning> toLearn)
        {
            //Speicher das Board 
            int[] stage = new int[9];
            Array.Copy(stradegy, stage, board.Length);

            //Falls der Aktuelle Spieler nicht der Gewinner ist wird das Board Invertiert
            if (player != winner)
            {
                stage = InvertBoard(stage);
            }

            Learning learn = new Learning();
            //Setzt den spieler der gewonnen hat
            learn.WinningPlayer = player;
            learn.Strategy = stage;
            toLearn.Add(learn);
        }

        //Methode um das Bord zu invertieren
        public int[] InvertBoard(int[] b)
        {
            int[] inverted = b;

            for (int i =0; i < inverted.Length; i++)
            {
                if (inverted[i] == 1)
                {
                    inverted[i] = 2;
                }
                else if (inverted[i] == 2)
                {
                    inverted[i] = 1;
                }
            }
            return inverted;
        }

        //KI Spielmodell
        public bool KI(int player)
        {
            List<Learning> MyBrain;

            //Auswahl des Memorys aufgrund des Computers ob 1 oder 2 also ob X oder O
            if (player == 1)
            {
                MyBrain = XMemory;
            }
            else
            {
                MyBrain= OMemory;
            }

            //Falls das Feld leer ist
            if (FieldEmpty)
            {
                return false;
            }
            else if (MyBrain.Count >0) //Falls schon Felder Platziert worden sind muss das Aktuelle Feld mit bisherige Stradegien verglichen werden
            {
                //platzierte felder errechnen
                int placedFields = 0;
                foreach (int i in board)
                {
                    if (i != 0)
                    {
                        placedFields++;
                    }
                }

                //Suche die beste Stradegy raus
                int rate = -1;
                int index = -1;
                int check = 0;
                foreach (Learning le in MyBrain)
                {
                    for (int i =0; i < le.Strategy.Length; i++)
                    {
                        if (board[i] != 0 && board[i] == le.Strategy[i])
                        {
                            check++;
                        }
                    }

                    if (check == placedFields && le.TimesWon > rate)
                    {
                        //Abspeichern der Gewinnrate
                        rate = le.TimesWon;
                        index = MyBrain.IndexOf(le);
                    }
                }

                //Falls eine Stradegy gewählt wurde
                if(index >= 0)
                {
                    //Laden des gewinn Grids
                    int[] virtualGrid = new int[9];
                    Array.Copy(MyBrain[index].Strategy, virtualGrid, virtualGrid.Length);

                    //Liste zum zwischenspeichern der noch verfügbaren Positionen
                    List<int> positions = new List<int>();

                    //Logik zum auswerten welcher der Werte welche Positionen der Form noch frei sind
                    for (int i =0; i < board.Length; i++)
                    {
                        //Falls das Board noch leer ist aber im Muster der Spieler platziert ist
                        if (board[i] == 0 && virtualGrid[i] == player)
                        {
                            positions.Add(i);
                        }
                    }

                    //Hier Algorithmus zur auswertung des nächsten Spielzuges--------------------------------
                }
            }

            return false;
        }


        //Strategisches entdecken eines Gewinns/verlustes
        private bool DetectWin(int p, int n)
        {
            int player;
            int empty;
            int place;
            //Horizontale überprüfung 
            for (int i = 0; i < 9; i+=3)
            {
                player = 0;
                empty = 0;
                place = 10;

                for (int l = i; l < i + 3; l++)
                {
                    if (board[l] == p)
                    {
                        player++;
                    }
                    else if (board[l] == 0)
                    {
                        empty++;
                        place = l;
                    }

                    if (place != 10 && player == 2 && empty == 1)
                    {
                        Button btn = Buttons[place];
                        PlaceMove(btn, n, place);
                        return true;
                    }
                }               
            }

            //Vertikale überprüfung
            for (int i = 0; i < 3; i++)
            {
                player = 0;
                empty = 0;
                place = 10;

                for (int l = i; l < 9; l += 3)
                {
                    if (board[l] == p)
                    {
                        player++;
                    }
                    else if (board[l] == 0)
                    {
                        empty++;
                        place = l;
                    }

                    if (place != 10 && player == 2 && empty == 1)
                    {
                        Button btn = Buttons[place];
                        PlaceMove(btn, n, place);
                        return true;
                    }
                }
            }

            // Diagonale Überprüfung von links oben nach rechts unten
            player = 0;
            empty = 0;
            place = 10;

            for (int i = 0; i < 9; i += 4)
            {
                if (board[i] == p)
                {
                    player++;
                }
                else if (board[i] == 0)
                {
                    empty++;
                    place = i;
                }

                if (place != 10 && player == 2 && empty == 1)
                {
                    Button btn = Buttons[place];
                    PlaceMove(btn, n, place);
                    return true;
                }
            }

            // Diagonale Überprüfung von rechts oben nach links unten
            player = 0;
            empty = 0;
            place = 10;

            for (int i = 2; i < 7; i += 2)
            {
                if (board[i] == p)
                {
                    player++;
                }
                else if (board[i] == 0)
                {
                    empty++;
                    place = i;
                }

                if (place != 10 && player == 2 && empty == 1)
                {
                    Button btn = Buttons[place];
                    PlaceMove(btn, n, place);
                    return true;
                }
            }

            return false;   
        }

    }
}