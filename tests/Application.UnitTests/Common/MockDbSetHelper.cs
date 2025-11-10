using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Application.UnitTests.Common;

/// <summary>
/// Helper class to create mock DbSets for testing
/// </summary>
public static class MockDbSetHelper
{
    public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
        mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(data.AddRange);
        mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity => data.Remove(entity));
        mockSet.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(entities =>
        {
            foreach (var entity in entities.ToList())
            {
                data.Remove(entity);
            }
        });

        return mockSet;
    }
}
