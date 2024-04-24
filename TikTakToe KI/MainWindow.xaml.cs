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

namespace TikTakToe_KI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public char Player = 'X';
        public char Computer = 'O';
        public bool YourTurn;

        public int ScoreX = 0;
        public int ScoreO = 0;

        public List<Button> Buttons = new List<Button>();
        public int[] board;
        Random random = new Random();
        public MainWindow()
        {
            InitializeComponent();

            foreach (Button btn in GameGrid.Children)
            {
                Buttons.Add(btn);
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
                if (YourTurn)
                {
                    btn.Content = Player;
                    CheckWin();
                    YourTurn = false;
                }
                else
                {
                    btn.Content = Computer;
                    CheckWin();
                    YourTurn = true;
                }
                
            }
        }

        //Reset des Games 
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

        //überprüfung ob jemand gewonnen hat
        private void CheckWin()
        {
            
        }

        private void AddScore(string Player)
        {

        }
    }
}