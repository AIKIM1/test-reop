using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LGC.GMES.MES.Common.Mvvm
{
    /// <summary>
    /// System.ComponentModel.INotifyPropertyChanged 구현. 모델로 변환해 준다.
    /// </summary>
    public abstract class BindableBase : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// 속성 값이 변경되면 발생하는 이벤트 핸들러
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected BindableBase()
        {
        }

        /// <summary>
        /// 속성값이 변경되면 리스너에게 알려준다.
        /// </summary>
        /// <param name="propertyName">리스너에게 알려줄 속성명. System.Runtime.CompilerServices.CallerMemberNameAttribute를 지원하는 컴파일러가 자동으로 제공해 준다.
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 오브젝트의 PropertyChanged 이벤트를 발생시킨다.
        /// </summary>
        /// <typeparam name="T">신규 속성 타입</typeparam>
        /// <param name="propertyExpression">신규 속성 람다식</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            this.OnPropertyChanged(PropertySupport.ExtractPropertyName<T>(propertyExpression));
        }

        /// <summary>
        /// 속성값이 입력값과 같은지 확인. 속성을 설정하고 필요시 리스너에게 알려준다.
        /// </summary>
        /// <typeparam name="T">타입</typeparam>
        /// <param name="storage">속성 참조자</param>
        /// <param name="value">입력값</param>
        /// <param name="propertyName">속성명. 이벤트 발생시 자동으로 가져옴.</param>
        /// <returns>값이 변경됐으면 True, 같으면 False</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }
    }
}