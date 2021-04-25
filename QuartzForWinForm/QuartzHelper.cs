using Quartz;
using Quartz.Impl;
using QuartzForWinForm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace QuartzForWinForm
{
    public class RemoteLoader : MarshalByRefObject
    {
        private Assembly assembly;
        //调度池
        private static Task<IScheduler> taskScheduler;
        public string LoadAssembly(string fullName)
        {
            //加载程序集
            assembly = Assembly.LoadFrom(fullName);
            return assembly.GetName().Name;
        }
        public void StartSchedulerReadConfig()
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            var dllName = assembly.GetName().Name;
            string currentDir = Path.GetFullPath("../../");
            //调度器
            IScheduler scheduler;
            try
            {
                //读取配置文件的Job节点
                XDocument xDoc = XDocument.Load(Path.Combine(currentDir, "App.config"));
                var jobScheduler = xDoc.Descendants("JobScheduler");
                var job = jobScheduler.Elements("Job").FirstOrDefault(t => t.Attribute("Name").Value == dllName);
                XElement jobDetailXElement, triggerXElement;
                //调度器工厂
                ISchedulerFactory factory = new StdSchedulerFactory();
                taskScheduler = factory.GetScheduler();
                scheduler = taskScheduler.Result;
                //获取任务名字
                jobDetailXElement = job.Element("JobDetail");
                //获取任务触发的时间
                triggerXElement = job.Element("Trigger");
                IJobDetail jobDetail = JobBuilder.Create(assembly.GetType(jobDetailXElement.Attribute("jobtype").Value)).WithIdentity(jobDetailXElement.Attribute("job").Value,
                                                    jobDetailXElement.Attribute("group").Value).Build();
                if (triggerXElement.Attribute("type").Value.Equals("CronTrigger"))
                {
                    //触发器
                    ISimpleTrigger trigger;
                    var triggerBuilder = TriggerBuilder.Create();
                    //开始时间
                    if (!string.IsNullOrEmpty(triggerXElement.Attribute("begintime").Value))
                    {
                        DateTimeOffset begintime = Convert.ToDateTime(triggerXElement.Attribute("begintime").Value);
                        triggerBuilder = triggerBuilder.StartAt(begintime);
                    }
                    //结束时间
                    if (!string.IsNullOrEmpty(triggerXElement.Attribute("endtime").Value))
                    {
                        DateTimeOffset endtime = Convert.ToDateTime(triggerXElement.Attribute("endtime").Value);
                        triggerBuilder = triggerBuilder.EndAt(endtime);
                    }
                    triggerBuilder = triggerBuilder.WithSimpleSchedule(t => t.WithIntervalInSeconds(Convert.ToInt32(triggerXElement.Attribute("expression").Value)).RepeatForever());//触发间隔 秒为单位
                    trigger = (ISimpleTrigger)triggerBuilder.Build();
                    //添加定时器
                    scheduler.ScheduleJob(jobDetail, trigger);
                }
                //开始执行定时器
                scheduler.Start();
            }
            catch (Exception EX)
            {
                throw EX;

            }
        }
    }

    public static class QuartzHelper
    {
        //Dll集合 Dll名为Key
        public static Dictionary<string, Detail> _AppDomainKeys = new Dictionary<string, Detail>();
        private static AppDomainSetup setup = new AppDomainSetup();
        /// <summary>
        /// 构造方法
        /// </summary>
        static QuartzHelper()
        {
            setup.ApplicationName = "Test";
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
            setup.CachePath = setup.ApplicationBase;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.ApplicationBase;
        }
        /// <summary>
        /// 启用Dll
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CheckOn(string path)
        {

            var dllname = PublicMethod.GetDllName(path);
            try
            {
                if (!path.EndsWith(".dll"))
                {
                    throw new Exception("只支持导入.dll为后缀的程序集");
                }
                var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, setup);
                string name = Assembly.GetExecutingAssembly().GetName().FullName;
                var remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(
                    name,
                    typeof(RemoteLoader).FullName);
                var dllName = remoteLoader.LoadAssembly(path);
                remoteLoader.StartSchedulerReadConfig();
            
                return dllName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 导入Dll
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static string Import(string path,string Type="Other")
        {

            var dllname = PublicMethod.GetDllName(path);
            try
            {
                if (!path.EndsWith(".dll"))
                {
                    throw new Exception("只支持导入.dll为后缀的程序集");
                }
                var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, setup);
                string name = Assembly.GetExecutingAssembly().GetName().FullName;
                var remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(
                    name,
                    typeof(RemoteLoader).FullName);
                var dllName = remoteLoader.LoadAssembly(path);
                remoteLoader.StartSchedulerReadConfig();
                _AppDomainKeys.Add(dllName, new Detail() { appDomain = appDomain, status = Status.on });
                return dllName;
            }
            catch (Exception ex)
            {
                if (!Type.Equals("Other")) {
                    PublicMethod.RemoveNode(dllname);
                }
               
                throw ex;
            }
        }
        /// <summary>
        /// 卸载
        /// 卸载DLL后会回收一切相关资源
        /// </summary>
        public static void Unload(string dllName)
        {
            try
            {
                if (!_AppDomainKeys.ContainsKey(dllName))
                {
                    throw new Exception($"当前{dllName}不存在");
                }
                AppDomain.Unload(_AppDomainKeys[dllName].appDomain);
                _AppDomainKeys.Remove(dllName);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
