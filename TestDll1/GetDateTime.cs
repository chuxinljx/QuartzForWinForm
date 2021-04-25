using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDll1
{
    public class GetDateTime : IJob
    {
        public  async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"));
            await Task.Delay(1000);
        }
    }
}
