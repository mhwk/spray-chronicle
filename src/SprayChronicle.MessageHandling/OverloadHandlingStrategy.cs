using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using SprayLocator;

namespace SprayChronicle.MessageHandling
{
    public sealed class OverloadHandlingStrategy<T> : IMessageHandlingStrategy
    {
        readonly static BindingFlags _bindingFlags = BindingFlags.Instance
                                                   | BindingFlags.NonPublic
                                                   | BindingFlags.Public
                                                   | BindingFlags.Static
                                                   | BindingFlags.DeclaredOnly;
        readonly MethodsForTypeDictionary _methodsForMessage = new MethodsForTypeDictionary();

        public OverloadHandlingStrategy()
        {
            TypeLocator.LocateRuntimeTypes()
                .Where(type => type == typeof(T) || type.GetTypeInfo().IsSubclassOf(typeof(T)))
                .SelectMany(type => type.GetMethods(_bindingFlags))
                .Where(method => method.GetParameters().Length > 0)
                .ToList()
                .ForEach(method => _methodsForMessage.Add(method.GetParameters().Select(p => p.ParameterType).ToArray(), method));
        }

        public bool AcceptsMessage(object subject, object message, params object[] arguments)
        {
            var method = MethodForMessage(subject, message, arguments);
            if (null == method) {
                return false;
            }
            return method.IsStatic == (null == subject);
        }

        public object ProcessMessage(object subject, object message, params object[] arguments)
        {
            var method = MethodForMessage(subject, message, arguments);
            if (null == method) {
                throw new UnhandledMessageException(string.Format(
                    "Message {0} not handled by {1} ({2})",
                    message.GetType(),
                    typeof(T),
                    null == subject ? "null" : subject.GetType().ToString()
                ));
            }
            if (null == subject && ! method.IsStatic) {
                throw new UnexpectedStateException(string.Format(
                    "Expected method to be static for message {0}",
                    message.GetType()
                ));
            }
            if (null != subject && method.IsStatic) {
                throw new UnexpectedStateException(string.Format(
                    "Expected method not to be static for message {0}",
                    message.GetType()
                ));
            }
            return method.Invoke(subject, BuildArguments(message, arguments));
        }

        private MethodInfo MethodForMessage(object subject, object message, params object[] arguments)
        {
            var args = BuildArguments(message, arguments);
            return _methodsForMessage.MethodsFor(args.Select(a => a.GetType()).ToArray())
                .FirstOrDefault();
        }

        private static object[] BuildArguments(object message, params object[] arguments)
        {
            var args = new List<object> {message};
            args.AddRange(arguments);
            return args.ToArray();
        }
    }
}
