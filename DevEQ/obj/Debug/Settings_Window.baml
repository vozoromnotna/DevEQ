   M S B A M L   `   `   `  ����@  <DevEQ, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:DevEQDevEQ  5   	       DevEQ.Settings_Window   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:DevEQ     5   	   X TXceed.Wpf.Toolkit, Version=4.3.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	     � Title$  Settings��5	   	   $	��244��6   $	��514��6'    � 	Resources 5
      #   DevEQ.InverseBooleanConverter   InverseBooleanConverter��%   &	          5   
   6(   5      .��5      � $��10,10,10,59q�6    5   
   $  Xceed.Wpf.Toolkit.DoubleUpDown �� 
TargetType �� Property �� Value%  Xceed.Wpf.Toolkit.IntegerUpDown��%l  '        '  Q     '�� �     '�� �     �� 5      "  6   .�5      ��  � ��$ 10,10,10, 0q�6+   5      �� 5      "  6   .�5      ��  � ��$ 10,10,10, 0q�6+   5      �� 5      " ��6   .�5      ��  � ��$ 10,10,10, 0q�6+   5      �� 5      " ��6   .�5      ��  � ��$ 10,10,10, 0q�6+   5      5       � ColumnDefinitions 5   
   �� 5      �� 5       � RowDefinitions 5    
   �� 5!      �� 5"      �� 5#      �� 5$      .+�5&   
   �� $��0��6   $��0��6   .��6-   ;9Количество точек при прочтении6M   �� 5'   
   $��1��6   $��0��6   .��6-   +)Максимальная мощность6D   �� 5(   
   $��2��6   $��0��6   $��11(�6-   .��6;   FDЗапись амплитуды для исходных частот6a   �� 5)   
   $��3��6   $��0��6   .��6-   53Количество точек при записи6J     5*   
   $��1��6   $��0��6-   �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Value 6:   �� *MainModel.PointsCount+  5+   
   $��1��6   $��1��6,   �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Value 69   �� *MainModel.maxIntense+�� 5,   
   -   5        	 ��Name$	 
ChB_SaveHz��5,      $��1��6'   $��2��67   
 d� 	IsChecked
 6D   �� *MainModel.SaveInitHz+  5-   
   $��1��6   $��3��6-    6:   �� *MainModel.PointsCountToSave+{�6h   �� *MainModel.SaveInitHz+ �� 	Converter# [  5.      5/      