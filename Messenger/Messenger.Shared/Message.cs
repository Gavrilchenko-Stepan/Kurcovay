using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Shared
{
    [Serializable]
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderDepartment { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public override string ToString() => $"{SenderName}: {Text} ({SentAt:HH:mm})";
    }
}
