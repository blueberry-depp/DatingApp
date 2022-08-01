using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Group
    {
        // When it creates the tables, it needs an empty constructor for this.
        public Group()
        {
        }

        // We don't need the collections in there, but we'll just allow ourselves to initialize this with the name of the group. 
        public Group(string name)
        {
            Name = name;
        }

        // This is the only property that we'll have as our key. This is also going to be primary key. We don't want duplicate groups in database. It'll
        // also index lists as well so that it makes it easier for the entity framework to find this particular entity.
        [Key]
        public string Name { get; set; }
        // The reason for initializing new List<Connection>() here is that when we create a new group, we automatically want a
        // new list inside there so we can just add the connection.
        public ICollection<Connection> Connections  { get; set; } = new List<Connection>();
    }
}
