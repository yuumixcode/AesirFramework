using System;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 参数信息解析数据
    /// </summary>
    [Summary("参数信息解析数据")]
    [Serializable]
    public class ParameterData : IParameterData
    {
        /// <summary>
        /// 创建参数信息解析数据实例
        /// </summary>
        public ParameterData(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name ?? string.Empty;
            ParameterType = parameterInfo.ParameterType;
            HasDefaultValue = parameterInfo.HasDefaultValue;
            DefaultValue = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : null;
            Direction = ParameterDirection.In;
            if (parameterInfo.IsOut)
            {
                Direction = ParameterDirection.Out;
            }
            else if (parameterInfo.ParameterType.IsByRef)
            {
                Direction = ParameterDirection.Ref;
            }
            else if (parameterInfo.IsRetval)
            {
                Direction = ParameterDirection.RetVal;
            }

            IsParams = parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
        }

        static string GetDefaultValueString(Type parameterType, object value) =>
            TypeAnalyzerUtility.GetFormattedDefaultValue(parameterType, value);

        #region IParameterData Members

        /// <summary>
        /// 参数名称
        /// </summary>
        [Summary("参数名称")]
        public string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [Summary("参数类型")]
        public Type ParameterType { get; }

        /// <summary>
        /// 是否有默认值
        /// </summary>
        [Summary("是否有默认值")]
        public bool HasDefaultValue { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        [Summary("默认值")]
        public object DefaultValue { get; }

        /// <summary>
        /// 参数方向（in/out/ref）
        /// </summary>
        [Summary("参数方向（in/out/ref）")]
        public ParameterDirection Direction { get; }

        /// <summary>
        /// 是否为 params 参数
        /// </summary>
        [Summary("是否为 params 参数")]
        public bool IsParams { get; }

        /// <summary>
        /// 生成格式化的参数字符串
        /// </summary>
        [Summary("生成格式化的参数字符串")]
        public string GetFormattedString()
        {
            var result = string.Empty;

            switch (Direction)
            {
                case ParameterDirection.Out:
                    result += "out ";
                    break;
                case ParameterDirection.Ref:
                    result += "ref ";
                    break;
            }

            if (IsParams)
            {
                result += "params ";
            }

            var typeName = ParameterType?.GetReadableTypeName() ?? "object";
            if (ParameterType?.IsByRef == true)
            {
                typeName = typeName.TrimEnd('&');
            }

            result += typeName;

            if (!string.IsNullOrEmpty(Name))
            {
                result += " " + Name;
            }

            if (HasDefaultValue)
            {
                result += " = ";
                result += GetDefaultValueString(ParameterType, DefaultValue);
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// 参数方向枚举
    /// </summary>
    [Summary("参数方向枚举")]
    public enum ParameterDirection
    {
        /// <summary>
        /// 输入参数
        /// </summary>
        [Summary("输入参数")]
        In = 0,

        /// <summary>
        /// 输出参数
        /// </summary>
        [Summary("输出参数")]
        Out = 1,

        /// <summary>
        /// 引用参数
        /// </summary>
        [Summary("引用参数")]
        Ref = 2,

        /// <summary>
        /// 返回值参数
        /// </summary>
        [Summary("返回值参数")]
        RetVal = 3
    }

    /// <summary>
    /// 参数信息解析数据接口
    /// </summary>
    [Summary("参数信息解析数据接口")]
    public interface IParameterData
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        [Summary("参数名称")]
        string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [Summary("参数类型")]
        Type ParameterType { get; }

        /// <summary>
        /// 是否有默认值
        /// </summary>
        [Summary("是否有默认值")]
        bool HasDefaultValue { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        [Summary("默认值")]
        object DefaultValue { get; }

        /// <summary>
        /// 参数方向（in/out/ref）
        /// </summary>
        [Summary("参数方向（in/out/ref）")]
        ParameterDirection Direction { get; }

        /// <summary>
        /// 是否为 params 参数
        /// </summary>
        [Summary("是否为 params 参数")]
        bool IsParams { get; }

        /// <summary>
        /// 生成格式化的参数字符串
        /// </summary>
        [Summary("生成格式化的参数字符串")]
        string GetFormattedString();
    }
}
