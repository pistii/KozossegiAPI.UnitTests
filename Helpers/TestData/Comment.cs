using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class CommentData
    {
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
    }
}
