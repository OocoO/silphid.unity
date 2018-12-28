﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using Silphid.Requests;
using UniRx;

namespace Silphid.Machina
{
    public class Machine<TState> : IMachine, IDisposable
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger(typeof(IMachine));
        
        private readonly object _initialState;
        private readonly bool _disposeOnCompleted;
        private readonly List<Rule> _rules = new List<Rule>();
        private readonly ReactiveProperty<object> _state;
        public IReadOnlyReactiveProperty<object> State => _state;

        protected readonly CompositeDisposable Disposables = new CompositeDisposable();
        protected bool IsDisposed { get; private set; }

        public Machine(object initialState = null, bool disposeOnCompleted = false)
        {
            _initialState = initialState;
            _disposeOnCompleted = disposeOnCompleted;
            _state = new ReactiveProperty<object>(initialState);

            this.Entering<IMachine>()
                .Subscribe(x => x.Start())
                .AddTo(Disposables);

            this.Exiting<IMachine>()
                .Subscribe(x => x.Complete())
                .AddTo(Disposables);
            
            State
                .PairWithPreviousOrDefault()
                .Subscribe(x => Log.Debug($"{Name} - {x.Item1 ?? "null"} -> {x.Item2 ?? "null"}"))
                .AddTo(Disposables);
        }

        public virtual string Name => GetType().Name;
        public override string ToString() => GetType().Name;

        #region IDisposer members

        public void Add(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        #endregion

        public void Start(object initialState = null)
        {
            AssertNotDisposed();
            Log.Info($"{Name} - Started");
            OnStarting(initialState);
        }

        public virtual void SetState(object state)
        {
            AssertNotDisposed();
            _state.Value = state;
        }

        void IMachine.Complete()
        {
            AssertNotDisposed();
            OnCompleted();
            Log.Info($"{Name} - Completed");
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Machine<TState>));
        }

        public virtual bool Handle(IRequest request)
        {
            var state = State.Value;
            return state != null && HandleWithState(request, state) ||
                   HandleWithRules(request, state);
        }

        private bool HandleWithState(IRequest request, object state)
        {
            var handler = state as IRequestHandler;
            if (handler?.Handle(request) ?? false)
            {
                Log.Info($"{Name} - {state} - {request} handled by state");
                return true;
            }
            
            return false;
        }

        private bool HandleWithRules(IRequest request, object state)
        {
            if (_rules.Any(rule => rule.Matches(state, request) && rule.Handle(request)))
            {
                Log.Info($"{Name} - {state ?? "null"} - {request} handled by rule");
                return true;
            }

            return false;
        }

        public IRule Always()
        {
            var rule = new Rule(_ => true);
            _rules.Add(rule);
            return rule;
        }

        public IRule When<T>() where T : TState =>
            WhenInternal<T>();

        public IRule When<T>(T state) where T : TState =>
            WhenInternal(state);

        public IRule When<T>(Predicate<T> predicate) where T : TState =>
            WhenInternal(predicate);

        public IRule WhenSubMachine() =>
            WhenInternal<IMachine>();

        public IRule WhenSubMachine<TMachine>() where TMachine : IMachine =>
            WhenInternal<TMachine>();

        public IRule WhenSubMachine<TMachine>(TMachine state) where TMachine : IMachine =>
            WhenInternal(state);

        public IRule WhenSubMachine<TMachine>(Predicate<TMachine> predicate) where TMachine : IMachine =>
            WhenInternal(predicate);

        private IRule WhenInternal<T>()
        {
            var rule = new Rule(x => x is T);
            _rules.Add(rule);
            return rule;
        }

        private IRule WhenInternal<T>(T state)
        {
            var rule = new Rule(x => Equals(x, state));
            _rules.Add(rule);
            return rule;
        }

        private IRule WhenInternal<T>(Predicate<T> predicate)
        {
            var rule = new Rule(x => x is T && predicate((T) x));
            _rules.Add(rule);
            return rule;
        }

        protected virtual void OnStarting(object initialState)
        {
            _state.Value = initialState ?? _initialState;
        }

        protected virtual void OnCompleted()
        {
            _state.Value = null;
            
            if (_disposeOnCompleted)
                Dispose();
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            
            Disposables.Dispose();
            _state.Dispose();
            IsDisposed = true;
        }
    }
}