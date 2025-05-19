using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Dto
{
    public class UserLoginDto
    {
        public required string UserName { get; set; }
        public required string PasswordString { get; set; }
    }
}
