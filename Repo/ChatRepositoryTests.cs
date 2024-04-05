using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    public class ChatRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IChatRepository<ChatRoom, Personal> _chatRepository;

        private DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            // Using In-Memory database for testing
            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IChatRepository<ChatRoom, Personal>, ChatRepository>();
            _serviceProvider = services.BuildServiceProvider();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _chatRepository = scopedServices.GetRequiredService<IChatRepository<ChatRoom, Personal>>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();
            dbContext.Database.EnsureDeleted();
        }

        public async void CreateFakeDb()
        {
            List<ChatRoom> roomList = new List<ChatRoom>()
            {
                new ChatRoom()
                {
                    chatRoomId = 1,
                    senderId = 1,
                    receiverId = 2,
                    startedDateTime = DateTime.Now,
                    endedDateTime = DateTime.Now,
                    ChatContents = new List<ChatContent>()
                    {
                        new ChatContent()
                        {
                            MessageId = 1,
                            AuthorId = 1,
                            chatContentId = 1,
                            message = "Hello",
                            sentDate = DateTime.Now
                        },
                        new ChatContent()
                        {
                            MessageId = 2,
                            AuthorId = 2,
                            chatContentId = 1,
                            message = "Weeeee",
                            sentDate = DateTime.Now
                        },
                    }
                },
                new ChatRoom()
                {
                    chatRoomId = 2,
                    senderId = 2,
                    receiverId = 3,
                    startedDateTime = DateTime.Now,
                    endedDateTime = DateTime.Now,
                     ChatContents = new List<ChatContent>()
                    {
                        new ChatContent()
                        {
                            MessageId = 3,
                            AuthorId = 2,
                            chatContentId = 1,
                            message = "Hello 1",
                            sentDate = DateTime.Now
                        },
                        new ChatContent()
                        {
                            MessageId = 4,
                            AuthorId = 3,
                            chatContentId = 1,
                            message = "Weeeee 2",
                            sentDate = DateTime.Now
                        },
                    }
                },
            };

            List<Personal> personals = new List<Personal>()
            {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1988-12-10"),
                    PlaceOfResidence = "Columbia"

                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1956-10-10"),
                    PlaceOfResidence = "Budapest"

                },
                new Personal()
                {
                    id = 3,
                    firstName = "Teszt",
                    lastName = "Ecske",
                    isMale = false,
                    DateOfBirth = DateOnly.Parse("1995-12-10"),
                    PlaceOfResidence = "Alabama"
                }
            };

            await _dbContext.AddRangeAsync(personals);
            await _dbContext.AddRangeAsync(roomList);
            await _dbContext.SaveChangesAsync();
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetAllChatRoomAsQuery_ReturnsChatRooms(int userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _chatRepository.GetAllChatRoomAsQuery(userId);

                Assert.That(result.Count, Is.EqualTo(userId == 1 ? 1 : 2));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.First().ChatContents, Is.Not.Empty);
                if (userId == 1)
                Assert.That(result, Is.Ordered.By("sentDate").Then.By("endedDateTime"));
            }
        }

        [Test]
        public async Task GetMessagePartnersById_OnlyMessagePartnersReturned()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                List<ChatRoom> fakeChatRoom = new List<ChatRoom>() {
                    new ChatRoom()
                    {
                        chatRoomId = 1,
                        senderId = 1,
                        receiverId = 2,
                        startedDateTime = DateTime.Now,
                        endedDateTime = DateTime.Now,
                        ChatContents = new List<ChatContent>()
                        {
                            new ChatContent()
                            {
                                MessageId = 1,
                                AuthorId = 1,
                                chatContentId = 1,
                                message = "Hello",
                                sentDate = DateTime.Now
                            },
                            new ChatContent()
                            {
                                MessageId = 2,
                                AuthorId = 2,
                                chatContentId = 1,
                                message = "Weeeee",
                                sentDate = DateTime.Now
                            },
                        }
                    },
                    new ChatRoom()
                    {
                        chatRoomId = 1,
                        senderId = 2,
                        receiverId = 3,
                        startedDateTime = DateTime.Now,
                        endedDateTime = DateTime.Now,
                        ChatContents = new List<ChatContent>()
                        {
                            new ChatContent()
                            {
                                MessageId = 1,
                                AuthorId = 2,
                                chatContentId = 1,
                                message = "Hello",
                                sentDate = DateTime.Now
                            },
                            new ChatContent()
                            {
                                MessageId = 2,
                                AuthorId = 3,
                                chatContentId = 1,
                                message = "Weeeee",
                                sentDate = DateTime.Now
                            },
                        }
                    }
                };

                var  result = await _chatRepository.GetMessagePartnersById(fakeChatRoom, 1);

                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result.First().id, Is.Not.EqualTo(1));
            }
        }
    }
}
