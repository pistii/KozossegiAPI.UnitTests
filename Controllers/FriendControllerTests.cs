using KozoskodoAPI.Controllers;
using KozoskodoAPI.Repo;
using KozoskodoAPI.Data;
using Moq;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KozossegiAPI.UnitTests.Helpers;

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
        private Mock<IPersonalRepository> _personalRepositoryMock = new();
        private Mock<INotificationRepository> _notificationRepositoryMock = new();

        [SetUp]
        public void Setup()
        {
            _friendRepositoryMock = new();
            _personalRepositoryMock = new();
            _notificationRepositoryMock = new();
            _friendControllerMock = FriendControllerMock.GetFriendControllerMock(_friendRepositoryMock, _personalRepositoryMock, _notificationRepositoryMock);
            dbContext = FriendControllerMock.GetDBContextMock();
        }

        
        [Test]
        [TestCase(1)]
        public async Task GetAll_QueryFriendsReturnAllFriendsWhichContainsId(int userId)
        {
            var objToReturn = dbContext.Object.Personal.Where(f => f.id == 1 || f.id == 2).AsEnumerable();
            _friendRepositoryMock.Setup(m => m.GetAll(It.IsAny<int>())).ReturnsAsync(objToReturn);

            var result = _friendControllerMock.GetAll(userId);

            var okResult = result.Result as OkObjectResult;
            IEnumerable<Personal> persons = okResult.Value as IEnumerable<Personal>;
            Assert.That(persons.Count, Is.GreaterThan(0));
            Assert.That(persons.Count, Is.EqualTo(2));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [TestCase(1, 2)]
        public async Task PostFriendRequest_SavesFriendshipIntoDatabase(int receiverId, int SenderId)
        {
            //2-es id "Teszt" küld baráti kérelmet 1-es id "Gipsz" felhasználónak.
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
        [TestCase(3, 2)]
        [Ignore("Notification is not updated as existing entity. Doesn't step into generic class")]
        public async Task PostFriendRequest_PreviousFriendRequestExists_ShouldOverwritePreviousOne(int receiverId, int SenderId)
        {
            var sender = dbContext.Object.Personal.First(x => x.id == SenderId);
            Personal receiver = dbContext.Object.Personal.First(x => x.id == receiverId);
            Notification existingNotificationOfRequested = new(3, 2, NotificationType.FriendRequest)
            {
                notificationId = 1,
                notificationContent = "ismerõsnek jelölt",
                notificationType = NotificationType.FriendRequest,
                isNew = false,
                createdAt = DateTime.Parse("2020-10-12 10:16")
            };
            receiver.Notifications.Add(existingNotificationOfRequested);

            Notification previousRequest = new(receiverId, SenderId, NotificationType.FriendRequest);

            _personalRepositoryMock.Setup(repo => repo.GetByIdAsync<Personal>(It.Is<int>(id => id == SenderId)))
                .ReturnsAsync(sender);
            _friendRepositoryMock.Setup(repo => repo.GetUserWithNotification(It.Is<int>(id => id == receiverId)))
                .ReturnsAsync(receiver);
            _notificationRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(existingNotificationOfRequested));

            //Act
            var actionResult = await _friendControllerMock.postFriendRequest(previousRequest);
            var okResult = actionResult as OkObjectResult;

            _notificationRepositoryMock.Verify(d => d.UpdateThenSaveAsync(It.IsAny<Notification>()), Times.Once());
            Assert.That(dbContext.Object.Notification.Count, Is.EqualTo(1));
            Assert.That(dbContext.Object.Notification.FirstOrDefault().createdAt, Is.Not.EqualTo(DateTime.Parse("2020-10-12 10:16")));
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        
        [Test]
        public async Task Delete_RemoveFriendshipFromDatabase()
        {
            //Arrange
            var dbContext = FriendControllerMock.GetDBContextMock();


            Friend friendshipToRemove = new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1 };

            _friendRepositoryMock.Setup(repo => repo.FriendshipExists(It.IsAny<Friend>())).ReturnsAsync(friendshipToRemove);

            //Act
            var actionResult = await _friendControllerMock.Delete(friendshipToRemove);
            var okResult = actionResult as OkObjectResult;

            //Assert
            Assert.That(!dbContext.Object.Friendship.Contains(friendshipToRemove));

            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }


        [Test]
        [TestCase(1, 4)]
        public async Task Put_FriendRequestReceivedNeverBeforeWasRequested_ShouldSaveFriendshipIntoDBAndSendNotificationToRequester(int receiverId, int senderId)
        {
            Friend_notificationId friendshipObject = new();
            friendshipObject.NotificationId = 1;
            friendshipObject.FriendId = senderId;
            friendshipObject.UserId = receiverId;
            friendshipObject.StatusId = 1;
            friendshipObject.FriendshipID = 1;
            Friend friend = friendshipObject;

            //The notification sent to the requested user
            Notification notification = new(3, 2, NotificationType.FriendRequest)
            {
                notificationId = 1,
                notificationContent = "ismerõsnek jelölt",
                notificationType = NotificationType.FriendRequest,
                isNew = false,
                createdAt = DateTime.Parse("2020-10-12 10:16")
            };
            string expected = "Mostantól ismerõsök vagytok.";


            _friendRepositoryMock.Setup(repo => repo.FriendshipExists(It.IsAny<Friend_notificationId>())).ReturnsAsync(friendshipObject);
            _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync<Notification>(It.IsAny<int>())).ReturnsAsync(notification);
            _friendRepositoryMock.Setup(repo => repo.SaveAsync());

            //Act
            var result = await _friendControllerMock.Put(friendshipObject);

            _friendRepositoryMock.Verify(d => d.SaveAsync(), Times.Once());

            //Asert
            var okResult = result as OkObjectResult;
            Notification? resultContent = okResult?.Value as Notification;
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
                notificationContent = "ismerõsnek jelölt",
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