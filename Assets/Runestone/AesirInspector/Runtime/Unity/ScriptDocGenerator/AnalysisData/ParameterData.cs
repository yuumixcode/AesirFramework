using System;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 参数信息解析数据
    /// </summary>
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
        public string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// 是否有默认值
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// 参数方向（in/out/ref）
        /// </summary>
        public ParameterDirection Direction { get; }

        /// <summary>
        /// 是否为 params 参数
        /// </summary>
        public bool IsParams { get; }

        /// <summary>
        /// 生成格式化的参数字符串
        /// </summary>
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
    public enum ParameterDirection
    {
        /// <summary>
        /// 输入参数
        /// </summary>
        In = 0,

        /// <summary>
        /// 输出参数
        /// </summary>
        Out = 1,

        /// <summary>
        /// 引用参数
        /// </summary>
        Ref = 2,

        /// <summary>
        /// 返回值参数
        /// </summary>
        RetVal = 3
    }

    /// <summary>
    /// 参数信息解析数据接口
    /// </summary>
    public interface IParameterData
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        Type ParameterType { get; }

        /// <summary>
        /// 是否有默认值
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// 参数方向（in/out/ref）
        /// </summary>
        ParameterDirection Direction { get; }

        /// <summary>
        /// 是否为 params 参数
        /// </summary>
        bool IsParams { get; }

        /// <summary>
        /// 生成格式化的参数字符串
        /// </summary>
        string GetFormattedString();
    }
}
