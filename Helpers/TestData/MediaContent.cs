
using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class MediaContentData
    {
        public static IQueryable<MediaContent> GetMediaContents()
        {
            var mediaContents = new List<MediaContent>()
            {
                new MediaContent()
                {
                    Id = 1,
                    FK_PostId = 2,
                    FileSize = 1024,
                    MediaType = "image/png",
                    FileName = "teszt"
                }
            };
            return mediaContents.AsQueryable();
        }
    }
}
