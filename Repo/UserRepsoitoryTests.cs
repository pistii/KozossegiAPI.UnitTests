using KozossegiAPI.Auth;
using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using KozossegiAPI.SMTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KozossegiAPI.UnitTests.Repo
{
    [TestFixture]
    public class UserRepsoitoryTests
    {
        private ServiceProvider _serviceProvider;
        private IUserRepository<user?> _userRepository;
        private IJwtUtils _jwtUtils;
        private IMailSender _mailSender;
        public DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            var JwtAppSettings = new Auth.Helpers.AppSettings
            {
                Secret = "this-is-a-secret-dont-tell-anyone"
            };

            services.Configure<Auth.Helpers.AppSettings>(options =>
            {
                options.Secret = JwtAppSettings.Secret;
            });

            var mailAppSettings = new SMTP.Helpers.AppSettings
            {
                Email = "test",
                Password = "test",
                Server = "8.8.8.8",
                Port = 123,
                SSL = 123
            };

            services.Configure<SMTP.Helpers.AppSettings>(options =>
            {
                options.Email = mailAppSettings.Email;
                options.Password = mailAppSettings.Password;
                options.Server = mailAppSettings.Server;
                options.Port = mailAppSettings.Port;
                options.SSL = mailAppSettings.SSL;
            });

            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IUserRepository<user>, UserRepository>();
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IMailSender, SendMail>();

            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task CreateFakeDb()
        {
            List<user> users = new List<user>()
            {
                new user()
                {
                    userID = 1,
                    email = "teszt1@gmail.com",
                    SecondaryEmailAddress = "test1@gmail.com",
                    password = "jelszo123",
                    Guid = new Guid().ToString(),
                    LastOnline = DateTime.Now,
                },
                new user()
                {
                    userID = 2,
                    email = "teszt2@gmail.com",
                    SecondaryEmailAddress = "test2@gmail.com",
                    password = "password",
                    LastOnline = DateTime.Now.AddMinutes(-30),
                    Guid = new Guid().ToString(),
                },
                new user()
                {
                    userID = 3,
                    email = "teszt3@gmail.com",
                    SecondaryEmailAddress = "test3@gmail.com",
                    password = "jelszo",
                    LastOnline = DateTime.Now.AddMinutes(-60),
                    Guid = new Guid().ToString(),
                }
            };

            List<Personal> personals = new List<Personal>()
            {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = true,
                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = true,
                },
                new Personal()
                {
                    id = 3,
                    firstName = "Teszt",
                    lastName = "Ecske",
                    isMale = false,
                }
            };

            List<Settings> settings = new List<Settings>()
            {
                new Settings()
                {
                    PK_Id = 1,
                    NextReminder = DateTime.Now.AddDays(1),
                },
                new Settings()
                {
                    PK_Id = 2,
                    NextReminder = DateTime.Now.AddDays(2),
                }
            };

            _dbContext.user.AddRange(users);
            _dbContext.Personal.AddRange(personals);
            _dbContext.Settings.AddRange(settings);

            await _dbContext.SaveChangesAsync();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _userRepository = scopedServices.GetRequiredService<IUserRepository<user>>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();

            _jwtUtils = scopedServices.GetRequiredService<IJwtUtils>();
            _mailSender = scopedServices.GetRequiredService<IMailSender>();
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUser()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetuserByIdAsync(1);
                Assert.That(result.email, Is.EqualTo("teszt1@gmail.com"));
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetUserByGuid_ShouldReturnUser()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetByGuid("00000000-0000-0000-0000-000000000000");
                Assert.That(result.email, Is.EqualTo("teszt1@gmail.com"));
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetPersonalWithSettingsAndUserAsync_ContainsSettingsAndUserTables()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetPersonalWithSettingsAndUserAsync(1);
                Assert.That(result.users.email, Is.EqualTo("teszt1@gmail.com"));
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        [TestCase("teszt1@gmail.com", "jelszo123")]
        [TestCase("teszt1@gmail.com")]
        [TestCase("", "jelszo123")]
        public async Task GetUserByEmailOrPassword_ReturnsObject(string email = null, string password = null)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetUserByEmailOrPassword(email, password);
                Assert.That(result.userID, Is.EqualTo(1));
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        [TestCase("teszt1@gmail.com", "jelszo")]
        [TestCase("teszt2@gmail.com", "jelszo123")]
        [TestCase("jelszo123")]
        public async Task GetUserByEmailOrPassword_RequestShouldFail(string email = null, string password = null)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetUserByEmailOrPassword(email, password);
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        [TestCase("teszt2@gmail.com")]
        [TestCase("teszt1@gmail.com")]
        [TestCase("teszt1@gmail.com", false)]
        public async Task GetUserByEmailAsync(string email, bool withPersonal = true)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                SetupDb(scope);
                CreateFakeDb();

                var result = await _userRepository.GetUserByEmailAsync(email, withPersonal);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.email, Is.EqualTo(email));
                Assert.That(result.personal, withPersonal ? Is.Not.Null : Is.Null);
            }
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();

            dbContext.Database.EnsureDeleted();
        }
    }
}
