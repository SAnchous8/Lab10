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

        private static void TestRealPascalCode()
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║   ТЕСТИРОВАНИЕ МОДУЛЯ ВВОДА-ВЫВОДА  ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();

            string content =
                "program example ( input, output );\n" +
                "const c = 3;\n" +
                "b = 56;\n" +
                "var a : 'a' .. 'c';\n" +
                "k, i : integer;\n" +
                "begin\n" +
                "read ( k, i );\n" +
                "for a := 'a' to 'c' do\n" +
                "case a of\n" +
                "k : i := i * k;\n" +
                "'b' : i := i + 1;\n" +
                "i : k := k + 2;\n" +
                "b : i := i - k;\n" +
                "c : i := ( i + k ) * 2\n" +
                "end;\n" +
                "writeln( i, k )\n" +
                "end.";

            string filePath = CreateTestFile("test_real.pas", content);
            InputOutput.OpenFile(filePath);

            InputOutput.Error(100, new TextPosition(10, 0));
            InputOutput.Error(100, new TextPosition(12, 0));
            InputOutput.Error(147, new TextPosition(13, 0));
            InputOutput.Error(147, new TextPosition(14, 0));

            while (true)
            {
                if (InputOutput.Ch == '\0')
                {
                    break;
                }

                InputOutput.NextCh();
            }

            InputOutput.CloseFile();
        }

        public static void RunAllTests()
        {
            InputOutput.PrintErrorTable();
            TestRealPascalCode();
            Console.WriteLine("\n=== Тест завершён ===");
        }
    }
}
