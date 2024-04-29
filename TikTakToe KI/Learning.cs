using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TikTakToe_KI.assets
{
    //Klasse Speichert Spielstandreihenfolge
    public class Learning
    {
        //Gewinnmuster wird hier eingeseichert
        public int[] Strategy = new int[9];

        //Idee noch ein Score für den Ersten Button zu erstellen

        public int WinningPlayer;

        //Wird benötigt wenn keine Felder platziert worden sind um die Strategie auszuwählen welche am besten funktioniert
        public int TimesWon = 0;
       
    }
}
