using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzForWinForm.Models
{
    public class Detail {
        public AppDomain appDomain { get; set; }
        public Status status { get; set; }
    }
    public class NodeDetail {
        public string Name { get; set; }
        public string Job { get; set; }
        public string Group { get; set; }
        public string Jobtype { get; set; }
        public string IntervalTime { get; set; }
        public string Begintime { get; set; }
        public string Endtime { get; set; }
    }
}
