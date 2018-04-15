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

        public Task Tell(T subject, params object[] arguments)
        {
            ResolveMethod(subject, arguments).Invoke(this, arguments);

            return Task.CompletedTask;
        }

        public Task<TResult> Ask<TResult>(T subject, params object[] arguments) where TResult : class
        {
            return Task.FromResult(ResolveMethod(subject, arguments).Invoke(subject, arguments) as TResult);
        }

        public void EachType(Action<Type> action)
        {
            _nameToMessage.ToList().ForEach(kv => action(kv.Value));
        }

        public bool Resolves(T subject, params Type[] types)
        {
            return _messageToMethod.MethodsForTypes(types).Any();
        }

        public bool Resolves<TResult>(T subject, params Type[] types)
        {
            return _messageToMethod.MethodsForTypes(types).Any(m => m.ReturnType == typeof(T));
        }

        private MethodInfo ResolveMethod(T subject, params object[] arguments)
        {
            var methods = _messageToMethod.MethodsFor(arguments);
            
            if ( ! methods.Any()) {
                throw new UnhandledMessageException(
                    $"[{typeof(T)}] No handler methods found with args {string.Join(", ", arguments.Select(a => a.GetType()))}"
                );
            }
            
            var stateMethods = null == subject
                ? _messageToMethod.MethodsFor(arguments).Where(method => method.IsStatic).ToList()
                : _messageToMethod.MethodsFor(arguments).Where(method => ! method.IsStatic && method.DeclaringType.IsInstanceOfType(subject)).ToList();

            if (methods.Any() && ! stateMethods.Any()) {
                throw new UnexpectedStateException(
                    $"[{typeof(T)}] No handler methods found for state {(null == subject ? "null" : subject.GetType().ToString())} with args {string.Join(", ", arguments.Select(a => a.GetType()))}"
                );
            }

            return stateMethods.First();
        }
    }
}
