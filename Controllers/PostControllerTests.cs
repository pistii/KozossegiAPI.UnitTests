using KozoskodoAPI.Controllers;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace KozossegiAPI.UnitTests.Controllers
{
    internal class PostControllerTests
    {
        private PostController postControllerMock;
        private static Mock<IPostRepository<PostDto>> _PostRepository = new();
        private IQueryable<PostDto> _posts;
        private static Mock<IChatRepository<ChatRoom, Personal>> _chatRepository = new();
        private static Mock<IStorageController> _storageController = new();

        [SetUp]
        public void Setup()
        {
            _PostRepository = new();
            postControllerMock = PostControllerMock.GetPostControllerMock(
                _PostRepository.Object,
                _chatRepository.Object,
                _storageController.Object);
            _posts = PostControllerMock.GetAllPostMock();
        }

        [Test]
        public async Task Get_UserRequestsPost_ReturnsOkStatusCodeForRequestedPostId()
        {
            var postWithRequestedId = new Post()
            {
                Id = 1,
                DateOfPost = DateTime.Parse("2023-01-01 10:10"),
            };
            _PostRepository.Setup(repo => repo.GetByIdAsync<Post>(It.IsAny<int>())).ReturnsAsync(postWithRequestedId);

            var response = await postControllerMock.Get(1);

            var okResult = response.Result as OkObjectResult;

            Assert.That(okResult.Value, Is.EqualTo(postWithRequestedId));
            Assert.IsInstanceOf<OkObjectResult>(okResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
            _PostRepository.Verify(x => x.GetByIdAsync<Post>(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [TestCase(9)]
        [TestCase(10)]
        public async Task GetAllPost_ReturnsAsMuchItemsAsItemPerRequest_ContainsTotalPages(int itemPerRequest)
        {
            var totalPost = _posts;
            var totalPages = totalPost.ToList().Count/itemPerRequest;
            var itemsToReturn = totalPost.Take(itemPerRequest).ToList();
            _PostRepository.Setup(repo => repo.GetAllPost(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(totalPost.ToList());
            _PostRepository.Setup(repo => repo.GetTotalPages(It.IsAny<List<PostDto>>(), It.IsAny<int>())).ReturnsAsync(totalPages);
            _PostRepository.Setup(repo => repo.Paginator(It.IsAny<List<PostDto>>(), It.IsAny<int>(), It.IsAny<int>())).Returns(itemsToReturn);

            var response = await postControllerMock.GetAllPost(1, 1, 1, itemPerRequest);
            
            Assert.That(response.Data.Count, Is.EqualTo(itemPerRequest));
            Assert.That(response.TotalPages, Is.EqualTo(5));
        }

        [Test]
        [TestCase(9, 3)]
        [TestCase(10, 1)]
        public async Task GetAllPost_ReturnsAsMuchItemsAsItemPerRequest_PageTwoContainsTotalPages(int itemPerRequest, int currentPage)
        {
            var totalPost = _posts;
            var totalPages = totalPost.ToList().Count / itemPerRequest;
            var itemsToReturn = totalPost.Skip(itemPerRequest*currentPage).Take(itemPerRequest).ToList();
            _PostRepository.Setup(repo => repo.GetAllPost(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(totalPost.ToList());
            _PostRepository.Setup(repo => repo.GetTotalPages(It.IsAny<List<PostDto>>(), It.IsAny<int>())).ReturnsAsync(totalPages);
            _PostRepository.Setup(repo => repo.Paginator(It.IsAny<List<PostDto>>(), It.IsAny<int>(), It.IsAny<int>())).Returns(itemsToReturn);

            var response = await postControllerMock.GetAllPost(1, 1, currentPage, itemPerRequest);

            Assert.That(response.Data.Count, Is.EqualTo(itemPerRequest));
            Assert.That(response.TotalPages, Is.EqualTo(5));
        }

        [Test]
        public async Task Post_CreateNewPostWithoutFile_ReturnsOkObjectResult()
        {
            var fakeDbContext = PostControllerMock.GetDBContextMock();
            var repo = new PostRepository(fakeDbContext.Object);
            Personal personal = new()
            {
                id = 1,
                firstName = "First",
                lastName = "last",
            };
            _PostRepository.Setup(repo => repo.GetByIdAsync<Personal>(It.IsAny<int>())).ReturnsAsync(personal);
            _PostRepository.Setup(repo => repo.InsertAsync<Post>(It.IsAny<Post>()));
            _PostRepository.Setup(repo => repo.InsertAsync<PersonalPost>(It.IsAny<PersonalPost>()));
            _PostRepository.Setup(repo => repo.SaveAsync());

            var createPost = new CreatePostDto(null, null, null)
            {
                userId = 1,
                SourceId = 1,
                postContent = "Test",
            };
            var result = await postControllerMock.Post(createPost);
            var okResult = result.Result as OkObjectResult;

            Assert.That(okResult.Value, Is.Not.Null);
            Assert.IsInstanceOf<OkObjectResult>(okResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        [Test]
        public async Task Post_CreateNewPostWithImage_ReturnsOkObjectResult()
        {
            var fakeDbContext = PostControllerMock.GetDBContextMock();
            var repo = new PostRepository(fakeDbContext.Object);

            //Get the test image
            string currentDirectory = Environment.CurrentDirectory;
            string projectRoot = Directory.GetParent(currentDirectory).Parent.Parent.FullName;
            string relativePath = Path.Combine("Helpers", "testAvatar.jpg");
            string absolutePath = Path.Combine(projectRoot, relativePath);
            //The image in bytes
            var image = File.ReadAllBytes(absolutePath);

            #region Prepare the test data
            Personal personal = new()
            {
                id = 1,
                firstName = "First",
                lastName = "last",
            };
            string fileName = "randomFileName";
            #endregion

            using (var stream = new MemoryStream(image.Length))
            {
                var file = new FormFile(stream, 0, image.Length, "name", absolutePath)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg"
                };
                //The entity for the method parameter
                var createPost = new CreatePostDto("name", "image/jpg", file)
                {
                    userId = 1,
                    SourceId = 1,
                    postContent = "Test",
                };

                _PostRepository.Setup(repo => repo.GetByIdAsync<Personal>(It.IsAny<int>())).ReturnsAsync(personal);
                _PostRepository.Setup(repo => repo.InsertSaveAsync<Post>(It.IsAny<Post>()));
                _storageController.Setup(repo => repo.AddFile(It.IsAny<FileUpload>(), It.IsAny<BucketSelector>())).ReturnsAsync(fileName);
                _PostRepository.Setup(repo => repo.InsertAsync<MediaContent>(It.IsAny<MediaContent>()));

                _PostRepository.Setup(repo => repo.InsertAsync<PersonalPost>(It.IsAny<PersonalPost>()));
                _chatRepository.Setup(repo => repo.GetChatPartenterIds(It.IsAny<int>())).Returns(new List<int>());
                _PostRepository.Setup(repo => repo.SaveAsync());

                var result = await postControllerMock.Post(createPost);
                var okResult = result.Result as OkObjectResult;

                Assert.That(okResult.Value, Is.Not.Null);
                Assert.IsInstanceOf<OkObjectResult>(okResult);
                Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);

                _PostRepository.VerifyAll();
                _storageController.Verify(r => r.AddFile(It.IsAny<FileUpload>(), It.IsAny<BucketSelector>()), Times.Once());
                _chatRepository.Verify(repo => repo.GetChatPartenterIds(It.IsAny<int>()), Times.Once());
            }
        }

        [Ignore("I'm not sure the notification sending is testable. But I'll leave it here in case of trying..")]
        [Test]
        public async Task Post_CreateNewPostWithoutFileAndSendsNotificationToCloserFriends_ReturnsOkObjectResult()
        {
            var fakeDbContext = PostControllerMock.GetDBContextMock();
            var repo = new PostRepository(fakeDbContext.Object);
            Personal personal = new()
            {
                id = 1,
                firstName = "First",
                lastName = "last",
            };
            _PostRepository.Setup(repo => repo.GetByIdAsync<Personal>(It.IsAny<int>())).ReturnsAsync(personal);
            _PostRepository.Setup(repo => repo.InsertAsync<Post>(It.IsAny<Post>()));
            _PostRepository.Setup(repo => repo.InsertAsync<PersonalPost>(It.IsAny<PersonalPost>()));
            _PostRepository.Setup(repo => repo.SaveAsync());

            var createPost = new CreatePostDto(null, null, null)
            {
                userId = 1,
                SourceId = 1,
                postContent = "Test",
            };
            var result = await postControllerMock.Post(createPost);
            var okResult = result.Result as OkObjectResult;

            Assert.That(okResult.Value, Is.Not.Null);
            Assert.IsInstanceOf<OkObjectResult>(okResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        [Test]
        public async Task Put_ModifiesPostOnlyContentChanges_ReturnsOkResult()
        {
            var fakeDbContext = PostControllerMock.GetDBContextMock();
            var repo = new PostRepository(fakeDbContext.Object);
            var post = new Post()
            {
                Id = 100    ,
                SourceId = 1,
                PostContent = "Test"
                //PostComments = new List<Comment>()
                //{
                //    new Comment()
                //    {
                //        FK_AuthorId = 2,
                //        commentId = 1,
                //        CommentText = "I like your post",
                //        CommentDate = DateTime.Parse("2023-01-01 10: 12"),
                //        PostId = 1,
                //    }
                //}
            };
            fakeDbContext.Object.Post.Add(post);


            _PostRepository.Setup(repo => repo.GetByIdAsync<Post>(It.IsAny<int>())).ReturnsAsync(post);
            _PostRepository.Setup(repo => repo.InsertSaveAsync(It.IsAny<Post>()));

            var param = new CreatePostDto(null, null, null)
            {
                SourceId = 1,
                postContent = "Changed.",
            };
            var result = await postControllerMock.Put(100, param);
            var postChanges = fakeDbContext.Object.Post.FirstOrDefault(x => x.Id == 100);

            var okResult = result as OkResult;

            Assert.That(postChanges.PostContent, Is.EqualTo(param.postContent));
            Assert.IsInstanceOf<OkResult>(okResult);
            Assert.AreEqual(okResult.StatusCode, StatusCodes.Status200OK);
        }

        [Test]
        public async Task Delete_RemovesPostAndPersonalConnectionTable_IfPostExistsWithId()
        {
            var post = new Post()
            {
                Id = 100,
                SourceId = 1,
                PostContent = "Test"
            };
            var personalPost = new PersonalPost()
            {
                personalPostId = 1,
                personId = 1,
                postId = 100,
            };
            _PostRepository.Setup(repo => repo.GetByIdAsync<Post>(It.IsAny<int>())).ReturnsAsync(post);
            _PostRepository.Setup(repo => repo.GetByIdAsync<PersonalPost>(It.IsAny<int>())).ReturnsAsync(personalPost);

            var result = await postControllerMock.Delete(100);
            var okResult = result as OkResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }
    }
}
