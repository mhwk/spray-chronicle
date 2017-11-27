using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadHandlingStrategy<T> : IMessageHandlingStrategy
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;

        private readonly MethodsForTypeDictionary _messageToMethod = new MethodsForTypeDictionary();
        
        private readonly Dictionary<string,Type> _nameToMessage = new Dictionary<string,Type>();
        
        public OverloadHandlingStrategy(ILocateTypes locator)
        {
            locator.LocateTypesWithParent<T>()
                .SelectMany(type => type.GetMethods(BindingFlags))
                .Where(method => method.GetParameters().Length > 0)
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

        public bool AcceptsMessage(object subject, IMessage message, params object[] arguments)
        {
            var argumentTypes = BuildArgumentTypes(message, arguments);
            var methods = _messageToMethod.MethodsForTypes(argumentTypes);

            return null == subject
                ? methods.Any(method => method.IsStatic)
                : methods.Any(method => ! method.IsStatic && method.DeclaringType.IsInstanceOfType(subject));
        }

        public object ProcessMessage(object subject, IMessage message, params object[] arguments)
        {
            var builtArguments = BuildArguments(message, arguments);
            var methods = _messageToMethod.MethodsFor(builtArguments);
            
            if ( ! methods.Any()) {
                throw new UnhandledMessageException(string.Format(
                    "Message {0} not handled by {1} ({2})",
                    message.Type,
                    typeof(T),
                    null == subject ? "null" : subject.GetType().ToString()
                ));
            }
            
            var stateMethods = null == subject
                ? _messageToMethod.MethodsFor(builtArguments).Where(method => method.IsStatic)
                : _messageToMethod.MethodsFor(builtArguments).Where(method => ! method.IsStatic && method.DeclaringType.IsInstanceOfType(subject));

            if (methods.Any() && ! stateMethods.Any()) {
                throw new UnexpectedStateException(string.Format(
                    "Message {0} not handled by state {1} ({2})",
                    message.Type,
                    typeof(T),
                    null == subject ? "null" : subject.GetType().ToString()
                ));
            }

            return stateMethods.First().Invoke(subject, builtArguments);
        }

        private object[] BuildArguments(IMessage message, params object[] arguments)
        {
            var args = new List<object> { };
            args.Add(MessageToPayload(message));
            args.AddRange(arguments);
            return args.ToArray();
        }

        private Type[] BuildArgumentTypes(IMessage message, params object[] arguments)
        {
            if ( ! _nameToMessage.ContainsKey(message.Type)) {
                return null;
            }
            
            var args = new List<Type> { };
            args.Add(_nameToMessage[message.Type]);
            args.AddRange(arguments.Select(a => a.GetType()).ToArray());
            return args.ToArray();
        }

        private object MessageToPayload(IMessage message)
        {
            if (!_nameToMessage.ContainsKey(message.Type)) {
                throw new UnhandledMessageException(string.Format(
                    "Message {0} not handled by {1} ({2})",
                    message.GetType(),
                    typeof(T),
                    string.Join(", ", _nameToMessage.Keys)
                ));
            }

            return message.Instance(_nameToMessage[message.Type]);
        }
    }
}
