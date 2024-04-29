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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TikTakToe_KI.assets;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        //aktiviere /deaktiviere Fastmode
        public bool fastmode = false;

        //Boolean zur überprüfung und zum warten bis überprüfung abgeschlossen ist
        public bool InCheck = false;

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

                }
                else
                {
                    YourTurn = true;
                    PlaceMove(btn, 2, index);

                }
            }
        }

        //Methode um ein Feld zu platzieren
        private void PlaceMove(Button btn, int n, int index)
        {
            if (!InCheck)
            {
                InCheck = true;
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

                CheckWin(n, index);
            }
        }

        //Button Reset Funktion 
        private void Reset_Click(object sender, RoutedEventArgs e)
        {

            ScoreX = 0;
            ScoreO = 0;
            XScore.Text = ScoreX.ToString();
            OScore.Text = ScoreO.ToString();
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
        public async void CheckWin(int p, int index)
        {
            bool won = false;
            bool draw = false;
            //Horizontal
            for (int i = 0; i < 9; i += 3)
            {
                if (board[i] == p && board[i + 1] == p && board[i + 2] == p)
                {
                    won = true;
                    await CheckLastTest(i, i+1, i+2, index, p, draw, won);
                    InCheck = false;
                    return;
                }
            }

            //Vertikal
            for (int i = 0; i < 3; i++)
            {
                if (board[i] == p && board[i + 3] == p && board[i + 6] == p)
                {

                    won = true;
                    await CheckLastTest(i, i + 3, i + 6, index, p, draw, won);
                    InCheck = false;
                    return;
                }
            }

            //Diagonal

            if (board[0] == p && board[4] == p && board[8] == p )
            {

                won = true;
                await CheckLastTest(0, 4, 8, index, p, draw, won);
                InCheck = false;
                return;
            }

            if (board[2] == p && board[4] == p && board[6] == p)
            {

                won = true;
                await CheckLastTest(2, 4, 6, index, p, draw, won);
                InCheck = false;
                return;
            }


            //Unentschieden erkennen
            int check = 0;
            for (int i = 0; i < 9; i += 3)
            {
                if (board[i] != 0 && board[i + 1] != 0 && board[i + 2] != 0)
                {
                    check++;
                }
            }

            if (check == 3)
            {
                draw = true;
                await CheckLastTest(0, 0, 0, index, p, draw, won);
                InCheck = false;
                return;
            }

            InCheck = false;

        }

        public async Task CheckLastTest (int i1 , int i2, int i3, int winMove, int player, bool draw, bool win)
        {
            if (win)
            {
                LearnMove(player);
                AddScore(player);
                if (!fastmode)
                {
                    PlaceWinner(player, winMove);
                    await Task.Delay(100);
                    await WinAnimation(i1,i2,i3,player);
                }
                NewGame();
            }
            else if(draw)
            {
                if (!fastmode)
                {
                    PlaceWinner(player, winMove);
                    await Task.Delay(100);
                }
                NewGame();
                
            }
        }

        //Methode um Button zu platzieren
        public void PlaceWinner(int player, int index)
        {
            Button btn = Buttons[index];
            string p = "";
            if (player == 1)
            {
                p = "X";
            }
            else
            {
                p = "O";
            }
            //Frontend Visualisieren
            btn.Content = p;
        }

        //Animation für das Userfeedback
        public async Task WinAnimation(int i1, int i2, int i3, int player)
        {
            string symbol = (player == 1) ? "X":"O";

            Button btn1 = Buttons[i1];
            Button btn2 = Buttons[i2];
            Button btn3 = Buttons[i3];
            for (int i = 0; i< 5; i++)
            {
                btn1.Content = "";
                btn2.Content = "";
                btn3.Content = "";

                await Task.Delay(100);

                btn1.Content= symbol;
                btn2.Content= symbol;
                btn3.Content= symbol;

                await Task.Delay(100);
            }
        }

        //Score und Anzahl der Gelernten Zügen speichern
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

        //Allgemeiner Botaufruf
        private void Bot()
        {
            while (true)
            {
                //Thread.Sleep(10);
                Dispatcher.Invoke(() =>
                {
                    //X Bot
                    if (C1.IsChecked == true && YourTurn && !InCheck)
                    {
                        //Spiele mit KI
                        if (C1KI.IsChecked ==true)
                        {
                            if (KI(1))
                            {
                                YourTurn = false;
                                return;
                            }
                        }

                        //Mit Spiellogikhilfe Spielen
                        if (C1Logic.IsChecked == true)
                        {
                            if (DetectWin(1, 1))
                            {
                                YourTurn = false;
                                return;
                            }
                            else if (DetectWin(2, 1))
                            {
                                YourTurn = false;
                                return;
                            }
                        }

                        RandomPick(1);
                        YourTurn = false;

                    }
                    else if (C2.IsChecked == true && YourTurn == false && !InCheck) //O Bot
                    {
                        //Spiele mit KI
                        if (C2KI.IsChecked == true)
                        {
                            if (KI(2))
                            {
                                YourTurn = true;
                                return;
                            }
                        }

                        //Mit Spiellogikhilfe Spielen
                        if (C2Logic.IsChecked == true)
                        {
                            if (DetectWin(2, 2))
                            {
                                YourTurn = true;
                                return;
                            }
                            else if (DetectWin(1, 2))
                            {
                                YourTurn = true;
                                return;
                            }
                        }

                        RandomPick(2);
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


        //KI Funktionen-------------------------------------------------------------------------------------------------

        //KI Spielmodell
        public bool KI(int player)
        {
            //Auswahl des Memorys aufgrund des Computers ob 1 oder 2 also ob X oder O
            List<Learning> Memory = (player == 1) ? XMemory : OMemory;

            //Liste aller möglichen Wege von Stradegien
            List<int> Positions = new List<int>();
            //Liste aller Scores
            List<int> finalScores = new List<int>();

            //Falls es gelernte Stradegien gibt und das Feld leer ist
            if (FieldEmpty && Memory.Count > 0)
            {
                //Zufälligen Zug eines der Besten Stradegien auswählen
                int move = ChooseFirst(Memory, player);

                //Falls move grösser als 0 ist
                if (move >= 0)
                {
                    PlaceMove(Buttons[move], player, move);
                    return true;
                }

            }//Falls es gelernte Stradegien gibt aber das Feld nicht leer ist
            else if (Memory.Count > 0)
            {
                int indexOfStradegy = BestStradegy(Memory, board);

                if (indexOfStradegy >= 0)
                {
                    Positions = FreeButtons(Memory, indexOfStradegy, player, board);
                    finalScores = CheckStradegysFirst(Memory, player, board); 
                }
                
            }

            //Falls Stradegien gefunden wurden 
            if (Positions.Count > 0 && finalScores.Count >0 )
            {
                //Besten Score auswerten
                int bestScore = -200;
                int bestMove = -1;

                for(int i = 0; i<finalScores.Count; i++)
                {
                    if (finalScores[i] > bestScore)
                    {
                        bestScore = finalScores[i];
                        bestMove = Positions[i];
                    }
                }

                if (bestMove >=0)
                {
                    PlaceMove(Buttons[bestMove], player, bestMove);
                    return true;
                }
                else { return false; }

            }
            else
            {
                return false;
            }
        }

        //Methode um durch alle Moves zu kommen und zu testen
        public List<int> CheckStradegysFirst(List<Learning>memory, int player, int[]Vboard)
        {
            //Liste der Scores und der Felder erstellen
            List<int> Scores = new List<int>();

            //Speicher des gegnerwertes
            int oponent = (player == 1) ? 2 : 1;

            //Beste Stradegie nach Score bewertet für das aktuelle Layout
            int strategyIndex = BestStradegy(memory, Vboard);

            if (strategyIndex >= 0)
            {
                //Leere Felder erkennen die die Stradegy belegen kann
                List<int> fieldIndex = FreeButtons(memory, strategyIndex, player, Vboard);

                foreach (int i in fieldIndex)
                {
                    //Kopie des aktuellen Feldes erstellen
                    int[] testboard = CopyBoard(Vboard);

                    //Platziere den Move auf deinen Board
                    testboard[i] = player;

                    //Prüfe ob dein Move ein gewinner war
                    int yourMoveScore = TestWin(testboard, player);

                    if (yourMoveScore > 0)
                    {
                        //Gewinnerzug direkt in Score Speichern
                        Scores.Add(1000);
                    }
                    else
                    {
                        //Liste aller leeren Felder bekommen
                        List<int> empty = GetEmptyFields(testboard);

                        if (empty.Count > 0)
                        {
                            int enemyScore = 0;
                            //jedes scenario was der Gegner machen könnte testen
                            foreach (int enemyMoves in empty)
                            {
                                int[] enemyBoard = CopyBoard(testboard);
                                //Gegner platzieren
                                enemyBoard[enemyMoves] = oponent;

                                int enemyMove = TestWin(enemyBoard, oponent);

                                if (enemyMove > 0)
                                {
                                    //Gewinnerzug direkt in Score Speichern
                                    enemyScore =- 1000;
                                }
                                else
                                {
                                    enemyScore = CheckStradegys(memory, player, enemyBoard);
                                }
                            }

                            Scores.Add(enemyScore);
                        }

                    }
                }
            }

            return Scores;
        }

        //Alle weitern Duchläufe müssen die Scores zusammengezählt werden und nach oben weiter gegeben werden
        public int CheckStradegys(List<Learning> memory, int player, int[] Vboard)
        {
            //Liste der Scores und der Felder erstellen
            List<int> Scores = new List<int>();

            //Speicher des gegnerwertes
            int oponent = (player == 1) ? 2 : 1;

            //Beste Stradegie nach Score bewertet für das aktuelle Layout
            int strategyIndex = BestStradegy(memory, Vboard);

            if (strategyIndex >= 0)
            {
                //Leere Felder erkennen die die Stradegy belegen kann
                List<int> fieldIndex = FreeButtons(memory, strategyIndex, player, Vboard);

                foreach (int i in fieldIndex)
                {
                    //Kopie des aktuellen Feldes erstellen
                    int[] testboard = CopyBoard(Vboard);

                    //Platziere den Move auf deinen Board
                    testboard[i] = player;

                    //Prüfe ob dein Move ein gewinner war
                    int yourMoveScore = TestWin(testboard, player);

                    if (yourMoveScore > 0)
                    {
                        //Gewinnerzug direkt in Score Speichern
                        Scores.Add(yourMoveScore);
                    }
                    else
                    {
                        //Liste aller leeren Felder bekommen
                        List<int> empty = GetEmptyFields(testboard);

                        if (empty.Count > 0)
                        {
                            int enemyScore = 0;
                            //jedes scenario was der Gegner machen könnte testen
                            foreach (int enemyMoves in empty)
                            {
                                int[] enemyBoard = CopyBoard(testboard);
                                //Gegner platzieren
                                enemyBoard[enemyMoves] = oponent;

                                int enemyMove = TestWin(enemyBoard, oponent);

                                if (enemyMove > 0)
                                {
                                    //Wenn gegner gewinnt
                                    enemyScore -= enemyMove;
                                }
                                else
                                {
                                    enemyScore = CheckStradegys(memory, player, enemyBoard);
                                }
                            }

                            Scores.Add(enemyScore);
                        }

                    }
                }
            }

            //Alle scores zusammenzählen und als Score zurückgeben
            int finalScore = 0;

            if (Scores.Count >0)
            {
                foreach (int i in Scores)
                {
                    finalScore += i;
                }
            }

            return finalScore;  
        }

        //Methode um das Board mit der Stradegie abgleicht und die Freien stellen der Vorlage zurückgibt
        public List<int> FreeButtons(List<Learning> memory, int index, int player, int[] board)
        {
            List<int> freebuttons = new List<int>();

            for (int i = 0; i < board.Length; i++)
            {
                if (memory[index].Strategy[i] == player && board[i] == 0)
                {
                    freebuttons.Add(i);
                }
            }

            return freebuttons;
        }

        //Funktion für KI um im ersten Zug einen Button auszuwählen
        public int ChooseFirst(List<Learning> Memory, int player)
        {
            //Bekomme die Beste Stradegie als Index
            int stratIndex = BestScore(Memory);

            //Liste der durchführbaren Moves
            List<int> moves = new List<int>();

            //überprüfung aller vom Spieler gesetzten Werde der Vorlage
            int index = 0;
            foreach (int i in Memory[stratIndex].Strategy)
            {
                if (i == player)
                {
                    moves.Add(index);
                }
                index++;
            }

            //Falls es Gültige Züge gibt
            if (moves.Count >0)
            {
                //Wählt zufällig ein Move aus
                int move = random.Next(moves.Count);
                return move;
            }
            else { return -1; }
        }

        //Funktion für KI um in testfällen alle leere Felder zu laden
        public List<int> GetEmptyFields(int[] test)
        {
            List<int> empty = new List<int>();

            int index = 0;
            foreach (int i in test)
            {
                if (i == 0)
                {
                    empty.Add(index);
                }
                index++;
            }

            return empty;
        }

        //Funktion für KI um im testscenario auf gewinn zu prüfen
        public int TestWin(int[] testField, int player)
        {
            //Score dieses Durchlaufes
            int score = 0;

            //Horizontal
            for (int i = 0; i < 9; i += 3)
            {
                if (testField[i] == player && testField[i + 1] == player && testField[i + 2] == player)
                {
                    score = +1;
                }
            }
            //Vertikal
            for (int i = 0; i < 3; i++)
            {
                if (testField[i] == player && testField[i + 3] == player && testField[i + 6] == player)
                {
                    score = +1;
                }
            }
            //Diagonal
            if (testField[0] == player && testField[4] == player && testField[8] == player ||
                testField[2] == player && testField[4] == player && testField[6] == player)
            {
                score = +1;
            }
            return score;
        }

        //Ki Funktion um Board copieren für Simulationen 
        public int[] CopyBoard(int[] origin)
        {
            int[] copy = new int[9];
            Array.Copy(origin, copy, origin.Length);
            return copy;
        }

        //Methode die beste Stradegy zu finden nach puren score
        public int BestScore(List<Learning> learnings)
        {
            //Stradegie Index der ausgewählten Stradegie
            int stradegyIndex = -1;

            //Prüfung des höchsten Scores
            int rate = -1;

            //Prüft alle Stradegien nach score
            foreach (Learning learned in learnings)
            {
                //Wenn Stradegie besser ist als vorherige
                if (learned.TimesWon > rate)
                {
                    //Abspeichern der Gewinnrate
                    rate = learned.TimesWon;
                    //Abspeichern des Indexes der Stradegie
                    stradegyIndex = learnings.IndexOf(learned);
                }
            }
            return stradegyIndex;
        }

        //Methode um die beste Stradegie zu finden (aufgrund der übereinstimmung)
        public int BestStradegy(List<Learning> learnings, int[] testBoard)
        {
            //Stradegie Index der ausgewählten Stradegie
            int stradegyIndex = -1;

            //platzierte felder errechnen
            int placedFields = 0;
            foreach (int i in testBoard)
            {
                if (i != 0)
                {
                    placedFields++;
                }
            }

            //Prüfung des höchsten Scores
            int rate = -1;

            //Prüft alle Stradegien ob diese mit dem auf dem Board Stimmen
            foreach (Learning learned in learnings)
            {
                //prüfung ob die Platzierten Felder mit den übereinstimmenden Felder übereinstimmen
                int check = 0;

                //Speichert die Anzahl der gesetzten Muster im Feld
                for (int i = 0; i < learned.Strategy.Length; i++)
                {
                    if (testBoard[i] != 0 && testBoard[i] == learned.Strategy[i])
                    {
                        check++;
                    }
                }

                //Wenn Stradegie besser ist als vorherige
                if (check == placedFields && learned.TimesWon > rate)
                {
                    //Abspeichern der Gewinnrate
                    rate = learned.TimesWon;
                    //Abspeichern des Indexes der Stradegie
                    stradegyIndex = learnings.IndexOf(learned);
                }
            }
            return stradegyIndex;
        }


    }
}