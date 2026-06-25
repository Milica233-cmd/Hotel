using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data;
using System.Data.SqlClient;

namespace Hotel

{
    public static class Podaci
    {
        public static SqlConnection MojaKonekcija;
        public static int IdTekucegReda;
        public static DataTable dt;
    }
    public class Metode
    {
        private static int brOutParam = 0;
        public static SqlCommand KreiranjeKomande(string StProcedura)
        {
            SqlCommand NekaKomanda = new SqlCommand("dbo." + StProcedura, Podaci.MojaKonekcija);
            NekaKomanda.CommandType = CommandType.StoredProcedure;
            return NekaKomanda;
        }
        public static SqlCommand KreiranjeKomande(string StProcedura, string[] ParamNazivi, System.Data.DbType[] ParamTipovi, object[] ParamVrednosti)
        {
            SqlCommand NekaKomanda = new SqlCommand("dbo." + StProcedura, Podaci.MojaKonekcija);
            NekaKomanda.CommandType = CommandType.StoredProcedure;
            DodajParametreKomandi(ref NekaKomanda, ParamNazivi, ParamTipovi, ParamVrednosti);
            return NekaKomanda;
        }
        private static void DodajParametreKomandi(ref SqlCommand NekaKomanda, string[] ParamNazivi, System.Data.DbType[] ParamTipovi, object[] ParamVrednosti)
        {
            for (int i = 0; i < ParamNazivi.Length; i++)
            {
                SqlParameter p = new SqlParameter();
                p.ParameterName = ParamNazivi[i];
                p.DbType = ParamTipovi[i];
                p.Direction = ParameterDirection.Input;
                if (ParamNazivi[i].Length >= 4)
                {
                    if (ParamNazivi[i].Substring(0, 4) == "@OUT")
                    {
                        p.Direction = ParameterDirection.Output;
                        brOutParam++;
                        p.Size = 50;
                    }
                }
                if (p.Direction == ParameterDirection.Input) p.Value = ParamVrednosti[i];
                NekaKomanda.Parameters.Add(p);
            }
        }

        public static void PopunjavanjeListBoxa(ListBox NekiListBox, DataTable dt, string DisplayMember)
        {
            Binding b = new Binding();
            b.Source = dt;
            NekiListBox.SetBinding(ListBox.ItemsSourceProperty, b);
            NekiListBox.DisplayMemberPath = DisplayMember;
            NekiListBox.SelectedValuePath = "ID";
        }

        public static void IzvrsiKomandu(SqlCommand NekaKomanda)
        {
            try
            {
                Podaci.MojaKonekcija.Open();
                NekaKomanda.ExecuteNonQuery();
            }
            catch (Exception greska)
            {
                MessageBox.Show(greska.Message);
            }
            finally
            {
                Podaci.MojaKonekcija.Close();
            }
        }
        public static DataTable PopuniDataTabeluSelectom(string StProcedura, DataTable mojaDTabela)
        {
            mojaDTabela.Rows.Clear();
            SqlCommand cmd = KreiranjeKomande(StProcedura);
            DataTable dt = new DataTable("dt");
            dt = mojaDTabela;
            try
            {
                Podaci.MojaKonekcija.Open();
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception greska)
            {
                MessageBox.Show(greska.Message);
            }
            finally
            {
                Podaci.MojaKonekcija.Close();
            }
            return dt;
        }


        //private void KreirajDtLokacija()
        //{
        //    dtLokacija = new DataTable("dtLokacija");
        //    dtLokacija.Columns.Add(new DataColumn("Id", Type.GetType("System.Int32")));
        //    dtLokacija.Columns.Add(new DataColumn("Naziv", Type.GetType("System.String")));
        //    dtLokacija.Columns.Add(new DataColumn("Sifra", Type.GetType("System.Int32")));
        //    dtLokacija.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
        //    dtLokacija.Columns.Add(new DataColumn("IsSelected", Type.GetType("System.Boolean")));
        //}

        public static void PunjenjeGridaDataTabelom(DataGrid NekiGrid, DataTable dt)
        {
            Binding b = new Binding();
            b.Source = dt;
            BindingOperations.SetBinding(NekiGrid, ItemsControl.ItemsSourceProperty, b);
            NekiGrid.SelectedValuePath = "ID";
        }

        public static void PunjenjeKomboBoksaDataTabelom(ComboBox NekiKombo, DataTable dt, string DisplayMemberPath)
        {
            NekiKombo.ItemsSource = dt.DefaultView;
            NekiKombo.DisplayMemberPath = DisplayMemberPath;
            NekiKombo.SelectedValuePath = "ID";
        }
        public static DataTable PuniDtReaderom(string StProcedura, string[] ParamNazivi,
           DbType[] ParamTipovi, object[] ParamVrednosti, DataTable mojaDTabela)
        {
            SqlCommand cmd = KreiranjeKomande(StProcedura, ParamNazivi, ParamTipovi, ParamVrednosti);
            DataTable dt = new DataTable("dt");
            dt = mojaDTabela;
            dt.Rows.Clear();
            try
            {
                Podaci.MojaKonekcija.Open();
                dt.Load(cmd.ExecuteReader(CommandBehavior.Default));
            }
            catch (Exception greska)
            {
                MessageBox.Show(greska.Message);
            }
            finally
            {
                Podaci.MojaKonekcija.Close();
            }
            return dt;
        }
        public static object[] IzvrsiKomanduSaOUT(string SProcedura, string[] ParamNazivi,
                     DbType[] ParamTipovi, object[] ParamVrednosti)
        {
            SqlCommand cmd = KreiranjeKomande(SProcedura, ParamNazivi, ParamTipovi, ParamVrednosti);
            try
            {
                Podaci.MojaKonekcija.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception greska)
            {
                MessageBox.Show(greska.Message);
            }
            finally
            {
                Podaci.MojaKonekcija.Close();
            }
            object[] OutParam = new object[brOutParam];
            int k = 0;
            for (int i = 0; i < cmd.Parameters.Count; i++)
            {
                if (cmd.Parameters[i].ParameterName.Substring(0, 4) == "@OUT")
                {
                    OutParam[k] = cmd.Parameters[i].Value;
                    k++;
                }
            }
            return OutParam;
        }
        public static void IzvrsiKomandu(string SProcedura,
    string[] ParamNazivi,
    DbType[] ParamTipovi,
    object[] ParamVrednosti)
        {
            SqlCommand cmd = KreiranjeKomande(
                SProcedura,
                ParamNazivi,
                ParamTipovi,
                ParamVrednosti);

            try
            {
                Podaci.MojaKonekcija.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception greska)
            {
                MessageBox.Show(greska.Message);
            }
            finally
            {
                Podaci.MojaKonekcija.Close();
            }
        }

    }

}