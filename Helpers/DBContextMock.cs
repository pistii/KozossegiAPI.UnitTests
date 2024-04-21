using Microsoft.EntityFrameworkCore;
using Moq;

namespace KozossegiAPI.UnitTests.Helpers
{
    public class DbContextMock
    {
        public static Mock<DbSet<TData>> GetMock<TData>(List<TData> lstData) where TData : class
        {
            IQueryable<TData> lstDataQueryable = lstData.AsQueryable();
            Mock<DbSet<TData>> dbSetMock = new Mock<DbSet<TData>>();

            dbSetMock.As<IQueryable<TData>>().Setup(s => s.Provider).Returns(lstDataQueryable.Provider);
            dbSetMock.As<IQueryable<TData>>().Setup(s => s.Expression).Returns(lstDataQueryable.Expression);
            dbSetMock.As<IQueryable<TData>>().Setup(s => s.ElementType).Returns(lstDataQueryable.ElementType);
            dbSetMock.As<IQueryable<TData>>().Setup(s => s.GetEnumerator()).Returns(() => lstDataQueryable.GetEnumerator());
            dbSetMock.Setup(x => x.Add(It.IsAny<TData>())).Callback<TData>(lstData.Add);
            dbSetMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<TData>>())).Callback<IEnumerable<TData>>(lstData.AddRange);
            dbSetMock.Setup(x => x.Remove(It.IsAny<TData>())).Callback<TData>(t => lstData.Remove(t));
            dbSetMock.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TData>>())).Callback<IEnumerable<TData>>(ts =>
            {
                foreach (var t in ts) { lstData.Remove(t); }
            });

            return dbSetMock;
        }
    }
}
