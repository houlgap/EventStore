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
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;

namespace EventStore.Projections.Core.Services.Processing
{
    class GetAllStatesWorkItem : WorkItem
    {
        private readonly IEnvelope _envelope;
        private readonly Guid _requestCorrelationId;
        private readonly IPublisher _publisher;

        private readonly
            RequestResponseDispatcher
                <ClientMessage.ReadStreamEventsBackward, ClientMessage.ReadStreamEventsBackwardCompleted>
            _readDispatcher;

        private readonly ProjectionNamesBuilder _namesBuilder;
        private readonly string _projectionName;

        public GetAllStatesWorkItem(
            IEnvelope envelope, Guid requestCorrelationId, CoreProjection projection, PartitionStateCache partitionStateCache,
            ProjectionNamesBuilder namesBuilder, string projectionName,
            RequestResponseDispatcher
                <ClientMessage.ReadStreamEventsBackward, ClientMessage.ReadStreamEventsBackwardCompleted> readDispatcher,
            IPublisher publisher)
            : base(projection, string.Empty)
        {
            if (envelope == null) throw new ArgumentNullException("envelope");
            if (partitionStateCache == null) throw new ArgumentNullException("partitionStateCache");
            _envelope = envelope;
            _requestCorrelationId = requestCorrelationId;
            _namesBuilder = namesBuilder;
            _projectionName = projectionName;
            _readDispatcher = readDispatcher;
            _publisher = publisher;
        }

        protected override void Load(CheckpointTag checkpointTag)
        {
            var psr = new PartitionedStateReader(
                _publisher, _requestCorrelationId, _readDispatcher, checkpointTag, _namesBuilder, _projectionName);
            psr.Start();
        }
    }
}