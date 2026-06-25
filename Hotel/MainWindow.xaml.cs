using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace Hotel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Podaci.MojaKonekcija = new SqlConnection("Data Source=DESKTOP-839UVL6;Database=Hotel;Integrated Security = SSPI");
        }

        private void btnGost_Click(object sender, RoutedEventArgs e)
        {
            Window Gost = new Gost();
            this.Close();
            Gost.ShowDialog();
        }

        private void btnSoba_Click(object sender, RoutedEventArgs e)
        {
            Window Soba = new Soba();
            this.Close();
            Soba.ShowDialog();
        }

        private void btnRezervacija_Click(object sender, RoutedEventArgs e)
        {
            Window Rezervacija = new Rezervacija();
            this.Close();
            Rezervacija.ShowDialog();
        }

        private void btnZaposleni_Click(object sender, RoutedEventArgs e)
        {
            Window Zaposleni = new Zaposleni();
            this.Close();
            Zaposleni.ShowDialog();
        }

        private void btnRacun_Click(object sender, RoutedEventArgs e)
        {
            Window Racun = new Racun();
            this.Close();
            Racun.ShowDialog();
        }

     

        private void btnUvid_Click(object sender, RoutedEventArgs e)
        {
            Window Uvid = new Uvid();
            this.Close();
            Uvid.ShowDialog();
        }
    }
}
