using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDll2
{
    public class GetAnimal : IJob
    {
        string[] _animals = { "猴子","兔子","老鹰","绵羊"};
        public async Task Execute(IJobExecutionContext context)
        {
            Random random = new Random();
            var animal = _animals[random.Next(0, _animals.Length)];
            Console.WriteLine(animal);
            await Task.Delay(1000);
        }
    }
}
