using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Agent
{
    public class DB {

        SQLiteConnection connection = null;
        public DB(string dbpath)
        {
            this.connection = DB.GetInstance(dbpath);
        }
        public static SQLiteConnection GetInstance(string dbpath)
        {
            string path = Environment.ExpandEnvironmentVariables(dbpath);

            bool dbexists = true;

            if (!File.Exists(Path.GetFullPath(path)))
            {
                SQLiteConnection.CreateFile(path);
                dbexists = false;
            }

            SQLiteConnection connection = new SQLiteConnection(
                string.Format("Data Source={0};", path)
            );

            connection.Open();

            if (!dbexists)
            {
                SQLiteCommand cmd = new SQLiteCommand(@"
                    CREATE TABLE CREDENTIALS (
                        ID INTEGER PRIMARY KEY,
                        USERNAME TEXT NOT NULL,
                        PASSWORD TEXT NOT NULL,
                        TIMESTAMP DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                ", connection);
                cmd.ExecuteNonQuery();
            }

            return connection;
        }

        public void Add(Credentials credentials)
        {
            SQLiteCommand cmd = new SQLiteCommand(@"
                INSERT INTO CREDENTIALS (username, password) VALUES (?, ?);
            ", connection);
            cmd.Parameters.Add(new SQLiteParameter("username", credentials.Username));
            cmd.Parameters.Add(new SQLiteParameter("password", credentials.Password));
            cmd.ExecuteNonQuery();
        }

        public void Remove(Credentials credentials)
        {
            SQLiteCommand cmd = new SQLiteCommand(@"
                DELETE FROM CREDENTIALS WHERE id = ?
            ", connection);
            cmd.Parameters.Add(new SQLiteParameter("id", credentials.Id));
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<Credentials> GetAll()
        {
            List<Credentials> credentials = new List<Credentials>();

            SQLiteCommand cmd = new SQLiteCommand(@"
                SELECT id, username, password, timestamp
                  FROM CREDENTIALS
                 ORDER BY timestamp ASC;
            ", connection);
            
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {

                    credentials.Add(new Credentials(
                        Convert.ToInt64(reader["id"].ToString()),
                        reader["username"].ToString(),
                        reader["password"].ToString(),
                        reader["timestamp"].ToString()
                    ));
                }
            }

            return credentials;
        }
    }
}