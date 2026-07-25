// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// URL 工具类，提供 URL 验证、规范化及参数解析方法
    /// </summary>
    [Summary("URL 工具类，提供 URL 验证、规范化及参数解析方法")]
    public static class UrlUtility
    {
        static bool Internal_IsValidWebProtocol(string scheme) =>
            string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 验证并规范化 URL，如果输入无效则返回回退 URL。
        /// </summary>
        [Summary("验证并规范化 URL，如果输入无效则返回回退 URL")]
        public static string ValidateAndNormalizeUrl(string inputUrl, string fallbackUrl)
        {
            inputUrl = inputUrl?.Trim() ?? "";

            if (string.IsNullOrEmpty(inputUrl))
            {
                return fallbackUrl;
            }

            if (Uri.TryCreate(inputUrl, UriKind.Absolute, out var uriResult) &&
                Internal_IsValidWebProtocol(uriResult.Scheme))
            {
                return uriResult.ToString();
            }

            return fallbackUrl;
        }

        /// <summary>
        /// 检查 URL 方案是否为有效的 Web 协议（HTTP 或 HTTPS）
        /// </summary>
        [Summary("检查 URL 方案是否为有效的 Web 协议（HTTP 或 HTTPS）")]
        public static bool IsValidWebProtocol(string scheme) =>
            Internal_IsValidWebProtocol(scheme);

        /// <summary>
        /// 解析 URL 查询参数并返回字典
        /// </summary>
        [Summary("解析 URL 查询参数并返回字典")]
        public static Dictionary<string, string> GetQueryParams(string url)
        {
            var paramsDict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(url))
            {
                return paramsDict;
            }

            try
            {
                var uri = new Uri(url);
                var query = uri.Query;
                if (string.IsNullOrEmpty(query))
                {
                    return paramsDict;
                }

                var pairs = query.TrimStart('?').Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        paramsDict[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
                    }
                    else if (keyValue.Length == 1)
                    {
                        paramsDict[Uri.UnescapeDataString(keyValue[0])] = string.Empty;
                    }
                }
            }
            catch
            {
                // 忽略解析错误
            }

            return paramsDict;
        }

        /// <summary>
        /// 将参数字典合并到指定 URL 中
        /// </summary>
        [Summary("将参数字典合并到指定 URL 中")]
        public static string AddQueryParams(string url, IDictionary<string, string> queryParams)
        {
            if (string.IsNullOrEmpty(url) || queryParams == null || queryParams.Count == 0)
            {
                return url;
            }

            var uriBuilder = new UriBuilder(url);
            var query = uriBuilder.Query.TrimStart('?');
            var existingParams = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=')).ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : string.Empty);

            foreach (var kvp in queryParams)
            {
                existingParams[Uri.EscapeDataString(kvp.Key)] = Uri.EscapeDataString(kvp.Value);
            }

            uriBuilder.Query = string.Join("&", existingParams.Select(p => $"{p.Key}={p.Value}"));
            return uriBuilder.ToString();
        }
    }
}
