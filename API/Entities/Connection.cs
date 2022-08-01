namespace API.Entities
{
    public class Connection
    {
        // Because this is an entity, we'll give entity framework a default constructor as well,
        // which it needs, otherwise we'll probably see an error. So we'll just generate a default constructor with no parameters.
        public Connection()
        {
        }

        // To make this class a little bit easier to use, we'll just add a constructor, when
        // we create a new instance of connection, we just need to open parentheses and pass in these two properties.
        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
        }

        // By convention, if we give it the name and then id, entity framework is going to automatically consider this the primary key.
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}