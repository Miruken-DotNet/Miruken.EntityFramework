namespace Miruken.EntityFramework.Tests.Api
{
    using System;

    public class PersonData
    {
        public int?      Id          { get; set; }
        public string    FirstName   { get; set; }
        public string    LastName    { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
