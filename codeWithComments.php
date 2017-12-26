<?php
function Separate()
{
	echo "\n-------------------------------------------------------------------------------------\n";
}

function InputLength()
{
	echo "Введите длину массива: ";
	$input = readline();
	settype($input, 'integer');
	return $input;
}

function SetArray($size)
{
	$array;
	for ($i = 0; $i < $size; $i++)
	{
		$index = $i + 1;
		echo "A[$index] = ";
		$input = readline();
		if (!settype($input, 'double'))
		{
			echo "Вводите только числа!\n";
			$i--;
		}
		$array[$i] = $input;
	}
	return $array;
}

function CreateArray()
{
	$size = InputLength();
	$array = SetArray($size);
	return $array;
}

function RandomArray()
{
	$size = rand(5, 20);
	for ($i = 0; $i < $size; $i++)
	{
		$array[$i] = rand(1, 100);
	}
	return $array;
}




function PrintArray($array, $size)
{
	$string;
	for ($i = 0; $i < $size; $i++)
	{
		$string .= "$array[$i]";
		if ($i != $size - 1)
		{
			$string .= ", ";
		}
	}
	$string .= "\n";
	return $string;
}

function PrintMenuLines($isArrayCreated)
{
	echo "\nВыберите пункт меню:\n";
	if (isArrayCreated)
	{
		echo "1.Изменить массив\n";
	}
	else
	{
		echo "1.Задать массив\n";
	}
	echo "2.Случайный массив\n";
	echo "3.Сортировка\n";
	echo "4.Выход\n";
}

function PrintSortsMenuLines()
{
	echo "\nВыберите номер сортировки:\n";
	echo "\t1.Сортировка вставками\n";
	echo "\t2.Сортировка выбором\n";
	echo "\t3.Сортировка пузырьком\n";
	echo "\t4.Назад\n";
}

function PrintAfterSortingArrayMenuLines()
{
	echo "\nВыберите следующее действие\n";
	echo "1.Показать ход сортировки\n";
	echo "2.Продолжить, сохранив измениния в массиве\n";
	echo "3.Продолжить, не сохраняя изменения в массиве\n";
}

function SelectMenuLine($max)
{
	$input;
	$isRightSelection = false;
	echo "\nВыбор: ";
	do
	{
		$input = readline();
		if (!settype($input, 'integer'))
		{
			echo "Введите целое число. Повторите попытку: ";
		}
		else
		{
			if ($input < 1 || $input > $max)
			{
				echo "Введите число в диапазоне [1; $max].\nПовторите попытку: ";
			}
			else
			{
				$isRightSelection = true;
			}
		}
		
	}
	while (!$isRightSelection);
	return $input;
}

function Menu(&$array)
{
	if ($array)
	{
		echo "Массив: ";
		echo PrintArray($array, count($array));
	}
	else
	{
		echo "Массив не задан\n";
	}
	PrintMenuLines(isArrayCreated);
	return SelectMenuLine(MENU_LINES_COUNT);
}

function SelectSortType()
{
	PrintSortsMenuLines();
	return SelectMenuLine(SORTS_MENU_LINES_COUNT);
}

function CheckArray(&$array)
{
	$count = count($array);
	if ($count == 0)
	{
		return false;
	}
	return true;
}

function ExecutingSelectedAction($selection, &$array)
{
	define ("EXIT_SELECTION", 4);
	switch ($selection)
	{
	case 1:
		$array = CreateArray();
		return true;
	case 2:
		$array = RandomArray();
		return true;
	case 3:
		if (CheckArray($array))
		{
			Sorting(SelectSortType(), $array);
		}
		else
		{
			echo "Массив не задан!";
		}
		return true;
	case EXIT_SELECTION:
		return false;
	}
}

function SortedArrayMessage(&$sortedArray, &$array, $time, $transpositionsCount, $process)
{
	do
	{
		Separate();
		echo "Массив отсортирован. Результат: ";
		echo PrintArray($sortedArray, count($sortedArray));
		echo "\nЗатрачено времени: $time мкс";
		echo "\nЧисло перестановок: $transpositionsCount\n\n";
		
	}
	while (AfterSortingMenu($process, $sortedArray, $array));
}

