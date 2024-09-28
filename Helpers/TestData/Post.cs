using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class PostData
    {
        public static IEnumerable<Post> GetPosts()
        {
            var posts = new List<Post>()
            {
                new Post()
                {
                    Id = 1,
                    Token = "8376f337-f14c-48ed-a394-c58e9b22238c",
                    DateOfPost = DateTime.Now,
                    PostContent = "This is a test!",
                    Likes = 16,
                    Dislikes = 1,
                },
                new Post()
                {
                       Id = 2,
                       Token = Guid.NewGuid().ToString(),
                       DateOfPost = DateTime.Now,
                       Likes = 16,
                       Dislikes = 1,
                },
                new Post()
                {
                    Id = 3,
                    Token = Guid.NewGuid().ToString(),
                    DateOfPost = DateTime.Now,
                    Likes = 1,
                    Dislikes = 0,
                },
            };
            return posts;
        }

        public static List<Post> GetManyPost(int qt)
        {
            int items = 1;
            List < Post > posts = new();
            while (items <= qt)
            {
                items++;
                posts.Add(
                    new Post() {
                    Token = Guid.NewGuid().ToString(),
                    PostContent = "Test " + items
                });
            }

            return posts;
        }
    }
}
