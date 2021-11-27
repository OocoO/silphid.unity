﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silphid.Injexit
{
    public static class IResolverExtensions
    {
        public static IResolver Using(this IResolver This, Action<IBinder> bind, bool isRecursive = true)
        {
            var overrideContainer = This.Create();
            bind(overrideContainer);
            return new OverrideResolver(This, overrideContainer, isRecursive);
        }

        public static IResolver Using(this IResolver This, IResolver overrideResolver, bool isRecursive = true) =>
            overrideResolver != null
                ? new OverrideResolver(This, overrideResolver, isRecursive)
                : This;

        public static IResolver UsingInstance(this IResolver This, Type type, object instance) =>
            This.Using(x => x.BindInstance(type, instance));

        public static IResolver UsingInstance<T>(this IResolver This, T instance) =>
            This.Using(x => x.BindInstance(instance));

        public static IResolver UsingOptionalInstance(this IResolver This, Type type, object instance) =>
            instance != null
                ? This.Using(x => x.BindInstance(type, instance))
                : This;

        public static IResolver UsingOptionalInstance<T>(this IResolver This, T instance) =>
            This.UsingOptionalInstance(typeof(T), instance);

        public static IResolver UsingInstances<T1, T2>(this IResolver This, T1 instance1, T2 instance2) =>
            This.Using(
                x =>
                {
                    x.BindInstance(instance1);
                    x.BindInstance(instance2);
                });

        public static IResolver UsingInstances<T1, T2, T3>(this IResolver This,
                                                           T1 instance1,
                                                           T2 instance2,
                                                           T3 instance3) =>
            This.Using(
                x =>
                {
                    x.BindInstance(instance1);
                    x.BindInstance(instance2);
                    x.BindInstance(instance3);
                });

        public static IResolver UsingInstances(this IResolver This, object[] instances) =>
            This.Using(x => x.BindInstancesToOwnTypes(instances));

        public static IResolver UsingOptionalInstances(this IResolver This, [CanBeNull] object[] instances) =>
            instances != null
                ? This.Using(x => x.BindOptionalInstancesToOwnTypes(instances))
                : This;

        public static IResolver UsingInstances(this IResolver This, IDictionary<Type, object> instances) =>
            This.Using(x => x.BindInstances(instances));

        public static IResolver UsingOptionalInstances(this IResolver This, [CanBeNull] IDictionary<Type, object> instances) =>
            instances != null && instances.Count > 0
                ? This.Using(x => x.BindInstances(instances))
                : This;
    
        public static object Resolve(this IResolver This,
                                     Type abstractionType,
                                     Type dependentType = null,
                                     string name = null)
        {
            try
            {
                var result = This.ResolveResult(abstractionType, dependentType, name);
                return result.ResolveInstance(This);
            }
            catch (DependencyException ex)
            {
                throw ex.With(This);
            }
        }

        public static T Resolve<T>(this IResolver This, Type dependentType = null, string name = null) =>
            (T) Resolve(This, typeof(T), dependentType, name);
    }
}