using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.FileSystem
{
    public class FileCommand
    {
        public const string cd = nameof(cd);
        public const string ls = nameof(ls);
        public const string mkdir = nameof(mkdir);
        public const string rm = nameof(rm);
        public const string get = nameof(get);
        public const string put = nameof(put);
        public const string rename = nameof(rename);


        public const int CommandFlagNone = 0;
        public const int CommandFlag = 100;
        public const int CommandFileBegin = 101;
        public const int CommandFileEnd = 102;
        public const int CommandFilePart = 103;
        public const int CommandFileSuccessful=104;
        public const int CommandFileFail = 105;

        public const int BufferMaxLength = 1024*5;
    }


}
