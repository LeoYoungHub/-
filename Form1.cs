using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace 爬虫
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			InitBrowser();
		}

		string domian = "https://www.kaoshibao.com/online/?paperId=652822&practice=&modal=1&is_recite=&ptype=&text=%E9%A1%BA%E5%BA%8F%E7%BB%83%E4%B9%A0&sequence=0";// "https://www.kaoshibao.com/";
		public ChromiumWebBrowser chromiumWebBrowser;
		public void InitBrowser()
		{
			Cef.Initialize(new CefSettings());
			chromiumWebBrowser = new ChromiumWebBrowser(domian);
			splitContainer1.Panel2.Controls.Add(chromiumWebBrowser);
			chromiumWebBrowser.Dock = DockStyle.Fill;
			chromiumWebBrowser.FrameLoadEnd += webbrowser_FrameLoadEnd;


		}   /// 回调事件

		string cookies = string.Empty;
		void visitor_SendCookie(CefSharp.Cookie obj)
		{
			cookies += obj.Domain.TrimStart('.') + "^" + obj.Name + "^" + obj.Value + "$";
		}

		async void setCookies()
		{
			var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
			await cookieManager.SetCookieAsync(domian, new CefSharp.Cookie
			{
				Domain = domian,
				Name = "kaoshibao",
				Value = "kaoshibao",
				Expires = DateTime.MinValue
			});
		}


		async void webbrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
			string _url = e.Url;
			//判断是否是需要获取cookie的页面
			if (_url.Contains(domian))
			{
				//注册获取cookie回调事件

				var visitor = new CookieMonster();
				//var list = visitor.GetCookieList("www.kaoshibao.com");
				var isSucess = visitor.setCookie("www.kaoshibao.com", "ceshi", "123456", true);
			}


		}

		private async void button1_Click(object sender, EventArgs e)
		{
			string name = "18126015434";
			string pwd = "yl123456";
			string nameClassid = "el-input__inner";
			//document.querySelector("#body > div > div.layout-container.prative-page > div.prative-box > div.topic.no-select > div > div.select-left.pull-left.options-w")
			//document.querySelector("#body > div > div.layout-container.prative-page > div.prative-box > div.topic.no-select > div > div.topic-top > div > div")
			CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('#body > div > div > div.form-main > form > div > div:nth-child(1) > div > div > div > input').value='18126015434'");
			CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('#body > div > div > div.form-main > form > div > div:nth-child(2) > div > div > div > input').value='yl123456'");
			CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('#body > div > div > div.form-main > form > div > div.mb20 > button').click()");




		}

		private void button2_Click(object sender, EventArgs e)
		{
			chromiumWebBrowser.GetBrowser().ShowDevTools();
		}

		private async void button3_Click(object sender, EventArgs e)
		{
			CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('div.tool-bar').children[6].click()");

			while (true)
			{
				await Task.Delay(500);
				string html = await CefWebBrowserControl.GetHtmlSource(chromiumWebBrowser);

				HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
				doc.LoadHtml(html);//html字符串
								   //获取标签里的值select-left pull-left options-w

				var enable = doc.DocumentNode.SelectSingleNode("//div[@class='next-preve']/button[2]");
				if (enable.Attributes["disabled"] != null && enable.Attributes["disabled"].Value.Equals("disabled"))
				{
					break;
				}

				var topic_type = doc.DocumentNode.SelectSingleNode("//span[@class='topic-type']").InnerText;
				var topic = doc.DocumentNode.SelectSingleNode("//div[@class='qusetion-box']").InnerText;

				if ("单选题 多选题 判断题 不定项选择题 排序题".Contains(topic_type))
				{
					var answers = doc.DocumentNode.SelectNodes("//div[@class='option']");

					listBox1.Items.Add(topic_type);
					listBox1.Items.Add(topic);
					foreach (var item in answers)
					{
						string value1 = item.SelectNodes("./span")[0].InnerText;
						string value2 = item.SelectNodes("./span")[1].InnerText;

						listBox1.Items.Add($"{value1}{value2}");
					}
				}

				var answerRight = doc.DocumentNode.SelectSingleNode("//p[@class='answer-right']/span").InnerText;
				var answerAnalysis = doc.DocumentNode.SelectSingleNode("//p[@class='answer-analysis']").InnerText;
				listBox1.Items.Add("答案:" + answerRight);
				listBox1.Items.Add("注释:" + answerAnalysis);

				CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('div.next-preve').children[1].click()");
			}

		}

		private void button4_Click(object sender, EventArgs e)
		{
			CefWebBrowserControl.ClickButtonByJsPath(chromiumWebBrowser, @"document.querySelector('div.tool-bar').children[6].click()");
		}
	}

	/// <summary>
	/// CefSharp的cookie的操作类
	/// </summary>
	public class CookieMonster : ICookieVisitor
	{
		public List<CefSharp.Cookie> cookies = new List<CefSharp.Cookie>();
		readonly ManualResetEvent gotAllCookies = new ManualResetEvent(false);


		/// <summary>
		/// 获取cookie
		/// </summary>
		/// <param name="DomainStr">根据域名获取，如果DomainStr为空则获取所有的cookie</param>
		/// <returns></returns>
		public List<CefSharp.Cookie> GetCookieList(string DomainStr = "")
		{
			var visitor = new CookieMonster();
			if (DomainStr.Length > 0)
			{
				var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
				if (cookieManager.VisitAllCookies(visitor))
				{
					visitor.WaitForAllCookies();
					return visitor.cookies.Where(p => p.Domain == DomainStr || p.Domain == "." + DomainStr).ToList();
				}
				else
				{
					return visitor.cookies;
				}
			}
			else
			{
				var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
				if (cookieManager.VisitAllCookies(visitor))
					visitor.WaitForAllCookies();
				return visitor.cookies;
			}
		}

		/// <summary>
		/// 给浏览器设置cookie
		/// </summary>
		/// <returns></returns>
		public async Task<bool> setCookie(string domainStr, string nameStr, string valueStr, bool ishttps)
		{
			string httpStr = "http";
			if (ishttps)
			{
				httpStr = "https";
			}
			var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
			var bol = await cookieManager.SetCookieAsync(httpStr + "://" + domainStr, new CefSharp.Cookie()
			{
				Domain = domainStr,
				Name = nameStr,
				Value = valueStr,
				Path = "/",
				HttpOnly = true,
				Expires = DateTime.Now.AddMinutes(1)
			});
			return bol;
		}

		public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
		{

			cookies.Add(cookie);
			if (count == total - 1)
				gotAllCookies.Set();
			return true;
		}

		public void WaitForAllCookies()
		{
			gotAllCookies.WaitOne();
		}

		public void Dispose()
		{
			//cookies.Clear();
		}
	}
}
