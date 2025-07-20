using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LGC.GMES.MES.ControlsLibrary
{
    /// <summary>
    /// XAML 파일에서 이 사용자 지정 컨트롤을 사용하려면 1a 또는 1b단계를 수행한 다음 2단계를 수행하십시오.
    ///
    /// 1a단계) 현재 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:LGC.GMES.MES.ControlsLibrary"
    ///
    ///
    /// 1b단계) 다른 프로젝트에 있는 XAML 파일에서 이 사용자 지정 컨트롤 사용.
    /// 이 XmlNamespace 특성을 사용할 마크업 파일의 루트 요소에 이 특성을 
    /// 추가합니다.
    ///
    ///     xmlns:MyNamespace="clr-namespace:LGC.GMES.MES.ControlsLibrary;assembly=LGC.GMES.MES.ControlsLibrary"
    ///
    /// 또한 XAML 파일이 있는 프로젝트의 프로젝트 참조를 이 프로젝트에 추가하고
    /// 다시 빌드하여 컴파일 오류를 방지해야 합니다.
    ///
    ///     솔루션 탐색기에서 대상 프로젝트를 마우스 오른쪽 단추로 클릭하고
    ///     [참조 추가]->[프로젝트]를 차례로 클릭한 다음 이 프로젝트를 찾아서 선택합니다.
    ///
    ///
    /// 2단계)
    /// 계속 진행하여 XAML 파일에서 컨트롤을 사용합니다.
    ///
    ///     <MyNamespace:OperationWindow/>
    ///
    /// </summary>
    public class BaseLayout : Control
    {
        #region 화면 속성
        public static readonly DependencyProperty IsSideMenuOpenProperty = DependencyProperty.Register("IsSideMenuOpen", typeof(bool), typeof(BaseLayout), new PropertyMetadata(false));
        public bool IsSideMenuOpen
        {
            get
            {
                return (bool)this.GetValue(IsSideMenuOpenProperty);
            }
            set
            {
                this.SetValue(IsSideMenuOpenProperty, value);
            }
        }

        public static readonly DependencyProperty SearchPanelVisibilityProperty = DependencyProperty.Register("SearchPanelVisibility", typeof(Visibility), typeof(BaseLayout), new PropertyMetadata(Visibility.Visible));
        public Visibility SearchPanelVisibility
        {
            get
            {
                return (Visibility)this.GetValue(SearchPanelVisibilityProperty);
            }
            set
            {
                this.SetValue(SearchPanelVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty SearchPanelProperty = DependencyProperty.Register("SearchPanel", typeof(object), typeof(BaseLayout), new PropertyMetadata(null));
        public object SearchPanel
        {
            get
            {
                return this.GetValue(SearchPanelProperty);
            }
            set
            {
                this.SetValue(SearchPanelProperty, value);
            }
        }

        public static readonly DependencyProperty ContentAreaProperty = DependencyProperty.Register("ContentArea", typeof(object), typeof(BaseLayout), new PropertyMetadata(null));
        public object ContentArea
        {
            get
            {
                return this.GetValue(ContentAreaProperty);
            }
            set
            {
                this.SetValue(ContentAreaProperty, value);
            }
        }

        public static readonly DependencyProperty OperationButtonAreaProperty = DependencyProperty.Register("OperationButtonArea", typeof(ObservableCollection<UIElement>), typeof(BaseLayout), new PropertyMetadata(null));
        public ObservableCollection<UIElement> OperationButtonArea
        {
            get
            {
                return (ObservableCollection<UIElement>)this.GetValue(OperationButtonAreaProperty);
            }
        }
        private StackPanel _spBtnArea = null;

        public static readonly DependencyProperty SideTopMenuProperty = DependencyProperty.Register("SideTopMenu", typeof(ObservableCollection<UIElement>), typeof(BaseLayout), new PropertyMetadata(null));
        public ObservableCollection<UIElement> SideTopMenu
        {
            get
            {
                return (ObservableCollection<UIElement>)this.GetValue(SideTopMenuProperty);
            }
        }
        private StackPanel _spSideTopBtnArea = null;

        void OperationButtonArea_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_spBtnArea == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (UIElement element in e.NewItems)
                    {
                        _spBtnArea.Children.Add(element);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (UIElement element in e.OldItems)
                    {
                        _spBtnArea.Children.Remove(element);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _spBtnArea.Children.Clear();
                    break;
                default:
                    break;
            }
        }

        void SideTopMenu_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_spSideTopBtnArea == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (UIElement element in e.NewItems)
                    {
                        _spSideTopBtnArea.Children.Add(element);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (UIElement element in e.OldItems)
                    {
                        _spSideTopBtnArea.Children.Remove(element);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _spSideTopBtnArea.Children.Clear();
                    break;
                default:
                    break;
            }
        }

        private Button _btnSearch;
        public event RoutedEventHandler SearchClick;
        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchClick != null)
            {
                SearchClick(this, e);
            }
        }

        public override void OnApplyTemplate()
        {
            _btnSearch = (Button)this.GetTemplateChild("btnSearch");
            _spBtnArea = (StackPanel)this.GetTemplateChild("spBtnArea");
            _spSideTopBtnArea = (StackPanel)this.GetTemplateChild("spSideTopBtnArea");

            base.OnApplyTemplate();

            foreach (UIElement element in OperationButtonArea)
            {
                _spBtnArea.Children.Add(element);
            }

            foreach (UIElement element in SideTopMenu)
            {
                _spSideTopBtnArea.Children.Add(element);
            }

            _btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
        }
        #endregion

        static BaseLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseLayout), new FrameworkPropertyMetadata(typeof(BaseLayout)));
        }

        public BaseLayout()
        {
            ObservableCollection<UIElement> elementList = new ObservableCollection<UIElement>();
            elementList.CollectionChanged += new NotifyCollectionChangedEventHandler(OperationButtonArea_CollectionChanged);
            this.SetValue(OperationButtonAreaProperty, elementList);

            elementList = new ObservableCollection<UIElement>();
            elementList.CollectionChanged += new NotifyCollectionChangedEventHandler(SideTopMenu_CollectionChanged);
            this.SetValue(SideTopMenuProperty, elementList);
        }

    }
}
