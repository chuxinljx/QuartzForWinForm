using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzForWinForm.Models
{
    public enum Status
    {
        /// <summary>
        /// 停用
        /// </summary>
        [Description("停用")]
        off = 0,
        /// <summary>
        /// 启用
        /// </summary>
         [Description("启用")]
        on = 1
    }
}
