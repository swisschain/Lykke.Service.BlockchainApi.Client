using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Client.Results.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public static class PendingEventsList
    {
        public static PendingEventsList<TEvent> From<TEvent>(IReadOnlyList<TEvent> events) 
            where TEvent : BasePendingEvent
        {
            return new PendingEventsList<TEvent>(events);
        }
    }

    [PublicAPI]
    public class PendingEventsList<TEvent>
        where TEvent : BasePendingEvent
    {
        public IReadOnlyList<TEvent> Events { get; }

        public PendingEventsList(IReadOnlyList<TEvent> events)
        {
            Events = events ?? Array.Empty<TEvent>();
        }
    }
}
