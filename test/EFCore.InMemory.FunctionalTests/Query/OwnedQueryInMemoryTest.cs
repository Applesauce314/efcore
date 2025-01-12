// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedQueryInMemoryTest : OwnedQueryTestBase<OwnedQueryInMemoryTest.OwnedQueryInMemoryFixture>
{
    public OwnedQueryInMemoryTest(OwnedQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public class OwnedQueryInMemoryFixture : OwnedQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
