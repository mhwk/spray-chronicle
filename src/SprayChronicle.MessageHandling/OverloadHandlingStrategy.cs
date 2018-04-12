using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadHandlingStrategy<T> : IMessageHandlingStrategy<T> where T : class
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;

        private readonly MethodsForTypeDictionary _messageToMethod = new MethodsForTypeDictionary();
        
        private readonly Dictionary<string,Type> _nameToMessage = new Dictionary<string,Type>();
        
        public OverloadHandlingStrategy(string methodName) : this(new ContextTypeLocator<T>(), methodName)
        {
        }
        
        public OverloadHandlingStrategy(ILocateTypes locator)
        {
            locator.LocateTypesWithParent<T>()
                .SelectMany(type => type.GetMethods(BindingFlags))
                .Where(method => method.GetParameters().Length > 0)
                .ToList()
                .ForEach(AddMethod);
        }

        public OverloadHandlingStrategy(ILocateTypes locator, string methodName)
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
            _messageToMethod.Add(method.GetParameters().Select(parameter => parameter.ParameterType).ToArray(), method);
            if ( ! _nameToMessage.ContainsKey(method.GetParameters().First().ParameterType.Name)) {
                _nameToMessage.Add(method.GetParameters().First().ParameterType.Name, method.GetParameters().First().ParameterType);
            }
            
            MessageHandlingMetadata.Append<T>(
                method.GetParameters().First().ParameterType.Name,
                method.GetParameters().First().ParameterType,
                method
            );
        }

        public Task Tell(T subject, IMessage message, params object[] arguments)
        {
            var builtArguments = BuildArguments(message, arguments);

            ResolveMethod(subject, builtArguments).Invoke(this, builtArguments);

            return Task.CompletedTask;
        }

        public Task<TResult> Ask<TResult>(T subject, IMessage message, params object[] arguments) where TResult : class
        {
            var builtArguments = BuildArguments(message, arguments);
            
            return Task.FromResult(ResolveMethod(subject, builtArguments).Invoke(subject, builtArguments) as TResult);
        }

        private MethodInfo ResolveMethod(T subject, params object[] builtArguments)
        {
            var methods = _messageToMethod.MethodsFor(builtArguments);
            
            if ( ! methods.Any()) {
                throw new UnhandledMessageException(
                    $"[{typeof(T)}] No handler methods found with args {string.Join(", ", builtArguments.Select(a => a.GetType()))}"
                );
            }
            
            var stateMethods = null == subject
                ? _messageToMethod.MethodsFor(builtArguments).Where(method => method.IsStatic).ToList()
                : _messageToMethod.MethodsFor(builtArguments).Where(method => ! method.IsStatic && method.DeclaringType.IsInstanceOfType(subject)).ToList();

            if (methods.Any() && ! stateMethods.Any()) {
                throw new UnexpectedStateException(
                    $"[{typeof(T)}] No handler methods found for state {(null == subject ? "null" : subject.GetType().ToString())} with args {string.Join(", ", builtArguments.Select(a => a.GetType()))}"
                );
            }

            return stateMethods.First();
        }

        private static object[] BuildArguments(IMessage message, params object[] arguments)
        {
            var args = new List<object> {message.Payload()};
            
            args.AddRange(arguments);
            
            return args.ToArray();
        }
    }
}
