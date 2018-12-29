using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ymodem;
namespace IAP
{
    
   
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }
      
        private void mainForm_Load(object sender, EventArgs e)//窗口创建时被调用
        {
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())//寻找可用串口
            {
                this.SerialPortComboBox.Items.Add(s);
                this.SerialPortComboBox.Text = this.SerialPortComboBox.Items[0].ToString();
            }
        }

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button.Text == "打开文件")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.pathTextBox.Text = openFileDialog.FileName.ToString();
                 }
             }
          
        }
        private System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort();
        private void downloadButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (button.Text == "开始升级")
            {
                try
                {
                    serialPort.PortName = SerialPortComboBox.Text;
                    serialPort.BaudRate = Convert.ToInt32(BaudRateComboBox.SelectedItem.ToString());
                    serialPort.Open();
                }
                catch   //错误处理
                {
                    MessageBox.Show("端口错误，请检查串口是否被占用");
                    return;
                }
                serialPort.Write("upgrade\r");          //写数据
                System.Threading.Thread.Sleep(200);     //延时200ms
                string str = serialPort.ReadExisting(); //字符串方式读
                int index = 0;

                index = str.IndexOf("Enter 9", index);
                if (index < 0)
                {
                    index = 0;
                    index = str.IndexOf("Enter  1", index);
                    if (index < 0)
                    {
                        MessageBox.Show("请检查设备连接");
                        serialPort.Close();
                        return;
                    }
                }
                else
                {
                    serialPort.Write("9");//写数据
                    System.Threading.Thread.Sleep(200);     //延时200ms
                    str = serialPort.ReadExisting();        //字符串方式读
                    index = 0;
                    index = str.IndexOf("Enter  1", index);
                    if (index < 0)
                    {
                        MessageBox.Show("请检查设备连接");
                        serialPort.Close();
                        return;
                    }
                }
                serialPort.Write("1");//写数据
                System.Threading.Thread.Sleep(200);     //延时200ms
                serialPort.Close();
                button.Text = "正在下载";
                ymodem = new Ymodem.Ymodem();
                ymodem.Path = pathTextBox.Text.ToString();
                ymodem.PortName = SerialPortComboBox.SelectedItem.ToString();
                ymodem.BaudRate = Convert.ToInt32(BaudRateComboBox.SelectedItem.ToString());
                downloadThread = new System.Threading.Thread(ymodem.YmodemUploadFile);
                ymodem.NowDownloadProgressEvent += new EventHandler(NowDownloadProgressEvent);
                ymodem.DownloadResultEvent += new EventHandler(DownloadFinishEvent);
                downloadThread.Start();
            } 
        }
        #region 下载进度委托及事件响应
        private delegate void NowDownloadProgress(int nowValue);
        private void NowDownloadProgressEvent(object sender, EventArgs e)
        {
            int value = Convert.ToInt32(sender);
            NowDownloadProgress count = new NowDownloadProgress(UploadFileProgress);
           this.Invoke(count, value);
        }
        private void UploadFileProgress(int count)
        {
            DownloadProgressBar.Value = count;
        }
        #endregion
        #region 下载完成委托及事件响应
        private delegate void DownloadFinish(bool finish);
        private void DownloadFinishEvent(object sender, EventArgs e)
        {
            bool finish = (Boolean)sender;
            DownloadFinish status = new DownloadFinish(UploadFileResult);
            this.Invoke(status,finish);
        }
        private void UploadFileResult(bool result)
        {
            if (result == true)
            {
                MessageBox.Show("下载成功");
                serialPort.Open();
                serialPort.Write("3");//写数据
                serialPort.Close();
                this.downloadButton.Text = "开始升级";
                this.DownloadProgressBar.Value = 0;

            }
            else
            {
                MessageBox.Show("下载失败");
                this.downloadButton.Text = "开始升级";
                this.DownloadProgressBar.Value = 0;
            }
        }
        #endregion

    }
}
