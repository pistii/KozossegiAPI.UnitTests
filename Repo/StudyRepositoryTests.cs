using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using KozossegiAPI.UnitTests.Helpers.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    internal class StudyRepositoryTests
    {
        private ServiceProvider _serviceProvider;

        private DBContext _dbContext = new();

        private IStudyRepository _studyRepository;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            services.AddScoped<IStudyRepository, StudyRepository>();
            _serviceProvider = services.BuildServiceProvider();

        }

        public void SetupDb(IServiceScope scope)
        {

            var scopedServices = scope.ServiceProvider;
            _studyRepository = scopedServices.GetRequiredService<IStudyRepository>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
            _dbContext.AddRange(StudyData.GetStudies());
            _dbContext.SaveChanges();
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public async Task UpdateStudies_ShouldUpdateAndModify_ReturnsHttpStatusCode200()
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var testUser = new Personal()
            {
                id = 1,
                firstName = "Gipsz",
                lastName = "Jakab",
                isMale = true,
                DateOfBirth = DateOnly.Parse("1988-12-10"),
                PlaceOfResidence = "Columbia",
            };
            var changedStudies = new List<StudyDto>() {
                new StudyDto()
                {
                    initId = 1,
                    Class = "class3",
                    EndYear = 2012,
                    StartYear = 2008,
                    SchoolName = "School"
                },
                new StudyDto()
                {
                    StartYear = DateTime.Now.AddYears(99).Year
                },
                new StudyDto()
                {
                    StartYear = DateTime.Now.AddYears(99).Year
                },
                new StudyDto()
                {
                    FK_UserId = 1,
                    Class = "class4"
                },
                new StudyDto()
                {
                    SchoolName = "test school"
                },
            };

            var result = await _studyRepository.UpdateStudies(changedStudies);

            var modifications = _dbContext.Study.SingleOrDefault(p => p.PK_Id == 1).Class;
            Assert.That(result, Is.TypeOf<HttpStatusCode>());
            Assert.That(modifications, Is.EqualTo("class1"));
        }
    }

    
}
