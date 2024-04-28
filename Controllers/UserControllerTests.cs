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

        [Test]
        [TestCase(1)]
        public async Task TurnOffReminder_UserSettingsExists_ShouldExtendNextTimeInterval(int userId)
            {
            //Set up user identity:
            //https://weblogs.asp.net/ricardoperes/unit-testing-the-httpcontext-in-controllers

            //Arrange
            var person = _dbContextMock.Object.Personal.First(p => p.id == userId); //Settings tábla nélkül
            person.Settings = new();

            UserControllerMock.MockHttpContext(userController, userId);

            _userRepositoryMock.Setup(repo => repo.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>())).ReturnsAsync(person);

            _userRepositoryMock.Setup(repo => repo.InsertSaveAsync(It.IsAny<Settings>())).Callback(() =>
                {
                _dbContextMock.Object.Settings.Add(person.Settings);
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

        [Test]
        public async Task SignUp_EmailIsUsed_ShouldReturnBadRequest()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync((user)null);

            var registerform = new RegisterForm()
            {
                userID = 1,
                email = "test",
                SecondaryEmailAddress = "test",
                Password = "password",
            };
            var result = await userController.SignUp(registerform);
            var badResult = result as BadRequestObjectResult;

            Assert.That(badResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(badResult.Value, Is.Not.Null);
        }

        [Test]
        public async Task SignUp_AddsUserToTheDb_ShouldReturnOkResult()
        {
            //In case of if it misses the templates, should add the email templates to: \bin\Debug\net7.0\templates
            var registerform = new RegisterForm()
            {
                userID = 1,
                email = "test",
                SecondaryEmailAddress = "test",
                Password = "password",
            };
            user user = registerform;

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.InsertSaveAsync<user>(It.IsAny<user>()));


            var result = await userController.SignUp(registerform);
            var badResult = result as OkObjectResult;

            Assert.That(badResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(badResult.Value, Is.Not.Null);
        }

        [Test]
        [TestCase(1)]
        public async Task ModifyUserInfo_OnlyAvatarUpload_ShouldReturnOkResult(int userId)
        {

            Personal user = new Personal()
            {
                id = 1,
                firstName = "testFirst",
                lastName = "testLast",
                users = _dbContextMock.Object.user.FirstOrDefault(x => x.userID == userId),
                Settings = new()
            };

            _userRepositoryMock.Setup(repo => repo.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>())).ReturnsAsync(user);
            _imageRepositoryMock.Setup(repo => repo.Upload(It.IsAny<AvatarUpload>()));
            _userRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<Personal>()));

            //Get the test image
            string currentDirectory = Environment.CurrentDirectory; //Directory.GetCurrentDirectory();
            string projectRoot = Directory.GetParent(currentDirectory).Parent.Parent.FullName;
            string relativePath = Path.Combine("Helpers", "testAvatar.jpg");
            string absolutePath = Path.Combine(projectRoot, relativePath);
            //The image in bytes
            var image = File.ReadAllBytes(absolutePath);
            using (var stream = new MemoryStream(image.Length))
            {
                //The image as it would arrive as FromForm 
                var file = new FormFile(stream, 0, image.Length, "name", absolutePath)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg"
                };
                //The test data
                ModifyUserInfoDTO userInfoDTO = new(1, "test", "image/jpg", file)
                {
                    UserId = userId,
                    File = file,
                    Name = file.Name,
                };


                var response = await userController.ModifyUserInfo(userInfoDTO);

                Assert.That(file, Is.Not.Null);
                Assert.That(response, Is.Not.Null);
                Assert.That(response, Is.InstanceOf<OkObjectResult>());
                _userRepositoryMock.Verify(x => x.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>()), Times.Once());
                _imageRepositoryMock.Verify(x => x.Upload(It.IsAny<AvatarUpload>()), Times.Once());
                _userRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<Personal>()), Times.Once());
            }
        }

        [Test]
        [TestCase(100)]
        public async Task ModifyUserInfo_UserTablePropertiesCorrectlyPassed(int userId)
        {
            //Arrange
            #region Prepare the test data

            var studies = new Studies()
            {
                PK_Id = 1,
                FK_UserId = userId,
            };

            var originalUser = new user()
            {
                userID = userId,
                email = "teszt@email.com",
                isActivated = true,
                isOnlineEnabled = true,
                LastOnline = DateTime.Parse("2023-04-11 10:43"),
                password = BCrypt.Net.BCrypt.HashPassword("tesztPw123456789"),
                registrationDate = DateTime.Parse("2011-04-01 10:13"),
                SecondaryEmailAddress = "teszt2@email.com",
                Studies = new List<Studies>() { }
            };
            originalUser.Studies.Add(studies);

            var originalPersonal = new Personal()
            {
                id = 100,
                DateOfBirth = DateOnly.Parse("2023-01-11"),
                firstName = "teszt1",
                middleName = "teszt2",
                lastName = "teszt3",
                isMale = true,
                avatar = "asdf123456",
                Workplace = "123456",
                users = originalUser,
            };

            ModifyUserInfoDTO userInfoDTO = new(userId, null, null, null)
            {
                UserId = userId,
                EmailAddress = "teszt2@email.com",
                firstName = "First",
                middleName = "Mid",
                lastName = "Last",
                isOnline = true,
                Pass1 = "1234567Random",
                Pass2 = "1234567Random",
                PhoneNumber = "1234567899123",
                SecondaryEmailAddress = "tesztSecondary@email.com",
                PlaceOfBirth = "Üzbegisztán",
                StartYear = 2014,
                EndYear = 2018,
                Class = "SchoolName",
                PlaceOfResidence = "Azerbajdzsán",
                Profession = "Software developer",
                SchoolName = "Random name",
                Workplace = "Teszt place"
            };
            #endregion
            
            _userRepositoryMock.Setup(repo => repo.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>())).ReturnsAsync(originalPersonal);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync<Studies>(It.IsAny<int>())).ReturnsAsync(studies);
            _userRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<Personal>()));
            //For the comparison I've added a user to the fake db.
            //This also simulates more likely the case when the user updates something.
            _dbContextMock.Object.user.Add(originalUser);
            _userRepositoryMock.Setup(repo => repo.GetuserByIdAsync(It.IsAny<int>())).ReturnsAsync(_dbContextMock.Object.user.FirstOrDefault(x => x.userID == userId));

            //Act
            var response = await userController.ModifyUserInfo(userInfoDTO);
            var compare = await userController.Get(userId);
            var userAfterModification = _dbContextMock.Object.user.FirstOrDefault(x => x.userID == userId);

            var okResult = response as OkObjectResult;
            var comparing = compare as OkObjectResult;
            var okResultUser = okResult.Value as user;
            var comparingUser = comparing.Value as user;

            //Assert
            _userRepositoryMock.Verify(x => x.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>()), Times.Once());
            _userRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<Personal>()), Times.Once());


            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(comparing.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.AreEqual(okResultUser, comparingUser);
            Assert.AreEqual(okResultUser.Studies, comparingUser.Studies);
            Assert.That(okResult.Value, Is.EqualTo(comparing.Value));
            }

        [Test]
        [TestCase(1)]
        public async Task ValidateToken_ActivateUser(int userId)
        {
            user fakeUser = new()
            {
                userID = userId,
                email = "teszt",
                password = "123456789"
            };
            UserControllerMock.MockHttpContext(userController, userId);

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailOrPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(fakeUser);
            _userRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<user>()));

            var response = await userController.ActivateUser();
            var okResult = response as OkObjectResult;

            _userRepositoryMock.Verify(x => x.GetUserByEmailOrPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _userRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<user>()), Times.Once());
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task ForgotPw_SendEmailToUser_ReturnsEncryptedVerificationCode()
        {
            string email = "teszt@teszt.com";
            user userWithEmail = new() //The returned user from GetUserByEmailAsync method
            {
                userID = 1,
                email = email,
                password = "123456789",
                Guid = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
                personal = new()
                {
                    lastName = "Teszt",
                    firstName = "Elek"
                }
            };
            string verificationCode = "000000";

            var userControllerParameter = new EncryptedDataDto()
            {
                Data = "U2FsdGVkX1/O0JrzRfMygyzy+pf0MOeUiA+huFpD3rY="
            };
            _encodeDecodeMock.Setup(repo => repo.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(email);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userWithEmail);
            _verificationCodeCacheMock.Setup(repo => repo.Create(It.IsAny<string>(), It.IsAny<string>()));
            _encodeDecodeMock.Setup(repo => repo.Encrypt(It.IsAny<string>(), It.IsAny<string>()));

            //Act
            var result = await userController.ForgotPw(userControllerParameter);
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            _encodeDecodeMock.Verify(x => x.Decrypt(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
            _verificationCodeCacheMock.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _encodeDecodeMock.Verify(x => x.Encrypt(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task ModifyPassword_VercodeIsCorrectAndModifiesPasswordToRequested()
        {
            var user = new user()
            {
                userID = 1,
                email = "teszt",
                password = "password"
            };
            ModifyPassword passwordForm = new()
            {
                Password1 = "TestPassword",
                Password2 = "TestPassword",
                otpKey = "000000"
            };


            _verificationCodeCacheMock.Setup(repo => repo.GetValue(It.IsAny<string>()));
            _userRepositoryMock.Setup(repo => repo.GetByGuid(It.IsAny<string>())).ReturnsAsync(user);
            _verificationCodeCacheMock.Setup(repo => repo.Remove(It.IsAny<string>()));
            _userRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<user>()));

            var result = await userController.ModifyPassword(passwordForm);

            var okResult = result as OkResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            _verificationCodeCacheMock.Verify(x => x.GetValue(It.IsAny<string>()));
            _userRepositoryMock.Verify(x => x.GetByGuid(It.IsAny<string>()));
            _verificationCodeCacheMock.Verify(x => x.Remove(It.IsAny<string>()));
            _userRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<user>()));
        }

        [Test]
        public async Task RestrictUser_CreatesNewRestriction_BlocksUserTemporarily()
        {
            var user = new user()
            {
                userID = 1,
                email = "teszt",
                password = "password"
            };
            var dto = new RestrictionDto()
            {
                FK_StatusId = 1,
                userId = 1,
                EndDate = DateTime.UtcNow.AddDays(1),
                Description = "User was bad"
            };

            _userRepositoryMock.Setup(repo => repo.GetuserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.InsertSaveAsync<Restriction>(It.IsAny<Restriction>()));
            _userRepositoryMock.Setup(repo => repo.InsertSaveAsync<UserRestriction>(It.IsAny<UserRestriction>()));

            var result = await userController.RestrictUser(dto);
            var okResult = result as OkResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            
            _userRepositoryMock.Verify(x => x.GetuserByIdAsync(It.IsAny<int>()));
            _userRepositoryMock.Verify(x => x.InsertSaveAsync(It.IsAny<Restriction>()));
            _userRepositoryMock.Verify(x => x.InsertSaveAsync(It.IsAny<UserRestriction>()));
        }


        [Test]
        [TestCase(1)]
        public async Task Delete_UserFound_FinishesDeletion(int userId)
        {
            user user = new()
            {
                userID = 1,
                email = "teszt",
            };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync<user>(It.IsAny<int>())).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.RemoveThenSaveAsync<user>(It.IsAny<user>()));

            var result = await userController.Delete(userId);
            var okResult = result as OkResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_UserNotFound_ReturnsNotFoundResult(int userId)
        {
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync<user>(It.IsAny<int>())).ReturnsAsync((user)null);
            _userRepositoryMock.Setup(repo => repo.RemoveThenSaveAsync<user>(It.IsAny<user>()));

            var result = await userController.Delete(userId);
            var okResult = result as NotFoundResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }
    }
}
