using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Shared
{
    [Serializable]
    public class NetworkPacket
    {
        public CommandType Command { get; set; }
        public int UserId { get; set; }
        public object Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
