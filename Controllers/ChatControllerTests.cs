using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Moq;
using KozoskodoAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KozossegiAPI.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using KozoskodoAPI.DTOs;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Services;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    public class ChatControllerTests
    {
        public readonly Mock<IChatRepository<ChatRoom, Personal>> _chatRepository = new();
        private IQueryable<ChatRoom> testData;
        private IQueryable<Personal> testDataPersonal;
        private IQueryable<user> testDataUsers;

        public ChatController _chatControllerMock;
        
        [SetUp]
        public void Setup()
        {
            _chatControllerMock = ChatControllerMock.GetControllerMock(_chatRepository);
            testData = ChatControllerMock.GetChatRooms();
            testDataPersonal = ChatControllerMock.GetPersonals();
            testDataUsers = ChatControllerMock.GetUsers();
        }

        [Test]
        public async Task GetChatRoom_ReturnsChatRoom()
        {
            // Arrange
            var expectedChatRoom = new ChatRoom()
            {
                endedDateTime = DateTime.Now,
                receiverId = 1,
                senderId = 2,
                startedDateTime = DateTime.Now.AddDays(-3),
                ChatContents = new List<ChatContent>() { }
            };
            _chatRepository.Setup(repo => repo.GetByIdAsync<ChatRoom>(It.IsAny<int>())).ReturnsAsync(expectedChatRoom); 
            
            //Act
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
            var expectedList = new List<ChatContent>();
            expectedList.AddRange(expected.ChatContents);
            var dto = expectedList.Select(s => s.ToDto()).ToList(); //Parse chatContent into ChatContentDto
            var sortedData = expectedList.OrderByDescending(x => x.sentDate).ToList();

            _chatRepository.Setup(repo => repo.GetChatRoomById(It.IsAny<int>())).ReturnsAsync(expected);

            _chatRepository.Setup(repo => repo.GetSortedChatContent(It.IsAny<int>())).Returns(sortedData);
            _chatRepository.Setup(repo => repo.Paginator<ChatContentDto>(It.IsAny<List<ChatContentDto>>(), It.IsAny<int>(), It.IsAny<int>())).Returns(dto);
            
            var result = await _chatControllerMock.GetChatContent(1);

            _chatRepository.Verify(x => x.GetSortedChatContent(It.IsAny<int>()), Times.Once);
            _chatRepository.Verify(x => x.Paginator<ChatContentDto>(It.IsAny<List<ChatContentDto>>(), It.IsAny<int>(), It.IsAny<int>()));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data, Is.InstanceOf<List<ChatContentDto>>());
            Assert.That(expected.ChatContents.Count, Is.GreaterThan(1));
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public async Task SendMessage_AddsNewMessageToChatContent_ChatRoomExists()
        {
            ChatDto testChatRoomParameter = new()
            {
                senderId = 1,
                AuthorId = 1,
                receiverId = 2,
                message = "Teszt",
                status = Status.Sent
            };
            var expectedChatRoom = testData.FirstOrDefault(room => room.receiverId == 1);
            _chatRepository.Setup(repo => repo.ChatRoomExists(It.IsAny<ChatDto>())).ReturnsAsync(expectedChatRoom!);
            _chatRepository.Setup(repo => repo.CreateChatRoom(It.IsAny<ChatDto>())).ReturnsAsync(expectedChatRoom!);

            var result = await _chatControllerMock.SendMessage(testChatRoomParameter);

            var okResult = result as OkObjectResult;
            ChatContent? actionResult = okResult?.Value as ChatContent;
            Assert.That(result, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(actionResult, Is.TypeOf<ChatContent>());
        }

        [Test]
        public async Task SendMessage_ChatRoomDoesntExists_CreateNewConversation()
        {
            ChatDto testChatRoomParameter = new()
            {
                senderId = 1,
                AuthorId = 1,
                receiverId = 2,
                message = "Teszt",
                status = Status.Sent
            };

            ChatRoom room = new ChatRoom
            {
                chatRoomId = 1,
                senderId = testChatRoomParameter.senderId,
                receiverId = testChatRoomParameter.receiverId,
                startedDateTime = DateTime.Now,
                endedDateTime = DateTime.Now
            };

            _chatRepository.Setup(repo => repo.ChatRoomExists(It.IsAny<ChatDto>())).ReturnsAsync((ChatRoom)null);
            _chatRepository.Setup(repo => repo.CreateChatRoom(It.IsAny<ChatDto>())).ReturnsAsync(room);

            var result = await _chatControllerMock.SendMessage(testChatRoomParameter);

            var okResult = result as OkObjectResult;
            ChatContent? actionResult = okResult?.Value as ChatContent;
            Assert.That(actionResult, Is.TypeOf<ChatContent>());
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        [Ignore("Unfinished method in controller. Test later")]
        public async Task UpdateMessage_ModifiesSelectedChatMessage()
        {
            var dbContext = ChatControllerMock.GetDBContextMock();
            Mock<IStorageController> storageController = new();
            var repo = new ChatRepository(dbContext.Object, storageController.Object);

            user uss = new()
            {
                userID = 1,
                email = "Teszt@teszt",
                isActivated = true,
                isOnlineEnabled = true,
                LastOnline = DateTime.Now,
                password = "123456789",
                

                personal = new()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",

                }
            };
            ChatContent content = new()
            {
                AuthorId = 1,
                chatContentId = 1,
                message = "hello. How are you?",
                MessageId = 1,
                sentDate = DateTime.Now
            };
            _chatRepository.Setup(x => x.GetByIdAsync<user>(It.IsAny<int>())).ReturnsAsync(() => uss);
            _chatRepository.Setup(x => x.GetByIdAsync<ChatContent>(It.IsAny<int>())).ReturnsAsync(content);

            var result = await _chatControllerMock.UpdateMessage(2, 1, "teszt");
            var data = dbContext.Object.ChatContent.Any(x => x.message == "teszt");
            Assert.That(data, Is.True);
            Assert.That(result, Is.Not.Null);
        }
    }
}
