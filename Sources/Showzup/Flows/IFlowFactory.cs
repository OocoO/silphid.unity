﻿using System;

namespace Silphid.Showzup.Flows
{
    public interface IFlowFactory
    {
        IFlow Create(Type type, IFlowOptions options);
    }
}