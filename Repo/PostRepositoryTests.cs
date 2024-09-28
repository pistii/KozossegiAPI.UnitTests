using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Repo;
using KozossegiAPI.UnitTests.Helpers.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KozossegiAPI.UnitTests.Repo
{
    public class PostRepositoryTests
    {
        private ServiceProvider _serviceProvider;

        private IPostRepository<PostDto> _postRepository;
        private IStorageRepository _storageRepository;
        public DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IPostRepository<PostDto>, PostRepository>();
            services.AddScoped<IStorageRepository, StorageRepository>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void Cleanup()
        {
            var dbContext = _serviceProvider.GetService<DBContext>();
            dbContext.Database.EnsureDeleted();
        }

        public void SetupDb(IServiceScope scope)
        {
            var scopedServices = scope.ServiceProvider;
            _postRepository = scopedServices.GetRequiredService<IPostRepository<PostDto>>();
            _storageRepository = scopedServices.GetRequiredService<IStorageRepository>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
        }

        [Test]
        [TestCase(1,1)]
        [TestCase(2, 1)]
        public async Task GetAllPost_ShouldReturnPostWithMediaContent(int profileId, int visitorId)
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var mediaContent = MediaContentData.GetMediaContents();
            var personalPost = PersonalPostData.GetPersonalPosts(profileId, visitorId, mediaContent);//This returns 15 items for the actual user
            var personal = PersonalData.GetUsers();

            _dbContext.AddRange(personal);
            _dbContext.AddRange(personalPost);

            await _dbContext.SaveChangesAsync();             

            var result = await _postRepository.GetAllPost(profileId, visitorId); //Itt sajnos már nincsenek összekötve, a tesztelendő metódusban

            var expected = result.Data.First();
            
             Assert.IsNotNull(expected.Post.MediaContent);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        //Returns 10 item depending on the visited person's posts. 
        public async Task GetAllPost_Returns10Item(int postedOnId, int visitorId)
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var personalPost = PersonalPostData.GetPersonalPosts(postedOnId, visitorId);//This returns 15 items for the actual user
            var personal = PersonalData.GetUsers();
            _dbContext.AddRange(personal);
            _dbContext.AddRange(personalPost);
            _dbContext.SaveChanges();

            var result = await _postRepository.GetAllPost(postedOnId, visitorId);
            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Count, Is.EqualTo(10));
                Assert.That(result.TotalPages, Is.EqualTo(2));
                Assert.That(result.Data.All(p => p.PostedToUserId == postedOnId));
            });
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        public async Task GetAllPost_ReturnsRemainingPostForTheSecondPageOfPaginator(int postedOnId, int visitorId)
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var personalPost = PersonalPostData.GetPersonalPosts(postedOnId, visitorId); //This returns 15 items for the actual user
            var personal = PersonalData.GetUsers();
            _dbContext.AddRange(personal);
            _dbContext.AddRange(personalPost);
            _dbContext.SaveChanges();

            var result = await _postRepository.GetAllPost(postedOnId, visitorId, 2);
            //Returns 5 items because the PersonalPostData.GetPersonalPosts returns 15 post in total, and de default value for the paginator itemperRequest is 10.
            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Count, Is.EqualTo(5));
                Assert.That(result.TotalPages, Is.EqualTo(2));
                Assert.That(result.Data.All(p => p.PostedToUserId == postedOnId));
            });
        }

        [Test]
        public async Task GetPostByTokenAsync_ShouldReturnPostWithGivenToken()
        {
            
            string Token = "8376f337-f14c-48ed-a394-c58e9b22238c";

            var result = _postRepository.GetPostByTokenAsync(Token);

            Assert.That(result, Is.Not.Null);
        }
    }
}
