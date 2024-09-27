
using KozossegiAPI.Models;

namespace KozossegiAPI.UnitTests.Helpers.TestData
{
    public static class SettingData
    {
        public static IQueryable<Settings> GetSettings()
        {
            var i = 1;
            List<Settings> settings = new List<Settings>();
            while (i < 10)
            {
                var setting = new Settings()
                {
                    FK_UserId = i,
                    PK_Id = i,
                    NextReminder = DateTime.Now
                };
                settings.Add(setting);
                i++;
            }

            return settings.AsQueryable();            
        }
    }
}
