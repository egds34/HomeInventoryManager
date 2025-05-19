using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Dto
{
    public class ValidationResultDto
    {
        public bool IsValid => Errors.Count == 0;
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}
