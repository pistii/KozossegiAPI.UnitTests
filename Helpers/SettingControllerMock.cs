using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Helpers
{
    internal class SettingControllerMock
    {
        public static Mock<DBContext> GetDBContextMock()
        {
            //Tesztadatok előkészítése
            List<user> users = GetUsers();
            List<Personal> personals = GetPersonals();
            List<Settings> settings = GetSettings();
            List<Study> studies = GetStudies();


            var dbContext = new Mock<DBContext>();

            var userMockSet = MockDbSetFactory.Create<user>(users);
            var personalMockSet = MockDbSetFactory.Create<Personal>(personals);
            var settingsMockSet = MockDbSetFactory.Create<Settings>(settings);
            var studiesMockSet = MockDbSetFactory.Create<Study>(studies);

            dbContext.Setup(x => x.user).Returns(userMockSet);
            dbContext.Setup(x => x.Personal).Returns(personalMockSet);
            dbContext.Setup(x => x.Settings).Returns(settingsMockSet);
            dbContext.Setup(x => x.Study).Returns(studiesMockSet);
            return dbContext;
        }

        private static List<user> GetUsers()
        {
            var users = new List<user>()
            {
                new user()
                {
                    userID = 1,
                    email = "test1",
                    isActivated = true,
                    password = "$2y$10$kQtlrV3z1zWJ4YvuHQtZhO/8STD9oZvvb89KF9yEI021GqkKjn7mm",
                    SecondaryEmailAddress = "test1.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
                new user()
                {
                    userID = 2,
                    email = "test2",
                    isActivated = true,
                    password = "fakepassword2",
                    SecondaryEmailAddress = "test2.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
                new user()
                {
                    userID = 3,
                    email = "test3",
                    isActivated = true,
                    password = "fakepassword3",
                    SecondaryEmailAddress = "test3.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
            };
            return users;
        }
        public static List<Personal> GetPersonals()
        {
            List<Personal> personals = new List<Personal>()
            {
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
            };
            return personals;
        }
        public static List<Study> GetStudies()
        {
            List<Study> studies = new List<Study>()
            {
                new Study()
                {
                    Class = "class1",
                    EndYear = 2012,
                    StartYear = 2008,
                    FK_UserId = 1,
                    SchoolName = "School"
                },
                new Study()
                {
                    Class = "class2",
                    StartYear = 2013,
                    EndYear = 2016,
                    FK_UserId = 1,
                    SchoolName = "School"
                },
                new Study()
                {
                    Class = "class3",
                    StartYear = 2013,
                    EndYear = 2016,
                    FK_UserId = 2,
                    SchoolName = "School"
                }
            };
            return studies;
        }
        public static List<Settings> GetSettings()
        {
            List<Settings> settings = new List<Settings>()
            {
                new Settings()
                {
                    FK_UserId = 1,
                    NextReminder = DateTime.Now.AddMinutes(1)
                },
                new Settings()
                {
                    FK_UserId = 2,
                    NextReminder = DateTime.Now.AddMinutes(999)
                }
            };
            return settings;
        }

    }
}
