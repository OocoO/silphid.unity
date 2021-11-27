﻿using System;
using NUnit.Framework;
using UniRx;

namespace Silphid.Sequencit.Test
{
    public abstract class SequencingTestBase
    {
        protected int _value;
        protected TestScheduler _scheduler;

        protected ICompletable CreateTimer(int ticks) =>
            Completable.Timer(TimeSpan.FromTicks(ticks), _scheduler);

        protected Action ShouldNotReachThisPoint => () => Assert.Fail("Should not reach this point");

        [SetUp]
        public virtual void Setup()
        {
            _value = 0;
            _scheduler = new TestScheduler();
        }

        [Test]
        public void CreateDelay_EnsureTestHelperBehavesAsExpected()
        {
            CreateTimer(10)
               .DoOnCompleted(() => _value = 123)
               .ObserveOn(_scheduler)
               .Subscribe();

            AssertValue(0);

            _scheduler.AdvanceTo(5);
            AssertValue(0);

            _scheduler.AdvanceTo(9);
            AssertValue(0);

            _scheduler.AdvanceBy(10);
            AssertValue(123);
        }

        protected void AssertValue(int expected)
        {
            Assert.That(_value, Is.EqualTo(expected));
        }
    }
}