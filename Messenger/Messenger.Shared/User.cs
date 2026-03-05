using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Shared
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }

        public override string ToString() => FullName;
    }
}
