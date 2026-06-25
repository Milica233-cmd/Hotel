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
    /// Interaction logic for Gost.xaml
    /// </summary>
    public partial class Gost : Window
    {
        public Gost()
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
                "SELECT ID, Ime, Prezime, Telefon FROM Gost", con);

            da.Fill(dt);

            //MessageBox.Show(dt.Rows.Count.ToString()); // proveri da radi

            reportViewer.LocalReport.DataSources.Clear();

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            reportViewer.LocalReport.DataSources.Add(rds);

            reportViewer.LocalReport.ReportPath =
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GostIzvestaj.rdlc");

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
            Metode.PopuniDataTabeluSelectom("GostSelectGrid", dt);
            Metode.PunjenjeGridaDataTabelom(dgGost, dt);
            CiscenjeTxtBoxova();
        }

        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@Ime", "@Prezime", "@Telefon", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.String, DbType.String, DbType.String, DbType.String };
            object[] ProfVrednosti = { txtIme.Text, txtPrezime.Text, txtTelefon.Text, 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("GostInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Gost vec postoji.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("GostSelectGrid", dt);
                Metode.PunjenjeGridaDataTabelom(dgGost, dt);
                CiscenjeTxtBoxova();
                txtIme.Focus();
            }
        }

        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@Ime", "@Prezime", "@Telefon" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.String, DbType.String, DbType.String };
            object[] ProfVrednosti = { Podaci.IdTekucegReda, txtIme.Text, txtPrezime.Text, txtTelefon.Text };

            SqlCommand cmd = Metode.KreiranjeKomande("GostUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);
            Metode.PopuniDataTabeluSelectom("GostSelectGrid", dt);
            Metode.PunjenjeGridaDataTabelom(dgGost, dt);
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
            txtTelefon.Text = "";
        }

        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView drv in dgGost.SelectedItems)
            {
                dt = new DataTable("dt");
                string[] ParNazivi = { "@ID" };
                DbType[] ParTipovi = { DbType.Int32 };
                object[] ParVrednosti = { (int)drv[0] };
                SqlCommand cmd = Metode.KreiranjeKomande("GostDelete", ParNazivi, ParTipovi, ParVrednosti);
                Metode.IzvrsiKomandu(cmd);
            }
            Metode.PopuniDataTabeluSelectom("GostSelectGrid", dt);
            Metode.PunjenjeGridaDataTabelom(dgGost, dt);

            CiscenjeTxtBoxova();
            txtIme.Focus();
        }
        private int dgIzabraniRed = -1;
        private void dgGost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGost.SelectedItem != null)
            {
                dt = new DataTable("dt");
                dgIzabraniRed = dgGost.SelectedIndex;
                DataRowView drv = (DataRowView)dgGost.SelectedItem;
                Podaci.IdTekucegReda = (int)drv[0];
                txtIme.Text = drv[1].ToString();
                txtPrezime.Text = drv[2].ToString();
                txtTelefon.Text = drv[3].ToString();

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
            System.IO.File.WriteAllBytes(@"C:\Hotelll\GostIzvestaj.pdf", bytes);

            string qrText = "file:///C:/Hotelll/GostIzvestaj.pdf";

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            QRCode qr = new QRCode(data);
            Bitmap slika = qr.GetGraphic(20);

            string putanja = @"C:\Hotelll\QRGost.png";
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
