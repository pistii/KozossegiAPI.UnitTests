using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class PersonalPostData
    {
        /// <summary>
        /// Gets a personalPost entity with it's child Posts. The ToUserId is for getAllPost identifies the user's profile where the post have been posted while the author is the person who posted it onto the toUserIds profile.
        /// </summary>
        /// <param name="toUserId"></param>
        /// <param name="authorId"></param>
        /// <returns>
        /// Returns 50 entity, from this 15 is the person's post where have been posted
        /// </returns>
        public static IQueryable<PersonalPost> GetPersonalPosts(int toUserId, int authorId, IQueryable<MediaContent>? mediaContent = null)
        {
            int i = 1;
            var personalposts = new List<PersonalPost>();
            while (i < 50)
            {
                //Add post to the testing users There should be 15 post attached to the touserId
                if (i < 16)
                {
                    var post = new PersonalPost()
                    {
                        PersonalPostId = i,
                        PostId = i,
                        PostedToId = toUserId,
                        AuthorId = authorId,
                        Posts = new Post()
                        {
                            Id = i,
                            Token = Guid.NewGuid().ToString(),
                            DateOfPost = DateTime.Now,
                            Likes = 1,
                            Dislikes = 0,
                            //MediaContent = mediaContent.FirstOrDefault(p => p.FK_PostId == i)
                            MediaContent = new MediaContent()
                            {
                                Id = i,
                                FK_PostId = i,
                                FileName = "image",
                                MediaType = "image/jpg"
                            }
                        }
                    };
                    personalposts.Add(post);
                }
                else
                {
                    var post = new PersonalPost()
                    {
                        PersonalPostId = i,
                        PostId = i,
                        PostedToId = i,
                        AuthorId = i,
                        Posts = new Post()
                        {
                            Id = i,
                            Token = Guid.NewGuid().ToString(),
                            DateOfPost = DateTime.Now,
                            Likes = 1,
                            Dislikes = 0
                        }
                    };
                    personalposts.Add(post);
                }
                i++;
            }
            return personalposts.AsQueryable();
        }
    }
}
