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

        [Test]
        public async Task Authenticate_AuthenticationShouldBeSuccessful_ReturnsHttpOk()
        {
            //In the case of more test case, should use generated passwords for example from https://bcrypt.online/
            //In this case of test the password is equals to the expected bcrypt password
            LoginDto loginDto = new LoginDto()
            {
                Password = "password",
                Email = "test1"
            };

            var personal = new Personal()
            {
                id = 1,
                firstName = "Gipsz",
                lastName = "Jakab",
                isMale = true,
                DateOfBirth = DateOnly.Parse("1988-12-10"),
                PlaceOfResidence = "Columbia",
            };
            string token = "123456789";
            var user = new user() //Stored in the database
            {
                userID = 1,
                email = "test",
                password = "$2y$10$kQtlrV3z1zWJ4YvuHQtZhO/8STD9oZvvb89KF9yEI021GqkKjn7mm",
                personal = new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = true,
                    DateOfBirth = DateOnly.Parse("1988-12-10"),
                    PlaceOfResidence = "Columbia",
    }
            };

            AuthenticateResponse response = new(personal, token);
            _jwtTokenManagerMock.Setup(repo => repo.Authenticate(It.IsAny<LoginDto>())).ReturnsAsync(response);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(user);

            var result = await userController.Authenticate(loginDto);


            _jwtTokenManagerMock.Verify(x => x.Authenticate(It.IsAny<LoginDto>()), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Once());

            var okResult = result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(okResult);
        }

        [Test]
        public async Task Authenticate_AuthenticationShouldBeFailed_BecauseUserDoesntExists()
    {
            //After the point of authentication, and because of it fails, should return with NotFound response
            LoginDto loginDto = new LoginDto()
            {
                Password = "password",
                Email = "test1"
            };
        
            _jwtTokenManagerMock.Setup(repo => repo.Authenticate(It.IsAny<LoginDto>())).ReturnsAsync((AuthenticateResponse)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync((user)null);

            var result = await userController.Authenticate(loginDto);
            

            _jwtTokenManagerMock.Verify(x => x.Authenticate(It.IsAny<LoginDto>()), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsInstanceOf<NotFoundObjectResult>(notFoundResult);
        }

        [Test]
        [TestCase(1)]
        public async Task Get_ReturnsUser_WithGivenId(int userId)
        {
            var expected = _dbContextMock.Object.user.Find(userId);
            _userRepositoryMock.Setup(repo => repo.GetuserByIdAsync(It.IsAny<int>())).ReturnsAsync(expected);

            var result = await userController.Get(userId);
            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expected));
        }
            
            for (int index = 1; index <= 10; index++)

        [Test]
        [TestCase(1)]
        public async Task TurnOffReminder_UserSettingsDoesntExist_ShouldSendNextReminderExactlyOneDayLater(int userId)
        {
            //Set up user identity:
            //https://weblogs.asp.net/ricardoperes/unit-testing-the-httpcontext-in-controllers

            //Arrange
            var person = _dbContextMock.Object.Personal.First(p => p.id == userId); //Settings tábla nélkül

            var setting = new Settings()
            {
                FK_UserId = userId,
                NextReminder = DateTime.Now.AddDays(1)
            };
            UserControllerMock.MockHttpContext(userController, userId);

            _userRepositoryMock.Setup(repo => repo.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>())).ReturnsAsync(person);

            _userRepositoryMock.Setup(repo => repo.InsertSaveAsync(It.IsAny<Settings>())).Callback(() =>
            {
                _dbContextMock.Object.Settings.Add(setting);
            });

            var userSettingsDTO = new UserSettingsDTO()
            {
                Days = 1,
                RemindUserOfUnfulfilledReg = true,
                isOnlineEnabled = true,
            };

            //Act
            var result = await userController.TurnOffReminder(userSettingsDTO);

            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(_dbContextMock.Object.Settings.Any(), Is.True);
            Assert.That(okResult.Value, Is.Not.Null);
        }

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
