/*************************************************************************************
 Created Date : 2015.09.30
      Creator : J.H.Lim
   Description : 공정진척관련 DataSet : 착/완공 대상LOT, 불량정보, 폐기정보, 연계자재정보, 연계SubLoT정보
--------------------------------------------------------------------------------------
 [Change History]
  2015.09.30 / J.H.Lim : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;


namespace LGC.GMES.MES.CMM001.Class
{
    public class MyClass
    {
        //공정진척관련 DataSet : 착/완공 대상LOT, 불량정보, 폐기정보, 연계자재정보, 연계SubLoT정보
        public static DataSet dsLotInfo;
        public DataSet staticDsLotInfo
        {
            set
            {
                dsLotInfo = staticDsLotInfo;
            }
            get
            {
                return staticDsLotInfo;
            }
        }

		//공정 검사 화면 -- 첨부파일 정보
		public static DataTable dtAttachedFile;
		public DataTable staticDtAttachedFile
		{
			set
			{
				dtAttachedFile = staticDtAttachedFile;
			}
			get
			{
				return staticDtAttachedFile;
			}
		}

        public static DataSet anyDataSet;
        public DataSet staticAnyDataSet
        {
            set
            {
                anyDataSet = staticAnyDataSet;
            }
            get
            {
                return staticAnyDataSet;
            }
        }

        public static DataTable anyDataTable;

        public DataTable staticAnyDataTable
        {
            set
            {
                anyDataTable = staticAnyDataTable;
            }
            get
            {
                return staticAnyDataTable;
            }
        }


        public static ArrayList aryGrd;

        public ArrayList staticAryGrd
        {
            set
            {
                aryGrd = staticAryGrd;
            }
            get
            {
                return staticAryGrd;
            }
        }

		//public static C1FlexGrid anyDataGrid;

		//public C1FlexGrid staticDataGrid
		//{
		//	set
		//	{
		//		anyDataGrid = staticDataGrid;
		//	}
		//	get
		//	{
		//		return staticDataGrid;
		//	}
		//}


        public static string staticValue = string.Empty;

        public string strValue
        {
            set
            {
                staticValue = strValue;
            }
            get
            {
                return staticValue;
            }
        }

        public interface IfReason
        {
            void SetData(string TypeID, string TypeName, string DefectID, string DefectName, int iRow);
        }

        public interface IfProcess
        {
            void SetData(string DataID, string DataName);
        }

        public interface IfEquipnent
        {
            void SetData(string DataID, string DataName);
        }

        public interface IfCalendar
        {
            void SetData(string DateName);
        }

        public interface IfFomRefresh
        {
            void RefreshData();
        }

        public interface IfPrinter
        {
            void SetData(string PrinterD, string PrinterName, int iRow);
        }

		/// <summary>
		///  staticOnePerson : POPUP창에서 선택된 1명의 인원 정보 호출한 폼으로 연결하기위한 선언
		/// </summary>
		public static List<Tuple<string, string, string, string, string>> staticOnePerson;

		/// <summary>
		///  staticPerson : POPUP창에서 선택된 인원의 정보 호출한 폼으로 연결하기위한 선언
		/// </summary>
		public static Dictionary<string, string> staticPerson;


		/// <summary>
		///  staticAffectLot : POPUP창에서 선택된 AFFECT LOT  정보 호출한 폼으로 연결하기위한 선언
		/// </summary>
		public static Dictionary<string, string> staticAffectLot;
    }
}
