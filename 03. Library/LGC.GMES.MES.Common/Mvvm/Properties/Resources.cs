using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace LGC.GMES.MES.Common.Mvvm.Properties
{
    /// <summary>
    /// 다국어 찾기를 위한 강력한 타입의 리소스 클래스.
    /// </summary>
    [CompilerGenerated]
    [DebuggerNonUserCode]
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class Resources
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        /// <summary>
        /// 지역화된 문자열 그 자체가 CompositeCommand를 등록할 수 없는 것을 찾는다.
        /// </summary>
        internal static string CannotRegisterCompositeCommandInItself
        {
            get
            {
                return Resources.ResourceManager.GetString("CannotRegisterCompositeCommandInItself", Resources.resourceCulture);
            }
        }

        /// <summary>
        /// 같은 명령을 다중 호출 방지.
        /// </summary>
        internal static string CannotRegisterSameCommandTwice
        {
            get
            {
                return Resources.ResourceManager.GetString("CannotRegisterSameCommandTwice", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   이 강력한 형식의 리소스 클래스를 사용 중인 모든 리소스를 조회하여 현재 쓰레드의 CurrentUICulture 속성을 재정의.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Resources.resourceCulture;
            }
            set
            {
                Resources.resourceCulture = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static string DelegateCommandDelegatesCannotBeNull
        {
            get
            {
                return Resources.ResourceManager.GetString("DelegateCommandDelegatesCannotBeNull", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   
        /// </summary>
        internal static string DelegateCommandInvalidGenericPayloadType
        {
            get
            {
                return Resources.ResourceManager.GetString("DelegateCommandInvalidGenericPayloadType", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   
        /// </summary>
        internal static string PropertySupport_ExpressionNotProperty_Exception
        {
            get
            {
                return Resources.ResourceManager.GetString("PropertySupport_ExpressionNotProperty_Exception", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   
        /// </summary>
        internal static string PropertySupport_NotMemberAccessExpression_Exception
        {
            get
            {
                return Resources.ResourceManager.GetString("PropertySupport_NotMemberAccessExpression_Exception", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   
        /// </summary>
        internal static string PropertySupport_StaticExpression_Exception
        {
            get
            {
                return Resources.ResourceManager.GetString("PropertySupport_StaticExpression_Exception", Resources.resourceCulture);
            }
        }

        /// <summary>
        ///   
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Resources.resourceMan, null))
                {
                    ResourceManager resourceManager = new ResourceManager("LGC.GMES.MES.Common.Mvvm.Properties.Resources", Type.GetTypeFromHandle(typeof(Resources).TypeHandle).GetTypeInfo().Assembly);
                    Resources.resourceMan = resourceManager;
                }

                return Resources.resourceMan;
            }
        }

        /// <summary>
        ///   
        /// </summary>
        internal static string WeakEventHandlerNotConstructedOnUIThread
        {
            get
            {
                return Resources.ResourceManager.GetString("WeakEventHandlerNotConstructedOnUIThread", Resources.resourceCulture);
            }
        }

        internal Resources()
        {
        }
    }
}