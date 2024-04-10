using Google.Api;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    public class FriendRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IFriendRepository _friendRepository;
        private IPersonalRepository<Personal> _personalRepository;
        private IUserRepository<user> _userRepository;

        public DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            // Using In-Memory database for testing
            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IPersonalRepository<Personal>, PersonalRepository>();
            services.AddScoped<IUserRepository<user>, UserRepository>();


            _serviceProvider = services.BuildServiceProvider();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _friendRepository = scopedServices.GetRequiredService<IFriendRepository>();
            _personalRepository = scopedServices.GetRequiredService<IPersonalRepository<Personal>>();
            _userRepository = scopedServices.GetRequiredService<IUserRepository<user>>();


            _dbContext = scopedServices.GetRequiredService<DBContext>();
        }

        public static IEnumerable<Friend> GetFriendsips()
        {
            var users = new List<Friend>() {
                new Friend() {
                    FriendshipID = 1,
                    UserId = 1,
                    FriendId = 2,
                    StatusId= 1,
                    FriendshipSince = DateTime.Now.AddDays(-1)
                },
                new Friend() {
                    FriendshipID = 2,
                    UserId = 1,
                    FriendId = 3,
                    StatusId = 4,
                },
                new Friend() {
                    FriendshipID = 3,
                    UserId = 1,
                    FriendId = 3,
                    StatusId = 4,
                }
                }.AsEnumerable();
            return users;
        }

        public static IEnumerable<Personal> GetUsers()
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
                }.AsEnumerable();
            return users;
        }


        public static IEnumerable<user> GetUser_Table()
        {
            var users = new List<user>()
            {
                new user()
                {
                    userID = 1,
                    email = "teszt@teszt.com"
                },
                new user()
                {
                    userID = 2,
                    email = "teszt1@teszt.com"
                },
                new user()
                {
                    userID = 3,
                    email = "teszt2@teszt.com"
                },
            }.AsEnumerable();
            return users;
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnExpectedFriend()
        {
            var mock = new Mock<IFriendRepository>();

            var expected = new Friend()
            {
                FriendshipID = 1,
                UserId = 1,
                FriendId = 2,
                StatusId = 1,
                FriendshipSince = DateTime.Now.AddDays(-1)
            };

            mock.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(expected);

            var result = await mock.Object.GetByIdAsync(1);

            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        public async Task GetAll_ShouldReturnUsersIfTheyAreFriends()
        {
            var mock = new Mock<IFriendRepository>();

            List<Personal> expected = new()
            {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = false,
                }
            };

            var users = GetUsers();

            mock.Setup(u => u.GetAll(1)).Returns(() => Task.FromResult(expected.AsEnumerable()));
            var result = await mock.Object.GetAll(1);

            Assert.Contains(expected.FirstOrDefault(), result.ToList());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetAllFriendAsync_ReturnsAllPersonWithFriendshipStatus()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);

                var users = GetUsers();
                await _dbContext.AddRangeAsync(users);

                var friendships = GetFriendsips();
                await _dbContext.AddRangeAsync(friendships);

                var usertable = GetUser_Table();
                await _dbContext.AddRangeAsync(usertable);


                await _dbContext.SaveChangesAsync();

                var result = await _friendRepository.GetAllFriendAsync(1);

                Assert.That(result, Is.Not.Empty);
            }
        }

        [Test]
        public async Task GetAllFriendAsync_DoesntHaveAnyFriends()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);

                var users = GetUsers();
                await _dbContext.AddRangeAsync(users);

                var friendships = GetFriendsips();
                await _dbContext.AddRangeAsync(friendships);

                var usertable = GetUser_Table();
                await _dbContext.AddRangeAsync(usertable);


                await _dbContext.SaveChangesAsync();

                var result = await _friendRepository.GetAllFriendAsync(3);

                Assert.That(result, Is.Empty);
            }
        }


        [Test]
        [TestCase(1, 1, "self")]
        [TestCase(1, 2, "friend")]
        [TestCase(2, 3, "nonfriend")]
        public async Task CheckIfUsersInRelation_UserVisitsProfilePage(int userId, int viewerId, string expectedResult)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);

                var friendships = GetFriendsips();
                await _dbContext.AddRangeAsync(friendships);
                await _dbContext.SaveChangesAsync();

                string result = await _friendRepository.CheckIfUsersInRelation(userId, viewerId);

                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public async Task Delete_RemoveFriendshipFromDb()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);

                var friendships = GetFriendsips();
                await _dbContext.AddRangeAsync(friendships);
                await _dbContext.SaveChangesAsync();

                var firstItem = friendships.First();
                //Act
                await _friendRepository.Delete(firstItem);
                _dbContext.SaveChanges();

                // Assert
                var removedItem = await _dbContext.Friendship.FindAsync(firstItem.FriendshipID);
                int removedCount = _dbContext.Friendship.Count();

                Assert.That(removedCount, Is.EqualTo(2));
                Assert.That(removedItem, Is.Null);
            }
        }

        [Test]
        public async Task Put_InsertFriendshipIntoDb()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                
                SetupDb(scope);

                var insertItem = new Friend_notificationId()
                {
                    NotificationId = 1,
                    FriendshipID = 1,
                    UserId = 1,
                    FriendId = 2,
                    StatusId = 1
                };

                //Act
                await _friendRepository.Put(insertItem);
                _dbContext.SaveChanges();

                // Assert
                var insertedItem = await _dbContext.Friendship.FindAsync(insertItem.FriendshipID);

                Assert.That(_dbContext.Friendship.Count(), Is.EqualTo(1));
                Assert.Contains(insertedItem, _dbContext.Friendship.ToList());
            }
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(3, 1)]

        public async Task FriendshipExists(int FriendId, int UserId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                var friendships = GetFriendsips();
                await _dbContext.AddRangeAsync(friendships);
                _dbContext.SaveChanges();

                Friend friendship = new Friend()
                {
                    UserId = UserId,
                    FriendId = FriendId
                };

                var result = await _friendRepository.FriendshipExists(friendship);

                Assert.That(result, Is.Not.Null);
            };
        }

        [Test]
        public async Task GetAllUserWhoHasBirthdayToday_ReturnsAllUserWhoCelebratesBrday()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                var today = DateTime.Now;
                var birtdayUsers = GetUsers();

                await _dbContext.AddRangeAsync(birtdayUsers); 
                await _dbContext.SaveChangesAsync();

                var result = await _friendRepository.GetAllUserWhoHasBirthdayToday();

                var expected = result.FirstOrDefault();
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(expected.id, Is.EqualTo(1));
                Assert.That(expected.DateOfBirth, Is.EqualTo(DateOnly.FromDateTime(DateTime.Now)));
            }
        }


        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();

            dbContext.Database.EnsureDeleted();
        }
    }
}
