using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Dto
{
    public class UserRegisterDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordString { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

    }
}
