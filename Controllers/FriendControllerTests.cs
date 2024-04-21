using KozoskodoAPI.Controllers;
using KozoskodoAPI.Repo;
using KozoskodoAPI.Data;
using Moq;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace KozossegiAPI.UnitTests.FriendControllerTests
{
    /*
     * https://learn.microsoft.com/en-us/aspnet/web-api/overview/testing-and-debugging/unit-testing-controllers-in-web-api
     * https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-8.0
    */
    [TestFixture]
    public class FriendControllerTests
    {
        private Mock<DBContext> dbContext = new();

        private FriendController _friendControllerMock;
        private Mock<IFriendRepository> _friendRepositoryMock = new();
        private Mock<IPersonalRepository<Personal>> _personalRepositoryMock = new();
        private Mock<INotificationRepository> _notificationRepositoryMock = new();

        Friend expected = null;
        Notification receiverUserNotification = new()
        {
            ReceiverId = 1,
            SenderId = 2,
            notificationId = 1,
            notificationContent = "Ismerõsnek jelölt.",
            isNew = true            
        };
        List<Friend> baseFriendDb;
        Friend_notificationId friendRequest;
        

        [SetUp]
        public void Setup()
        {
            _friendControllerMock = FriendControllerMock.GetFriendControllerMock(_friendRepositoryMock, _personalRepositoryMock, _notificationRepositoryMock);
            dbContext = FriendControllerMock.GetDBContextMock();
        }


        [Test]
        public async Task GetAll_QueryFriendsReturnAllFriendsWhichContainsId()
        {
            var userId = 1;
            //Barátok
            var friends = new List<Personal>()
            {
                new Personal { 
                    id=1, firstName = "Gipsz", middleName = "Jakab", 
                    friends = new() { FriendshipID=1, UserId = 3, FriendId = userId, StatusId = 1}, 
                },
                new Personal { id=2, firstName = "John", lastName = "Doe",
                    friends = new() { FriendshipID = 2, UserId = userId, FriendId = 2, StatusId = 1},
                },
                new Personal { id=3, firstName = "Dog", lastName = "Cat",
                    friends = new() { FriendshipID = 3, UserId = 4, FriendId = userId, StatusId = 1},
                }
            };

            //Nem barát
            var testData = new List<Personal>()
            {
                new Personal {
                    id= 4, firstName = "Teszt", middleName = "Teszt",
                    friends = new() { FriendshipID=1, UserId = 4, FriendId = 5, StatusId = 1},
                },
            };

            _dbContextMock.Setup(x => x.Add(It.IsAny<Personal>())) //Foreach helyett
            .Callback((Personal item) =>
            {
                testData.Add(item);
                friends.Add(item);
            });

            var result = _friendRepositoryMock.Setup(m => m.GetAll(1).Result).Returns(friends); 

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task PostFriendRequest_SavesFriendshipIntoDatabase()
        {
            List<Personal> baseDb = new List<Personal>() { 
                new Personal { id = 1, firstName = "Teszt1", lastName = "Teszt1" },
                new Personal { id = 2, firstName = "Teszt2", lastName = "Teszt2" }
            };

            int receiverId = 1;
            int SenderId = 2;
            Notification parameter = new(receiverId, SenderId, NotificationType.FriendRequest);

            _personalRepositoryMock.Setup(repo => repo.Get(It.IsAny<int>()))
                .ReturnsAsync((int id) => baseDb.FirstOrDefault(person => person.id == id));
            _dbContextMock.Setup(x => x.Add(It.IsAny<Personal>())) //Foreach helyett
            .Callback((Personal item) =>
            {
                baseDb.Add(item);
            });

            var actionResult = await _friendControllerMock.postFriendRequest(parameter);
            
            var okResult = actionResult as OkObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        [Test]
        public async Task PostFriendRequest_ShouldNotSendTheNotificationToRequestedUser()
        {
            List<Personal> baseDb = new List<Personal>() {
                new Personal { id = 1, firstName = "Teszt1", lastName = "Teszt1" },
                new Personal { id = 2, firstName = "Teszt2", lastName = "Teszt2" }
            };
            //Arrange
            _personalRepositoryMock.Setup(repo => repo.Get(It.IsAny<int>()));
            _dbContextMock.Setup(x => x.Add(It.IsAny<Personal>())) //Foreach helyett
            .Callback((Personal item) =>
            {
                baseDb.Add(item);
            });

            int receiverId = 3;
            int SenderId = 2;
            Notification parameter = new(receiverId, SenderId, NotificationType.FriendRequest);
            var mockSet = new Mock<DbSet<Personal>>();

            //Act
            var actionResult = await _friendControllerMock.postFriendRequest(parameter);
            var nocontentResult = actionResult as NoContentResult;

            //Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<NoContentResult>(actionResult);
            Assert.AreEqual(StatusCodes.Status204NoContent, nocontentResult.StatusCode);
        }

        [Test]
        public async Task Delete_RemoveFriendshipFromDatabase()
        {
            //Arrange
            List<Friend> friends = new List<Friend>()
            {
                new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1 },
                new() { FriendshipID = 1, UserId = 2, FriendId = 1, StatusId = 1 },
                new() { FriendshipID = 1, UserId = 1, FriendId = 2, StatusId = 1 },
                new() { FriendshipID = 1, UserId = 4, FriendId = 5, StatusId = 1 }
            };
            var mockSet = new Mock<DbSet<Friend>>();
            _dbContextMock.Setup(m => m.Friendship).Returns(mockSet.Object);
            Friend friendshipToRemove = new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1 };
            //Act
            var actionResult = _friendControllerMock.Delete(friendshipToRemove);
            //Assert
            Assert.That(!friends.Contains(friendshipToRemove));
        }

        [Test]
        public async Task Put_FriendRequestReceivedNeverBeforeWasRequested_ShouldSaveFriendshipIntoDBAndSendNotificationToRequester()
        {
            //Arrange
            _dbContextMock.Setup(x => x.Add(It.IsAny<Friend>()))
            .Callback((Friend item) =>
            {
                baseFriendDb.Add(item);
            });

            //Act
            var result = _friendControllerMock.Put(friendRequest);

            //Asert
            var okResult = result.Result as OkObjectResult;
            Notification? resultContent = okResult?.Value as Notification;
            string expected = "Mostantól ismerõsök vagytok.";

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
            Assert.That(expected, Is.EqualTo(resultContent.notificationContent));
        }

        [Test]
        public async Task Put_FriendRequestReceivedButItWasRejected_ShouldOnlySendNotificationToRequester()
        {
            //Arrange
            _dbContextMock.Setup(x => x.Add(It.IsAny<Friend>()))
            .Callback((Friend item) =>
            {
                baseFriendDb.Add(item);
            });
            friendRequest.StatusId = 4; //Rejected friend request

            //Act
            var result = _friendControllerMock.Put(friendRequest);
            //Asert

            var okResult = result.Result as OkObjectResult;
            Notification? resultContent = okResult?.Value as Notification;

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
            Assert.That(resultContent.notificationContent.Contains("elutasítva".ToLower()));
        }

        [Test]
        public async Task Put_NotificationDoesntExist()
        {
            //Arrange
            _dbContextMock.Setup(x => x.Add(It.IsAny<Friend>()))
            .Callback((Friend item) =>
            {
                baseFriendDb.Add(item);
            });

            Notification expected = new();
            //_notificationRepositoryMock.Setup(repo => repo.GetNotification(It.IsAny<Friend_notificationId>()).Result).Returns(expected);
            _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync<Notification>(It.IsAny<int>()).Result).Returns(expected);
            friendRequest.StatusId = 1;
            friendRequest.NotificationId = 0;
            
            //Act
            var result = _friendControllerMock.Put(friendRequest);
            
            //Asert
            var nullResult = result.Result;
            Notification? resultContent = nullResult as Notification;
            var okResult = result.Result as OkObjectResult;

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }
    }
}