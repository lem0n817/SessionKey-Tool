using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace 小程序解密
{
    public partial class MainForm : Form
    {
        // 定义UI控件
        private TextBox textBoxPlaintext;  // 明文输入框
        private TextBox textBoxEncrypted; // 密文输入框
        private TextBox textBoxKey;       // 密钥输入框
        private TextBox textBoxIV;        // IV输入框
        private ComboBox comboBoxKeyEncoding; // 密钥编码选择框
        private ComboBox comboBoxIVEncoding;  // IV编码选择框
        private Button buttonDecrypt;     // 解密按钮
        private Button buttonEncrypt;     // 加密按钮
        private Button buttonUrlEncode;   // URL编码按钮
        private Button buttonUrlDecode;   // URL解码按钮
        private Button buttonBase64Encode; // Base64编码按钮
        private Button buttonBase64Decode; // Base64解码按钮
        private Button buttonClear;       // 清空内容按钮

        /// <summary>
        /// 构造函数，初始化窗体和控件
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.ControlBox = true;
            this.Text = "微信小程序加解密工具 by Lemon安全团队@Lemoni";
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 设置主窗口居中弹出
            this.StartPosition = FormStartPosition.CenterScreen;

            // 初始化所有新按钮（确保在 InitializeLayout 之前初始化）
            buttonBase64Encode = new Button() { Text = "Base64编码", Dock = DockStyle.Fill };
            buttonBase64Decode = new Button() { Text = "Base64解码", Dock = DockStyle.Fill };
            buttonUrlEncode = new Button() { Text = "URL编码", Dock = DockStyle.Fill };
            buttonUrlDecode = new Button() { Text = "URL解码", Dock = DockStyle.Fill };
            buttonClear = new Button() { Text = "清空内容", Dock = DockStyle.Fill };

            // 绑定事件
            buttonBase64Encode.Click += Base64Encode_Click;
            buttonBase64Decode.Click += Base64Decode_Click;
            buttonUrlEncode.Click += UrlEncode_Click;
            buttonUrlDecode.Click += UrlDecode_Click;

            // 初始化布局
            InitializeLayout();
        }

        /// <summary>
        /// 初始化窗体布局
        /// </summary>
        private void InitializeLayout()
        {
            // 创建主布局容器
            TableLayoutPanel mainLayoutPanel = new TableLayoutPanel();
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.ColumnCount = 4; // 固定4列
            mainLayoutPanel.RowCount = 8;    // 总行数8行

            // 清空原有行和列样式，重新定义
            mainLayoutPanel.RowStyles.Clear();
            mainLayoutPanel.ColumnStyles.Clear();

            // 设置列宽（四列，每列25%）
            for (int i = 0; i < 4; i++)
            {
                mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            // 设置行高（优化后的紧凑布局）
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 0: Session_Key标签行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // 1: Session_Key输入行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 2: IV标签行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // 3: IV输入行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // 4: 加密/解密按钮行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // 5: 明文/密文文本框行
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));   // 6: 编码按钮行（四列）
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // 7: 字体调整和清空按钮行

            // 创建标签控件
            Label keyLabel = new Label()
            {
                Text = "Session_Key",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label ivLabel = new Label()
            {
                Text = "IV",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 创建输入框和编码选择框
            textBoxKey = new TextBox() { Dock = DockStyle.Fill };
            textBoxIV = new TextBox() { Dock = DockStyle.Fill };
            comboBoxKeyEncoding = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            comboBoxKeyEncoding.Items.AddRange(new string[] { "Base64", "Hex", "UTF-8" });
            comboBoxKeyEncoding.SelectedIndex = 0;
            comboBoxIVEncoding = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            comboBoxIVEncoding.Items.AddRange(new string[] { "Base64", "Hex", "UTF-8" });
            comboBoxIVEncoding.SelectedIndex = 0;

            // 创建加密/解密按钮
            buttonDecrypt = new Button() { Text = "<--解密", Dock = DockStyle.Fill };
            buttonEncrypt = new Button() { Text = "加密-->", Dock = DockStyle.Fill };

            // 创建字体调整按钮
            Button btnIncreaseFont = new Button() { Text = "放大+", Dock = DockStyle.Fill };
            Button btnDecreaseFont = new Button() { Text = "缩小-", Dock = DockStyle.Fill };

            // 创建明文/密文文本框
            textBoxPlaintext = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 12)
            };
            SetupPlaceholderTextBox(textBoxPlaintext, "明文");
            textBoxEncrypted = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 12)
            };
            SetupPlaceholderTextBox(textBoxEncrypted, "密文");

            // 添加控件到主布局容器
            // === 第0行：Session_Key标签 ===
            mainLayoutPanel.Controls.Add(keyLabel, 0, 0);
            mainLayoutPanel.SetColumnSpan(keyLabel, 4); // 跨4列

            // === 第1行：Session_Key输入框 + 编码选择 ===
            TableLayoutPanel keyRowPanel = new TableLayoutPanel();
            keyRowPanel.ColumnCount = 2;
            keyRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            keyRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            keyRowPanel.Dock = DockStyle.Fill;
            keyRowPanel.Controls.Add(textBoxKey, 0, 0);
            keyRowPanel.Controls.Add(comboBoxKeyEncoding, 1, 0);
            mainLayoutPanel.Controls.Add(keyRowPanel, 0, 1);
            mainLayoutPanel.SetColumnSpan(keyRowPanel, 4);

            // === 第2行：IV标签 ===
            mainLayoutPanel.Controls.Add(ivLabel, 0, 2);
            mainLayoutPanel.SetColumnSpan(ivLabel, 4);

            // === 第3行：IV输入框 + 编码选择 ===
            TableLayoutPanel ivRowPanel = new TableLayoutPanel();
            ivRowPanel.ColumnCount = 2;
            ivRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            ivRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            ivRowPanel.Dock = DockStyle.Fill;
            ivRowPanel.Controls.Add(textBoxIV, 0, 0);
            ivRowPanel.Controls.Add(comboBoxIVEncoding, 1, 0);
            mainLayoutPanel.Controls.Add(ivRowPanel, 0, 3);
            mainLayoutPanel.SetColumnSpan(ivRowPanel, 4);

            // === 第4行：加密/解密按钮 ===
            mainLayoutPanel.Controls.Add(buttonDecrypt, 2, 4);
            mainLayoutPanel.SetColumnSpan(buttonDecrypt, 2);
            mainLayoutPanel.Controls.Add(buttonEncrypt, 0, 4);
            mainLayoutPanel.SetColumnSpan(buttonEncrypt, 2);

            // === 第5行：明文/密文文本框 ===
            mainLayoutPanel.Controls.Add(textBoxPlaintext, 0, 5);
            mainLayoutPanel.SetColumnSpan(textBoxPlaintext, 2);
            mainLayoutPanel.Controls.Add(textBoxEncrypted, 2, 5);
            mainLayoutPanel.SetColumnSpan(textBoxEncrypted, 2);

            // === 第6行：编码按钮（四列） ===
            mainLayoutPanel.Controls.Add(buttonUrlEncode, 0, 6);
            mainLayoutPanel.Controls.Add(buttonUrlDecode, 1, 6);
            mainLayoutPanel.Controls.Add(buttonBase64Encode, 2, 6);
            mainLayoutPanel.Controls.Add(buttonBase64Decode, 3, 6);

            // === 第7行：底部按钮行 ===
            TableLayoutPanel buttonRowPanel = new TableLayoutPanel();
            buttonRowPanel.ColumnCount = 4; // 四列，每列25%
            for (int i = 0; i < 4; i++)
            {
                buttonRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }
            buttonRowPanel.Dock = DockStyle.Fill;

            // 添加按钮到buttonRowPanel
            buttonRowPanel.Controls.Add(btnIncreaseFont, 0, 0); // 列0：放大+
            buttonRowPanel.Controls.Add(btnDecreaseFont, 1, 0); // 列1：缩小-
            buttonRowPanel.Controls.Add(buttonClear, 2, 0);     // 列2：清空内容
            Button btnParseRequest = new Button() { Text = "解析请求", Dock = DockStyle.Fill };
            buttonRowPanel.Controls.Add(btnParseRequest, 3, 0); // 列3：解析请求

            // 绑定事件
            btnParseRequest.Click += ParseRequest_Click;
            btnIncreaseFont.Click += (s, e) => AdjustFontSize(+2);
            btnDecreaseFont.Click += (s, e) => AdjustFontSize(-2);
            buttonClear.Click += Clear_Click;

            // 将buttonRowPanel添加到主布局的第7行
            mainLayoutPanel.Controls.Add(buttonRowPanel, 0, 7);
            mainLayoutPanel.SetColumnSpan(buttonRowPanel, 4);

            // 绑定其他事件
            buttonDecrypt.Click += Decrypt_Click;
            buttonEncrypt.Click += Encrypt_Click;
            buttonUrlEncode.Click += UrlEncode_Click;
            buttonUrlDecode.Click += UrlDecode_Click;
            buttonBase64Encode.Click += Base64Encode_Click;
            buttonBase64Decode.Click += Base64Decode_Click;

            // 设置窗体最小尺寸
            this.MinimumSize = new Size(800, 600);

            // 添加主布局到窗体
            this.Controls.Add(mainLayoutPanel);
        }

        /// <summary>
        /// 全局字体大小调整方法
        /// </summary>
        /// <param name="delta">字体调整增量</param>
        private float currentFontSize = 12f;
        private void AdjustFontSize(int delta)
        {
            currentFontSize = Math.Max(10f, Math.Min(24f, currentFontSize + delta));
            textBoxKey.Font = new Font(textBoxKey.Font.FontFamily, currentFontSize);
            textBoxIV.Font = new Font(textBoxIV.Font.FontFamily, currentFontSize);
            textBoxPlaintext.Font = new Font(textBoxPlaintext.Font.FontFamily, currentFontSize);
            textBoxEncrypted.Font = new Font(textBoxEncrypted.Font.FontFamily, currentFontSize);
        }

        /// <summary>
        /// 解密按钮点击事件
        /// </summary>
        private void Decrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string encryptedText = textBoxEncrypted.Text;
                string keyText = textBoxKey.Text;
                string ivText = textBoxIV.Text;
                string keyEncoding = comboBoxKeyEncoding.SelectedItem.ToString();
                string ivEncoding = comboBoxIVEncoding.SelectedItem.ToString();

                byte[] keyBytes = Decode(keyText, keyEncoding);
                byte[] ivBytes = Decode(ivText, ivEncoding);

                // 检查Key和IV长度
                if (keyBytes.Length != 16 && keyBytes.Length != 24 && keyBytes.Length != 32)
                    throw new Exception("密钥长度必须为16/24/32字节");
                if (ivBytes.Length != 16)
                    throw new Exception("IV长度必须为16字节");

                byte[] encryptedData = Convert.FromBase64String(encryptedText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encryptedData))
                    using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream))
                    {
                        textBoxPlaintext.Text = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解密失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加密按钮点击事件
        /// </summary>
        private void Encrypt_Click(object sender, EventArgs e)
        {
            // 空值检查
            if (string.IsNullOrEmpty(textBoxPlaintext.Text) ||
                textBoxPlaintext.Text == textBoxPlaintext.Tag.ToString()) // 检查占位符状态
            {
                return;
            }

            try
            {
                string plainText = textBoxPlaintext.Text;
                string keyText = textBoxKey.Text;
                string ivText = textBoxIV.Text;
                string keyEncoding = comboBoxKeyEncoding.SelectedItem.ToString();
                string ivEncoding = comboBoxIVEncoding.SelectedItem.ToString();

                byte[] keyBytes = Decode(keyText, keyEncoding);
                byte[] ivBytes = Decode(ivText, ivEncoding);

                // 检查Key和IV长度
                if (keyBytes.Length != 16 && keyBytes.Length != 24 && keyBytes.Length != 32)
                    throw new Exception("密钥长度必须为16/24/32字节");
                if (ivBytes.Length != 16)
                    throw new Exception("IV长度必须为16字节");

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        textBoxEncrypted.Text = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加密失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 根据编码类型解码字符串为字节数组
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="encodingOption">编码选项</param>
        /// <returns>解码后的字节数组</returns>
        private byte[] Decode(string input, string encodingOption)
        {
            switch (encodingOption)
            {
                case "Base64":
                    return Convert.FromBase64String(input);
                case "Hex":
                    return HexToBytes(input);
                case "ASCII":
                    return Encoding.ASCII.GetBytes(input);
                default:
                    throw new ArgumentException("未知编码类型");
            }
        }

        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>字节数组</returns>
        private byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new FormatException("十六进制字符串必须为偶数长度");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }

        /// <summary>
        /// URL编码按钮点击事件
        /// </summary>
        private void UrlEncode_Click(object sender, EventArgs e)
        {
            textBoxEncrypted.Text = HttpUtility.UrlEncode(textBoxEncrypted.Text);
        }

        /// <summary>
        /// 解析请求按钮点击事件
        /// </summary>
        private void ParseRequest_Click(object sender, EventArgs e)
        {
            // 创建并显示新窗口
            ParseRequestForm dialog = new ParseRequestForm();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // 获取解析后的数据并填充到主窗口
                textBoxEncrypted.Text = dialog.EncryptedData;
                textBoxIV.Text = dialog.IV;
                textBoxKey.Text = dialog.SessionKey;
            }
        }

        /// <summary>
        /// URL解码按钮点击事件
        /// </summary>
        private void UrlDecode_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxEncrypted.Text = HttpUtility.UrlDecode(textBoxEncrypted.Text);
            }
            catch
            {
                MessageBox.Show("URL解码失败，请检查格式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清空内容按钮点击事件
        /// </summary>
        private void Clear_Click(object sender, EventArgs e)
        {
            textBoxKey.Text = "";
            textBoxIV.Text = "";
            textBoxPlaintext.Text = "";
            textBoxEncrypted.Text = "";
            comboBoxKeyEncoding.SelectedIndex = 0;
            comboBoxIVEncoding.SelectedIndex = 0;
        }

        /// <summary>
        /// Base64编码按钮点击事件
        /// </summary>
        private void Base64Encode_Click(object sender, EventArgs e)
        {
            string input = textBoxEncrypted.Text.Trim();
            if (input == "密文" || string.IsNullOrEmpty(input)) return;

            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                string encoded = Convert.ToBase64String(bytes);
                textBoxEncrypted.Text = encoded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"编码失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Base64解码按钮点击事件
        /// </summary>
        private void Base64Decode_Click(object sender, EventArgs e)
        {
            string input = textBoxEncrypted.Text.Trim();
            if (input == "密文" || string.IsNullOrEmpty(input)) return;

            try
            {
                byte[] bytes = Convert.FromBase64String(input);
                string decoded = System.Text.Encoding.UTF8.GetString(bytes);
                textBoxEncrypted.Text = decoded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解码失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置文本框占位符
        /// </summary>
        /// <param name="textBox">目标文本框</param>
        /// <param name="placeholderText">占位符文本</param>
        private void SetupPlaceholderTextBox(TextBox textBox, string placeholderText)
        {
            textBox.Text = placeholderText;
            textBox.ForeColor = Color.Gray;
            textBox.Tag = placeholderText;

            textBox.TextChanged += (s, e) =>
            {
                var tb = (TextBox)s;
                tb.ForeColor = (tb.Text == tb.Tag.ToString()) ? Color.Gray : Color.Black;
            };

            textBox.Enter += (s, e) =>
            {
                var tb = (TextBox)s;
                if (tb.Text == tb.Tag.ToString())
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };

            textBox.Leave += (s, e) =>
            {
                var tb = (TextBox)s;
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = tb.Tag.ToString();
                    tb.ForeColor = Color.Gray;
                }
            };
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
        }
    }
}