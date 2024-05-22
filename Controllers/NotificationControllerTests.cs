using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KozossegiAPI.UnitTests.Controllers
{
    internal class NotificationControllerTests
    {
        public Mock<INotificationRepository> notificationRepository;
        public NotificationController controller;
        public static IQueryable<NotificationWithAvatarDto> GetNotifications() {
            List<NotificationWithAvatarDto> notifications = new()
            {
                new NotificationWithAvatarDto()
                {
                    ReceiverId = 1,
                    isNew = true,
                    createdAt = DateTime.Now,
                    notificationContent = "Be my friend test",
                    SenderId = 1,
                    notificationId = 1,
                    notificationType = NotificationType.FriendRequest
                },
                new NotificationWithAvatarDto()
                {
                    ReceiverId = 1,
                    isNew = true,
                    createdAt = DateTime.Now,
                    notificationContent = "Be my friend test",
                    SenderId = 2,
                    notificationId = 2,
                    notificationType = NotificationType.FriendRequest
                },
                new NotificationWithAvatarDto()
                {
                    ReceiverId = 1,
                    isNew = true,
                    createdAt = DateTime.Now,
                    notificationContent = "Be my friend test",
                    SenderId = 3,
                    notificationId = 3,
                    notificationType = NotificationType.FriendRequest
                },
                new NotificationWithAvatarDto()
                {
                    ReceiverId = 1,
                    isNew = true,
                    createdAt = DateTime.Now,
                    notificationContent = "Be my friend test",
                    SenderId = 4,
                    notificationId = 4,
                    notificationType = NotificationType.FriendRequest
                },
                new NotificationWithAvatarDto()
                {
                    ReceiverId = 1,
                    isNew = true,
                    createdAt = DateTime.Now,
                    notificationContent = "Be my friend test",
                    SenderId = 5,
                    notificationId = 5,
                    notificationType = NotificationType.FriendRequest
                }
            };

            return notifications.AsQueryable();
        }
        
        [SetUp]
        public void Setup()
        {
            notificationRepository = new();
            controller = new(notificationRepository.Object);
        }

        [Test]
        public async Task GetAll_ReturnsAllUserSpecificNotification_AsOkObjectResult()
        {
            //Returns the notification to user by receiverId.
            //Since the GetNotifications contains all the receiverId = 1, should return all the items.
            var notifications = GetNotifications().ToList();
            notificationRepository.Setup(repo => repo.GetAll_PersonNotifications(It.IsAny<int>())).ReturnsAsync(notifications);

            var result = await controller.GetAll(1);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            notificationRepository.VerifyAll();
        }

        [Test]
        public async Task NotificationReaded_MarkNotificationWithGivenIdAsNotNew()
        {
            var notification = GetNotifications().First();
            notificationRepository.Setup(repo => repo.GetByIdAsync<Notification>(It.IsAny<int>())).ReturnsAsync(notification);
            notificationRepository.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<Notification>()));

            var result = await controller.NotificationRead(1);

            var okResult = result as OkResult;
            Assert.That(notification.isNew, Is.False);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            notificationRepository.VerifyAll();
        }
    }
}
