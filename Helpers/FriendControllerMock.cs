using KozoskodoAPI.Controllers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Helpers
{
    public class FriendControllerMock
    {
        //Test Data
        public static IQueryable<Personal> GetPersonals()
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
            return personals.AsQueryable();
        }
        public static IQueryable<Friend> GetFriends() {
            var friendTable = new List<Friend>()
            {
                new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1}, //1-es 3-as kapcsolat
                new() { FriendshipID = 2, UserId = 3, FriendId = 2, StatusId = 1}, //2-es 3-as kapcsolat
                new() { FriendshipID = 3, UserId = 3, FriendId = 1, StatusId = 3}, //1-es barátnak jelölte 3-ast
                new() { FriendshipID = 4, UserId = 2, FriendId = 1, StatusId = 1}, //2-es 1-es kapcsolat
            }.AsQueryable();
            return friendTable;
        }
        public static IQueryable<Notification> GetNotifications()
        {
            var notifications = new List<Notification>()
            {
                new(3, 2, NotificationType.FriendRequest)
                {
                    notificationId = 1,
                    notificationContent = "ismerősnek jelölt",
                    notificationType = NotificationType.FriendRequest,
                    isNew = false,
                    createdAt = DateTime.Parse("2020-10-12 10:16")
                }
            }.AsQueryable();
            return notifications;
        }

        //Tested Controller's interfaces
        private static Mock<IHubContext<NotificationHub, INotificationClient>> _notificationHub = new();
        private static Mock<IMapConnections> _connections = new();

        public static FriendController GetFriendControllerMock(Mock<IFriendRepository> friendRepository, Mock<IPersonalRepository> personalRepository, Mock<INotificationRepository> notificationRepository)
        {
            var friendController = new FriendController(
                friendRepository.Object,
                personalRepository.Object,
                notificationRepository.Object,
                _notificationHub.Object,
                _connections.Object
                );
            return friendController;
        }

        public static Mock<DBContext> GetDBContextMock()
        {
            //Tesztadatok előkészítése
            List<Personal> personal = GetPersonals().ToList();
            List<Friend> friendTable = GetFriends().ToList();
            List<Notification> notifications = GetNotifications().ToList();

            var dbContext = new Mock<DBContext>();

            var personalMockSet = MockDbSetFactory.Create<Personal>(personal);
            var friendMockSet = MockDbSetFactory.Create<Friend>(friendTable);
            var notificationMock = MockDbSetFactory.Create<Notification>(notifications);

            dbContext.Setup(x => x.Personal).Returns(personalMockSet);
            dbContext.Setup(x => x.Friendship).Returns(friendMockSet);
            dbContext.Setup(x => x.Notification).Returns(notificationMock);

            return dbContext;
        }
    }
}
