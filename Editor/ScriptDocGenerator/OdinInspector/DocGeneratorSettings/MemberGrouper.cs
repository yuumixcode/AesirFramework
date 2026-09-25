using System;
using System.Collections.Generic;
using System.Linq;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>成员分组（文档生成共享）：常量 → 声明 → 继承 → 运算符。</summary>
    internal enum MemberGroup
    {
        None = 0,
        Constant,
        Declared,
        Inherited,
        Operator
    }

    /// <summary>
    /// 成员分组引擎：Default 与 Zensical 两生成器共享的分组核心——
    /// API 成员过滤 → 按选择器分组 → 按固定顺序（常量 → 声明 → 继承 → 运算符）输出非空分组。
    /// 替代各生成器手写的"三旗标探测循环"（阈值与守卫类的分组错误曾集中于此）。
    /// </summary>
    internal static class MemberGrouper
    {
        /// <summary>分组的固定输出顺序。</summary>
        public static readonly MemberGroup[] GroupOrder =
            { MemberGroup.Constant, MemberGroup.Declared, MemberGroup.Inherited, MemberGroup.Operator };

        /// <summary>
        /// 过滤 API 成员并按选择器分组（按 <see cref="GroupOrder" /> 顺序输出，空分组不输出）。
        /// </summary>
        public static List<(MemberGroup Group, List<IDerivedMemberData> Items)> GroupApiMembers(
            IEnumerable<IDerivedMemberData> members,
            Func<IDerivedMemberData, MemberGroup> groupSelector)
        {
            var groups = new List<(MemberGroup, List<IDerivedMemberData>)>();
            foreach (var group in GroupOrder)
            {
                var items = members.Where(m => m.IsApiMember() && groupSelector(m) == group).ToList();
                if (items.Count > 0)
                {
                    groups.Add((group, items));
                }
            }

            return groups;
        }

        /// <summary>分组标签文本（如"常量"/"声明的"/"继承的"/"运算符"）。</summary>
        public static string GetGroupLabel(MemberGroup group) =>
            group switch
            {
                MemberGroup.Constant => "常量",
                MemberGroup.Declared => "声明的",
                MemberGroup.Inherited => "继承的",
                MemberGroup.Operator => "运算符",
                _ => string.Empty
            };
    }
}
