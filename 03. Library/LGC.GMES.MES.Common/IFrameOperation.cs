using System.Data;

namespace LGC.GMES.MES.Common
{
    public delegate void ReceiveScannerDataHandler(string portName, string value);

    public interface IFrameOperation
    {
        void PrintMessage(string msg);

        void PrintFrameMessage(string msg, bool isUrgent = false);

        bool Barcode_ZPL_Print(DataRow drserialprint, string zpl);

        bool Barcode_ZPL_USB_Print(string zpl);

        bool Barcode_ZPL_LPT_Print(DataRow drserialprint, string zpl);

        bool PrintUsbBarcodeEquipment(string zplCode, string equipmentId);

        bool PrintUsbBarCodeLabelByUniversalTransformationFormat8(string zplCode);

        bool PrintUsbBarCodeLabel(string zplCode, string labelCode);


        //bool PrintZPL(string labelType, string zpl);

        //bool PrintZPL(string labelType, string shopid, string modelsuffix, DataSet ds, int copies);

        void OpenMenu(string menuID, bool reopen = false, params object[] param);

        void OpenMenuFORM(string sMenuID, string sFormID, string sNameSpace, string sMenuName, bool reopen = false, params object[] param);

        void OpenDummyMenu(object menuID, bool reopen = false, params object[] param); //개발 초기 메뉴 데이터 없이 구동 하도록 개발함.

        bool CheckScannerState();

        event ReceiveScannerDataHandler ReceiveScannerData;

        string MENUID { get; }

        string AUTHORITY { get; }

        object[] Parameters { get; }
        bool IsCurrupted { get; set; }

        void ClearFrameMessage();

        void PageFixed(bool isfixed = false);
        
        bool SendScanData(string data);    
    }
}