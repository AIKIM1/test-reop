using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001.Common
{
	public static class CommonCodeColor
	{
		public static Color GetCommonColor( string cmcode, Color failOverColor ) {
			string[] clrcomp = cmcode.Split( ',' );
			if( clrcomp.Length == 1 ) {
				try {
					return (Color)ColorConverter.ConvertFromString( clrcomp[0] );
				} catch( Exception ) {
					return failOverColor;
				}
			} else if( clrcomp.Length == 3 ) {
				try {
					int r = Int32.Parse( clrcomp[0] );
					int g = Int32.Parse( clrcomp[1] );
					int b = Int32.Parse( clrcomp[2] );
					return Color.FromRgb( (byte)r, (byte)g, (byte)b );
				} catch( Exception ) {
					return failOverColor;
				}
			}
			return failOverColor;
		}
	}
}
