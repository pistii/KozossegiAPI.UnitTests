using KozoskodoAPI.Controllers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.UnitTests.Helpers
{
    public class ChatControllerMock
    {
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
                    ChatContents = new List<ChatContent>() {}
                },
                new ChatRoom() {
                    chatRoomId = 2,
                    endedDateTime = DateTime.UtcNow,
                    receiverId = 1,
                    senderId = 3,
                    startedDateTime = DateTime.Now.AddDays(-3),
                    ChatContents = new List<ChatContent>()
                    {
                        new ChatContent()
                        {
                            MessageId = 1,
                            AuthorId = 1,
                            chatContentId = 2,
                            message = "hello. How are you?",
                            sentDate = DateTime.Now,
                            status = Status.Sent
                        },
                        new ChatContent()
                        {
                            MessageId = 2,
                            AuthorId = 3,
                            chatContentId = 1,
                            message = "Hi",
                            sentDate = DateTime.Now,
                            status = Status.Sent
                        }
                    }
                },
            }.AsQueryable();
        }
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
                    firstName = "Kiwikamaho",
                    lastName = "Hujahou",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1987-10-10"),
                    PlaceOfResidence = "Hawaii"

                },
            };
            return personals.AsQueryable();
        }
        public static IQueryable<ChatContent> GetChatContents()
        {
            return new List<ChatContent>()
            {
                new ChatContent()
                {
                    MessageId = 1,
                    AuthorId = 1,
                    chatContentId = 2,
                    message = "hello. How are you?",
                    sentDate = DateTime.Now,
                    status = Status.Sent
                },
                new ChatContent()
                {
                    MessageId = 2,
                    AuthorId = 3,
                    chatContentId = 1,
                    message = "Hi",
                    sentDate = DateTime.Now,
                    status = Status.Sent
                }
            }.AsQueryable();
        }
        public static IQueryable<user> GetUsers()
        {
            List<user> users = new List<user>()
            {
                new user()
                {
                    userID = 1,
                    email = "test1@test.com",
                    isActivated = true,
                    password = "password",
                                        
                }
            };
            return users.AsQueryable();
        }

        private static readonly Mock<IHubContext<ChatHub, IChatClient>> _hubContextMock = new();
        private static readonly Mock<IMapConnections> _connectionsMock = new();

        public static ChatController GetControllerMock(Mock<IChatRepository<ChatRoom, Personal>> _chatRepository)
        {
            
            //A tesztelendő osztályból egy példány
            ChatController _chatControllerMock = new(
               _hubContextMock.Object,
               _connectionsMock.Object,
               _chatRepository.Object
               );

            return _chatControllerMock;
        }

        public static Mock<DBContext> GetDBContextMock()
        {
            //Tesztadatok előkészítése
            List<ChatRoom> chatRooms = GetChatRooms().ToList();
            List<Personal> personals = GetPersonals().ToList();
            List<ChatContent> chatcontents = GetChatContents().ToList();
            
            var dbContext = new Mock<DBContext>();

            var contentMcokSet = MockDbSetFactory.Create<ChatContent>(chatcontents);
            var personalMockSet = MockDbSetFactory.Create<Personal>(personals);
            var chatroomMockSet = MockDbSetFactory.Create<ChatRoom>(chatRooms);

            dbContext.Setup(x => x.ChatContent).Returns(contentMcokSet);
            dbContext.Setup(x => x.Personal).Returns(personalMockSet);
            dbContext.Setup(x => x.ChatRoom).Returns(chatroomMockSet);

            return dbContext;
        }
    }
}
