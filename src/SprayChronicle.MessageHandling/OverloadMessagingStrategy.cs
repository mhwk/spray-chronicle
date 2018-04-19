using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadMessagingStrategy<T> : IMessagingStrategy, IMessagingStrategy<T>
        where T : class
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;

        private readonly MethodsForTypeDictionary _typesToMethod = new MethodsForTypeDictionary();
        
        private readonly Dictionary<string,Type> _nameToType = new Dictionary<string,Type>();
        
        public OverloadMessagingStrategy() : this(new ContextTypeLocator<T>())
        {
        }
        
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
            _typesToMethod.Add(method.GetParameters().Select(parameter => parameter.ParameterType).ToArray(), method);
            if ( ! _nameToType.ContainsKey(method.GetParameters().First().ParameterType.Name)) {
                _nameToType.Add(method.GetParameters().First().ParameterType.Name, method.GetParameters().First().ParameterType);
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
            if (_nameToType.ContainsKey(messageName)) return _nameToType[messageName];
            
            var messageList = string.Join(", ", _nameToType.Select(kv => kv.Key));
            throw new ArgumentException($"Message {messageName} not valid for {typeof(T)}, accepts only ({messageList})");

        }

        public bool Resolves(string messageName)
        {
            return _nameToType.ContainsKey(messageName);
        }

        public bool Resolves(params Type[] types)
        {
            return _typesToMethod.MethodsFor(types).Any();
        }

        public bool Resolves(params object[] arguments)
        {
            return Resolves(arguments.Select(a => a.GetType()).ToArray());
        }

        public bool Resolves<TResult>(params Type[] types)
        {
            return _typesToMethod.MethodsFor(types).Any(m => m.ReturnType == typeof(TResult));
        }

        public bool Resolves<TResult>(params object[] arguments)
        {
            return Resolves<TResult>(arguments.Select(a => a.GetType()).ToArray());
        }

        private MethodInfo ResolveMethod(T subject, params object[] arguments)
        {
            var methods = _typesToMethod.MethodsFor(arguments);

            if ( ! methods.Any()) {
                var providedArgs = string.Join(", ", arguments.Select(a => a.GetType().FullName));
                var suggestedArgs = string.Join(", ", _typesToMethod.SuggestList(subject));
                throw new UnroutableMessageException(
                    $"[{typeof(T)}] Not handled ({providedArgs}), try one of ({suggestedArgs})"
                );
            }
            
            var stateMethods = null == subject
                ? _typesToMethod.MethodsFor(arguments).Where(method => method.IsStatic).ToList()
                : _typesToMethod.MethodsFor(arguments).Where(method => ! method.IsStatic && method.DeclaringType.IsInstanceOfType(subject)).ToList();

            if (methods.Any() && ! stateMethods.Any()) {
                throw new UnexpectedStateException(
                    $"[{typeof(T)}] No handler methods found for state {(null == subject ? "null" : subject.GetType().ToString())} with args {string.Join(", ", arguments.Select(a => a.GetType()))}"
                );
            }

            return stateMethods.First();
        }
    }
}
