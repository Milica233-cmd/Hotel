using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Reporting.WinForms;
using QRCoder;
using System.Drawing;
using System.IO;

namespace Hotel
{
    /// <summary>
    /// Interaction logic for Uvid.xaml
    /// </summary>
    public partial class Uvid : Window
    {
         int selectedUvidId;
        public Uvid()
        {
            InitializeComponent();
            Podaci.MojaKonekcija = new SqlConnection(@"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");
        }
        private DataTable dt;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSobe();
            LoadTree();
        }
        private void LoadSobe()
        {
            using (SqlConnection con = new SqlConnection(
                @"Data Source=.;Initial Catalog=Hotel;Integrated Security=True"))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT ID, TipSobe FROM Soba", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                cbSoba.ItemsSource = dt.DefaultView;
                cbSoba.DisplayMemberPath = "TipSobe";
                cbSoba.SelectedValuePath = "ID";    
            }
        }
        private void LoadTree()
        {
            tvUvid.Items.Clear();

            using (SqlConnection con = new SqlConnection(
                @"Data Source=.;Initial Catalog=Hotel;Integrated Security=True"))
            {
                SqlDataAdapter da = new SqlDataAdapter();

                da.SelectCommand = new SqlCommand(@"SELECT u.ID, u.Sifra, u.SobaID, s.TipSobe FROM Uvid u LEFT JOIN Soba s ON u.SobaID = s.ID  ORDER BY s.TipSobe, u.Sifra", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                TreeViewItem root = new TreeViewItem();
                root.Header = "Uvid";

                var grupe = dt.AsEnumerable()
                      .GroupBy(r => r["TipSobe"].ToString());

                foreach (var grupa in grupe)
                {
                    TreeViewItem sobaNode = new TreeViewItem();
                    sobaNode.Header = grupa.Key;

                    foreach (var row in grupa)
                    {
                        TreeViewItem stavka = new TreeViewItem();
                        stavka.Header = row["Sifra"].ToString();
                        stavka.Tag = row["ID"];

                        stavka.Selected += Item_Selected;

                        sobaNode.Items.Add(stavka);
                    }

                    root.Items.Add(sobaNode);
                }

                tvUvid.Items.Add(root);
            }
        }

        private void Item_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item == null || item.Tag == null)
                return;

            selectedUvidId = Convert.ToInt32(item.Tag);

            using (SqlConnection con = new SqlConnection(@"Data Source=.;Initial Catalog=Hotel;Integrated Security=True"))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT u.Sifra, u.SobaID FROM Uvid u WHERE u.ID = @ID", con);

                cmd.Parameters.AddWithValue("@ID", selectedUvidId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtSifra.Text = dr["Sifra"].ToString();
                    cbSoba.SelectedValue = dr["SobaID"];
                }
            }
        }
        
        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@Sifra", "@SobaID", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.String, DbType.Int32, DbType.String };
            object[] ProfVrednosti = { txtSifra.Text, Convert.ToInt32(cbSoba.SelectedValue), 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("UvidInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Sifra vec postoji.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("UvidSelect", dt);

                CiscenjeTxtBoxova();

            }

            LoadTree();
        }
        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@Sifra", "@SobaID" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.String, DbType.Int32 };
            object[] ProfVrednosti = { selectedUvidId, txtSifra.Text, Convert.ToInt32(cbSoba.SelectedValue) };

            SqlCommand cmd = Metode.KreiranjeKomande("UvidUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);

            Metode.PopuniDataTabeluSelectom("UvidSelect", dt);

            LoadTree();

           
           
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {

            if (selectedUvidId == 0)
            {
                MessageBox.Show("Izaberi stavku za brisanje!");
                return;
            }
            dt = new DataTable("dt");
            string[] ParNazivi = { "@ID" };
            DbType[] ParTipovi = { DbType.Int32 };
            object[] ParVrednosti = { selectedUvidId };
            SqlCommand cmd = Metode.KreiranjeKomande("UvidDelete", ParNazivi, ParTipovi, ParVrednosti);
            Metode.IzvrsiKomandu(cmd);
        
             Metode.PopuniDataTabeluSelectom("UvidSelect", dt);


            LoadTree();

            CiscenjeTxtBoxova();
        }
        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            CiscenjeTxtBoxova();
        }
        private void CiscenjeTxtBoxova()
        {
            txtSifra.Text = "";
            cbSoba.SelectedIndex = -1;
        }

        private void cbSoba_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
    
}
