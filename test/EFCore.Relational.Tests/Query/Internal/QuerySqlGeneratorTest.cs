// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QuerySqlGeneratorTest
    {
        [Theory]
        [InlineData("INSERT something")]
        [InlineData("SELECTANDSOMEOTHERSTUFF")]
        [InlineData("SELECT")]
        [InlineData("SELEC")]
        [InlineData("- bad comment\nSELECT something")]
        [InlineData("SELECT-\n1")]
        [InlineData("")]
        [InlineData("--SELECT")]
        public void CheckComposableSql_throws(string sql)
            => Assert.Equal(
                RelationalStrings.FromSqlNonComposable,
                Assert.Throws<InvalidOperationException>(
                    () => CreateDummyQuerySqlGenerator().CheckComposableSql(sql)).Message);

        [Theory]
        [InlineData("SELECT something")]
        [InlineData("   SELECT something")]
        [InlineData("-- comment\n SELECT something")]
        [InlineData("-- comment1\r\n --\t\rcomment2\r\nSELECT something")]
        [InlineData("SELECT--\n1")]
        [InlineData("  /* comment */ SELECT--\n1")]
        [InlineData("  /* multi\n*line\r\n * comment */ \nSELECT--\n1")]
        [InlineData("SELECT/* comment */1")]
        public void CheckComposableSql_does_not_throw(string sql)
            => CreateDummyQuerySqlGenerator().CheckComposableSql(sql);

        private DummyQuerySqlGenerator CreateDummyQuerySqlGenerator()
            => new DummyQuerySqlGenerator(
                new QuerySqlGeneratorDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            new TestRelationalTypeMappingSource(
                                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies())));

        private class DummyQuerySqlGenerator : QuerySqlGenerator
        {
            public DummyQuerySqlGenerator([NotNull] QuerySqlGeneratorDependencies dependencies)
                : base(dependencies)
            {
            }
            public new void CheckComposableSql(string sql)
                => base.CheckComposableSql(sql);
        }
        [Fact]
        public void VisitSqlFunction_without_package()
        {

            var functionExpression = new SqlFunctionExpression("dbo", "NiladicMethodFoo1", false, typeof(int), null);
            var command = VisitSqlFunctionTestBase(functionExpression);
            Assert.Equal("SELECT \"dbo\".\"NiladicMethodFoo1\"", command.CommandText);
        }


        [Fact]
        public void VisitSqlFunction_with_package_and_namedarguments()
        {
            var arguments = new Dictionary<SqlExpression, bool>() {
                {new SqlExpressions.SqlParameterExpression(Expression.Parameter(typeof(string),"widgetName"),new Microsoft.EntityFrameworkCore.Storage.StringTypeMapping("VARCHAR",System.Data.DbType.String)), false},
                {new SqlExpressions.SqlParameterExpression(Expression.Parameter(typeof(int),"widgetCount"),new Microsoft.EntityFrameworkCore.Storage.IntTypeMapping("INTEGER",System.Data.DbType.Int32)), false}  };

            var functionExpression = new SqlFunctionExpression("dbo", "barpkg", "MethodFoo1", arguments.Keys, false, arguments.Values, typeof(int), null);

            var command = VisitSqlFunctionTestBase(functionExpression);

            Assert.Equal("SELECT \"dbo\".\"barpkg\".\"MethodFoo1\"(@widgetName, @widgetCount)", command.CommandText);
        }

        [Fact]
        public void VisitSqlFunction_with_package()
        {

            var functionExpression = new SqlFunctionExpression("dbo", "barpkg", "NiladicMethodFoo1", false, typeof(int), null);
            var command = VisitSqlFunctionTestBase(functionExpression);
            Assert.Equal("SELECT \"dbo\".\"barpkg\".\"NiladicMethodFoo1\"", command.CommandText);
        }

        private IRelationalCommand VisitSqlFunctionTestBase(SqlFunctionExpression functionExpression)
        {
            var selectExpression = new SelectExpression(functionExpression);
            selectExpression.ApplyProjection();
            var querySqlGenerator = CreateDummyQuerySqlGenerator();
            var command = querySqlGenerator.GetCommand(selectExpression);
            return command;
        }
    }
}
