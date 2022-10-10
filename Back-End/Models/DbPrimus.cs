using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Web_ASP.Models
{
    public class DbPrimus
    {
        protected SqlConnection conn = new SqlConnection
        {
            ConnectionString = Startup.ConnectionString
        };
        private string table_pH = "bandeng_ph";
        private string table_beri_pakan = "bandeng_makan";
        private string table_record_pakan = "bandeng_pakan";

        public Primus getCurrentData()
        {
            Primus item = new Primus();
            int jml = getRowNumbers();
            conn.Open();
            string query = "SELECT TOP (1) * FROM " + table_pH + " ORDER BY waktu_input DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                item.nilaiPh = (float)Convert.ToDouble(dataReader["nilai_ph"]);
                item.levelPh = Convert.ToString(dataReader["level_ph"]);
                item.kondisi = Convert.ToString(dataReader["kondisi"]);
                item.waktu_input = Convert.ToString(dataReader["waktu_input"]);
            }
            item.jumlah_data = jml;
            dataReader.Close();
            conn.Close();
            return item;
        }

        public IList<Primus> getData(string query)
        {
            IList<Primus> records = new List<Primus>();
            conn.Open();

            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                Primus item = new Primus();
                item.nilaiPh = (float)Convert.ToDouble(dataReader["nilai_ph"]);
                item.levelPh = Convert.ToString(dataReader["level_ph"]);
                item.kondisi = Convert.ToString(dataReader["kondisi"]);
                item.waktu_input = Convert.ToString(dataReader["waktu_input"]);
                records.Add(item);
            }
            dataReader.Close();
            conn.Close();
            return records;
        }

        public IList<Primus> getAllData()
        {
            return getData("SELECT * FROM " + table_pH + " ORDER BY waktu_input");
        }

        public IList<Primus> getLastData(int jml)
        {
            return getData("SELECT * FROM " + table_pH + " WHERE waktu_input IN (SELECT TOP (" + jml + ") waktu_input FROM " + table_pH + " ORDER BY waktu_input DESC)");
        }

        public IList<Primus> getDataPhBetween(string dariWaktu, string keWaktu)
        {
            return getData("SELECT * FROM " + table_pH + " WHERE waktu_input BETWEEN '" + dariWaktu + "' AND '" + keWaktu + "' ORDER BY waktu_input");
        }

        public int getRowNumbers()
        {
            int count = 0;
            conn.Open();
            string query = "SELECT COUNT(*) FROM " + table_pH + "";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                count = Convert.ToInt32(dataReader[0]);
            }
            dataReader.Close();
            conn.Close();

            return count;
        }

        public int addData(float nilai_ph, string level_ph, string kondisi, string timeNow)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO " + table_pH + " (nilai_ph, level_ph, kondisi, waktu_input) VALUES (@nilaiPh, @level_ph, @kondisi, @waktu_input)", conn);
            cmd.Parameters.AddWithValue("@nilaiPh", nilai_ph);
            cmd.Parameters.AddWithValue("@level_ph", level_ph);
            cmd.Parameters.AddWithValue("@kondisi", kondisi);
            cmd.Parameters.AddWithValue("@waktu_input", timeNow);
            int row_affected = cmd.ExecuteNonQuery();
            conn.Close();
            return row_affected;
        }

        public int deleteTopOneData()
        {
            return execute("DELETE TOP (5000) FROM " + table_pH);
        }

        // method pakan

        // method untuk membantu mengambil data di table record pakan
        public IList<FeedRecords> getFeedRecords(string query)
        {
            IList<FeedRecords> records = new List<FeedRecords>();

            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                FeedRecords item = new FeedRecords();
                item.beratPakan = (float)Convert.ToDouble(dataReader["berat_pakan"]);
                item.waktuPakan = Convert.ToString(dataReader["waktu_input"]);
                records.Add(item);
            }
            dataReader.Close();
            conn.Close();

            return records;
        }

        public IList<FeedRecords> getFeedRecordsBetween(string dariWaktu, string keWaktu)
        {
            return getFeedRecords("SELECT * FROM " + table_record_pakan + " WHERE waktu_input BETWEEN '" + dariWaktu + "' AND '" + keWaktu + "' ORDER BY waktu_input");
        }
        public Feed getFeedShortRecords()
        {
            Feed item = new Feed();

            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + table_beri_pakan, conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                item.beratPakan = (float)Convert.ToDouble(dataReader["berat_pakan"]);
                item.sisaPakan = Convert.ToInt32(dataReader["sisa_pakan"]);
                item.waktuPakan = Convert.ToString(dataReader["waktu_pakan"]);
                item.kondisi = Convert.ToInt32(dataReader["kondisi"]);
            }
            dataReader.Close();
            conn.Close();

            return item;
        }

        public int updatePakan(int kondisi, float berat_pakan, int sisa_pakan, string waktu_pakan)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("UPDATE " + table_beri_pakan + " SET kondisi = @kondisi, berat_pakan = @berat_pakan, sisa_pakan = @sisa_pakan, waktu_pakan = @waktu_pakan WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@kondisi", kondisi);
            cmd.Parameters.AddWithValue("@berat_pakan", berat_pakan);
            cmd.Parameters.AddWithValue("@sisa_pakan", sisa_pakan);
            cmd.Parameters.AddWithValue("@waktu_pakan", waktu_pakan);
            cmd.Parameters.AddWithValue("@id", 1);
            int affectedRow = cmd.ExecuteNonQuery();
            conn.Close();
            return affectedRow;
        }

        public int addFeedRecord(float berat_pakan, string waktu_pakan)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO " + table_record_pakan + " (berat_pakan, waktu_input) VALUES (@berat_pakan, @waktu_input)", conn);
            cmd.Parameters.AddWithValue("@berat_pakan", berat_pakan);
            cmd.Parameters.AddWithValue("@waktu_input", waktu_pakan);
            int affectedRow = cmd.ExecuteNonQuery();
            conn.Close();
            return affectedRow;
        }

        // Update Kondisi Pakan
        public int updateKondisi(int kondisi)
        {
            return execute("UPDATE " + table_beri_pakan + " SET kondisi = " + kondisi + " WHERE id = 1");
        }

        public string feedCheck()
        {
            string hasil = "";

            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + table_beri_pakan + " WHERE id = 1", conn);
            SqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                hasil = Convert.ToString(dataReader["kondisi"]);
            }
            dataReader.Close();
            conn.Close();
            return hasil;
        }
        public int execute(string query)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            int row_affected = cmd.ExecuteNonQuery();
            conn.Close();
            return row_affected;
        }
    }

    public class FeedRecords
    {
        public float beratPakan { get; set; }
        public string waktuPakan { get; set; }
    }

    public class Feed
    {
        public float beratPakan { get; set; }
        public int sisaPakan { get; set; }
        public string waktuPakan { get; set; }
        public int kondisi { get; set; }

    }
}