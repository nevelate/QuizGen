using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    public class Test
    {
        public string? Question { get; set; }

        public string? CorrectAnswer { get; set; }

        public IEnumerable<string?>? OtherAnswers { get; set; }
    }
}
