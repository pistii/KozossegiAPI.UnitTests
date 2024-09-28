using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class PersonalData
    {
        public static IEnumerable<Personal> GetUsers()
        {
            var users = new List<Personal>() {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1988-12-10"),
                    PlaceOfResidence = "Columbia",
                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1956-10-10"),
                    PlaceOfResidence = "Budapest",
                },
                new Personal()
                {
                    id = 3,
                    firstName = "Kiwikamaho",
                    lastName = "Hujahou",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1987-10-10"),
                    PlaceOfResidence = "Hawaii",
                },
                 new Personal()
                {
                    id = 4,
                    firstName = "Albatrosz",
                    lastName = "Aladin",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1910-10-10"),
                    PlaceOfResidence = "Missisippi",
                },
                new Personal()
                {
                    id = 5,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = false,
                },
                new Personal()
                {
                    id = 6,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = false,
                },
                
                }.AsEnumerable();
            return users;
        }


    }
}
