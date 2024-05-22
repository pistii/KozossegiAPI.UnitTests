using KozoskodoAPI.Controllers;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KozossegiAPI.UnitTests.Controllers
{
    [TestFixture]
    public class CommentControllerTests
    {
        public Mock<IPostRepository<Comment>> _postRepositoryMock = new();
        public CommentController commentController;

        public CommentControllerTests()
        {
            commentController = new(_postRepositoryMock.Object);
        }

        [SetUp]
        public void Setup()
        {
            _postRepositoryMock = new();
            commentController = new(_postRepositoryMock.Object);
        }

        [Test]
        public async Task Get_ReturnsASingleCommentById()
        {
            var comment = new Comment()
            {
                commentId = 1,
                PostId = 1,
                CommentText = "Test",
                CommentDate = DateTime.Now,
            };
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync<Comment>(It.IsAny<int>())).ReturnsAsync(comment);

            var result = await commentController.Get(1);
            var okResult = result as OkObjectResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result, Is.Not.Null);
            _postRepositoryMock.Verify(x => x.GetByIdAsync<Comment>(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public async Task Post_UserWritesANewComment_SuccessfullyAddsCommentToPost()
        {
            var user = new Personal()
            {
                id = 1,
                firstName = "Teszt",
                lastName = "Elek"
            };
            var post = new Post()
            {
                Id = 1,
                PostContent = "post text",
                DateOfPost = DateTime.Parse("2023-01-01 10:12"),
                PostComments = new List<Comment>()
                {
                    new Comment()
                    {
                        FK_AuthorId = 1,
                        PostId = 1,
                        CommentDate =  DateTime.Parse("2023-01-01 10:13"),
                        CommentText = "comment text",
                        commentId = 16,
                    }
                }
            };

            _postRepositoryMock.Setup(repo => repo.GetByIdAsync<Personal>(It.IsAny<int>())).ReturnsAsync(user);
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync<Post>(It.IsAny<int>())).ReturnsAsync(post);
            _postRepositoryMock.Setup(repo => repo.InsertSaveAsync(It.IsAny<Comment>()));

            NewCommentDto dto = new()
            {
                commenterId = 1,
                commentTxt = "This is a great test!",
                postId = 1
            };
            var result = await commentController.Post(dto);

            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.InstanceOf<Comment>());
            _postRepositoryMock.Verify(x => x.GetByIdAsync<Personal>(It.IsAny<int>()), Times.Once());
            _postRepositoryMock.Verify(x => x.GetByIdAsync<Post>(It.IsAny<int>()), Times.Once());
            _postRepositoryMock.Verify(x => x.InsertSaveAsync(It.IsAny<Comment>()), Times.Once());
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_RemovesCommentWhenUserDeletes_ReturnsOkResult(int commentId)
        {
            var comment = new Comment()
            {
                FK_AuthorId = 1,
                PostId = 1,
                CommentDate = DateTime.Parse("2023-01-01 10:13"),
                CommentText = "comment text",
                commentId = 16,
            };
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync<Comment>(It.IsAny<int>())).ReturnsAsync(comment);
            
            var result = await commentController.Delete(commentId);
            var okResult = result as OkResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task Put_ModifiesCommentByPostId_ReturnsOkResultWhenPostIsFound()
        { 
            NewCommentDto changedComment = new()
            {
                CommentId = 1,
                commenterId = 1,
                commentTxt = "This is a great test!",
                postId = 1
            };
            Comment originalComment = new Comment()
            {
                commentId = 1,
                FK_AuthorId = 1,
                CommentText = "Hello",
                PostId = 1,
                CommentDate = DateTime.Parse("2023-01-12 10:13")
            };

            _postRepositoryMock.Setup(repo => repo.GetByIdAsync<Comment>(It.IsAny<int>())).ReturnsAsync(originalComment);
            _postRepositoryMock.Setup(repo => repo.UpdateThenSaveAsync(It.IsAny<Comment>()));

            var result = await commentController.Put(1, changedComment);

            var okResult = result as OkResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(originalComment.CommentText, Is.EqualTo(changedComment.commentTxt));
            _postRepositoryMock.Verify(x => x.GetByIdAsync<Comment>(It.IsAny<int>()), Times.Once());
            _postRepositoryMock.Verify(x => x.UpdateThenSaveAsync(It.IsAny<Comment>()), Times.Once());
        }
    }
}
