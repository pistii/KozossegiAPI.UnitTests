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
        [TestCase(1, 2)]
        public async Task PostFriendRequest_SavesFriendshipIntoDatabase(int receiverId, int SenderId)
        {
            //2-es id "Teszt" kÃ¼ld barÃ¡ti kÃ©relmet 1-es id "Gipsz" felhasznÃ¡lÃ³nak.
            Notification parameter = new(receiverId, SenderId, NotificationType.FriendRequest);

            var sender = dbContext.Object.Personal.First(x => x.id == SenderId);
            var receiver = dbContext.Object.Personal.First(x => x.id == receiverId);


            _personalRepositoryMock.Setup(repo => repo.GetByIdAsync<Personal>(It.Is<int>(id => id == parameter.SenderId)))
                .ReturnsAsync(sender);
            _friendRepositoryMock.Setup(repo => repo.GetUserWithNotification(It.Is<int>(id => id == parameter.ReceiverId)))
                .ReturnsAsync(receiver);

            var actionResult = await _friendControllerMock.postFriendRequest(parameter);
            
            var okResult = actionResult as OkObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        [Test]
        [TestCase(3, 2)]
        public async Task PostFriendRequest_ShouldNotSendTheNotificationToRequestedUser_BecauseUserCannotBeFound(int receiverId, int SenderId)
        {
            //Arrange
            var sender = dbContext.Object.Personal.First(x => x.id == SenderId);
            Personal receiver = null;
            Notification parameter = new(receiverId, SenderId, NotificationType.FriendRequest);

            _personalRepositoryMock.Setup(repo => repo.GetByIdAsync<Personal>(It.Is<int>(id => id == SenderId)))
                .ReturnsAsync(sender);
            _friendRepositoryMock.Setup(repo => repo.GetUserWithNotification(It.Is<int>(id => id == receiverId)))
                .ReturnsAsync(receiver);


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
            var dbContext = FriendControllerMock.GetDBContextMock();


            Friend friendshipToRemove = new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1 };
            //Act
            var actionResult = await _friendControllerMock.Delete(friendshipToRemove);
            var okResult = actionResult as OkObjectResult;

            //Assert
            Assert.That(!dbContext.Object.Friendship.Contains(friendshipToRemove));

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
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
        [TestCase(3, 1)]
        public async Task Put_FriendRequestReceivedButItWasRejected_ShouldOnlySendNotificationToRequester(int receiverId, int senderId)
        {
            //Arrange
            Friend_notificationId friend_NotificationId = new()
            {
                NotificationId = 1,
                FriendId = senderId,
                UserId = receiverId,
                StatusId = 4,
                FriendshipID = 1,
            };

            Notification notification = new(3, 2, NotificationType.FriendRequest)
            {
                notificationId = 1,
                notificationContent = "ismerÅ‘snek jelÃ¶lt",
                notificationType = NotificationType.FriendRequest,
                isNew = false,
                createdAt = DateTime.Parse("2020-10-12 10:16")
            };

            _friendRepositoryMock.Setup(repo => repo.FriendshipExists(It.IsAny<Friend_notificationId>())).ReturnsAsync(friend_NotificationId);
            _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync<Notification>(It.IsAny<int>())).ReturnsAsync(notification);
            _notificationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Notification>()));
            _friendRepositoryMock.Setup(repo => repo.SaveAsync());
            _friendRepositoryMock.Setup(repo => repo.Delete(friend_NotificationId));


            //Act
            var result = _friendControllerMock.Put(friend_NotificationId);

            //Assert

            _notificationRepositoryMock.Verify(d => d.UpdateAsync(It.IsAny<Notification>()), Times.Once());
            _friendRepositoryMock.Verify(d => d.Delete(It.IsAny<Friend_notificationId>()), Times.Once());
            _friendRepositoryMock.Verify(d => d.SaveAsync(), Times.Once());

            var okResult = result.Result as OkObjectResult;
            Notification? resultContent = okResult?.Value as Notification;

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
            Assert.That(resultContent.notificationContent.Contains("elutasítva".ToLower()));
        }

        [Test]
        [TestCase(3, 1)]
        public async Task Put_NotificationDoesntExist_ShouldOnlySaveFriendshipWithoutNotification(int receiverId, int senderId)
        {
            //Arrange
            Friend_notificationId friend_NotificationId = new()
            {
                NotificationId = 1,
                FriendId = senderId,
                UserId = receiverId,
                StatusId = 1,
                FriendshipID = 1,
            };


            _friendRepositoryMock.Setup(repo => repo.FriendshipExists(It.IsAny<Friend_notificationId>())).ReturnsAsync(friend_NotificationId);
            _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync<Notification>(It.IsAny<int>())).ReturnsAsync((Notification)null);
            _friendRepositoryMock.Setup(repo => repo.SaveAsync());
            
            //Act
            var result = _friendControllerMock.Put(friend_NotificationId);

            //Assert

            _notificationRepositoryMock.Verify(d => d.UpdateAsync(It.IsAny<Notification>()), Times.Never());
            _friendRepositoryMock.Verify(d => d.SaveAsync(), Times.Once());
            
            var okResult = result.Result as OkObjectResult;
            Notification? resultContent = okResult?.Value as Notification;

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
            Assert.That(resultContent, Is.EqualTo(null));
        }
    }
}