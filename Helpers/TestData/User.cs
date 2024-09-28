
using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class UserData
    {
        public static IQueryable<user> GetUsers()
        {
            List<user> users = new List<user>();

            var user1 = new user()
            {
                userID = 1,
                email = "test1",
                isActivated = true,
                password = "$2y$10$kQtlrV3z1zWJ4YvuHQtZhO/8STD9oZvvb89KF9yEI021GqkKjn7mm",
                SecondaryEmailAddress = "test1@test.test",
                LastOnline = DateTime.Now.AddMinutes(-10)
            };
            var user2 = new user()
            {
                userID = 2,
                email = "test2",
                isActivated = true,
                password = "fakepassword2",
                SecondaryEmailAddress = "test2.2",
                LastOnline = DateTime.Now.AddMinutes(-10)
            };
            var user3 = new user()
            {
                userID = 3,
                email = "test3",
                isActivated = true,
                password = "fakepassword3",
                SecondaryEmailAddress = "test3.2",
                LastOnline = DateTime.Now.AddMinutes(-10)
            };
            users.Add(user1); 
            users.Add(user2); 
            users.Add(user3);

            var i = 4;
            while (i < 15)
            {
                var user = new user()
                {
                    userID = i,
                    email = $"test{i}@test.com",
                    isActivated = true,
                    password = "password",                    
                };
                users.Add(user);
                i++;
            }


            return users.AsQueryable();
        }
    }
}
