using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Controllers;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KozoskodoAPI.Data;
using KozossegiAPI.Controllers.Cloud;

namespace KozossegiAPI.UnitTests.Helpers
{
    internal class PostControllerMock
    {
        private static Mock<INotificationRepository> _InotificationRepository = new();

        public static PostController GetPostControllerMock(IPostRepository<PostDto> postRepository, IChatRepository<ChatRoom, Personal> chatRepository, IStorageController storageController)
        {
            PostController _postControllerMock = new(
                storageController,
                _InotificationRepository.Object,
                postRepository,
                chatRepository
            );

            return _postControllerMock;
        }

        #region methods to GetAllPost method. Returns a PostDto list with 50 entities
        public static IQueryable<PostDto> GetAllPostMock()
        {
            var postDtos = new List<PostDto>();
            int i = 0;
            while (i < 50)
            {
                postDtos.Add(GetPost(i));
                i++;
            }
            return postDtos.AsQueryable();
        }

        public static PostDto GetPost(int id)
        {
            var post = new PostDto()
            {
                PostId = 1,
                FullName = "John Doe " + id,
            };
            return post;
        }
        #endregion

        public static List<Post> GetPosts()
        {
            var posts = new List<Post>();
            int i = 0;
            while (i < 50)
            {
                var post = new Post();
                post.Id = i;
                post.PostContent = $"This is the {i}.rd/th post";
                post.SourceId = i;
                posts.Add(post);
                i++;
            }
            return posts;

        }

        public static List<Personal> GetPersonals()
        {
            //Creates a list with 49 "personal" table
            var personals = new List<Personal>();
            int i = 0;
            while (i < 50)
            {
                var person = new Personal();
                person.id = i;
                person.firstName = "First" + i.ToString();
                person.lastName = "Last" + i.ToString();
                personals.Add(person);
                i++;
            }
            return personals;
        }

        public static List<PersonalPost> GetPersonalPosts()
        {
            var personalPosts = new List<PersonalPost>();
            int i = 0;
            while (i < 50)
            {
                var personalPost = new PersonalPost();
                personalPost.personalPostId = i;
                personalPost.personId = i;
                personalPost.postId = i;
                i++;
            }
            return personalPosts;
        }

        public static Mock<DBContext> GetDBContextMock()
        {          
            var dbContext = new Mock<DBContext>();

            var personalMockSet = MockDbSetFactory.Create<Personal>(GetPersonals());
            var personalPostMockSet = MockDbSetFactory.Create<PersonalPost>(GetPersonalPosts());
            var postMockSet = MockDbSetFactory.Create<Post>(GetPosts());

            dbContext.Setup(x => x.Personal).Returns(personalMockSet);
            dbContext.Setup(x => x.PersonalPost).Returns(personalPostMockSet);
            dbContext.Setup(x => x.Post).Returns(postMockSet);

            return dbContext;
        }
    }
}
