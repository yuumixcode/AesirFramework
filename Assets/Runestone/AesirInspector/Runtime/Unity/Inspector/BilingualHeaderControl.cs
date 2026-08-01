using System;
using System.Diagnostics;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 双语顶部说明控件，用于模块的简单介绍。
    /// </summary>
    [Serializable]
    public class BilingualHeaderControl
    {
        [SerializeField]
        public BilingualDisplayAsStringControl headerName;

        [SerializeField]
        public BilingualDisplayAsStringControl headerIntroduction;

        readonly string _chineseIntroduction;
        readonly string _englishIntroduction;
        string _targetUrl;

        public BilingualHeaderControl(string chineseName,
            string englishName = null,
            string chineseIntroduction = null,
            string englishIntroduction = null,
            string targetUrl = null)
        {
            headerName = new BilingualDisplayAsStringControl(chineseName, englishName)
            {
                fontSize = 30,
                alignment = TextAlignment.Left
            };
            _chineseIntroduction = chineseIntroduction;
            _englishIntroduction = englishIntroduction ?? chineseIntroduction;
            headerIntroduction =
                new BilingualDisplayAsStringControl(_chineseIntroduction, _englishIntroduction)
                {
                    fontSize = 14,
                    enableRichText = true,
                    alignment = TextAlignment.Left
                };
            _targetUrl = targetUrl ?? AesirInspectorWebLinks.GitUrl;
        }

        /// <summary>
        /// 是否隐藏标题介绍
        /// </summary>
        public bool HideHeaderIntroduction => string.IsNullOrWhiteSpace(_chineseIntroduction) &&
                                              string.IsNullOrWhiteSpace(_englishIntroduction);

        /// <summary>
        /// 打开相关文档链接
        /// </summary>
        public void OpenUrl()
        {
            var validatedUrl = UrlUtility.ValidateAndNormalizeUrl(_targetUrl, AesirInspectorWebLinks.GitUrl);
            Application.OpenURL(validatedUrl);
        }

        public void PlaceholderMethod1() { }

        [Conditional("UNITY_EDITOR")]
        public void SwitchLanguage()
        {
            if (AesirInspectorLanguageSettingsSO.CurrentIsChinese)
            {
                AesirInspectorLanguageSettingsSO.SetEnglish();
            }
            else
            {
                AesirInspectorLanguageSettingsSO.SetChinese();
            }
        }

        public void PlaceholderMethod2() { }
    }
}
