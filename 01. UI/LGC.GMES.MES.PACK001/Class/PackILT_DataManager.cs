/*************************************************************************************
 Created Date : 2023.03.30
      Creator : 김선준
   Decription : Partial ILT - Rack Data Service
--------------------------------------------------------------------------------------
 [Change History]
   2023.03.30   김선준  : Partial ILT Rack Data Service 
**************************************************************************************/
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001.Class
{
    public class PackILT_DataManager
    {
        public static PackILT_DataManager Instance;
        public SocketEventManager SocketEventManager { get; set; }

        public PackILT_DataManager()
        {
        }

        // Socket Event Handler 
        public void AddILTObjectEventHandler(ISocketILTObjectEventHandler eventHandler)
        {
            this.SocketEventManager.AddILTObjectEventHandler(eventHandler);
        }

        public void RemoveILTObjectEventHandler(ISocketILTObjectEventHandler eventHandler)
        {
            this.SocketEventManager.RemoveILTObjectEventHandler(eventHandler);
        }
    }

    public interface ISocketILTObjectEventHandler
    {
        // 변경된 ILTObject를 포함하는 Folder를 표시하고 있는 경우 Reload
        // 해당 ILTObject의 속성을 보여주는 화면에서  reload
        void OnILTObjectPropertyModified(Dictionary<bool, List<bool>> modifiedObjectPathDic);
    }

    public partial class SocketEventManager
    {
        private List<ISocketILTObjectEventHandler> ILTObjectEventHandlers = new List<ISocketILTObjectEventHandler>();

        public void OnILTObjectNotification(ILTObjectFolder _ILTObjectFolder)
        {
            this.OnILTObjectPropertyModified(_ILTObjectFolder);
        }

        public void AddILTObjectEventHandler(ISocketILTObjectEventHandler eventHandler)
        {
            this.ILTObjectEventHandlers.Add(eventHandler);
        }

        public void RemoveILTObjectEventHandler(ISocketILTObjectEventHandler eventHandler)
        {
            this.ILTObjectEventHandlers.Remove(eventHandler);
        }

        public void OnILTObjectPropertyModified(ILTObjectFolder _ILTObjectFolder)
        {
            #region MyRegion
            if (_ILTObjectFolder == null)
                return;

            Dictionary<bool, List<bool>> modifiedObjectPathDic = new Dictionary<bool, List<bool>>();
            // Modify :: Folder 내 list가 변경된 것이 아니라 Folder 내 Object의 속성이 변경 됨

            List<bool> modifiedObjectPaths = new List<bool>();
            modifiedObjectPathDic.Add(_ILTObjectFolder.bRackName, modifiedObjectPaths);
            #endregion

            this.OnILTObjectPropertyModifiedImpl(modifiedObjectPathDic);
        }

        private void OnILTObjectPropertyModifiedImpl(Dictionary<bool, List<bool>> modifiedObjectPathDic)
        {
            // 리스트 창 등에서 처리토록
            if (this.ILTObjectEventHandlers.Count > 0)
            {
                foreach (ISocketILTObjectEventHandler handler in this.ILTObjectEventHandlers.ToArray())
                    handler.OnILTObjectPropertyModified(modifiedObjectPathDic);
            }
        }
    }

    public partial class ILTObjectFolder : object, System.ComponentModel.INotifyPropertyChanged
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
