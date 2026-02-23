using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;
        // used @event because event is a reserved keyword in C#
        void Publish<T>(T @event) where T : Event;
        // where is used to specify constraints on the type parameters. In this case, T must be a type that inherits from Event, and TH must be a type that implements IEventHandler<T>.
        void Subscribe<T, TH>()
                where T : Event
                where TH : IEventHandler<T>;
    }
}
