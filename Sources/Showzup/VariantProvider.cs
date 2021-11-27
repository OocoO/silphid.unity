using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class VariantProvider : IVariantProvider
    {
        public List<IVariantGroup> AllVariantGroups { get; }

        public ReactiveProperty<VariantSet> GlobalVariants { get; } =
            new ReactiveProperty<VariantSet>(VariantSet.Empty);

        public VariantProvider(params IVariantGroup[] allVariantGroups)
        {
            AllVariantGroups = allVariantGroups.ToList();
        }

        public static IVariantProvider From<T1>() => From(typeof(T1));
        public static IVariantProvider From<T1, T2>() => From(typeof(T1), typeof(T2));
        public static IVariantProvider From<T1, T2, T3>() => From(typeof(T1), typeof(T2), typeof(T3));
        public static IVariantProvider From<T1, T2, T3, T4>() => From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static IVariantProvider From<T1, T2, T3, T4, T5>() =>
            From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public static IVariantProvider From<T1, T2, T3, T4, T5, T6>() =>
            From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

        public static IVariantProvider From<T1, T2, T3, T4, T5, T6, T7>() => From(
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5),
            typeof(T6),
            typeof(T7));

        public static IVariantProvider From<T1, T2, T3, T4, T5, T6, T7, T8>() => From(
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5),
            typeof(T6),
            typeof(T7),
            typeof(T8));

        public static IVariantProvider From<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => From(
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5),
            typeof(T6),
            typeof(T7),
            typeof(T8),
            typeof(T9));

        public static IVariantProvider From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => From(
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5),
            typeof(T6),
            typeof(T7),
            typeof(T8),
            typeof(T9),
            typeof(T10));

        public static IVariantProvider From(params Type[] variantTypes) =>
            new VariantProvider(
                variantTypes.Select(GetVariantGroupFromVariantType)
                            .ToArray());

        private static IVariantGroup GetVariantGroupFromVariantType(Type type)
        {
            var property = type.BaseType.GetProperty(
                "Group",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (property == null || !property.PropertyType.IsAssignableTo<IVariantGroup>())
                throw new InvalidOperationException(
                    $"Variant type {type.Name} must have a static Group property of type IVariantGroup.");

            return (IVariantGroup) property.GetValue(null);
        }
    }
}