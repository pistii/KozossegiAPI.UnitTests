using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozossegiAPI.Controllers;
using KozossegiAPI.DTOs;
using KozossegiAPI.Repo;
using KozossegiAPI.Services;
using KozossegiAPI.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    class SettingControllerTests
    {
        private Mock<DBContext> _dbContextMock;
        private SettingController settingController;

        private Mock<ISettingRepository> _settingRepositoryMock;
        private Mock<ISettingService> _settingServiceMock;
        private Mock<IStudyRepository> _studyRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _settingRepositoryMock = new Mock<ISettingRepository>();
            _settingServiceMock = new Mock<ISettingService>();
            _studyRepositoryMock = new Mock<IStudyRepository>();
            _dbContextMock = SettingControllerMock.GetDBContextMock();

            settingController = new SettingController(
                _dbContextMock.Object,
                _settingRepositoryMock.Object,
                _settingServiceMock.Object,
                _studyRepositoryMock.Object
                );
        }

        [Test]
        public async Task Update_OnlyAvatarUpload_ShouldReturnOkResult()
        {

            Personal user = _dbContextMock.Object.Personal.First();
            

            _settingRepositoryMock.Setup(repo => repo.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>())).ReturnsAsync(user);
            _settingServiceMock.Setup(repo => repo.ModifyUserDataIfChanged(It.IsAny<ModifyUserInfoDTO>(), It.IsAny<Personal>()));
            _settingRepositoryMock.Setup(repo => repo.UpdateAvatarIfChanged(It.IsAny<ModifyUserInfoDTO>()));

            //Get the test image
            string currentDirectory = Environment.CurrentDirectory;
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
                ModifyUserInfoDTO userInfoDTO = new();
                userInfoDTO.File = file;

                var response = await settingController.Update(userInfoDTO);

                Assert.That(file, Is.Not.Null);
                Assert.That(response, Is.Not.Null);
                Assert.That(response, Is.InstanceOf<OkObjectResult>());
                _settingRepositoryMock.Verify(x => x.GetPersonalWithSettingsAndUserAsync(It.IsAny<int>()), Times.Once());
                _settingServiceMock.Verify(x => x.ModifyUserDataIfChanged(It.IsAny<ModifyUserInfoDTO>(), It.IsAny<Personal>()), Times.Once());
                _settingRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<Personal>()), Times.Once());
            }
        }


    }
}
