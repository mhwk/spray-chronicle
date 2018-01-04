using System;
using Autofac;
using Autofac.Core;

namespace SprayChronicle.EventHandling
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterEventHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<AsyncEventHandlingModule>();
            return builder;
        }
        
        public static ContainerBuilder RegisterEventHandler<THandler>(this ContainerBuilder builder, string stream)
            where THandler : IHandleEvents
        {
            builder.RegisterModule(new EventHandlingModule.Persistent<THandler>(stream));
            return builder;
        }
        
        public static ContainerBuilder RegisterEventHandler<THandler>(this ContainerBuilder builder, string stream, Func<IComponentRegistry,bool> condition)
            where THandler : IHandleEvents
        {
            builder.RegisterModule(new EventHandlingModule.Persistent<THandler>(stream, condition));
            return builder;
        }
    }
}
