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
    /// Interaction logic for Soba.xaml
    /// </summary>
    public partial class Soba : Window
    {
        public Soba()
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
                "SELECT ID, BrojSobe, TipSobe, Cena FROM Soba", con);

            da.Fill(dt);

            //MessageBox.Show(dt.Rows.Count.ToString()); // proveri da radi

            reportViewer.LocalReport.DataSources.Clear();

            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            reportViewer.LocalReport.DataSources.Add(rds);

            reportViewer.LocalReport.ReportPath =
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SobaIzvestaj.rdlc");

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
            Metode.PopuniDataTabeluSelectom("SobaSelect", dt);

            DataView view = new DataView(dt);

            DataTable dtTipovi = view.ToTable(true, "TipSobe");

            cmbTipSobe.ItemsSource = dtTipovi.DefaultView;
            cmbTipSobe.DisplayMemberPath = "TipSobe";

            Metode.PunjenjeGridaDataTabelom(dgSoba, dt);
            CiscenjeTxtBoxova();
        }
        private int dgIzabraniRed = -1;
        private void dgSoba_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSoba.SelectedItem != null)
            {
                dt = new DataTable("dt");
                dgIzabraniRed = dgSoba.SelectedIndex;
                DataRowView drv = (DataRowView)dgSoba.SelectedItem;
                Podaci.IdTekucegReda = (int)drv[0];
                txtBrojSobe.Text = drv[1].ToString();
                cmbTipSobe.Text = drv[2].ToString();
                txtCena.Text = drv[3].ToString();

            }
        }

        private void btnUnesi_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@BrojSobe", "@TipSobe", "@Cena", "@OUTStudentVecPostoji" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.String, DbType.Int32, DbType.String };
            object[] ProfVrednosti = { Convert.ToInt32(txtBrojSobe.Text), cmbTipSobe.Text, Convert.ToInt32(txtCena.Text), 0 };

            object[] OutParam = Metode.IzvrsiKomanduSaOUT("SobaInsert", ProfNazivi, ProfTipovi, ProfVrednosti);
            string poruka = "";
            if (OutParam[0].ToString() != "0") poruka += "Soba je popunjena.";
            if (poruka != "")
            {
                MessageBox.Show(poruka);
                CiscenjeTxtBoxova();
            }
            else
            {
                Metode.PopuniDataTabeluSelectom("SobaSelect", dt);
                Metode.PunjenjeGridaDataTabelom(dgSoba, dt);
                CiscenjeTxtBoxova();
                txtBrojSobe.Focus();
            }
        }

        private void btnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            dt = new DataTable("dt");
            string[] ProfNazivi = { "@ID", "@BrojSobe", "@TipSobe", "@Cena" };
            DbType[] ProfTipovi = { DbType.Int32, DbType.Int32, DbType.String, DbType.Int32 };
            object[] ProfVrednosti = { Podaci.IdTekucegReda, Convert.ToInt32(txtBrojSobe.Text), cmbTipSobe.Text, Convert.ToInt32(txtCena.Text) };

            SqlCommand cmd = Metode.KreiranjeKomande("SobaUpdate", ProfNazivi, ProfTipovi, ProfVrednosti);
            Metode.IzvrsiKomandu(cmd);
            Metode.PopuniDataTabeluSelectom("SobaSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgSoba, dt);
            CiscenjeTxtBoxova();
            txtBrojSobe.Focus();
        }

        private void btnOdustani_Click(object sender, RoutedEventArgs e)
        {
            CiscenjeTxtBoxova();
        }
        private void CiscenjeTxtBoxova()
        {
            txtBrojSobe.Text = "";
            cmbTipSobe.SelectedIndex = -1;
            txtCena.Text = "";
        }

        private void btnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            this.Close();
            MainWindow.ShowDialog();
        }

        private void btnObrisi_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView drv in dgSoba.SelectedItems)
            {
                dt = new DataTable("dt");
                string[] ParNazivi = { "@ID" };
                DbType[] ParTipovi = { DbType.Int32 };
                object[] ParVrednosti = { (int)drv[0] };
                SqlCommand cmd = Metode.KreiranjeKomande("SobaDelete", ParNazivi, ParTipovi, ParVrednosti);
                Metode.IzvrsiKomandu(cmd);
            }
            Metode.PopuniDataTabeluSelectom("SobaSelect", dt);
            Metode.PunjenjeGridaDataTabelom(dgSoba, dt);

            CiscenjeTxtBoxova();
            txtBrojSobe.Focus();
        }

        private void cmbTipSobe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cmbTipSobe.SelectedItem == null)
            //    return;

            //DataRowView red = (DataRowView)cmbTipSobe.SelectedItem;
            //string tip = red["TipSobe"].ToString();

        
            //DataTable dtTemp = new DataTable();
            //Metode.PopuniDataTabeluSelectom("SobaSelect", dtTemp);

            //DataView view = new DataView(dtTemp);
            //view.RowFilter = $"TipSobe = '{tip}'";

            //if (view.Count > 0)
            //{
            //    txtBrojSobe.Text = view[0]["BrojSobe"].ToString();
            //    txtCena.Text = view[0]["Cena"].ToString();
            //}
        }

        private void btnIzvestaj_Click(object sender, RoutedEventArgs e)
        {
            UcitajIzvestaj();
        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            UcitajIzvestaj();

            byte[] bytes = reportViewer.LocalReport.Render("PDF");
            Directory.CreateDirectory(@"C:\Hotelll\");
            System.IO.File.WriteAllBytes(@"C:\Hotelll\SobaIzvestaj.pdf", bytes);

            string qrText = "file:///C:/Hotelll/SobaIzvestaj.pdf";

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

            QRCode qr = new QRCode(data);
            Bitmap slika = qr.GetGraphic(20);

            string putanja = @"C:\Hotelll\QRSoba.png";
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
