namespace Agent
{
    public class Credentials
    {

        public Credentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public Credentials(long id, string username, string password, string timestamp) : this(username, password)
        {
            this.Id = id;
            this.Timestamp = timestamp;
        }

        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Timestamp { get; set; }
    }
}