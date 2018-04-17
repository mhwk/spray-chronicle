using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadMessagingStrategy<T> : IMessagingStrategy<T> where T : class
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;

        private readonly MethodsForTypeDictionary _messageToMethod = new MethodsForTypeDictionary();
        
        private readonly Dictionary<string,Type> _nameToMessage = new Dictionary<string,Type>();
        
        public OverloadMessagingStrategy(string methodName) : this(new ContextTypeLocator<T>(), methodName)
        {
        }
        
        public OverloadMessagingStrategy(ITypeLocator locator)
        {
            locator.LocateTypesWithParent<T>()
                .SelectMany(type => type.GetMethods(BindingFlags))
                .Where(method => method.GetParameters().Length > 0)
                .ToList()
                .ForEach(AddMethod);
        }

        public OverloadMessagingStrategy(ITypeLocator locator, string methodName)
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

        public Type ToType(string messageName)
        {
            if (_nameToMessage.ContainsKey(messageName)) return _nameToMessage[messageName];
            
            var messageList = string.Join(", ", _nameToMessage.Select(kv => kv.Key));
            throw new ArgumentException($"Message {messageName} not valid for {typeof(T)}, accepts only ({messageList})");

        }

        public bool Resolves(string messageName)
        {
            return _nameToMessage.ContainsKey(messageName);
        }

        public bool Resolves(params Type[] types)
        {
            return _messageToMethod.MethodsForTypes(types).Any();
        }

        public bool Resolves(params object[] arguments)
        {
            return Resolves(arguments.Select(a => a.GetType()).ToArray());
        }

        public bool Resolves<TResult>(params Type[] types)
        {
            return _messageToMethod.MethodsForTypes(types).Any(m => m.ReturnType == typeof(T));
        }

        public bool Resolves<TResult>(params object[] arguments)
        {
            return Resolves<TResult>(arguments.Select(a => a.GetType()).ToArray());
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
