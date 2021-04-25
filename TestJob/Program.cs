using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestJob
{
    class Program
    {
        static string callingDomainName = Path.GetFullPath("../../");
        static void Main(string[] args)
        {
            QuartzHelper.Start(Path.Combine(callingDomainName, "TestDll1.dll"));
            QuartzHelper.Start(Path.Combine(callingDomainName, "TestDll2.dll"));
            System.Threading.Thread.Sleep(5000);
            QuartzHelper.Unload("TestDll2");
            //Helper.AppDomainCreate(Path.Combine(callingDomainName, "TestDll1.dll"));
            //System.Threading.Thread.Sleep(5000);
            //Helper.Unload("TestDll1");
            //Helper._AppDomainKeys.Remove("TestDll1");
            //Helper.AppDomainCreate(Path.Combine(callingDomainName, "TestDll1.dll"));
            //Console.WriteLine("选择功能：");
            //Console.WriteLine("1、导入DLL");
            //Console.WriteLine("2、卸载DLL");
            //Console.WriteLine("3、查看当前工作池的DLL");
            //Console.WriteLine("请输入......");
            //var result = Console.ReadLine();
            //var name=AppDomainHelper.AppDomainCreate(Path.Combine(callingDomainName, "TestDll1.dll"));
            //Console.WriteLine(name);
            //AppDomainHelper.AppDomainCreate(Path.Combine(callingDomainName, "TestDll2.dll"));
            //AppDomainHelper.Unload("TestDll1");
            //AppDomainHelper.Unload("TestDll2");
            //JobManage.StartSchedulerReadConfig(Path.Combine(callingDomainName, "TestDll1.dll"));
            // LocalLoader ll = new LocalLoader();
            //ll.LoadAssembly(Path.Combine(callingDomainName, "TestDll1.dll"));
            //ll.LoadAssembly(Path.Combine(callingDomainName, "TestDll2.dll"));
            //ll.Unload();
            //JobManage.StartSchedulerReadConfig();
            Console.ReadKey();
        }
        public class RemoteLoader : MarshalByRefObject
        {
            private Assembly assembly;
            public void LoadAssembly(string fullName)
            {
                assembly = Assembly.LoadFrom(fullName);
            }
            public string FullName
            {
                get { return assembly.FullName; }
            }
        }
        public class LocalLoader
        {
            private AppDomain appDomain;
            private RemoteLoader remoteLoader;
            public LocalLoader()
            {
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationName = "Test";
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                setup.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
                setup.CachePath = setup.ApplicationBase;
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = setup.ApplicationBase;
                appDomain = AppDomain.CreateDomain("TestDomain", null, setup);
                string name = Assembly.GetExecutingAssembly().GetName().FullName;
                remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(
                    name,
                    typeof(RemoteLoader).FullName);
            }
            public void LoadAssembly(string fullName)
            {
                remoteLoader.LoadAssembly(fullName);
            }
            public void Unload()
            {
                AppDomain.Unload(appDomain);
                appDomain = null;
            }
            public string FullName
            {
                get
                {
                    return remoteLoader.FullName;
                }
            }
        }
    }
}
