using QuartzForWinForm.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace QuartzForWinForm
{
    public static  class PublicMethod
    {
		static string _configpath = Path.Combine(Path.GetFullPath("../../"), "App.config");
		/// <summary>
		/// 获取Dll名称
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetDllName(string path) {
			try
			{
				var begIndex = path.LastIndexOf("\\");
				var endIndex = path.LastIndexOf(".");
				var length = path.Length;
				if (path.Length>0) {
					var dllname = path.Remove(endIndex);
					 dllname = dllname.Remove(0,begIndex+1);
					return dllname;
				}
				return "";
			
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}
		/// <summary>
		/// 根据枚举名获取枚举的描述 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objName"></param>
		/// <returns></returns>
		public static string ToDescription<T>(this string objName)
			where T : struct //泛型   struct 值类型
		{
			try
			{
				if (int.TryParse(objName, out int num))
				{
					var name = Enum.GetName(typeof(T), num);  //根据枚举的Value获取枚举的name
					if (!string.IsNullOrEmpty(name))
					{
						objName = name;
					}
				}
				if (!Enum.TryParse(objName, out T obj))  //把name转换成枚举类型
				{
					return objName;
				}
				Type t = obj.GetType();//获取枚举的类型
				FieldInfo fi = t.GetField(objName);
				var arrDesc = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
				var desc = (DescriptionAttribute[])arrDesc;
				return desc[0].Description;
			}
			catch (Exception ex)
			{

				return objName;
			}

		}
		/// <summary>
		/// 关闭时删除所有相关配置文件
		/// </summary>
		public static void CloseRemove() {
			try
			{


				//读取配置文件的Job节点
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(_configpath);
				XmlNodeList nodeList = xmlDocument.SelectNodes("/configuration/JobScheduler/Job");
				XmlNode appsetting = xmlDocument.SelectSingleNode("/configuration/JobScheduler");
				foreach (XmlNode node in nodeList)
				{
						appsetting.RemoveChild(node);
				}
				xmlDocument.Save(_configpath);


			}
			catch (Exception ex)
			{

				throw ex;
			}
		}
		/// <summary>
		/// 根据Dll名称删除节点
		/// </summary>
		/// <param name="nodeName"></param>
		public static void RemoveNode(string nodeName) {
			try
			{
		
			
				//读取配置文件的Job节点
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(_configpath);
				XmlNodeList nodeList= xmlDocument.SelectNodes("/configuration/JobScheduler/Job");
				XmlNode appsetting = xmlDocument.SelectSingleNode("/configuration/JobScheduler");
				foreach (XmlNode node in nodeList)
				{
					if (node.Attributes["Name"].Value==nodeName) {
						appsetting.RemoveChild(node);
					}
				}
				xmlDocument.Save(_configpath);


			}
			catch (Exception ex)
			{

				throw ex;
			}
        }
		/// <summary>
		/// 初始化时添加JobScheduler节点
		/// </summary>
		public static void AddJobScheduler() {
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(_configpath);
			XmlNode appsetting = xmlDocument.SelectSingleNode("/configuration/JobScheduler");
			if (appsetting==null) {
				XmlElement jobNode = xmlDocument.CreateElement("JobScheduler");
				xmlDocument.SelectSingleNode("/configuration").AppendChild(jobNode);
				xmlDocument.Save(_configpath);
			}
			
		}
		/// <summary>
		/// 新增节点
		/// </summary>
		/// <param name="node"></param>
		public static void AddNode(NodeDetail node)
		{
			try
			{
				if (string.IsNullOrEmpty(node.Name)) {
					return;
				}
				//读取配置文件的Job节点
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(_configpath);
				XmlNode appsetting = xmlDocument.SelectSingleNode("/configuration/JobScheduler");

				XmlElement jobNode = xmlDocument.CreateElement("Job");//创建一个Job节点
				jobNode.SetAttribute("Name", node.Name);
			 
				XmlElement jobDetailNode = xmlDocument.CreateElement("JobDetail");
				jobDetailNode.SetAttribute("job", node.Job);
				jobDetailNode.SetAttribute("group", node.Group);
				jobDetailNode.SetAttribute("jobtype",node.Jobtype);

				XmlElement triggerNode = xmlDocument.CreateElement("Trigger");
				triggerNode.SetAttribute("type", "CronTrigger");
				triggerNode.SetAttribute("expression", node.IntervalTime);
				triggerNode.SetAttribute("begintime", node.Begintime);
				triggerNode.SetAttribute("endtime", node.Endtime);

				jobNode.AppendChild(jobDetailNode);
				jobNode.AppendChild(triggerNode);

				appsetting.AppendChild(jobNode);
				xmlDocument.Save(_configpath);


			}
			catch (Exception ex)
			{

				throw ex;
			}
		}
		/// <summary>
		/// 查询是否有相同节点名称的节点
		/// True为存在；否则False
		/// </summary>
		/// <param name="nodeName"></param>
		/// <returns></returns>
		public static bool QueryNode(string nodeName) {
			try
			{
				//读取配置文件的Job节点
				XDocument xDoc = XDocument.Load(_configpath);
				var jobScheduler = xDoc.Descendants("JobScheduler");
				return jobScheduler.Elements("Job").Any(t => t.Attribute("Name").Value == nodeName);
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

	}
}
