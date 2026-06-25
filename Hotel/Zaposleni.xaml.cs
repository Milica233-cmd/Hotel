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
    /// Interaction logic for Zaposleni.xaml
    /// </summary>
    public partial class Zaposleni : Window
    {
        public Zaposleni()
        {
            InitializeComponent();
            Podaci.MojaKonekcija = new SqlConnection(@"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");
        }
        private void UcitajIzvestaj()
        {

            SqlConnection con = new SqlConnection(
         @"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");

            DataTable dt = new DataTable();

            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT ID, Ime, Prezime, Pozicija FROM Zaposleni", con);

            da.Fill(dt);

            //MessageBox.Show(dt.Rows.Count.ToString()); // proveri da radi

            reportViewer.LocalReport.DataSources.Clear();

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            reportViewer.LocalReport.DataSources.Add(rds);

            reportViewer.LocalReport.ReportPath =
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZaposleniIzvestaj.rdlc");

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
            {
                btnIzmeni.IsEnabled = true;
            }
            dt = new DataTable("dt");
            Metode.PopuniDataTabeluSelectom("ZaposleniSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgZaposleni, dt);
            CiscenjeTxtBoxova();
        }
        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@Ime", "@Prezime", "@Pozicija", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.String, DbType.String, DbType.String, DbType.String };
            object[] ProfVrednosti = { txtIme.Text, txtPrezime.Text, txtPozicija.Text, 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("ZaposleniInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Zaposleni vec postoji.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("ZaposleniSelect", dt);
                Metode.PunjenjeGridaDataTabelom(dgZaposleni, dt);
                CiscenjeTxtBoxova();
                txtIme.Focus();
            }
        }

        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@Ime", "@Prezime", "@Pozicija" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.String, DbType.String, DbType.String };
            object[] ProfVrednosti = { Podaci.IdTekucegReda, txtIme.Text, txtPrezime.Text, txtPozicija.Text };

            SqlCommand cmd = Metode.KreiranjeKomande("ZaposleniUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);
            Metode.PopuniDataTabeluSelectom("ZaposleniSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgZaposleni, dt);
            CiscenjeTxtBoxova();
            txtIme.Focus();
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            CiscenjeTxtBoxova();
        }
        private void CiscenjeTxtBoxova()
        {
            txtIme.Text = "";
            txtPrezime.Text = "";
            txtPozicija.Text = "";
        }
        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView drv in dgZaposleni.SelectedItems)
            {
                dt = new DataTable("dt");
                string[] ParNazivi = { "@ID" };
                DbType[] ParTipovi = { DbType.Int32 };
                object[] ParVrednosti = { (int)drv[0] };
                SqlCommand cmd = Metode.KreiranjeKomande("ZaposleniDelete", ParNazivi, ParTipovi, ParVrednosti);
                Metode.IzvrsiKomandu(cmd);
            }
            Metode.PopuniDataTabeluSelectom("ZaposleniSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgZaposleni, dt);

            CiscenjeTxtBoxova();
            txtIme.Focus();
        }
        private int dgIzabraniRed = -1;
        private void dgZaposleni_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgZaposleni.SelectedItem != null)
            {
                dt = new DataTable("dt");
                dgIzabraniRed = dgZaposleni.SelectedIndex;
                DataRowView drv = (DataRowView)dgZaposleni.SelectedItem;
                Podaci.IdTekucegReda = (int)drv[0];
                txtIme.Text = drv[1].ToString();
                txtPrezime.Text = drv[2].ToString();
                txtPozicija.Text = drv[3].ToString();

            }
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
            System.IO.File.WriteAllBytes(@"C:\Hotelll\ZaposleniIzvestaj.pdf", bytes);

            string qrText = "file:///C:/Hotelll/ZaposleniIzvestaj.pdf";

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            QRCode qr = new QRCode(data);
            Bitmap slika = qr.GetGraphic(20);

            string putanja = @"C:\Hotelll\QRZaposleni.png";
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
