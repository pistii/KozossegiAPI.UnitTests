﻿using KozoskodoAPI.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KozossegiAPI.UnitTests.Helpers
{
    public static class MockDbSetFactory
    {
        // Creates a mock DbSet from the specified data.
        public static DbSet<T> Create<T>(List<T> data) where T : class
        {
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            dbSetMock.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(item => data.Add(item));
            dbSetMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(items => data.AddRange(items));

            dbSetMock.Setup(x => x.AddRange(data));

            return dbSetMock.Object;
        }
    }
}
