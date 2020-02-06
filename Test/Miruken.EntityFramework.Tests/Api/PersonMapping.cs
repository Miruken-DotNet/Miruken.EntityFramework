namespace Miruken.EntityFramework.Tests.Api
{
    using Callback;
    using Domain;
    using Map;

    public class PersonMapping : Handler
    {
        [Maps]
        public static PersonData Map(
            Person     entity,
            PersonData data)
        {
            data ??= new PersonData();
            data.Id          = entity.Id;
            data.FirstName   = entity.FirstName;
            data.LastName    = entity.LastName;
            data.DateOfBirth = entity.DateOfBirth;
            return data;
        }

        [Maps]
        public static Person Map(
            PersonData data,
            Person     entity)
        {
            entity ??= new Person();

            if (data.FirstName != null)
                entity.FirstName = data.FirstName;

            if (data.LastName != null)
                entity.LastName = data.LastName;

            if (data.DateOfBirth.HasValue)
                entity.DateOfBirth = data.DateOfBirth.Value;

            return entity;
        }
    }
}
