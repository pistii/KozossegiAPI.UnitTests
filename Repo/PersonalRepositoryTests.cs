using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    public class PersonalRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IPersonalRepository<Personal> _personalRepository;
        
        private DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            // Using In-Memory database for testing
            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IPersonalRepository<Personal>, PersonalRepository>();
            _serviceProvider = services.BuildServiceProvider();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _personalRepository = scopedServices.GetRequiredService<IPersonalRepository<Personal>>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
            
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();

            dbContext.Database.EnsureDeleted();
        }

        public async Task CreateFakeDb()
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
                    firstName = "Teszt",
                    lastName = "Ecske",
                    isMale = false,
                    DateOfBirth = DateOnly.Parse("1995-12-10"),
                    PlaceOfResidence = "Alabama"
                }
            };

            await _dbContext.AddRangeAsync(personals);
            await _dbContext.SaveChangesAsync();
        }

        [Test]
        public async Task FilterPersons_ReturnsFilteredPersonsDoesNotContainRequesterUser()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                await CreateFakeDb();
                var result = _personalRepository.FilterPersons(1);

                Assert.That(result, Is.Ordered.By("PlaceOfResidence").Then.By("DateOfBirth"));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(r => r.id == 1), Is.False);
            }
        }

        [Test]
        public async Task Get_ReturnsUserById()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                await CreateFakeDb();

                var result = await _personalRepository.Get(1);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.id, Is.EqualTo(1));
            }
        }
    }
}
