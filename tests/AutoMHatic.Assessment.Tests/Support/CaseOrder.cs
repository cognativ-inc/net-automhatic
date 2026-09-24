using AutoMHatic.Assessment.Tests;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestCaseOrderer(typeof(CaseOrderer))]
[assembly: TestCollectionOrderer(typeof(CollectionOrderer))]

namespace AutoMHatic.Assessment.Tests;

// Runs exercises in order (Exercise01 first) and each exercise's cases in order (C01 first),
// so the first failure reported is the next case to work on.
public sealed class CaseOrderer : ITestCaseOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase =>
        [.. testCases.OrderBy(testCase => testCase.TestMethodName, StringComparer.Ordinal)
            .ThenBy(testCase => testCase.TestCaseDisplayName, StringComparer.Ordinal)];
}

public sealed class CollectionOrderer : ITestCollectionOrderer
{
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(
        IReadOnlyCollection<TTestCollection> testCollections)
        where TTestCollection : ITestCollection =>
        [.. testCollections.OrderBy(collection => collection.TestCollectionDisplayName, StringComparer.Ordinal)];
}
