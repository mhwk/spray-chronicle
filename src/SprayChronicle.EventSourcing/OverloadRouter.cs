using System;
using System.Linq;
using System.Reflection;

namespace SprayChronicle.EventSourcing
{
    public sealed class OverloadRouter<T> : IEventRouter<T> where T : IEventSourcable<T>
    {
        public IEventSourcable<T> Route(IEventSourcable<T> sourcable, DomainMessage domainMessage)
        {
            var typeInfo = null == sourcable ? typeof(T).GetTypeInfo() : sourcable.GetType().GetTypeInfo();
            
            try {
                var method = typeInfo.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.GetParameters().Length > 0)
                    .Where(m => m.GetParameters()[0].ParameterType.Equals(domainMessage.Payload.GetType()))
                    // .Where(m => m.ReturnType.GetTypeInfo().IsAssignableFrom(typeof(T)))
                    .FirstOrDefault();
                
                if (null == method) {
                    throw new UnknownDomainMessageException(string.Format(
                        "Domain message {0} is unknown for {1}",
                        domainMessage.Payload.GetType(),
                        typeInfo
                    ));
                }

                if (null == sourcable && ! method.IsStatic) {
                    throw new InvalidStateException(string.Format(
                        "A static mutator is expected for {0} as sourcable {1} does not exist",
                        domainMessage.Payload.GetType(),
                        typeInfo
                    ));
                }

                if (null != sourcable && method.IsStatic) {
                    throw new InvalidStateException(string.Format(
                        "A non-static mutator is expected for {0} as sourcable already exists",
                        domainMessage.Payload.GetType(),
                        typeInfo
                    ));
                }

                return (T) method.Invoke(sourcable, new object[] { domainMessage.Payload });
            } catch (Exception error) {
                throw new UnhandledDomainMessageException(
                    string.Format(
                        "Domain message with payload {0} is not handled by {1} using router {2}",
                        domainMessage.Payload.GetType(),
                        typeInfo,
                        GetType()
                    ),
                    error
                );
            }
        }
    }
}
