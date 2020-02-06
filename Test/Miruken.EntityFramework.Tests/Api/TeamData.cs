namespace Miruken.EntityFramework.Tests.Api
{
    public class TeamData
    {
        public int?         Id      { get; set; }
        public string       Name    { get; set; }
        public PersonData   Coach   { get; set; }
        public PersonData[] Players { get; set; }
    }
}
