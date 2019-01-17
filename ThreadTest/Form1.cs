using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ThreadTest
{
    public partial class Form1 : Form
    {
        //最大线程数量, 小于等于64
        static int MaxThreadNum = 64;

        static Thread thread_StatusStrip;
        static Thread thread_ScanMonitor;
        Thread thread_StartScan;

        static Dictionary<string, IPEntity> IPEntities = new Dictionary<string, IPEntity>();
        static List<string> IPList = new List<string>();

        public Form1()
        {
            InitializeComponent();

            //载入IP和文件
            GetLocalIP();
            LoadConfig();

            ThreadPool.SetMaxThreads(MaxThreadNum, MaxThreadNum);

            thread_StatusStrip = new Thread(RefreshStatus);
            thread_StatusStrip.Start();

            thread_ScanMonitor = new Thread(StartScan);
            thread_ScanMonitor.Start();
        }

        //载入列表
        private void GetLocalIP()
        {
            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress item in ipArray)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string IP_String = item.ToString();
                        int a = IP_String.LastIndexOf(".");
                        //255是广播地址
                        for (int i = 1; i < 255; i++)
                        {
                            string IPAddr = IP_String.Substring(0, a + 1) + i;
                            if(!IPEntities.ContainsKey(IPAddr))
                            {
                                IPEntities.Add(IPAddr, null);
                                IPList.Add(IPAddr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.StackTrace + "\r\n" + ex.Message, "错误", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        //载入文件
        private void LoadConfig()
        {
            if (!File.Exists("List.txt")) return;
            StreamReader sr = new StreamReader("List.txt");
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                IPEntity ipEntity = new IPEntity(line);
                if (!IPEntities.ContainsKey(ipEntity.IPAddr))
                {
                    IPEntities.Add(ipEntity.IPAddr, ipEntity);
                    IPList.Add(ipEntity.IPAddr);
                }
                else
                {
                    IPEntities[ipEntity.IPAddr] = ipEntity;
                }
            }
            sr.Close();
        }

        private void RefreshStatus()
        {
            while (true)
            {
                int workerThreads;
                int completionPortThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
                UpdateStatusStrip(MaxThreadNum - workerThreads);
                Thread.Sleep(100);
            }
        }

        private void StartScan()
        {
            while (true)
            {
                Thread.Sleep(10000);
                if (thread_StartScan == null)
                {
                    thread_StartScan = new Thread(RunThreadPool);
                    thread_StartScan.Start();
                }
                else if(thread_StartScan.ThreadState == ThreadState.Stopped || thread_StartScan.ThreadState == ThreadState.Aborted)
                {
                    thread_StartScan = new Thread(RunThreadPool);
                    thread_StartScan.Start();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEntity iPEntity = new IPEntity("Frank|192.168.1.1|00:00:00:00:00|true|2019-01-08 11:09:21");
            textBox1.Text = iPEntity.ToString();

            thread_StartScan = new Thread(RunThreadPool);
            thread_StartScan.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<string, IPEntity> tempIPList = new Dictionary<string, IPEntity>();
            foreach(var key in IPEntities.Keys)
            {
                if (IPEntities[key] != null) tempIPList.Add(key, IPEntities[key]);
            }

            DataGridViewRow[] dataGridViewRows = new DataGridViewRow[tempIPList.Count];

            int i = 0;
            foreach(var key in tempIPList.Keys)
            {
                IPEntity ipEntity = tempIPList[key];
                textBox1.Text += ipEntity.ToString() + "\r\n";

                dataGridViewRows[i] = new DataGridViewRow();
                dataGridViewRows[i].CreateCells(dataGridView1);
                dataGridViewRows[i].Cells[0].Value = ipEntity.ComputerName;
                dataGridViewRows[i].Cells[1].Value = ipEntity.IPAddr;
                dataGridViewRows[i].Cells[2].Value = ipEntity.MacAddr;
                dataGridViewRows[i].Cells[3].Value = ipEntity.Status;
                dataGridViewRows[i].Cells[4].Value = ipEntity.LastOnline;
                i++;
            }
            dataGridView1.Rows.AddRange(dataGridViewRows);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Dictionary<string, IPEntity> tempIPList = new Dictionary<string, IPEntity>();
            tempIPList.Add("10.10.64.39", null);
            tempIPList.Add("10.10.64.157", null);
            tempIPList.Add("10.10.64.172", null);

            foreach(var key in tempIPList.Keys)
            {
                IPEntity ipEntity = new IPEntity(key, DateTime.Now);
                ipEntity.ComputerName = ipEntity.GetHostName(key);
                ipEntity.MacAddr = ipEntity.GetMacBySendARP(key);

                textBox1.Text += ipEntity.ToString() + "\r\n";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DateTime dt1 = DateTime.Parse("2019-01-08 17:05:39");
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            if (ts.TotalSeconds > 60 * 60 * 24 * 10)
            {
                MessageBox.Show("yes");
            }
            else
            {
                MessageBox.Show("no");
            }
        }

        private void RunThreadPool()
        {
            //MessageBox.Show(IPEntities.Count + "," + IPList.Count);
            //任务组数
            int GroupNum = IPEntities.Count / MaxThreadNum;
            if (GroupNum * MaxThreadNum != IPEntities.Count) GroupNum++;

            for(int i = 0; i < GroupNum; i++)
            {
                int Remain = i < GroupNum - 1 ? MaxThreadNum : IPEntities.Count + MaxThreadNum - GroupNum * MaxThreadNum;
                //MessageBox.Show(Remain + "," + IPEntities.Count + "," + GroupNum);
                ManualResetEvent[] _ManualEvents = new ManualResetEvent[Remain];
                for (int j = 0; j < Remain; j++)
                {
                    _ManualEvents[j] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolMethod), new object[] { i * MaxThreadNum + j, _ManualEvents[j] });
                }
                WaitHandle.WaitAll(_ManualEvents);
            }
            UpdateDataGridView(null);
            thread_StartScan.Abort();
        }

        public void ThreadPoolMethod(object obj)
        {
            //TODO: Add your code here
            object[] objs = (object[])obj;

            int index = (int)objs[0];
            string IPAddr = IPList[index];

            IPEntity ipEntity = IPEntities[IPAddr];
            if(ipEntity == null)
            {
                ipEntity = new IPEntity(IPAddr, DateTime.Now);
                ipEntity.ComputerName = ipEntity.GetHostName(IPAddr);
                ipEntity.MacAddr = ipEntity.GetMacBySendARP(IPAddr);
                if(ipEntity.ComputerName != "" && ipEntity.MacAddr != "00-00-00-00-00-00")
                {
                    ipEntity.Status = true;
                }
                IPEntities[IPAddr] = ipEntity;
            }
            else
            {
                ipEntity.ComputerName = ipEntity.GetHostName(IPAddr);
                ipEntity.MacAddr = ipEntity.GetMacBySendARP(IPAddr);
                if (ipEntity.ComputerName != "" && ipEntity.MacAddr != "00-00-00-00-00-00")
                {
                    ipEntity.Status = true;
                    ipEntity.LastOnline = DateTime.Now;
                }
                else
                {
                    ipEntity.Status = false;
                }
                IPEntities[IPAddr] = ipEntity;
            }

            //UpdateUI((int)objs[0]);

            ManualResetEvent e = (ManualResetEvent)objs[1];
            e.Set();
        }

        private delegate void UpdateDataGridViewCallBack(object obj);

        private void UpdateDataGridView(object obj)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDataGridViewCallBack(UpdateDataGridView), obj);
            }
            else
            {
                Dictionary<string, IPEntity> tempIPList = new Dictionary<string, IPEntity>();
                foreach (var key in IPEntities.Keys)
                {
                    IPEntity ipEntity = IPEntities[key];
                    if (ipEntity != null)
                    {
                        if(ipEntity.ComputerName != "" && ipEntity.MacAddr != "00-00-00-00-00-00")
                        {
                            DateTime dt1 = ipEntity.LastOnline;
                            DateTime dt2 = DateTime.Now;
                            TimeSpan ts = dt2 - dt1;
                            if(!(ts.TotalSeconds > 60 * 60 * 24 * 10))
                            {
                                tempIPList.Add(key, ipEntity);
                            }
                        }
                    }
                }
                
                dataGridView1.Rows.Clear();
                DataGridViewRow[] dataGridViewRows = new DataGridViewRow[tempIPList.Count];

                int i = 0;
                foreach (var key in tempIPList.Keys)
                {
                    IPEntity ipEntity = tempIPList[key];
                    textBox1.Text += ipEntity.ToString() + "\r\n";

                    dataGridViewRows[i] = new DataGridViewRow();
                    dataGridViewRows[i].CreateCells(dataGridView1);
                    dataGridViewRows[i].Cells[0].Value = ipEntity.ComputerName;
                    dataGridViewRows[i].Cells[1].Value = ipEntity.IPAddr;
                    dataGridViewRows[i].Cells[2].Value = ipEntity.MacAddr;
                    dataGridViewRows[i].Cells[3].Value = ipEntity.Status;
                    dataGridViewRows[i].Cells[4].Value = ipEntity.LastOnline;
                    i++;
                }
                dataGridView1.Rows.AddRange(dataGridViewRows);
            }
        }

        private delegate void UpdateTextCallBack(int i);

        private void UpdateText(int i)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateTextCallBack(UpdateText), i);
            }
            else
            {
                this.textBox1.AppendText(i + "\r\n");
            }
        }

        private delegate void UpdateStatusStripCallBack(int i);

        private void UpdateStatusStrip(int i)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateStatusStripCallBack(UpdateStatusStrip), i);
            }
            else
            {
                this.toolStripStatusLabel2.Text = i + "个";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(thread_StatusStrip != null)
            {
                thread_StatusStrip.Abort();
            }
            if(thread_ScanMonitor != null)
            {
                thread_ScanMonitor.Abort();
            }
            if(thread_StartScan != null)
            {
                thread_StartScan.Abort();
            }

            StreamWriter sw = new StreamWriter("List.txt", false);
            foreach (var key in IPEntities.Keys)
            {
                IPEntity ipEntity = IPEntities[key];
                if (ipEntity != null)
                {
                    sw.WriteLine(ipEntity.ToString());
                }
            }
            sw.Close();
        }
    }

    class IPEntity
    {
        public string ComputerName { get; set; }
        public string IPAddr { get; set; }
        public string MacAddr { get; set; }
        public bool Status { get; set; }
        public DateTime LastOnline { get; set; }
        public IPEntity(string IPAddr, DateTime dateTime)
        {
            this.IPAddr = IPAddr;
            this.LastOnline = dateTime;
        }
        public IPEntity(string ComputerName, string IPAddr, string MacAddr, bool Status, DateTime LastOnline)
        {
            this.ComputerName = ComputerName;
            this.IPAddr = IPAddr;
            this.MacAddr = MacAddr;
            this.Status = Status;
            this.LastOnline = LastOnline;
        }
        public IPEntity(string Info)
        {
            string[] strs = Info.Split('|');
            ComputerName = strs[0];
            IPAddr = strs[1];
            MacAddr = strs[2];
            Status = Convert.ToBoolean(strs[3]);
            LastOnline = Convert.ToDateTime(strs[4]);
        }

        #region 获取MAC地址
        ///<summary>
        /// 通过SendARP获取网卡Mac
        /// 网络被禁用或未接入网络（如没插网线）时此方法失灵
        ///</summary>
        ///<param name="remoteIP">网络IP</param>
        ///<returns>网卡Mac</returns>
        public string GetMacBySendARP(string remoteIP)
        {
            StringBuilder macAddress = new StringBuilder();
            try
            {
                Int32 remote = WinAPI.inet_addr(remoteIP);
                Int64 macInfo = new Int64();
                Int32 length = 6;
                WinAPI.SendARP(remote, 0, ref macInfo, ref length);
                string temp = Convert.ToString(macInfo, 16).PadLeft(12, '0').ToUpper();
                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        macAddress.Append(temp.Substring(x - 2, 2));
                    }
                    else
                    {
                        macAddress.Append(temp.Substring(x - 2, 2) + "-");
                    }
                    x -= 2;
                }
                return macAddress.ToString();
            }
            catch
            {
                return "00-00-00-00-00-00";
            }
        }
        #endregion

        #region 获取主机名
        public string GetHostName(string ip)
        {
            try
            {
                string HostName = Dns.GetHostEntry(IPAddress.Parse(ip)).HostName;
                if (HostName == ip) return "";
                return HostName;
            }
            catch
            {
                return "";
            }
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}",ComputerName,IPAddr,MacAddr,Status,LastOnline.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}