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
        private static TextPosition _positionNow = new TextPosition();
        private static string _line = "";
        private static byte _lastInLine = 0;
        private static List<Err> _err = new List<Err>();
        private static StreamReader _file;
        private static uint _errCount = 0;
        private static bool _endOfFile = false;

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

        public static Dictionary<byte, string> ErrorTable = new Dictionary<byte, string>()
        {
            { 1, "Ошибка ввода-вывода" },
            { 2, "Слишком много ошибок в строке" },
            { 50, "Неверный символ в программе" },
            { 51, "Пропущен идентификатор" },
            { 52, "Пропущена точка с запятой" },
            { 53, "Пропущена точка" },
            { 54, "Пропущено двоеточие" },
            { 55, "Пропущена запятая" },
            { 56, "Пропущена левая скобка" },
            { 57, "Пропущена правая скобка" },
            { 58, "Пропущен оператор присваивания :=" },
            { 100, "Неизвестный идентификатор" },
            { 101, "Ожидалось ключевое слово begin" },
            { 102, "Ожидалось ключевое слово end" },
            { 103, "Пропущено ключевое слово program" },
            { 200, "Целочисленная константа вне диапазона" },
            { 201, "Вещественная константа вне диапазона" },
            { 202, "Недопустимый символ в строке" },
            { 203, "Константа превышает допустимый предел" },
            { 250, "Неожиданный конец файла" }
        };

        public static void OpenFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine("Ошибка: файл " + filePath + " не найден!");
                return;
            }

            _file = new StreamReader(filePath);
            _errCount = 0;
            _endOfFile = false;
            _positionNow = new TextPosition(1, 0);

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

                if (_err.Count > 0)
                {
                    ListErrors();
                }

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
                Console.WriteLine(_line);
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

                _err = new List<Err>();
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
            Console.WriteLine("Компиляция завершена: ошибок — " + _errCount + "!");
            _endOfFile = true;
            _ch = '\0';

            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        static void ListErrors()
        {
            int pos = 6 - $"{_positionNow.LineNumber} ".Length;
            string s = "";

            foreach (Err item in _err)
            {
                _errCount = _errCount + 1;
                s = "**";

                if (_errCount < 10)
                {
                    s = s + "0";
                }

                s = s + _errCount + "**";

                while (s.Length - 1 < pos + item.ErrorPosition.CharNumber)
                {
                    s = s + " ";
                }

                s = s + "^ ошибка код " + item.ErrorCode;

                if (ErrorTable.ContainsKey(item.ErrorCode))
                {
                    s = s + " (" + ErrorTable[item.ErrorCode] + ")";
                }

                Console.WriteLine(s);
            }
        }

        public static void Error(byte errorCode, TextPosition position)
        {
            if (_err.Count <= _errMax)
            {
                Err e = new Err(position, errorCode);
                _err.Add(e);
            }
        }

        public static void PrintErrorTable()
        {
            Console.WriteLine("\n=== ТАБЛИЦА ОШИБОК ===");
            Console.WriteLine("Код | Описание");
            Console.WriteLine("----+---------");

            foreach (var item in ErrorTable)
            {
                Console.WriteLine(item.Key.ToString().PadLeft(3) + " | " + item.Value);
            }

            Console.WriteLine();
        }
    }
}