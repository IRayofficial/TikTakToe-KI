using System;
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
using static System.Formats.Asn1.AsnWriter;

namespace TikTakToe_KI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Attribute
        public char Player = 'X';
        public char Computer = 'O';
        public bool YourTurn;

        public int ScoreX = 0;
        public int ScoreO = 0;

        public List<Button> Buttons = new List<Button>();
        public int[] board = new int[9];
        Random random = new Random();
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
                    PlaceMove(btn, 1, index);
                    CheckWin(1);
                    YourTurn = false;
                }
                else
                {
                    PlaceMove(btn, 2, index);
                    CheckWin(2);
                    YourTurn = true;
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
            btn.Content = p;
            board[index] = n;
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

            board = new int[9];
        }

        //Startet Partie
        private void Start_Click(object sender, RoutedEventArgs e)
        {

        }

        //Stopt Partie
        private void Stop_Click(object sender, RoutedEventArgs e)
        {

        }

        //Gewinnüberprüfung
        private void CheckWin(int p)
        {
            //Horizontal
            for (int i = 0; i <9; i+=3)
            {
                if (board[i] == p && board[i+1] == p && board[i+2] == p)
                {
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
                    AddScore(p);
                    NewGame();
                    return;
                }
            }

            //Diagonal

            if ((board[0] == p && board[4] == p && board[8] == p || board[2] == p && board[4] == p && board[6] == p))
            {
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
            }
            else
            {
                ScoreO++;
                OScore.Text = ScoreO.ToString();
            }
        }

        //Allgemeiner Botaufruf
        private void Bot()
        {
            while (true)
            {
                Thread.Sleep(100);
                Dispatcher.Invoke(() =>
                {
                    if (C1.IsChecked == true && YourTurn)
                    {
                        if (DetectWin(1, 1))
                        {
                            YourTurn = !YourTurn;
                        }
                        else if (DetectWin(2, 1))
                        {
                            YourTurn = !YourTurn;
                        }
                        else
                        {
                            RandomPick(1);
                            YourTurn = !YourTurn;
                        }

                        CheckWin(1);
                    }

                    if (C2.IsChecked == true && YourTurn == false)
                    {
                        if (DetectWin(2, 2))
                        {
                            YourTurn = !YourTurn;
                        }
                        else if (DetectWin(1, 2))
                        {
                            YourTurn = !YourTurn;
                        }
                        else
                        {
                            RandomPick(2);
                            YourTurn = !YourTurn;
                        }

                        CheckWin(2);
                    }
                });
                Thread.Sleep(100);
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


        //Strategisches entdecken eines Gewinns/verlustes
        private bool DetectWin(int p, int n)
        {
            int player = 0;
            int empty = 0;
            int place = 10;
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