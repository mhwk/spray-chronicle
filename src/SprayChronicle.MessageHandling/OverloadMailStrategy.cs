using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadMailStrategy<T> : IMailStrategy<T>
        where T : class
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;

        private readonly Dictionary<string,Type> _nameToType = new Dictionary<string,Type>();
        
        private readonly Dictionary<Type,List<MethodInfo>> _typeToMethods = new Dictionary<Type,List<MethodInfo>>();
        
        public OverloadMailStrategy() : this(new ContextTypeLocator<T>())
        {
        }
        
        public OverloadMailStrategy(string methodName) : this(new ContextTypeLocator<T>(), methodName)
        {
        }
        
        public OverloadMailStrategy(ITypeLocator locator)
        {
            locator.LocateTypesWithParent<T>()
                .SelectMany(type => type.GetMethods(BindingFlags))
                .Where(method => method.GetParameters().Length > 0)
                .ToList()
                .ForEach(AddMethod);
        }

        public OverloadMailStrategy(ITypeLocator locator, string methodName)
        {
            locator.LocateTypesWithParent<T>()
                .SelectMany(type => type.GetMethods(BindingFlags))
                .Where(method => method.GetParameters().Length > 0)
                .Where(method => method.Name == methodName)
                .ToList()
                .ForEach(AddMethod);
        }

        private void AddMethod(MethodInfo method)
        {
            if ( ! _nameToType.ContainsKey(method.GetParameters().First().ParameterType.Name)) {
                _nameToType.Add(method.GetParameters().First().ParameterType.Name, method.GetParameters().First().ParameterType);
            }

            if (!_typeToMethods.ContainsKey(method.GetParameters().First().ParameterType)) {
                _typeToMethods.Add(method.GetParameters().First().ParameterType, new List<MethodInfo>());
            }
            _typeToMethods[method.GetParameters().First().ParameterType].Add(method);
        }

        public Type ToType(string messageName)
        {
            if (_nameToType.ContainsKey(messageName)) return _nameToType[messageName];
            
            var messageList = string.Join(", ", _nameToType.Select(kv => kv.Key));
            throw new UnsupportedMessageException($"Message {messageName} not valid for {typeof(T)}, accepts only ({messageList})");

        }

        public bool Resolves(string messageName)
        {
            return _nameToType.ContainsKey(messageName);
        }

        public bool Resolves(object message)
        {
            return Resolves(message.GetType().Name);
        }

        public Task Tell(T subject, object message, DateTime epoch)
        {
            Invoke(subject, message, epoch);

            return Task.CompletedTask;
        }

        private object Invoke(T subject, object message, DateTime epoch)
        {
            var methods = ResolveMethods(subject, message);

            foreach (var method in methods) {
                var parameters = method.GetParameters();
                if (parameters.Length == 2 && parameters[1].ParameterType == epoch.GetType()) {
                    return method.Invoke(subject, new[] {message, epoch});
                }
                
                return method.Invoke(subject, new[] {message});
            }
            
            throw new UnsupportedMessageException($"Message {message.GetType().Name} not handled");
        }

        public async Task<TResult> Ask<TResult>(T subject, object message, DateTime epoch) where TResult : class
        {
            var result = Invoke(subject, message, epoch);

            if (!(result is Task task)) {
                return (TResult) result;
            }

            await task;
            
            return (TResult)((dynamic)task).Result;
        }

        private MethodInfo[] ResolveMethods(T subject, object message)
        {
            if ( ! _typeToMethods.ContainsKey(message.GetType())) {
                var suggestedArgs = string.Join(", ", _typeToMethods.Select(kv => kv.Key.Name));
                throw new UnsupportedMessageException(
                    $"[{typeof(T)}] Unknown message ({message.GetType().Name}), try one of ({suggestedArgs})"
                );
            }
            
            var methods = _typeToMethods[message.GetType()];
            
            var stateMethods = null == subject
                ? methods.Where(method => method.IsStatic).ToArray()
                : methods.Where(method => ! method.IsStatic && (null == method.DeclaringType || method.DeclaringType.IsInstanceOfType(subject))).ToArray();

            if (methods.Any() && ! stateMethods.Any()) {
                throw new UnexpectedStateException(
                    $"[{typeof(T)}] No handler methods found for state {(null == subject ? "null" : subject.GetType().Name)} for message {string.Join(", ", message.GetType().Name)}"
                );
            }

            return stateMethods;
        }
    }
}
