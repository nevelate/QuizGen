using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;

namespace QuizGen.Models
{
    internal class Test
    {
        public FormattedText? Question { get; set; }
        public FormattedText? Answer { get; set; }

        public IEnumerable<FormattedText>? AnswerVariants { get; set; }
    }
}
