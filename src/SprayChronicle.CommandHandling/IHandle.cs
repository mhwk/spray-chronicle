﻿using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface IHandle
    {
        
    }
    
    public interface IHandle<in TCommand>
        where TCommand : class
    {
        Task<Handled> Handle(TCommand command);
    }
}
