using System.Collections.Generic;
using FluentAssert;
using NUnit.Framework;

namespace RunTimeTypeCreator.Tests
{
    [TestFixture]
    public class RunTimeTypeCreatorTests
    {
        [Test]
        public void CanLoadAndExecuteType()
        {
            const string source = @"
using RunTimeTypeCreator.Tests;

public class TestTypeClass : RunTimeTypeCreatorTests.ITestType
{
    public bool Success { get { return true; } }
}";
            List<string> compilationErrors;
            var type = RunTimeTypeCreator.CreateType<ITestType>(source, 
                new[]{"RunTimeTypeCreator.Tests.dll"}, 
                out compilationErrors);
            
            type.Success.ShouldBeTrue();
        }

        public static bool TestVariable = false;

        [Test]
        public void CanAccessVariablesInThisDomain()
        {
            const string source = @"
using RunTimeTypeCreator.Tests;

public class TestTypeClass : RunTimeTypeCreatorTests.ITestType
{
    public bool Success { get { return RunTimeTypeCreatorTests.TestVariable; } }
}";
            List<string> compilationErrors;
            var type = RunTimeTypeCreator.CreateType<ITestType>(source,
                new[] { "RunTimeTypeCreator.Tests.dll" },
                out compilationErrors);

            TestVariable = false;
            type.Success.ShouldBeFalse();
            TestVariable = true;
            type.Success.ShouldBeTrue();
        }

        [Test]
        public void ReturnsCompilationErrors()
        {
            const string source = "this wont compile";

            List<string> compilationErrors;
            var type = RunTimeTypeCreator.CreateType<ITestType>(source,
                new[] { "RunTimeTypeCreator.Tests.dll" },
                out compilationErrors);

            compilationErrors.ShouldNotBeNull();
            compilationErrors.Count.ShouldBeGreaterThan(0);
            type.ShouldBeNull();
        }

        public interface ITestType
        {
            bool Success { get; }
        }
    }
}

