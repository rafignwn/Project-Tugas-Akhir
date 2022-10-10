using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Web_ASP.Models
{
    public class DbUser
    {
        protected SqlConnection conn = new SqlConnection
        {
            ConnectionString = Startup.ConnectionString
        };

        private const string TABLE_USERS = "users";

        public string RefreshTokenCheck(string username)
        {
            string token = "";
            conn.Open();
            SqlCommand sql = new SqlCommand($"SELECT refreshToken FROM {TABLE_USERS} WHERE username = '{username}'", conn);
            SqlDataReader dataReader = sql.ExecuteReader();
            while (dataReader.Read())
            {
                token = Convert.ToString(dataReader["refreshToken"]);
            }
            dataReader.Close();
            conn.Close();

            return token;
        }

        public UserModel CheckUser(string Username)
        {
            conn.Open();
            UserModel User = new UserModel();

            SqlCommand sql = new SqlCommand($"SELECT * FROM {TABLE_USERS} WHERE username = @username", conn);
            sql.Parameters.AddWithValue("@username", Username);
            SqlDataReader dataReader = sql.ExecuteReader();
            while (dataReader.Read())
            {
                User.Username = Convert.ToString(dataReader["username"]);
                User.EmailAddress = Convert.ToString(dataReader["email"]);
                User.Role = Convert.ToString(dataReader["role"]);
                User.Password = Convert.ToString(dataReader["password"]);
            }
            dataReader.Close();
            conn.Close();

            return User;
        }

        public UserModel GetUserWithRefreshToken(string refreshToken)
        {
            conn.Open();
            UserModel User = new UserModel();

            try
            {
                SqlCommand sql = new SqlCommand($"SELECT * FROM {TABLE_USERS} WHERE substring([refreshToken], 1, 4096) = @Original_refreshToken", conn);
                sql.Parameters.AddWithValue("@Original_refreshToken", refreshToken);
                SqlDataReader dataReader = sql.ExecuteReader();
                while (dataReader.Read())
                {
                    User.Username = Convert.ToString(dataReader["username"]);
                    User.EmailAddress = Convert.ToString(dataReader["email"]);
                    User.Role = Convert.ToString(dataReader["role"]);
                    User.Password = Convert.ToString(dataReader["password"]);
                }
                dataReader.Close();
            }
            catch (System.Exception)
            {
                return null;
            }
            conn.Close();

            return User;
        }

        public int DestroyRefreshToken(string username)
        {
            conn.Open();

            SqlCommand sql = new SqlCommand($"UPDATE {TABLE_USERS} SET refreshToken = '' WHERE username = @username", conn);
            sql.Parameters.AddWithValue("@username", username);
            int affectedRow = sql.ExecuteNonQuery();
            conn.Close();

            return affectedRow;
        }

        public int UpdateRefreshToken(string Username, string RefreshToken)
        {
            conn.Open();

            SqlCommand sql = new SqlCommand($"UPDATE {TABLE_USERS} SET refreshToken = @refreshToken WHERE username = @username", conn);
            sql.Parameters.AddWithValue("@refreshToken", RefreshToken);
            sql.Parameters.AddWithValue("@username", Username);
            int affectedRow = sql.ExecuteNonQuery();
            conn.Close();

            return affectedRow;
        }

        public int AddUser(UserModel newUser)
        {
            try
            {
                conn.Open();

                SqlCommand sql = new SqlCommand($"INSERT INTO {TABLE_USERS} (fname, lname, username, email, role, password) VALUES (@Original_fname, @Original_lname, @username, @email, @role, @password)", conn);
                sql.Parameters.AddWithValue("@Original_fname", newUser.Fname);
                sql.Parameters.AddWithValue("@Original_lname", newUser.Lname);
                sql.Parameters.AddWithValue("@username", newUser.Username);
                sql.Parameters.AddWithValue("@email", newUser.EmailAddress);
                sql.Parameters.AddWithValue("@role", newUser.Role);
                sql.Parameters.AddWithValue("@password", Cryptography.Encrypt(newUser.Password));
                int affectedRow = sql.ExecuteNonQuery();

                conn.Close();
                return affectedRow;
            }
            catch (SqlException)
            {
                return 0;
            }
        }
    }
}

// user petani
/**
*{
    "Fname": "Uncle",
    "Lname": "Raju",
    "Username": "opetlarilari",
    "EmailAddress": "opetdahbesar@gmail.com",
    "Role": "petani",
    "Password": "sepimerajuk"
}
*/