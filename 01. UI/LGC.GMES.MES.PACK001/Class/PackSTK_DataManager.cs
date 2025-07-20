/*************************************************************************************
 Created Date : 2023.06.26
      Creator : 김선준
   Decription : Pack STK - Rack Data Service
--------------------------------------------------------------------------------------
 [Change History]
   2023.03.30   김선준  : Pack STK Rack Data Service 
**************************************************************************************/
using System.Collections.Generic;
using System.Data;

namespace LGC.GMES.MES.PACK001.Class
{
    public class PackSTK_DataManager
    {
        public static PackSTK_DataManager Instance;
        public SocketEventManager SocketEventManager { get; set; }

        public PackSTK_DataManager()
        {
        }

        // Socket Event Handler 
        public void AddSTKObjectEventHandler(ISocketSTKObjectEventHandler eventHandler)
        {
            try
            {
                this.SocketEventManager.AddSTKObjectEventHandler(eventHandler);
            }
            catch (System.Exception)
            {
            }
        }

        public void RemoveSTKObjectEventHandler(ISocketSTKObjectEventHandler eventHandler)
        {
            try
            {
                this.SocketEventManager.RemoveSTKObjectEventHandler(eventHandler);
            }
            catch (System.Exception)
            {
            }
        }
    }

    public interface ISocketSTKObjectEventHandler
    {
        // 변경된 STKObject를 포함하는 Folder를 표시하고 있는 경우 Reload
        // 해당 STKObject의 속성을 보여주는 화면에서  reload
        void OnSTKObjectPropertyModified(STKObjectFolder _STKObjectFolder);
    }

    public partial class SocketEventManager
    {
        private List<ISocketSTKObjectEventHandler> STKObjectEventHandlers = new List<ISocketSTKObjectEventHandler>();

        public void OnSTKObjectNotification(STKObjectFolder _STKObjectFolder)
        {
            this.OnSTKObjectPropertyModified(_STKObjectFolder);
        }

        public void AddSTKObjectEventHandler(ISocketSTKObjectEventHandler eventHandler)
        {
            this.STKObjectEventHandlers.Add(eventHandler);
        }

        public void RemoveSTKObjectEventHandler(ISocketSTKObjectEventHandler eventHandler)
        {
            this.STKObjectEventHandlers.Remove(eventHandler);
        }

        public void OnSTKObjectPropertyModified(STKObjectFolder _STKObjectFolder)
        {
            #region MyRegion
            if (_STKObjectFolder == null) return;
             
            #endregion

            this.OnSTKObjectPropertyModifiedImpl(_STKObjectFolder);
        }

        private void OnSTKObjectPropertyModifiedImpl(STKObjectFolder _STKObjectFolder)
        {
            // 리스트 창 등에서 처리토록
            if (this.STKObjectEventHandlers.Count > 0)
            {
                foreach (ISocketSTKObjectEventHandler handler in this.STKObjectEventHandlers.ToArray())
                    handler.OnSTKObjectPropertyModified(_STKObjectFolder);
            }
        }
    }

    public partial class STKObjectFolder : object, System.ComponentModel.INotifyPropertyChanged
    {
        private bool _bRackName; 
        public bool bRackName
        {
            get
            {
                return this._bRackName;
            }
            set
            {
                this._bRackName = value;
                this.RaisePropertyChanged("bRackName");
            }
        }

        private DataTable _dtRackInfo;
        public DataTable dtRackInfo
        {
            get
            {
                return this._dtRackInfo;
            }
            set
            {
                this._dtRackInfo = value;
                this.RaisePropertyChanged("dtRackInfo");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
