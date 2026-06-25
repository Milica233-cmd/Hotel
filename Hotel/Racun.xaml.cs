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
    /// Interaction logic for Racun.xaml
    /// </summary>
    public partial class Racun : Window
    {
        public Racun()
        {
            InitializeComponent();
            Podaci.MojaKonekcija = new SqlConnection(@"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");
        }
        private void UcitajIzvestaj()
        {

            SqlConnection con = new SqlConnection(
         @"Server=DESKTOP-839UVL6;Database=Hotel;Trusted_Connection=True;");

            DataTable dt = new DataTable();

            SqlDataAdapter da = new SqlDataAdapter(@"SELECT ra.ID, CONCAT(LTRIM(RTRIM(g.Ime)), ' ',LTRIM(RTRIM(g.Prezime)),' - Soba ',CAST(s.BrojSobe AS nvarchar(10))) AS Rezervacija, ra.Iznos, ra.DatumIzdavanja FROM Racun ra
                            INNER JOIN Rezervacija r ON r.ID = ra.RezervacijaID INNER JOIN Gost g ON g.ID = r.GostID INNER JOIN Soba s ON s.ID = r.SobaID", con);

            da.Fill(dt);

            //MessageBox.Show(dt.Rows.Count.ToString()); // proveri da radi

            reportViewer.LocalReport.DataSources.Clear();

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            reportViewer.LocalReport.DataSources.Add(rds);

            reportViewer.LocalReport.ReportPath =
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RacunIzvestaj.rdlc");

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
            if (dgRacun.SelectedIndex == -1)
            {
                btnIzmeni.IsEnabled = false;
                btnObrisi.IsEnabled = false;


            }
            else
            {
                btnIzmeni.IsEnabled = true;
                btnObrisi.IsEnabled = true;

            }
            DataTable dtRezervacija = new DataTable();
            Metode.PopuniDataTabeluSelectom("RezervacijaSelect", dtRezervacija);

            cboRezervacija.ItemsSource = dtRezervacija.DefaultView;
            cboRezervacija.DisplayMemberPath = "Gost";
            cboRezervacija.SelectedValuePath = "ID";

            dt = new DataTable();
            Metode.PopuniDataTabeluSelectom("RacunSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRacun, dt);
        }
        private void OmogucavanjeUnosa()
        {
            btnUnesi.IsEnabled = true;
            btnIzmeni.IsEnabled = true;
            btnObrisi.IsEnabled = true;
        }
        private void cboRezervacija_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboRezervacija.SelectedItem == null)
                return;

            DataRowView red = (DataRowView)cboRezervacija.SelectedItem;

            DateTime datumOd = Convert.ToDateTime(red["DatumOd"]);
            DateTime datumDo = Convert.ToDateTime(red["DatumDo"]);

            int brojDana = (datumDo - datumOd).Days;

            if (brojDana <= 0)
                brojDana = 1;

            int cena = Convert.ToInt32(red["Cena"]);

            txtIznos.Text = (brojDana * cena).ToString();

            txtDatumIzdavanja.Text = datumOd.ToString("dd.MM.yyyy") + " - " +
                                     datumDo.ToString("dd.MM.yyyy");


        }

        private void txtIznos_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtDatumIzdavanja_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@RezervacijaID", "@Iznos", "@DatumIzdavanja", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.Int32, DbType.String, DbType.String };
            object[] ProfVrednosti = { cboRezervacija.SelectedValue, Convert.ToInt32(txtIznos.Text), txtDatumIzdavanja.Text, 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("RacunInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Racun vec postoji.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("RacunSelect", dt);
                Metode.PunjenjeGridaDataTabelom(dgRacun, dt);
                CiscenjeTxtBoxova();

            }
        }

        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@RezervacijaID", "@Iznos", "@DatumIzdavanja" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.Int32, DbType.Int32, DbType.String };
            object[] ProfVrednosti = { Podaci.IdTekucegReda, cboRezervacija.SelectedValue, Convert.ToInt32(txtIznos.Text), txtDatumIzdavanja.Text };

            SqlCommand cmd = Metode.KreiranjeKomande("RacunUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);

            Metode.PopuniDataTabeluSelectom("RacunSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRacun, dt);
            CiscenjeTxtBoxova();
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            CiscenjeTxtBoxova();
        }
        private void CiscenjeTxtBoxova()
        {
            txtIznos.Text = "";
            txtDatumIzdavanja.Text = "";
            cboRezervacija.Focus();
            
        }
        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView drv in dgRacun.SelectedItems)
            {
                dt = new DataTable("dt");
                string[] ParNazivi = { "@ID" };
                DbType[] ParTipovi = { DbType.Int32 };
                object[] ParVrednosti = { (int)drv[0] };
                SqlCommand cmd = Metode.KreiranjeKomande("RacunDelete", ParNazivi, ParTipovi, ParVrednosti);
                Metode.IzvrsiKomandu(cmd);
            }
            Metode.PopuniDataTabeluSelectom("RacunSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgRacun, dt);

            CiscenjeTxtBoxova();
            cboRezervacija.Focus();
            
        }

        private void dgRacun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRacun.SelectedItem == null)
                return;

            DataRowView drv = dgRacun.SelectedItem as DataRowView;

            if (drv == null)
                return;

            Podaci.IdTekucegReda = Convert.ToInt32(drv["ID"]);

            cboRezervacija.SelectedValue = drv["RezervacijaID"];

            if (drv["DatumIzdavanja"] != DBNull.Value)
                txtDatumIzdavanja.Text = Convert.ToString(drv["DatumIzdavanja"]);
            else
                txtDatumIzdavanja.Text = "";

            // Dugmad
            btnUnesi.IsEnabled = true;
            btnIzmeni.IsEnabled = true;
            btnObrisi.IsEnabled = true;
            btnOdustani.IsEnabled = true;
        }

        private void dgRacun_AutoGeneratedColumns(object sender, EventArgs e)
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
            System.IO.File.WriteAllBytes(@"C:\Hotelll\RacunIzvestaj.pdf", bytes);

            string qrText = "file:///C:/Hotelll/RacunIzvestaj.pdf";

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            QRCode qr = new QRCode(data);
            Bitmap slika = qr.GetGraphic(20);

            string putanja = @"C:\Hotelll\QRRacun.png";
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
