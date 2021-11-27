﻿using System;
using Silphid.Requests;
using UniRx;

namespace Silphid.Machina
{
    public interface IMachine : IRequestHandler
    {
        ReadOnlyReactiveProperty<object> State { get; }
        IObservable<Transition> Transitions { get; }

        void Start(object initialState = null);
        void Complete();
        void Enter(IMachine machine);
        void ExitState();
    }

    public interface IMachine<in TState> : IMachine
    {
        void Enter(TState state);
    }
}