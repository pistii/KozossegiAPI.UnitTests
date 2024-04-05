using Moq;
using KozoskodoAPI.Data;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Repo;
using KozoskodoAPI.SMTP;
using KozoskodoAPI.SMTP.Storage;
using KozoskodoAPI.Security;
using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using NUnit.Framework;
using KozoskodoAPI;
using KozoskodoAPI.DTOs;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    class UserControllerTests
    {
        private Mock<DBContext> _dbContextMock;
        
        private Mock<IJwtTokenManager> _jwtTokenManagerMock;
        private Mock<IJwtUtils> _jwtUtilsMock;
        private Mock<IFriendRepository> _friendRepositoryMock;
        private Mock<IUserRepository<user>> _userRepostioryMock;

        private Mock<IPostRepository<PostDto>> _postRepositoryMock;
        private Mock<IImageRepository> _imageRepositoryMock;
        private Mock<IMailSender> _mailSenderMock;
        private Mock<IVerificationCodeCache> _verificationCodeCacheMock;
        private Mock<IEncodeDecode> _encodeDecodeMock;

        [SetUp]
        public void Setup()
        {
            _dbContextMock = new Mock<DBContext>();
            _jwtTokenManagerMock = new Mock<IJwtTokenManager>();
            _jwtUtilsMock = new Mock<IJwtUtils>();
            _friendRepositoryMock = new Mock<IFriendRepository>();
            _userRepostioryMock = new Mock<IUserRepository<user>>();
            _postRepositoryMock = new Mock<IPostRepository<PostDto>>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _mailSenderMock = new Mock<IMailSender>();
            _verificationCodeCacheMock = new Mock<IVerificationCodeCache>();
            _encodeDecodeMock = new Mock<IEncodeDecode>();

        }

        [Test]
        [Ignore("Test later")]
        public async Task  GetUser_User_ReturnUserIfExists()
        {
           var mock = new Mock<IUserRepository<user>>();
            var userController = new usersController(
                _jwtTokenManagerMock.Object, 
                _jwtUtilsMock.Object, 
                _friendRepositoryMock.Object,
                _postRepositoryMock.Object,
                _imageRepositoryMock.Object,
                _userRepostioryMock.Object,
                _mailSenderMock.Object, 
                _verificationCodeCacheMock.Object, 
                _encodeDecodeMock.Object, 
                _dbContextMock.Object);

            int userId = 1;
            var userShouldReturn = new user()
            {
                userID = userId,
                email = "valami@gmail.com"
            };

            var response = await userController.Get(userId);

            Assert.That(response, Is.EqualTo(userShouldReturn));
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
