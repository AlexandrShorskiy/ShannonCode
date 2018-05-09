using SimpleTCP;
using System;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace Client
{
    public partial class Form1 : Form
    {
        public string[] Res;
        public List<Part> letters = new List<Part>();
        public string result = "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZzyxwvutsrqponmlkjihgfedcba" +
            "АаБбВвГгҐґДдЕеЄєЖжЗзИиІіЇїЙйКкЛлМмНнОоПпРрСсТтУуФфХхЦцЧчШшЩщЬьЮюЯя" + ".,;:!¡?¿⁇⸮‽⸘…‐‒–—―[](){}⟨⟩„“«»“”‘’‹›";
        public double[] p1;
        public Dictionary<char, string> CodingLetters = new Dictionary<char,string>();

        public Form1()
        {
            InitializeComponent();
        }

        SimpleTcpServer server;
        SimpleTcpClient client;

        private void Form1_Load(object sender, EventArgs e)
        {
            b_UnConnect.Enabled = false;
            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;

            server = new SimpleTcpServer();
            server.Delimiter = 0x13;
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            try
            {
                tb_Status.Invoke((MethodInvoker)delegate()
                {
                    tb_Status.Text += e.MessageString;
                    e.ReplyLine(string.Format("\nВы: \t{0}", e.MessageString));
                });
            }
            catch { };
        }

        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            try
            {
                tb_Status.Invoke((MethodInvoker)delegate()
                {
                    tb_Status.Text += "\n" + e.MessageString;
                });
                tb_Message.Text = "";
            }
            catch{}
        }

        private void b_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                client.Connect(tb_host.Text, int.Parse(tb_port.Text));
                b_Connect.Enabled = false;
                b_UnConnect.Enabled = true;
                tb_Status.Text += "\r\nПодключено...\r\n";
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка");
            }
        }

        private void b_UnConnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
            tb_Status.Text += "\r\nОтключено!\r\n";
            b_UnConnect.Enabled = false;
            b_Connect.Enabled = true;
        }

        private void b_Start_Click(object sender, EventArgs e)
        {
            byte[] addr = { 127, 0, 0, 1 };
            tb_Status.Text += "Сервер запущен...\r\n";
            System.Net.IPAddress ip = new System.Net.IPAddress(addr);
            server.Start(ip, Convert.ToInt32(textBox2.Text));
        }

        private void b_Stop_Click(object sender, EventArgs e)
        {
            if (server.IsStarted)
            {
                server.Stop();
                tb_Status.Text += "\r\nСервер выключен!";
            }
        }

        private void b_Send_Click(object sender, EventArgs e)
        {
            try
            {
                CodingLetters.Clear();
                Coder(dataGridView1, tb_Message.Text.ToArray());
            }
            catch { }
            try { CoderMessage(tb_Message.Text, richTextBox1); }
            catch { }
            try { DecoderMessage(richTextBox1.Text, richTextBox2); }
            catch { }
            try { client.WriteLineAndGetReply(tb_Message.Text, TimeSpan.FromSeconds(1)); }
            catch { };
        }

        public void Coder(DataGridView grid, char[] message)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();
            grid.Columns.Add("", "Буква");
            grid.Columns.Add("", "Вероятность");
            grid.Columns.Add("", "Кодирование");
            var str = message.Distinct().ToArray();
            p1 = new double[str.Length];
            for (int j = 0; j < str.Length; j++)
            {
                int count = 0;
                for (int i = j; i < message.Length; i++)
                    if (str[j] == message[i])
                        count++;
                p1[j] = Math.Round((Convert.ToDouble(count) / message.Length),5);
                grid.Rows.Add(str[j], p1[j], "");
                letters.Add(new Part { PartName = str[j], PartCount = p1[j] });
            }
            int k = p1.Length;
            Res = new string[k];
            Array.Sort(p1);
            Array.Reverse(p1);
            Shennon_Fano(0, k - 1);
            for (int j = 0; j < str.Length; j++)
            {
                grid[2, j].Value = Res[j];
                CodingLetters.Add(str[j], Res[j]);
            }
        }
        public void CoderMessage(string  message, RichTextBox TextWindow)
        {
            string str = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (CodingLetters.ContainsKey(message[i]))
                    str += CodingLetters[message[i]];
            }
            TextWindow.Text = str;
        }
        public void DecoderMessage(string message, RichTextBox TextWindow)
        {
            string str = "";
            string code = "";
            Dictionary<string,char> newDeict = ParseDictionary(CodingLetters);
            for (int i = 0; i < message.Length; i++)
            {
                code += message[i];
                if (newDeict.ContainsKey(code))
                {
                    str += newDeict[code];
                    code = "";
                }
            }
            TextWindow.Text = str;
        }
        public Dictionary<string,char> ParseDictionary(Dictionary<char, string> dict)
        {
            Dictionary<string, char> newDeict = new Dictionary<string, char>();
            foreach(char x in dict.Keys)
            {
                newDeict.Add(dict[x], x);
            }
            return newDeict;
        }

        double schet1 = 0;
        double schet2 = 0;
        public void Shennon_Fano(int Left, int Right)
        {
            int n;
            if (Left < Right)
            {
                n = Sequence_division(Left, Right);
                for (int i = Left; i <= Right; i++)
                {
                    if (i <= n) Res[i] += Convert.ToByte(0);
                    else Res[i] += Convert.ToByte(1);
                }
                Shennon_Fano(Left, n);
                Shennon_Fano(n + 1, Right);
            }
        }
        public int Sequence_division(int Left, int Right)
        {
            int m;
            schet1 = 0;
            for (int i = Left; i <= Right - 1; i++)
                schet1 = schet1 + p1[i];

            schet2 = p1[Right];
            m = Right;
            while (schet1 >= schet2)
            {
                m = m - 1;
                schet1 = schet1 - p1[m];
                schet2 = schet2 + p1[m];
            }
            return m;
        }
    }
}
