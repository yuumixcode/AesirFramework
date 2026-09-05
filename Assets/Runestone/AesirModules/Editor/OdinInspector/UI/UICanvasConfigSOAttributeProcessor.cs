using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="UICanvasConfigSO" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 Odin 专属显示特性（分组、条件显示等），使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class UICanvasConfigSOAttributeProcessor : OdinAttributeProcessor<UICanvasConfigSO>
    {
        /// <summary>
        /// 为各序列化字段注入 Odin 显示特性（分组、条件显示等）。
        /// </summary>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(UICanvasConfigSO.renderMode):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    attributes.Add(new ReadOnlyAttribute());
                    break;

                case nameof(UICanvasConfigSO.pixelPerfect):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    break;

                case nameof(UICanvasConfigSO.planeDistance):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    break;

                case nameof(UICanvasConfigSO.sortingLayerName):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    attributes.Add(new ValueDropdownAttribute(nameof(UICanvasConfigSO.GetSortingLayerNames)));
                    break;

                case nameof(UICanvasConfigSO.additionalShaderChannels):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    break;

                case nameof(UICanvasConfigSO.vertexColorAlwaysGammaSpace):
                    attributes.Add(new BoxGroupAttribute("Canvas Component"));
                    break;

                case nameof(UICanvasConfigSO.scaleMode):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    break;

                case nameof(UICanvasConfigSO.referenceResolution):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsScaleWithScreenSize)));
                    break;

                case nameof(UICanvasConfigSO.screenMatchMode):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsScaleWithScreenSize)));
                    break;

                case nameof(UICanvasConfigSO.scaleFactor):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsConstantPixelSize)));
                    break;

                case nameof(UICanvasConfigSO.matchWidthOrHeight):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(nameof(UICanvasConfigSO
                        .ScaleModeIsScaleWithScreenSizeAndScreenMatchModeIsMatchWidthOrHeight)));
                    break;

                case nameof(UICanvasConfigSO.referencePixelsPerUnit):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    break;

                case nameof(UICanvasConfigSO.physicalUnit):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsConstantPhysicalSize)));
                    break;

                case nameof(UICanvasConfigSO.fallbackScreenDPI):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsConstantPhysicalSize)));
                    break;

                case nameof(UICanvasConfigSO.defaultSpriteDPI):
                    attributes.Add(new BoxGroupAttribute("Canvas Scaler Component"));
                    attributes.Add(new ShowIfAttribute(
                        nameof(UICanvasConfigSO.ScaleModeIsConstantPhysicalSize)));
                    break;

                case nameof(UICanvasConfigSO.ignoreReversedGraphics):
                    attributes.Add(new BoxGroupAttribute("Graphic Raycaster Component"));
                    break;

                case nameof(UICanvasConfigSO.blockingObjects):
                    attributes.Add(new BoxGroupAttribute("Graphic Raycaster Component"));
                    break;

                case nameof(UICanvasConfigSO.blockingMask):
                    attributes.Add(new BoxGroupAttribute("Graphic Raycaster Component"));
                    break;
            }
        }
    }
}