function WriteProcess(&$transpositionsCount, &$process, &$arr, $count)
{
	$transpositionsCount++;
	$process .= "Шаг $transpositionsCount: ";
	$process .= PrintArray($arr, $count);
}

function ShowProcess($process)
{
	Separate();
	echo $process;
	Separate();
}

function AfterSortingMenu(&$process, &$sortedArray, &$array)
{
	PrintAfterSortingArrayMenuLines();
	$selection = SelectMenuLine(AFTER_SORTING_ARRAY_MENU_LINES);
	switch ($selection)
	{
	case 1:
		ShowProcess($process);
		return true;
	case 2:
		$array = $sortedArray;
		return false;
	case 3:
		return false;
	}
}

//-----------------------------------------------------------------------------------------------
function Sorting($selection, &$array)
{
	// selection - номер выбранной сортировки
	// array - исходный массив, '&' - передача пар-ра по ссылке
	$newArray; $transpositionsCount; $count; $process; $timeBefore // !!!Объявление переменных
	$newArray = $array; // создание массива, сортировка которого не приведёт к изменению исходного массива
	$transpositionsCount = 0; // счётчик числа перестановок
	$count = count($array) // count($array) - ф-ция, которая возвращает число элементов массива
	$process = "Исходный массив: "; // строка, в которую
	// будет записываться состояние массива на каждом шаге сортировки
	process .= PrintArray($array, $count); // PrintArray - вывод массива строкой
	// .= - объединение строк
	$timeBefore = microtime(true); // время до начала сортировки в мкс, true - возвращает дробное число микросекунд
	switch ($selection)
	{
	case 1:
		InsertSort($newArray, $transpositionsCount, $process);
		break;
	case 2:
		SelectSort($newArray, $transpositionsCount, $process);
		break;
	case 3:
		BubbleSort($newArray, $transpositionsCount, $process);
		break;
	}
	$timeAfter; $time; // !!!Объявление переменных
	$timeAfter = microtime(true); // время после окончания сортировки
	$time = $timeAfter - $timeBefore; // затраченное на сортировку время
	SortedArrayMessage($newArray, $array, $time, $transpositionsCount, $process);
	// вывод сообщения о завершении сортировки с выводом затраченного времени, числа
	// перестановок, возможностью посмотреть ход сортировки и выбор дальнейшего действия
}

function InsertSort(&$arr, &$transpositionsCount, &$process)
{
	// сортировка вставкой
	$count = count($arr);
	for ($i = 1; $i < $count; $i++)
	{
		$cur_val = $arr[$i];
		$j = $i - 1;
		while ($j >= 0 && $arr[$j] > $cur_val)
		{
			$arr[$j + 1] = $arr[$j];
			$arr[$j] = $cur_val;
			$j--;
			WriteProcess($transpositionsCount, $process, $arr, $count); // запись хода сортировки
		}
	}
}

function SelectSort(&$arr, &$transpositionsCount, &$process)
{
	// сортировка выбором
	$count= count($arr);
	for ($i = 0; $i < $count; $i++)
	{
		$k = $i;
		for($j = $i + 1; $j < $count; $j++)
		{
			if ($arr[$k] > $arr[$j])
			{
				$k = $j;
			}
			if ($k != $i)
			{
				$tmp = $arr[$i];
				$arr[$i] = $arr[$k];
				$arr[$k] = $tmp;
				WriteProcess($transpositionsCount, $process, $arr, $count);
			}
		}
	}
}

function BubbleSort(&$arr, &$transpositionsCount, &$process)
{
	// сортировка пузырьком
	$count = count($arr);
	for ($i = 0; $i < $count; $i++)
	{
		for ($j = $count - 1; $j > $i; $j--)
		{
			if ($arr[$j] < $arr[$j - 1])
			{
				$tmp = $arr[$j];
				$arr[$j] = $arr[$j - 1];
				$arr[$j - 1] = $tmp;
				WriteProcess($transpositionsCount, $process, $arr, $count);
			}
		}
	}
}
//-----------------------------------------------------------------------------------------------

define("MENU_LINES_COUNT", 4);
define("SORTS_MENU_LINES_COUNT", 4);
define("AFTER_SORTING_ARRAY_MENU_LINES", 3);
$array;
$selection;
do
{
	Separate();
	$selection = Menu($array);
	Separate();
}
while (ExecutingSelectedAction($selection, $array));
?>

