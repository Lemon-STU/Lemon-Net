using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public class FileDataEvent:EventArgs
    {
        public bool IsSuccessful { get;private set; }
        public string FileName { get;private set; }

        public FileDataEvent(bool isSuccessful,string FileName)
        {
            this.IsSuccessful = isSuccessful;
            this.FileName = FileName;
        }
    }
}
