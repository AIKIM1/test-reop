using System;

namespace LGC.GMES.MES.Common
{
    /// <summary>
    /// 오브젝트 인스턴스가 활성화되고 액티비티 변경 알림을 정의하는 인터페이스
    /// </summary>
    public interface IActiveAware
    {
        /// <summary>
        /// 개체가 활성화되어 있는지 여부를 나타내는 값을 가져오거나 설정한다.
        /// </summary>
        /// <value>오브젝트가 활성화되면 true, 아니면 false</value>
        bool IsActive { get; set; }

        /// <summary>
        /// 속성 값 변경될 때 발생
        /// </summary>
        event EventHandler IsActiveChanged;
    }
}