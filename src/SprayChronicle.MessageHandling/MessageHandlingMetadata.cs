using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SprayChronicle.MessageHandling
{
    public static class MessageHandlingMetadata
    {
        private static readonly Dictionary<Type, List<Metadata>> Map = new Dictionary<Type, List<Metadata>>();
        
        public static void Append<T>(string messageName, Type messageType, MethodInfo method)
        {
            All<T>().Add(new Metadata(
                messageName,
                messageType,
                method
            ));
        }

        public static List<Metadata> All<T>()
        {
            return All(typeof(T));
        }

        public static List<Metadata> All(Type handler)
        {
            if (!Map.ContainsKey(handler)) {
                Map.Add(handler, new List<Metadata>());
            }

            return Map[handler];
        }

        public static bool Accepts<T>(object message)
        {
            return Accepts<T>(message.GetType());
        }

        public static bool Accepts<T>(Type messageType)
        {
            return null != All<T>().FirstOrDefault(m => m.MessageType == messageType);
        }

        public static bool Accepts(Type handler, Type messageType)
        {
            return null != All(handler).FirstOrDefault(m => m.MessageType == messageType);
        }

        public static Type For<T>(string eventType)
        {
            return All<T>().FirstOrDefault(m => m.EventType == eventType)?.MessageType;
        }

        public sealed class Metadata
        {
            public readonly string EventType;

            public readonly Type MessageType;

            public readonly MethodInfo Method;

            public Metadata(string eventType, Type messageType, MethodInfo method)
            {
                EventType = eventType;
                MessageType = messageType;
                Method = method;
            }
        }
    }
}
