using Moq;
using KozoskodoAPI.Data;
using KozoskodoAPI.Repo;
using KozoskodoAPI.Models;
using KozoskodoAPI.DTOs;
using KozossegiAPI.UnitTests.Helpers;
using KozoskodoAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Security;
using KozoskodoAPI.SMTP.Storage;
using KozossegiAPI.SMTP;
using KozoskodoAPI.Controllers.Cloud;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    class UserControllerTests
    {
        private Mock<DBContext> _dbContextMock;
        private usersController userController;
        
        private Mock<IFriendRepository> _friendRepositoryMock;
        private Mock<IUserRepository<user>> _userRepositoryMock;
        private Mock<IPostRepository<PostDto>> _postRepositoryMock;
        private Mock<IJwtTokenManager> _jwtTokenManagerMock;
        private Mock<IImageRepository> _imageRepositoryMock = new();
        private Mock<IMailSender> _mailSenderMock = new();
        private Mock<IVerificationCodeCache> _verificationCodeCacheMock = new();
        private Mock<IEncodeDecode> _encodeDecodeMock = new();

        private static Mock<IJwtUtils> _jwtUtilsMock = new();
        [SetUp]
        public void Setup()
        {
            _friendRepositoryMock = new Mock<IFriendRepository>();
            _userRepositoryMock = new Mock<IUserRepository<user>>();
            _postRepositoryMock = new Mock<IPostRepository<PostDto>>();
            _jwtTokenManagerMock = new();

            userController = UserControllerMock.GetUserControllerMock(
                _jwtTokenManagerMock.Object, 
                _jwtUtilsMock.Object, 
                _friendRepositoryMock.Object,
                _postRepositoryMock.Object,
                _imageRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mailSenderMock.Object, 
                _verificationCodeCacheMock.Object, 
                _encodeDecodeMock.Object
                );
            _dbContextMock = UserControllerMock.GetDBContextMock();
        }

        [Test]
        [TestCase(1)]
        public async Task GetUser_ReturnUserIfExists_ShouldReturnUserWithId1(int userId)
            {
            var user = new user()
            {
                userID = 1,
                email = "test",
                password = "testPw"
            };

            _userRepositoryMock.Setup(repo => repo.GetuserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);

            var response = await userController.Get(userId);

            var okResult = response as OkObjectResult;

            Assert.That(okResult.Value, Is.EqualTo(user));
            Assert.IsInstanceOf<OkObjectResult>(okResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }
    }


    public class IUserControllerMock
    {
        
        //public static TContext GetMock<TContext>() where TContext : DbContext
        //{
        //    Mock<DbSet<user>> dbSetMock = new Mock<DbSet<user>>();
        //    Mock<DbContext> dbContext = new Mock<DbContext>();

        //    dbSetMock.Setup(s => s.Count()).Returns(It.IsAny<user>);
            
        //    return dbContext.Object;
        //}

        private static List<user> GenerateTestData()
        {
            List<user> lstUser = new();
            
            for (int index = 1; index <= 10; index++)
            {
                lstUser.Add(new user
                {
                    userID = index,
                    email = "UserEmail" + index + "@gmail.com",
                    password = "12345678",
                    SecondaryEmailAddress = "SecondaryUserEmail" + index + "@gmail.com"
                });
            }
            return lstUser;
        }
    }
}
