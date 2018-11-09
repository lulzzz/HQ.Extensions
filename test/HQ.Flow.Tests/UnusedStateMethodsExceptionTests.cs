﻿// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using HQ.Flow.Tests.Fixtures;
using HQ.Flow.Tests.States;
using Xunit;

namespace HQ.Flow.Tests
{
    public class UnusedStateMethodsExceptionTests
    {
        [Fact]
        public void Constructor_throws_when_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var exception = new UnusedStateMethodsException(null);
                exception.GetObjectData(null, new StreamingContext());
            });
        }

        [Fact]
        public void GetObjectData_throws_when_null()
        {
            using (new StateProviderFixture())
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    var exception = new UnusedStateMethodsException(typeof(MissingStateForStateMethod).GetMethods());
                    exception.GetObjectData(null, new StreamingContext());
                });
            }
        }

        [Fact]
        public void Round_trip_serialization_test()
        {
            var left = new UnusedStateMethodsException(typeof(MissingStateForStateMethod).GetMethods());
            var buffer = new byte[4096];

            var formatter = new BinaryFormatter();

            using (var serialized = new MemoryStream(buffer))
            {
                using (var deserialized = new MemoryStream(buffer))
                {
                    formatter.Serialize(serialized, left);

                    var right = (UnusedStateMethodsException) formatter.Deserialize(deserialized);

                    Assert.Equal(left.StateMethods, right.StateMethods);
                    Assert.Equal(left.InnerException?.Message, right.InnerException?.Message);
                    Assert.Equal(left.Message, right.Message);
                }
            }
        }
    }
}
