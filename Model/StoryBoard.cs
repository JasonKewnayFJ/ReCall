using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReCall___.Model
{
    public class StoryBoard
    {
        public string CurrentBoardNote { get; set; } = null;
        public string PreviousBoardNote { get; set; } = null;
        public List<String> Stories { get; set; } = null;
    }
}
