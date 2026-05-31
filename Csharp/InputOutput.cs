using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    struct TextPosition
    {
        private uint _lineNumber;
        private byte _charNumber;

        public TextPosition(uint ln = 0, byte c = 0)
        {
            _lineNumber = ln;
            _charNumber = c;
        }

        public uint LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                _lineNumber = value;
            }
        }

        public byte CharNumber
        {
            get
            {
                return _charNumber;
            }
            set
            {
                _charNumber = value;
            }
        }
    }

    struct Err
    {
        private TextPosition _errorPosition;
        private byte _errorCode;

        public Err(TextPosition errorPosition, byte errorCode)
        {
            _errorPosition = errorPosition;
            _errorCode = errorCode;
        }

        public TextPosition ErrorPosition
        {
            get
            {
                return _errorPosition;
            }
            set
            {
                _errorPosition = value;
            }
        }

        public byte ErrorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                _errorCode = value;
            }
        }
    }

    class InputOutput
    {
        private const byte _errMax = 9;

        private static char _ch;
        private static TextPosition _positionNow;
        private static string _line;
        private static byte _lastInLine;
        private static List<Err> _err;
        private static StreamReader _file;
        private static uint _errCount;
        private static bool _endOfFile;
        private static Dictionary<byte, string> _errorTable;

        private static Dictionary<uint, List<Err>> _errorsByLine;

        static InputOutput()
        {
            _ch = '\0';
            _positionNow = new TextPosition(0, 0);
            _line = "";
            _lastInLine = 0;
            _err = new List<Err>();
            _file = null;
            _errCount = 0;
            _endOfFile = false;
            _errorsByLine = new Dictionary<uint, List<Err>>();

            _errorTable = new Dictionary<byte, string>()
            {
                { 1, "ошибка ввода-вывода" },
                { 2, "слишком много ошибок в строке" },
                { 50, "неверный символ в программе" },
                { 51, "пропущен идентификатор" },
                { 52, "пропущена точка с запятой" },
                { 53, "пропущена точка" },
                { 54, "пропущено двоеточие" },
                { 55, "пропущена запятая" },
                { 56, "пропущена левая скобка" },
                { 57, "пропущена правая скобка" },
                { 58, "пропущен оператор присваивания :=" },
                { 100, "использование имени не соответствует описанию" },
                { 101, "ожидалось ключевое слово begin" },
                { 102, "ожидалось ключевое слово end" },
                { 103, "пропущено ключевое слово program" },
                { 147, "тип метки не совпадает с типом выбирающего выражения" },
                { 200, "целочисленная константа вне диапазона" },
                { 201, "вещественная константа вне диапазона" },
                { 202, "недопустимый символ в строке" },
                { 203, "константа превышает допустимый предел" },
                { 250, "неожиданный конец файла" }
            };
        }

        public static char Ch
        {
            get
            {
                return _ch;
            }
            set
            {
                _ch = value;
            }
        }

        public static TextPosition PositionNow
        {
            get
            {
                return _positionNow;
            }
            set
            {
                _positionNow = value;
            }
        }

        public static List<Err> Err
        {
            get
            {
                return _err;
            }
        }

        public static Dictionary<byte, string> ErrorTable
        {
            get
            {
                return _errorTable;
            }
        }

        public static void OpenFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Ошибка: файл " + filePath + " не найден!");
                return;
            }

            _file = new StreamReader(filePath);
            _errCount = 0;
            _endOfFile = false;
            _positionNow = new TextPosition(1, 0);
            _errorsByLine = new Dictionary<uint, List<Err>>();

            ReadNextLine();

            if (_line != null && _line.Length > 0)
            {
                _ch = _line[0];
                _lastInLine = (byte)(_line.Length - 1);
            }
            else
            {
                _ch = '\0';
                _lastInLine = 0;
                _endOfFile = true;
                End();
            }
        }

        public static void CloseFile()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        static public void NextCh()
        {
            if (_endOfFile)
            {
                return;
            }

            if (_positionNow.CharNumber >= _lastInLine)
            {
                ListThisLine();

                ListErrorsForLine(_positionNow.LineNumber);

                ReadNextLine();

                if (_endOfFile)
                {
                    _ch = '\0';
                    End();
                    return;
                }

                _positionNow.LineNumber = _positionNow.LineNumber + 1;
                _positionNow.CharNumber = 0;
            }
            else
            {
                _positionNow.CharNumber = (byte)(_positionNow.CharNumber + 1);
            }

            if (!_endOfFile && _line != null && _positionNow.CharNumber < _line.Length)
            {
                _ch = _line[_positionNow.CharNumber];
            }
            else
            {
                _ch = '\0';
            }
        }

        private static void ListThisLine()
        {
            if (_line != null)
            {
                string lineNumber = _positionNow.LineNumber.ToString();

                while (lineNumber.Length < 4)
                {
                    lineNumber = " " + lineNumber;
                }

                Console.WriteLine(lineNumber + " " + _line);
            }
        }

        private static void ReadNextLine()
        {
            if (_file != null && !_file.EndOfStream)
            {
                _line = _file.ReadLine();

                if (_line == null)
                {
                    _line = "";
                    _endOfFile = true;
                }
                else
                {
                    _lastInLine = _line.Length > 0 ? (byte)(_line.Length - 1) : (byte)0;
                }
            }
            else
            {
                _line = "";
                _lastInLine = 0;
                _endOfFile = true;
            }
        }

        static void End()
        {
            Console.WriteLine();
            Console.WriteLine("Компиляция окончена: ошибок - " + _errCount + " !");
            _endOfFile = true;
            _ch = '\0';

            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        private static void ListErrorsForLine(uint lineNumber)
        {
            if (!_errorsByLine.ContainsKey(lineNumber))
            {
                return;
            }

            List<Err> errorsForLine = _errorsByLine[lineNumber];

            foreach (Err item in errorsForLine)
            {
                _errCount = _errCount + 1;

                string errorLine = "**";

                if (_errCount < 10)
                {
                    errorLine = errorLine + "0";
                }

                errorLine = errorLine + _errCount + "**";

                int spacesCount = 4 + 1 + (int)item.ErrorPosition.CharNumber;

                while (errorLine.Length < spacesCount)
                {
                    errorLine = errorLine + " ";
                }

                errorLine = errorLine + "^ ошибка код " + item.ErrorCode;
                Console.WriteLine(errorLine);

                string descLine = "****** ";

                if (_errorTable.ContainsKey(item.ErrorCode))
                {
                    descLine = descLine + _errorTable[item.ErrorCode];
                }

                Console.WriteLine(descLine);
            }
        }

        public static void Error(byte errorCode, TextPosition position)
        {
            uint lineNum = position.LineNumber;

            if (!_errorsByLine.ContainsKey(lineNum))
            {
                _errorsByLine[lineNum] = new List<Err>();
            }

            if (_errorsByLine[lineNum].Count <= _errMax)
            {
                Err e = new Err(position, errorCode);
                _errorsByLine[lineNum].Add(e);
            }
        }

        public static void PrintErrorTable()
        {
            Console.WriteLine("\n=== ТАБЛИЦА ОШИБОК ===");
            Console.WriteLine("Код | Описание");
            Console.WriteLine("----+---------");

            foreach (var item in _errorTable)
            {
                Console.WriteLine(item.Key.ToString().PadLeft(3) + " | " + item.Value);
            }

            Console.WriteLine();
        }
    }
}
