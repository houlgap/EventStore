// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using EventStore.Projections.Core.Messages;
using EventStore.Projections.Core.Services.Processing;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.core_service
{
    [TestFixture]
    public class when_unsubscribing_a_subscribed_projection : TestFixtureWithProjectionCoreService
    {
        private TestCoreProjection _committedeventHandler;
        private Guid _projectionCorrelationId;

        private TestCoreProjection _committedeventHandler2;
        private Guid _projectionCorrelationId2;

        [SetUp]
        public new void Setup()
        {
            _committedeventHandler = new TestCoreProjection();
            _committedeventHandler2 = new TestCoreProjection();
            _projectionCorrelationId = Guid.NewGuid();
            _projectionCorrelationId2 = Guid.NewGuid();
            _service.Handle(
                new ProjectionSubscriptionManagement.Subscribe(
                    _projectionCorrelationId, _committedeventHandler, CheckpointTag.FromPosition(0, 0),
                    CreateCheckpointStrategy(), 1000));
            _service.Handle(
                new ProjectionSubscriptionManagement.Subscribe(
                    _projectionCorrelationId2, _committedeventHandler2, CheckpointTag.FromPosition(0, 0),
                    CreateCheckpointStrategy(), 1000));
            // when
            _service.Handle(new ProjectionSubscriptionManagement.Unsubscribe(_projectionCorrelationId));
        }

        [Test]
        public void committed_events_are_no_longer_distributed_to_the_projection()
        {
            _service.Handle(
                new ProjectionCoreServiceMessage.CommittedEventDistributed(
                    _projectionCorrelationId, new EventPosition(10, 5), "test", -1, false, CreateEvent()));
            Assert.AreEqual(0, _committedeventHandler.HandledMessages.Count);
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void the_projection_cannot_be_resumed()
        {
            _service.Handle(new ProjectionSubscriptionManagement.Resume(_projectionCorrelationId));
            _service.Handle(
                new ProjectionCoreServiceMessage.CommittedEventDistributed(
                    _projectionCorrelationId, new EventPosition(10, 5), "test", -1, false, CreateEvent()));
            Assert.AreEqual(0, _committedeventHandler.HandledMessages.Count);
        }
    }
}
