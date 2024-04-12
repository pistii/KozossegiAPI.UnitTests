using FirebaseAdmin.Auth;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    public class NotificationRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IFriendRepository _friendRepository;
        private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hubContextMock = new();
        private Mock<IMapConnections> _connections = new();
        private NotificationRepository notificationRepository;
        private DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            
            _serviceProvider = services.BuildServiceProvider();

            
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();
            dbContext.Database.EnsureDeleted();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _friendRepository = scopedServices.GetRequiredService<IFriendRepository>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
            notificationRepository = new NotificationRepository(
                _dbContext,
                _friendRepository,
                _hubContextMock.Object,
                _connections.Object
            );
        }

        public static List<Personal> GetUsers()
        {
            var users = new List<Personal>() {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = true,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = false,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
                },
                new Personal()
                {
                    id = 3,
                    firstName = "Teszt1",
                    lastName = "Elek1",
                    isMale = true,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
                }
                }.ToList();
            return users;
        }

        public static IEnumerable<Friend> GetFriends()
        {
            var friends = new List<Friend>()
            {
                new Friend()
                {
                    FriendshipID = 1,
                    FriendId = 1,
                    UserId = 2,
                    StatusId = 1,
                },
                new Friend()
                {
                    FriendshipID = 2,
                    FriendId = 3,
                    UserId = 2,
                    StatusId = 1
                }
            };
            return friends;
        }

        public static IEnumerable<Notification> GetNotifications_OlderThan30Days()
        {
            var notifications = new List<Notification>()
            {
                new Notification()
                {
                    createdAt = DateTime.Now.AddDays(-30),
                    notificationId = 1,
                    notificationType = NotificationType.FriendRequestAccepted
                },
                new Notification()
                {
                    createdAt = DateTime.Now.AddDays(-29),
                    notificationId = 2,
                    notificationType = NotificationType.FriendRequestAccepted
                },
                new Notification()
                {
                    createdAt = DateTime.Now.AddDays(-31),
                    notificationId = 3,
                    notificationType = NotificationType.FriendRequestAccepted
                },
                new Notification()
                {
                    createdAt = DateTime.Now.AddDays(-40),
                    notificationId = 4,
                    notificationType = NotificationType.FriendRequest
                },
                new Notification()
                {
                    createdAt = DateTime.Now.AddDays(2),
                    notificationId = 5,
                    notificationType = NotificationType.Birthday
                },
                new Notification()
                {
                    createdAt = DateTime.Now,
                    notificationId = 6,
                    notificationType = NotificationType.Birthday
                }
            };
            return notifications;
        }


        [Test]
        public async Task BirthdayNotification_SendsNotificationToFriendsOfUser()
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);
            
            var usersWhoHasBirthdayToday = GetUsers();
            var friends = GetFriends();

            await _dbContext.AddRangeAsync(usersWhoHasBirthdayToday);
            await _dbContext.AddRangeAsync(friends);
            await _dbContext.SaveChangesAsync();


            await notificationRepository.BirthdayNotification();

            var notification = await _dbContext.Notification.FirstAsync();
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification.notificationContent.Contains("születésnap"));
        }

        [Test]
        public async Task GetDeletableNotifications_ReturnsNotificationsIfMoreThan30DaysOld()
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);
            

            var notifications = GetNotifications_OlderThan30Days();
            await _dbContext.AddRangeAsync(notifications);
            await _dbContext.SaveChangesAsync();

            var result = await notificationRepository.GetDeletableNotifications();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(item => item.createdAt <= DateTime.Now.AddDays(-30)));
        }

        [Test]
        public async Task SelectNotifications_DeleteSelectedItemsFromDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var notifications = GetNotifications_OlderThan30Days();
            await _dbContext.Notification.AddRangeAsync(notifications);
            await _dbContext.SaveChangesAsync();

            await notificationRepository.SelectNotification();

            var remainingNotifications = await _dbContext.Notification.ToListAsync();
            Assert.That(remainingNotifications.Count, Is.EqualTo(4));
            Assert.That(remainingNotifications.Any(item => item.createdAt >= DateTime.Now.AddDays(-30)));
        }
    }
}
