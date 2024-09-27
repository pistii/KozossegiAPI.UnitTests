using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using KozossegiAPI.UnitTests.Helpers.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    internal class SettingRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private ISettingRepository _settingRepository;

        private DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<ISettingRepository, SettingRepository>();
            _serviceProvider = services.BuildServiceProvider();

            var scope = _serviceProvider.CreateScope();

            var scopedServices = scope.ServiceProvider;
            _settingRepository = scopedServices.GetRequiredService<ISettingRepository>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();

            var personal = PersonalData.GetUsers();
            var setting = SettingData.GetSettings();
            var user = UserData.GetUsers();
            var studies = StudyData.GetStudies();
            _dbContext.AddRange(personal);
            _dbContext.AddRange(setting);
            _dbContext.AddRange(user);
            _dbContext.AddRange(studies);
            _dbContext.SaveChanges();
        }

        
        [TearDown]
        public void Cleanup()
        {
            
            var dbContext = _serviceProvider.GetService<DBContext>();
            if (dbContext != null)
                dbContext.Database.EnsureDeleted();
        }


        [Test]
        public async Task GetSettings_ReturnsSettins()
        {
            var result = await _settingRepository.GetSettings(1);

            Assert.IsNotNull(result);
        }
    }
}
