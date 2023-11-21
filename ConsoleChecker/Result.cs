using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChecker
{

    public enum Status
    {
        Good,
        Bad,
        Free,
        Premium,
        Ban,
        None
    }

    public class Result
    {
        private string email;
        private string password;
        private Status status;

        public string Email { get => email; set => email = value; }
        public string Password { get => password; set => password = value; }
        public Status Status { get => status; set => status = value; }
    }
}
