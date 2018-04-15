using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class SubscriptionRouter : ICommandRouter
    {
        public delegate Task Dispatch(object query);
        
        private readonly Dictionary<Type,Dispatch> _dispatchers = new Dictionary<Type,Dispatch>();

        public void Subscribe(Type commandType, Dispatch dispatcher)
        {
            _dispatchers.Add(commandType, dispatcher);
        }

        public SubscriptionRouter Subscribe(ICommandRouterSubscriber handler)
        {
            handler.Subscribe(this);

            return this;
        }

        public async Task Route(params object[] commands)
        {
            foreach (var command in commands) {
                if (!_dispatchers.ContainsKey(command.GetType())) {
                    var list = string.Join(", ", _dispatchers.Select(kv => kv.Key.Name));
                    throw new UnhandledCommandException($"Command {command.GetType()} is not included in dispatcher list: {list}");
                }

                if (!(_dispatchers[command.GetType()]
                    .GetMethodInfo()
                    .Invoke(_dispatchers[command.GetType()], new[] {command}) is Task task)) {
                    throw new UnhandledCommandException($"Command {command.GetType()} not a task");
                }

                await task;
            }
        }
    }
}
