using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public struct ExecuteResult
    {
        public bool Success{get { return ErrCode == 0; }}
        public bool Fail { get { return ErrCode != 0; } }
        /// <summary>
        /// if ErrCode is not 0 means there's error,otherwise the Message or MessageBuffer can passed the result
        /// </summary>
        public int ErrCode {  get; set; }
        public string Result { get; set; }
        public byte[] ResultBuffer { get; set; }


        public static ExecuteResult Empty
        {
            get
            {
                return new ExecuteResult() { ResultBuffer = new byte[0] };
            }
        }
    }
}
