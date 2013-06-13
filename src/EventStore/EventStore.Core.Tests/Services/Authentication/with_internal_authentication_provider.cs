﻿// Copyright (c) 2012, Event Store LLP
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
using System.Security.Principal;
using EventStore.Core.Helpers;
using EventStore.Core.Messaging;
using EventStore.Core.Services.Transport.Http.Authentication;
using EventStore.Core.Tests.Helpers;
using EventStore.Core.Tests.Services.Transport.Http.Authentication;

namespace EventStore.Core.Tests.Services.Authentication
{
    public class with_internal_authentication_provider : TestFixtureWithExistingEvents
    {
        protected IODispatcher _ioDispatcher;
        protected InternalAuthenticationProvider _internalAuthenticationProvider;

        protected void SetUpProvider()
        {
            _ioDispatcher = new IODispatcher(_bus, new PublishEnvelope(_bus));
            _bus.Subscribe(_ioDispatcher.BackwardReader);
            _bus.Subscribe(_ioDispatcher.ForwardReader);
            _bus.Subscribe(_ioDispatcher.Writer);
            _bus.Subscribe(_ioDispatcher.StreamDeleter);
            _bus.Subscribe(_ioDispatcher);

            PasswordHashAlgorithm passwordHashAlgorithm = new StubPasswordHashAlgorithm();
            _internalAuthenticationProvider = new InternalAuthenticationProvider(this._ioDispatcher, passwordHashAlgorithm, 1000);
            _bus.Subscribe(_internalAuthenticationProvider);
        }
    }

    class TestAuthenticationRequest : InternalAuthenticationProvider.AuthenticationRequest
    {
        private readonly Action _unauthorized;
        private readonly Action<IPrincipal> _authenticated;
        private readonly Action _error;

        public TestAuthenticationRequest(
            string name, string suppliedPassword, Action unauthorized, Action<IPrincipal> authenticated, Action error)
            : base(name, suppliedPassword)
        {
            _unauthorized = unauthorized;
            _authenticated = authenticated;
            _error = error;
        }

        public override void Unauthorized()
        {
            _unauthorized();
        }

        public override void Authenticated(IPrincipal principal)
        {
            _authenticated(principal);
        }

        public override void Error()
        {
            _error();
        }
    }
}