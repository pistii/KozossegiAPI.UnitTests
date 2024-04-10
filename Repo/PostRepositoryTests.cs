using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
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
    public class PostRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IPostRepository<PostDto> postRepository;
        public DBContext _dbContext = new();

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DBContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            services.AddScoped<IPostRepository<PostDto>, PostRepository>();

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
            postRepository = scopedServices.GetRequiredService<IPostRepository<PostDto>>();
            _dbContext = scopedServices.GetRequiredService<DBContext>();
        }

        public static IEnumerable<Personal> GetUsers()
        {
            var users = new List<Personal>() {
                new Personal()
                {
                    id = 1,
                    firstName = "Gipsz",
                    lastName = "Jakab",
                    isMale = false,
                },
                new Personal()
                {
                    id = 2,
                    firstName = "Teszt",
                    lastName = "Elek",
                    isMale = false,
                }
                }.AsEnumerable();
            return users;
        }


        public static IEnumerable<PersonalPost> GetPersonalPosts()
        {
            var personalposts = new List<PersonalPost>()
            {
                new PersonalPost()
                {
                   personalPostId = 1,
                   postId = 1,
                   personId = 1,
                   Posts = new Post()
                    {
                       Id = 1,
                       SourceId = 1,
                       DateOfPost = DateTime.Now,
                       Likes = 16,
                       Dislikes = 1,
                    },
                },
                new PersonalPost()
                {
                   personalPostId = 2,
                   postId = 2,
                   personId = 2,
                   Posts = new Post()
                   {
                       Id = 2,
                       SourceId = 2,
                       DateOfPost = DateTime.Now,
                       Likes = 12,
                       Dislikes = 0,
                   },
                }
            };
            return personalposts;
        }

        public static IEnumerable<Post> GetPosts()
        {
            var posts = new List<Post>()
            {
                new Post()
                {
                   Id = 1,
                   SourceId = 1,
                   DateOfPost = DateTime.Now,
                   Likes = 16,
                   Dislikes = 1,
                },
                new Post()
                {
                   Id = 2,
                   SourceId = 2,
                   DateOfPost = DateTime.Now,
                   Likes = 12,
                   Dislikes = 0,
                },
            };
            return posts;
        }

        public static IEnumerable<Comment> GetComments()
        {
            var comments = new List<Comment>()
            {
                new Comment()
                {
                    commentId = 1,
                    FK_AuthorId = 1,
                    CommentDate = DateTime.Now,
                    CommentText = "Test comment",
                    PostId = 1,
                }
            };
            return comments;
        }

        public static IEnumerable<MediaContent> GetMediaContents()
        {
            var mediaContents = new List<MediaContent>()
            {
                new MediaContent()
                {
                    Id = 1,
                    ContentType = ContentType.Image,
                    FileName = "teszt",
                    MediaContentId = 1
                }
            };
            return mediaContents;
        }

        [Test]
        [TestCase(1,1)]
        [TestCase(2, 1)]
        public async Task GetAllPost_ShouldReturnPostObject(int profileId, int userId)
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var personalposts = GetPersonalPosts();
            var comments = GetComments();
            var mediaContents = GetMediaContents();
            var persons = GetUsers();
            await _dbContext.AddRangeAsync(personalposts);
            await _dbContext.AddRangeAsync(comments);
            await _dbContext.AddRangeAsync(persons);
            await _dbContext.AddRangeAsync(mediaContents);
            await _dbContext.SaveChangesAsync();

            var result = await postRepository.GetAllPost(profileId, userId);

            var expected = result.First();
            if (profileId == 1)
            {
                Assert.That(expected.Likes, Is.EqualTo(16));
                Assert.That(expected.FullName, Is.EqualTo("Gipsz Jakab"));
                Assert.That(expected.PostComments.Any(c => c.CommentText.Contains("Test")));
                Assert.That(expected.MediaContents.Any(mc => mc.ContentType.Equals(ContentType.Image)));
            } else
            {
                Assert.That(expected.Likes, Is.EqualTo(12));
                Assert.That(expected.FullName, Is.EqualTo("Teszt Elek"));
                Assert.That(expected.PostComments.Count, Is.EqualTo(0));
                Assert.That(expected.MediaContents.Count, Is.EqualTo(0));
            }
        }

        [Test]
        [TestCase(1)] //Should return with comments
        [TestCase(2)] //Without comments
        public async Task GetPostWithCommentsById(int postId)
        {
            using var scope = _serviceProvider.CreateScope();
            SetupDb(scope);

            var posts = GetPosts();
            var comments = GetComments();
            await _dbContext.AddRangeAsync(comments);
            await _dbContext.AddRangeAsync(posts);
            await _dbContext.SaveChangesAsync();

            var result = await postRepository.GetPostWithCommentsById(postId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.PostComments, postId == 1 ? Is.Not.Empty : Is.Empty);
        }
    }
}
