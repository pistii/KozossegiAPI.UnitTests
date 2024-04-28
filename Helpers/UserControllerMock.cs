using KozoskodoAPI.Auth;
using KozoskodoAPI.Controllers;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozoskodoAPI.Security;
using KozoskodoAPI.SMTP.Storage;
using KozossegiAPI.SMTP;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KozossegiAPI.UnitTests.Helpers
{
    public static class UserControllerMock
    {

        private static IQueryable<user> GetUsers()
        {
            var users = new List<user>()
            {
                new user()
                {
                    userID = 1,
                    email = "test1",
                    isActivated = true,
                    password = "$2y$10$kQtlrV3z1zWJ4YvuHQtZhO/8STD9oZvvb89KF9yEI021GqkKjn7mm",
                    SecondaryEmailAddress = "test1.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
                new user()
                {
                    userID = 2,
                    email = "test2",
                    isActivated = true,
                    password = "fakepassword2",
                    SecondaryEmailAddress = "test2.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
                new user()
                {
                    userID = 3,
                    email = "test3",
                    isActivated = true,
                    password = "fakepassword3",
                    SecondaryEmailAddress = "test3.2",
                    LastOnline = DateTime.Now.AddMinutes(-10)
                },
            }.AsQueryable();
            return users;
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
                    PlaceOfResidence = "Columbia",
                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1956-10-10"),
                    PlaceOfResidence = "Budapest",
                },
                new Personal()
                {
                    id = 3,
                    firstName = "Kiwikamaho",
                    lastName = "Hujahou",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1987-10-10"),
                    PlaceOfResidence = "Hawaii",
                },
                 new Personal()
                {
                    id = 4,
                    firstName = "Albatrosz",
                    lastName = "Aladin",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1910-10-10"),
                    PlaceOfResidence = "Missisippi",
                },
            };
            return personals.AsQueryable();
        }
        public static IQueryable<Friend> GetFriends()
        {
            var friendTable = new List<Friend>()
            {
                new() { FriendshipID = 1, UserId = 3, FriendId = 1, StatusId = 1}, //1-es 3-as kapcsolat
                new() { FriendshipID = 2, UserId = 3, FriendId = 2, StatusId = 1}, //2-es 3-as kapcsolat
                new() { FriendshipID = 3, UserId = 3, FriendId = 1, StatusId = 3}, //1-es barátnak jelölte 3-ast
                new() { FriendshipID = 4, UserId = 2, FriendId = 1, StatusId = 1}, //2-es 1-es kapcsolat
            }.AsQueryable();
            return friendTable;
        }
        public static IQueryable<Post> GetPosts()
        {
            var postsTable = new List<Post>()
            {
                new Post()
                {
                    Id = 1,
                    Likes = 3,
                    Dislikes = 0,
                    DateOfPost = DateTime.Parse("2024-04-22 20:23"),
                    PostContent = "Post test message",
                    PostComments = new List<Comment>()
                    {
                        new Comment()
                        {
                            PostId = 1,
                            FK_AuthorId = 1,
                            CommentDate = DateTime.Parse("2024-04-22 20:24"),
                            CommentText = "Comment to post",
                            commentId = 1,
                        },
                        new Comment()
                        {
                            PostId = 1,
                            FK_AuthorId = 1,
                            CommentDate = DateTime.Parse("2024-04-22 20:24"),
                            CommentText = "Comment to post",
                            commentId = 2,
                        },
                    }
                },
                new Post()
                {
                    Id = 2,
                    Likes = 1,
                    Dislikes = 0,
                    DateOfPost = DateTime.Parse("2024-04-22 20:23"),
                    PostContent = "This is the second post message",
                },
                new Post()
                {
                    Id = 3,
                    Likes = 1,
                    Dislikes = 0,
                    DateOfPost = DateTime.Parse("2024-04-22 20:23"),
                    PostContent = "This is the third post message",
                }
            }.AsQueryable();
            return postsTable;
        }
        public static IQueryable<PersonalPost> GetPersonalPosts() {
            var personalPostTable = new List<PersonalPost>()
            {
                new PersonalPost()
                {
                    postId = 1, //The reference key to the post
                    personalPostId = 1, //primary key
                    personId = 1, //ID 1 user posts,
                },
                new PersonalPost()
                {
                    postId = 2,
                    personalPostId = 2,
                    personId = 2, 
                },
            }.AsQueryable();

            return personalPostTable;
        }

        public static usersController GetUserControllerMock(
            IJwtTokenManager jwtTokenManager,
            IJwtUtils jwtUtils,
            IFriendRepository friendRepository,
            IPostRepository<PostDto> postRepository,
            IImageRepository imageRepository,
            IUserRepository<user> userRepository,

            IMailSender mailSender,
            IVerificationCodeCache verCodeCache,
            IEncodeDecode encodeDecode
            )
        {

            var userController = new usersController(
                jwtTokenManager,
                jwtUtils,
                friendRepository,
                postRepository,
                imageRepository,
                userRepository,

                mailSender,
                verCodeCache,
                encodeDecode
            );
            return userController;
        }

        public static Mock<DBContext> GetDBContextMock()
        {
            //Tesztadatok előkészítése
            List<user> users = GetUsers().ToList();
            List<Personal> personals = GetPersonals().ToList();
            List<Friend> friends = GetFriends().ToList();
            List<PersonalPost> personalPosts = GetPersonalPosts().ToList();
            List<Post> posts = GetPosts().ToList();
            List<Settings> settings = new List<Settings>();

            var dbContext = new Mock<DBContext>();

            var userMockSet = MockDbSetFactory.Create<user>(users);
            var personalMockSet = MockDbSetFactory.Create<Personal>(personals);
            var friendMockSet = MockDbSetFactory.Create<Friend>(friends);
            var personalPostsMockSet = MockDbSetFactory.Create<PersonalPost>(personalPosts); 
            var postsMockSet = MockDbSetFactory.Create<Post>(posts);
            var settingsMock = MockDbSetFactory.Create<Settings>(settings);

            dbContext.Setup(x => x.user).Returns(userMockSet);
            dbContext.Setup(x => x.Personal).Returns(personalMockSet);
            dbContext.Setup(x => x.Friendship).Returns(friendMockSet);
            dbContext.Setup(x => x.Post).Returns(postsMockSet);
            dbContext.Setup(x => x.PersonalPost).Returns(personalPostsMockSet);
            dbContext.Setup(x => x.Settings).Returns(settingsMock);
            return dbContext;
        }

        public static void MockHttpContext(this usersController userController, int userId)
        {

            var user = GetDBContextMock().Object.user.First(p => p.userID == userId);

            var fakeContext = new Dictionary<object, object?>
            {
                { "User", user }
            };
            var context = new DefaultHttpContext()
            {
                Items = fakeContext
            };
            userController.ControllerContext.HttpContext = context;
        }
    }
}
