   M S B A M L   `   `   `  ����D  @MSiSvInfT, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MSiSvInfT	MSiSvInfT  5   	   $    MSiSvInfT.WindowHolstedMetrics   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   )localclr-namespace:MSiSvInfT     5   	     � Title$%  Метрики Холстеда��5   	   $	��500��6"   $	��800��6/   $��20(�6;   .��5      � -   5         �Name$
 grid��5       � RowDefinitions 5   
   �� 5      $��40 �6   �� 5	      $��* �6   �� 5
      $��52 �6   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��40 �6    � ColumnDefinitions 5   
   �� 5      �� 5      .+�5   
   �� $��0��6   $��2��6    $��0��64   $%��Метрики Холстеда��6A   $��Center=�6\   �� 5   
   -   5         ��Name$ OperatorsScrollViewer��5      $��0��66   $��1��6F   $��1q�6S   $��5,0,5,0q�6g    � ContextMenu 5      �� 5      .@�5      m� -   5         m�Name$ ChangeOperatorsTable��5       m� IsCheckable .  6Q   5      5      �� 5   
   -   5        $ OperandsScrollViewer��5      $��1��65   $��1��6E   $��1q�6R   $��5,0,5,0q�6f    5      �� 5      .@�5      m� -   5        $ ChangeOperandsTable��5       .  6P   5      5!      �� 5"   
   -   5        $ OperatorsEndScrollViewer��5"      $��0��69   $��2��6I   $��1q�6V   $��5,0,5,0q�6j   �� 5#   
   -   5        $ OperandsEndScrollViewer��5#      $��1��68   $��2��6H   $��1q�6U   $��5,0,5,0q�6i   �� 5$   
   -   5         ��Name$ ProgramsDictionary��5$      $��0��60   $��2��6@   $��3��6T   $)��#Словарь программы: ��6a   $��Center=�6|   $��Center=�6�   ��6�   �� 	 �� RelativeSource	 �� 
 �� Mode$
 FindAncestor=� �� AncestorType$ WindowL� �� Path$ 
Background��� 5%   
   -	   5        $ ProgramsLength��5%      $��0��6,   $��2��6<   $��4��6P   $%��Длина программы: ��6^   $��Center=�6w   $��Center=�6�   ��6�   �� 	 �� $
 FindAncestor=�$ WindowL�$ 
Background��� 5&   
   -
   5        $ ProgramsVolume��5&      $��0��6,   $��2��6<   $��5��6P   $%��Объём программы: ��6^   $��Center=�6w   $��Center=�6�   ��6�   �� 	 �� $
 FindAncestor=�$ WindowL�$ 
Background�5'      5(      