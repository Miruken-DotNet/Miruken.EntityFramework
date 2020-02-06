namespace Miruken.EntityFramework.Tests.Domain
{
    using System.Collections.Generic;

    public class Team
    {
        public int          Id      { get; set; }
        public string       Name    { get; set; }
        public Person       Coach   { get; set; }
        public List<Person> Players { get; set; } = new List<Person>();
    }
}
