﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenExecutedMultiple<TState> : RavenExecuted
        where TState : class
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutedMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState>());
        }
    }
    
    public class RavenExecutedMultiple<TState,TFilter> : RavenExecuted
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutedMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState,TFilter>());
        }
    }
}