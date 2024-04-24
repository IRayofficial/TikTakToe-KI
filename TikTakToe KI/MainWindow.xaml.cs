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

        }

        //Random Funktion zum auswählen eines leeren Feldes
        private void RandomPick()
        {
            bool flag = false;
            while (flag == false)
            {
                int y = random.Next(9);
                if (board[y] == 0)
                {
                    flag = true;

                }
            }

            /*
             * 
             */
        }


        //Strategisches entdecken eines Gewinns/verlustes
        private void DetectWin(int p)
        {
            //Horizontale überprüfung 
            for (int i = 0; i < 9; i+=3)
            {
                int player = 0;
                int empty = 0;
                int place = 10;

                for (int l = i; l < l + 3; l++)
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
                }

                if (place != 10 && player == 2 && empty == 1)
                {
                    Button btn = Buttons[place];
                    PlaceMove(btn, p, place);

                }
            }
        }

    }
}