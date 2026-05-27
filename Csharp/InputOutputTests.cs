using System;
using System.IO;

namespace Компилятор
{
    static class InputOutputTests
    {
        private static string CreateTestFile(string fileName, string content)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + fileName;
            File.WriteAllText(filePath, content);
            return filePath;
        }

        private static void TestEmptyFile()
        {
            Console.WriteLine("=== Тест 1: Пустой файл ===");
            string filePath = CreateTestFile("test_empty.pas", "");
            InputOutput.OpenFile(filePath);
            Console.WriteLine("Ожидается: 'Компиляция завершена: ошибок — 0!'");
            Console.WriteLine("Результат:");
            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestSingleLine()
        {
            Console.WriteLine("=== Тест 2: Одна строка ===");
            string filePath = CreateTestFile("test_single.pas", "program hello;");
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: построчный вывод, 0 ошибок");
            Console.WriteLine("Результат:");

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                Console.WriteLine("  Символ: '" + InputOutput.Ch + "' (код: " + (int)InputOutput.Ch + ")");
                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestMultipleLines()
        {
            Console.WriteLine("=== Тест 3: Несколько строк ===");
            string content = "program test;\nbegin\n  writeln('Hello');\nend.";
            string filePath = CreateTestFile("test_multi.pas", content);
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: 4 строки вывода, 0 ошибок");
            Console.WriteLine("Результат:");

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestLineWithErrors()
        {
            Console.WriteLine("=== Тест 4: Демонстрация вывода ошибок ===");
            string filePath = CreateTestFile("test_errors.pas", "program test;\nbegin\n  x := 1 @ 2;\nend.");
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: корректный формат вывода ошибок");
            Console.WriteLine("Результат:");

            InputOutput.Error(101, new TextPosition(1, 10));
            InputOutput.Error(102, new TextPosition(3, 8));

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestCharPositions()
        {
            Console.WriteLine("=== Тест 5: Проверка позиций ===");
            string filePath = CreateTestFile("test_pos.pas", "abc\ndef");
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: правильные номера строк и позиций");
            Console.WriteLine("Результат:");

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                Console.WriteLine("  Строка " + InputOutput.PositionNow.LineNumber +
                                  ", Символ " + InputOutput.PositionNow.CharNumber +
                                  ": '" + InputOutput.Ch + "'");
                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestSpecialChars()
        {
            Console.WriteLine("=== Тест 6: Спецсимволы ===");
            string filePath = CreateTestFile("test_special.pas", "x := (a + b) * c;");
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: корректное чтение +, *, (, )");
            Console.WriteLine("Результат:");

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                Console.WriteLine("  Символ: '" + InputOutput.Ch + "' (код: " + (int)InputOutput.Ch + ")");
                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        private static void TestEmptyLines()
        {
            Console.WriteLine("=== Тест 7: Пустые строки ===");
            string content = "program test;\n\nbegin\n\n  writeln('Hello');\n\nend.";
            string filePath = CreateTestFile("test_empty_lines.pas", content);
            InputOutput.OpenFile(filePath);

            Console.WriteLine("Ожидается: корректная обработка пустых строк");
            Console.WriteLine("Результат:");

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
            Console.WriteLine();
        }

        public static void RunAllTests()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║   ТЕСТИРОВАНИЕ МОДУЛЯ ВВОДА-ВЫВОДА  ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            InputOutput.PrintErrorTable();

            TestEmptyFile();
            TestSingleLine();
            TestMultipleLines();
            TestLineWithErrors();
            TestCharPositions();
            TestSpecialChars();
            TestEmptyLines();

            Console.WriteLine("=== Все тесты завершены ===");
        }
    }
}