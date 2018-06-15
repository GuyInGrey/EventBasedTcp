using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBasedTCP
{
    public static class TcpOptions
    {
        public static char EndMessageCode { get; set; } = (char)25;
        public static char EndConnectionCode { get; set; } = (char)4;
    }
}
