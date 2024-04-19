using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KozoskodoAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    public class ChatControllerTests
    {
        private readonly Mock<DBContext> _dbContextMock = new();
        private readonly Mock<IHubContext<ChatHub, IChatClient>> _hubContextMock = new();
        private readonly Mock<IMapConnections> _connectionsMock = new();
        private readonly Mock<IChatRepository<ChatRoom, Personal>> _chatRepository = new();
        private readonly Mock<IUserRepository<user>> _userRepository = new();
        private ChatController _chatControllerMock;
        
        public static IQueryable<ChatRoom> GetChatRooms()
        {
            return new List<ChatRoom>()
            {
                new ChatRoom() {
                    chatRoomId = 1,
                    endedDateTime = DateTime.UtcNow,
                    receiverId = 1,
                    senderId = 2,
                    startedDateTime = DateTime.Now.AddDays(-3),
                    ChatContents = { }
                },
                new ChatRoom() {
                    chatRoomId = 1,
                    endedDateTime = DateTime.UtcNow,
                    receiverId = 1,
                    senderId = 2,
                    startedDateTime = DateTime.Now.AddDays(-3),
                    ChatContents = { }
                },
            }.AsQueryable();
        }
        List<ChatRoom> baseDb = GetChatRooms().ToList();

        [SetUp]
        public void Setup()
        {
            _chatControllerMock = new(
               _dbContextMock.Object,
               _hubContextMock.Object,
               _connectionsMock.Object,
               _chatRepository.Object,
               _userRepository.Object
               );
            //List<ChatRoom> baseDb = GetChatRooms().ToList();
            _dbContextMock.Setup(x => x.Add(It.IsAny<ChatRoom>())).Callback((ChatRoom room) =>
            {
                baseDb.Add(room);
            });
        }

        [Test]
        public async Task GetChatRoom_ReturnsChatRoom()
        {
            // Arrange
            var expectedChatRoom = new ChatRoom()
            {
                chatRoomId = 1,
                endedDateTime = DateTime.UtcNow,
                receiverId = 1,
                senderId = 2,
                startedDateTime = DateTime.Now.AddDays(-3),
                ChatContents = { }
            };
            _chatRepository.Setup(repo => repo.GetChatRoomById(1)).ReturnsAsync(expectedChatRoom);

            // Act
            var actionResult = await _chatControllerMock.GetChatRoom(1);
            
            // Assert
            var okResult = actionResult as OkObjectResult;
            ChatRoom? actualChatRoom = okResult?.Value as ChatRoom;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(actualChatRoom, Is.EqualTo(expectedChatRoom));
        }

        [Test]
        public async Task GetChatRoom_ReturnsBadRequest_WhenChatRoomNotFound()
        {
            // Arrange
            _chatRepository.Setup(repo => repo.GetChatRoomById(It.IsAny<int>())).ReturnsAsync((ChatRoom)null);


            // Act
            var actionResult = await _chatControllerMock.GetChatRoom(1);

            // Assert
            var result = actionResult as BadRequestResult;

            Assert.That(actionResult, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        [TestCase(1)]
        [TestCase(3)]
        public async Task GetAllChatRoom_IfUserHasChatRoom_ReturnChatRoom(int userId)
        {
            //Arrange
            _chatRepository.Setup(repo => repo.GetAllChatRoomAsQuery(userId)).ReturnsAsync(testData.Where(room => room.senderId == userId || room.receiverId == userId));

            _chatRepository.Setup(repo => repo.GetMessagePartnersById(testData.ToList(), userId)).ReturnsAsync(testDataPersonal);

            //Act
            var result = await _chatControllerMock.GetAllChatRoom(userId);

            Assert.That(result.Count, userId == 1 ? Is.EqualTo(2) : Is.EqualTo(0));
        }

        
        [Test]
        [TestCase(1)]
        public async Task GetAllChatRoom_FilterToChatRoom_ReturnChatRoomWhichContainsSearchValue(int userId)
        {
            //Arrange
            var expectedChatRooms = testData;
            var messagePartners = testDataPersonal;
            _chatRepository.Setup(repo => repo.GetAllChatRoomAsQuery(It.IsAny<int>())).ReturnsAsync(expectedChatRooms);
            _chatRepository.Setup(repo => repo.GetMessagePartnersById(It.IsAny<List<ChatRoom>>(), It.IsAny<int>())).ReturnsAsync(messagePartners.Where(person => person.id == 2 || person.id == 3));

            //Act
            var result = await _chatControllerMock.GetAllChatRoom(userId, "hello");

            var filterIsSuccessful = result.Any(item => item.Key.ChatContents.Any(content => content.message.ToLower().Contains("hello")));
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(filterIsSuccessful);
        }

        [Test]
        public async Task GetChatContent_ReturnChatMessagesIfChatContentExists()
        {
            var expected = new ChatRoom()
            {
                chatRoomId = 1,
                receiverId = 1,
                senderId = 2,
                endedDateTime = DateTime.Now,
                startedDateTime = DateTime.Now.AddDays(-1),
                ChatContents = new List<ChatContent>()
                {
                    new ChatContent()
                    {
                        message = "helo",
                        chatContentId = 1
                    },
                    new ChatContent()
                    {
                        message = "szia",
                        chatContentId = 1
                    },
                }
            };
            
            _chatRepository.Setup(repo => repo.GetChatRoomById(It.IsAny<int>())).ReturnsAsync(expected);
            var expectedList = new List<ChatContent>();
            expectedList.AddRange(expected.ChatContents);

            _chatRepository
                .Setup(repo => repo.GetSortedEntities<ChatContent, DateTime?>(It.IsAny<Func<ChatContent, DateTime?>>(), It.IsAny<Expression<Func<ChatContent, bool>>>()))
                .Returns(expectedList);
            _chatRepository.Setup(repo => repo.Paginator<ChatContent>(It.IsAny<List<ChatContent>>(), It.IsAny<int>(), It.IsAny<int>())).Returns(expectedList);
            
            var result = await _chatControllerMock.GetChatContent(1);

            _chatRepository.Verify(x => x.GetSortedEntities<ChatContent, DateTime?>(It.IsAny<Func<ChatContent, DateTime?>>(), It.IsAny<Expression<Func<ChatContent, bool>>>()), Times.Once);
            _chatRepository.Verify(x => x.Paginator<ChatContent>(It.IsAny<List<ChatContent>>(), It.IsAny<int>(), It.IsAny<int>()));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data, Is.InstanceOf<List<ChatContent>>());
            Assert.That(expected.ChatContents.Count, Is.GreaterThan(1));
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }
    }
}
