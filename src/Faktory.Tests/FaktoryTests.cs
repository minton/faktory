﻿using System;
using Faktory.Logging;
using NUnit.Framework;

namespace Faktory.Tests
{
    [TestFixture]
    public class FaktoryTests
    {
        static readonly Action<string> UpdateStatus = s => TestContext.Progress.WriteLine(s);
        static readonly TestLogWriter LogWriter = new TestLogWriter();

        [OneTimeSetUp]
        public void Init()
        {
            FaktoryRunner.LogWriter = LogWriter;
        }

        [Test]
        public void WithoutOverrideRunBuild_ItThrows()
        {
            FaktoryRunner.BootUp("", UpdateStatus);
            var faktory = new TestFaktoryWithOutOverridingRunBuild();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "Please override the Build() method.");
        }

        [Test]
        public void WithoutCallingExecute_Runs()
        {
            FaktoryRunner.BootUp("", UpdateStatus);
            var faktory = new TestFaktoryWithOutCallingExecute();
            FaktoryRunner.Run(faktory);

            Assert.False(faktory.Executed);
            CollectionAssert.Contains(LogWriter.AllMessages, "Please call Execute() method.");
        }

        [Test]
        public void WithoutAnyTasks_Runs()
        {
            FaktoryRunner.BootUp("", UpdateStatus);
            var faktory = new TestFaktoryWithNoTasks();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "No Tasks found.");
        }

        [Test]
        public void WithOutRequiredParams_ShowError()
        {
            FaktoryRunner.BootUp("", UpdateStatus);
            var faktory = new TestFaktoryWithRequiredParameter();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "Missing required argument 'required'");
        }

        [Test]
        public void WithRequiredButInvalidValueParams_DisplaysError()
        {
            FaktoryRunner.BootUp("required", UpdateStatus);
            var faktory = new TestFaktoryWithRequiredParameter();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "Argument 'required' has invalid value.");

        }

        [Test]
        public void WithRequiredParams_Runs()
        {
            FaktoryRunner.BootUp("required=yes", UpdateStatus);
            var faktory = new TestFaktoryWithRequiredParameter();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "Options: [{'required'->'yes'}]");
        }

        [Test]
        public void MultipleParams_PrintsOptionsOnRun()
        {
            FaktoryRunner.BootUp("A=1 B=2 C=3", UpdateStatus);
            var faktory = new TestFaktoryWithNoTasks();
            FaktoryRunner.Run(faktory);

            CollectionAssert.Contains(LogWriter.AllMessages, "Options: [{'A'->'1'}{'B'->'2'}{'C'->'3'}]");
        }

        [Test]
        public void WithMultipleTasksButFirstFails_Throws()
        {
            FaktoryRunner.BootUp("", UpdateStatus);
            var faktory = new TestFaktoryWithMultipleTasksFirstFails();
            faktory.SetStatusUpdater(UpdateStatus);
            FaktoryRunner.Run(faktory);

            Assert.That("Exception of type 'System.Exception' was thrown.", Is.Not.Null.And.Matches(new ContainsTrimmed(LogWriter.AllMessages)));
            Assert.That("Second task ran!", Is.Not.Null.And.Not.Matches(new ContainsTrimmed(LogWriter.AllMessages)));
        }
    }

    public class TestFaktoryWithOutOverridingRunBuild : Faktory { }

    public class TestFaktoryWithOutCallingExecute : Faktory
    {
        protected override void RunBuild() { }
    }

    public class TestFaktoryWithNoTasks : Faktory
    {
        protected override void RunBuild() { Execute(); }
    }

    public class TestFaktoryWithRequiredParameter : Faktory
    {
        protected override void RunBuild()
        {
            Requires("required");
        }
    }

    public class TestFaktoryWithMultipleTasksFirstFails : Faktory
    {
        protected override void RunBuild()
        {
            Run(() => throw new Exception())
                .Then(() => Log("Second task ran!"))
                .Execute();
        }
    }
}