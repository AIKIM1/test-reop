   M S B A M L   `   `   `  ����  LGC.GMES.MES.MainFrame?    9LGC.GMES.MES.MainFrame.ConfigWindows.Common_sub.UserCheck   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  GMES User Checking��5   	   $	��250��6$   $	��140��60    � WindowStartupLocation$ CenterScreen=�6=    � 	Resources 5       �� Source�� 5   
   $@ :/LGC.GMES.MES.ControlsLibrary;component/ControlStyles.xaml?�6   %    .��5      �  � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   ��  &  "Content_InputForm_LabelBorderStyle#���   6   ._�5      �� $��ID��6      Content_InputForm_LabelStyle#���  6"   $	��9,3q�6Y   5      �� 5   
   $��1��6   $��-1,0,0,0q�6!   ._�5      �� -   5         ��Name$
 tbID��5       �� 
IsReadOnly .  " Content_InputForm_TextBoxStyle#���  67   $	��8,3q�6p   5      �� 5   
   #���   6   $��1��6N   $��0,-1,0,0q�6[   ._�5      �� $��PW��6   #���  6"   $	��9,3q�6Y   5      �� 5   
   $��1��6   $��1��6   $��	-1,-1,0,0q�6.   ._�5      G� -   5         G�Name$
 tbPW��5      $	��8,3q�6(   5      �� 5!   
   	 �� Orientation$	 
Horizontal=�6   $��2��6.   $��2��6;   $��Right=�6O   $��Bottom=�6k   .+�5"      �� -   5        $��5q�5"      $��80��6   ��6    ��   GlobalObjectDicConverter
 �� 	Converter#
 [  �� ConverterParameter$ OK��6�   �� 5#      -   5        $��5q�5#      $��80��6�   ��6    �� #
 [ $ Cancel��6�   5$      5%      5'      