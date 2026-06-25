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
    /// Interaction logic for Rezervacija.xaml
    /// </summary>
    public partial class Rezervacija : Window
    {
        public Rezervacija()
        {
            InitializeComponent();
            Podaci.MojaKonekcija = new SqlConnection(@"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");
        }
        private void UcitajIzvestaj()
        {

            SqlConnection con = new SqlConnection(
         @"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");

            DataTable dt = new DataTable();

            SqlDataAdapter da = new SqlDataAdapter("SELECT r.ID, g.Ime + ' ' + g.Prezime AS Gost, s.BrojSobe AS Soba, s.Cena AS Cena, (DATEDIFF(day, r.DatumOd, r.DatumDo) + 1) * s.Cena AS UkupnaCena,  r.DatumOd, r.DatumDo " + "FROM Rezervacija r " + "INNER JOIN Gost g ON r.GostID = g.ID " + "INNER JOIN Soba s ON r.SobaID = s.ID", con);

            da.Fill(dt);

            //MessageBox.Show(dt.Rows.Count.ToString()); // proveri da radi

            reportViewer.LocalReport.DataSources.Clear();

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            reportViewer.LocalReport.DataSources.Add(rds);

            reportViewer.LocalReport.ReportPath =
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RezervacijaIzvestaj.rdlc");

            try
            {
                ReportParameter rp = new ReportParameter("Datum",
                    DateTime.Now.ToString("yyyy-MM-dd"));

                reportViewer.LocalReport.SetParameters(new[] { rp });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            reportViewer.RefreshReport();
        }
        private DataTable dt;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            if (dgRezervacija.SelectedIndex == -1)
            {
                btnIzmeni.IsEnabled = false;
                btnObrisi.IsEnabled = false;


            }
            else
            {
                btnIzmeni.IsEnabled = true;
                btnObrisi.IsEnabled = true;

            }
            DataTable dtGost = new DataTable();
            Metode.PopuniDataTabeluSelectom("GostSelect", dtGost);

            cboGost.ItemsSource = dtGost.DefaultView;
            cboGost.DisplayMemberPath = "Gost";
            cboGost.SelectedValuePath = "ID";


            DataTable dtSoba = new DataTable();
            Metode.PopuniDataTabeluSelectom("SobaSelect", dtSoba);

            cboSoba.ItemsSource = dtSoba.DefaultView;
            cboSoba.DisplayMemberPath = "BrojSobe";
            cboSoba.SelectedValuePath = "ID";

            //dt = new DataTable("dt");
            Metode.PopuniDataTabeluSelectom("RezervacijaSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRezervacija, dt);
            CiscenjeTxtBoxova();
        }
        private void OmogucavanjeUnosa()
        {
            btnUnesi.IsEnabled = true;
            btnIzmeni.IsEnabled = true;
            btnObrisi.IsEnabled = true;
        }

        private void cboGost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OmogucavanjeUnosa();
            if (cboGost.SelectedValue == null)
                return;

            int GostID = Convert.ToInt32(cboGost.SelectedValue);
        }

        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@GostID", "@SobaID", "@DatumOd", "@DatumDo", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.Int32 , DbType.Int32, DbType.Date, DbType.Date, DbType.String };
            object[] ProfVrednosti = { cboGost.SelectedValue, cboSoba.SelectedValue, dtpDatumOd.SelectedDate, dtpDatumDo.SelectedDate, 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("RezervacijaInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Rezervacija vec postoji.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("RezervacijaSelect", dt);
                Metode.PunjenjeGridaDataTabelom(dgRezervacija, dt);
                CiscenjeTxtBoxova();
                
            }
        }

        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@GostID", "@SobaID", "@DatumOd", "@DatumDo" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.Int32, DbType.Int32, DbType.Date, DbType.Date };
            object[] ProfVrednosti = { Podaci.IdTekucegReda, cboGost.SelectedValue, cboSoba.SelectedValue, dtpDatumOd.SelectedDate, dtpDatumDo.SelectedDate };
 
            SqlCommand cmd = Metode.KreiranjeKomande("RezervacijaUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);

            Metode.PopuniDataTabeluSelectom("RezervacijaSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRezervacija, dt);
            CiscenjeTxtBoxova();
          
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            CiscenjeTxtBoxova();
        }
        private void CiscenjeTxtBoxova()
        {
            dtpDatumOd.SelectedDate = null;
            dtpDatumDo.SelectedDate = null;
            cboGost.Focus();
            cboSoba.Focus();
        }
        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView drv in dgRezervacija.SelectedItems)
            {
                dt = new DataTable("dt");
                string[] ParNazivi = { "@ID" };
                DbType[] ParTipovi = { DbType.Int32 };
                object[] ParVrednosti = { (int)drv[0] };
                SqlCommand cmd = Metode.KreiranjeKomande("RezervacijaDelete", ParNazivi, ParTipovi, ParVrednosti);
                Metode.IzvrsiKomandu(cmd);
            }
            Metode.PopuniDataTabeluSelectom("RezervacijaSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRezervacija, dt);

            CiscenjeTxtBoxova();
            cboGost.Focus();
            cboSoba.Focus();
        }

        private void cboSoba_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OmogucavanjeUnosa();
            if (cboSoba.SelectedValue == null)
                return;

            int SobaID = Convert.ToInt32(cboSoba.SelectedValue);
        }
        private void dgRezervacija_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRezervacija.SelectedItem != null)
            {
                DataRowView drv = (DataRowView)dgRezervacija.SelectedItem;

                // ID trenutnog reda
                Podaci.IdTekucegReda = Convert.ToInt32(drv["ID"]);

                // ComboBox - dete
                cboGost.SelectedValue = Convert.ToInt32(drv["GostID"]);
                cboSoba.SelectedValue = Convert.ToInt32(drv["SobaID"]);

                // Datum
                if (drv["DatumOd"] != DBNull.Value)
                    dtpDatumOd.SelectedDate = Convert.ToDateTime(drv["DatumOd"]);
                else
                    dtpDatumOd.SelectedDate = null;

                if (drv["DatumDo"] != DBNull.Value)
                    dtpDatumDo.SelectedDate = Convert.ToDateTime(drv["DatumDo"]);
                else
                    dtpDatumDo.SelectedDate = null;

                // Dugmad
                btnUnesi.IsEnabled = false;
                btnIzmeni.IsEnabled = true;
                btnObrisi.IsEnabled = true;
                btnOdustani.IsEnabled = true;
            }
        }
    

        private void dgRezervacija_AutoGeneratedColumns(object sender, EventArgs e)
        {

        }

        private void btnIzvestaj_Click(object sender, RoutedEventArgs e)
        {
            UcitajIzvestaj();
        }

        private void btnQRkod_Click(object sender, RoutedEventArgs e)
        {
            UcitajIzvestaj();

            byte[] bytes = reportViewer.LocalReport.Render("PDF");
            Directory.CreateDirectory(@"C:\Hotelll\");
            System.IO.File.WriteAllBytes(@"C:\Hotelll\RezervacijaIzvestaj.pdf", bytes);

            string qrText = "file:///C:/Hotelll/RezervacijaIzvestaj.pdf";

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            QRCode qr = new QRCode(data);
            Bitmap slika = qr.GetGraphic(20);

            string putanja = @"C:\Hotelll\QRRezervacija.png";
            slika.Save(putanja);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(putanja);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            imgQrKod.Source = bitmap;

           
        }
    }
}
