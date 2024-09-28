using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class StudyData
    {
        public static List<Study> GetStudies()
        {
            return new List<Study>()
            {
                new Study()
                {
                    PK_Id = 1,
                    FK_UserId = 1,
                    Class = "class1",
                    EndYear = 2012,
                    StartYear = 2008,
                    SchoolName = "School"
                },
                new Study()
                {
                    PK_Id = 2,
                    FK_UserId = 1,
                    Class = "class2",
                    StartYear = 2013,
                    EndYear = 2016,
                    SchoolName = "School"
                },
                new Study()
                {
                    PK_Id = 3,
                    FK_UserId = 2,
                    Class = "class3",
                    StartYear = 2013,
                    EndYear = 2016,
                    SchoolName = "School"
                }
            };
        }
    }
}
