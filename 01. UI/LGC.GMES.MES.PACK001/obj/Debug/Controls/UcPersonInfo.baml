   M S B A M L   `   `   `  ����  LGC.GMES.MES.PACK0010    *LGC.GMES.MES.PACK001.Controls.UcPersonInfo   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      $��25��5   7   $	��200��6C   
���   �  � 	Resources  5       �� MergedDictionaries �� Source�� 5	   
    5
      �� 5      $@ :/LGC.GMES.MES.ControlsLibrary;component/ControlStyles.xaml?�6%   %    5      .��5      � -   5         �Name$ grdUserInfo��5       � RowDefinitions 5   
   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $
��Auto �6   �� 5      $	��100 �6   �� 5      $��25 �6   �� 5      $��0 �6   .+�5   
   �� -   5         ��Name$ txtTitle��5   ?   $��1��6   $
��Auto��6$   $
��Auto��61       Content_InputForm_LabelStyle#��[  6�   ��6Q   ��  �� ConverterParameter$ 	작업자��  GlobalObjectDicConverter �� 	Converter# [ 	 �� Mode$	 OneWay=�6�   �� 5   
   -   5        
 ��Name$
 txtUser��5   =   $��2��6   $
��Auto��6"   $
��Auto��6/      SearchCondition_TextBoxStyle#��[ 6N   $��Stretch=�6�   $��Stretch=�6�   6
  �� 5   
   -   5         ��Name$ btnUserSearch��5   8   $��3��6   $��25��6!   $��25��6,   ��5      �� 5       �� ImageSource$% ../images/icon_input_search.png��6   5!      5#      5$      