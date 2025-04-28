using System.Windows.Forms;
using System.Text.Json;

namespace 小程序解密
{
    public partial class ParseRequestForm : Form
    {
        // 定义公共属性，用于返回解析后的数据
        public string EncryptedData { get; private set; }
        public string IV { get; private set; }
        public string SessionKey { get; private set; }

        // 输入框控件
        private TextBox inputTextBox;

        /// <summary>
        /// 构造函数，初始化窗体
        /// </summary>
        public ParseRequestForm()
        {
            this.Text = "你好";
            this.StartPosition = FormStartPosition.CenterParent;
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
            mainLayoutPanel.ColumnCount = 1; // 单列布局
            mainLayoutPanel.RowCount = 3;   // 总行数3行

            // 设置行高
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 第0行：标题标签
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 第1行：输入框
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // 第2行：确认按钮

            // 标题标签
            Label titleLabel = new Label()
            {
                Text = "输入请求体JSON",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 输入框（多行）
            inputTextBox = new TextBox() // 将 inputTextBox 保存为类成员变量
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 12)
            };

            // 确认按钮
            Button confirmButton = new Button()
            {
                Text = "确认",
                Dock = DockStyle.Fill
            };
            confirmButton.Click += Confirm_Click;

            // 添加控件到布局
            mainLayoutPanel.Controls.Add(titleLabel, 0, 0);         // 第0行：标题标签
            mainLayoutPanel.Controls.Add(inputTextBox, 0, 1);       // 第1行：输入框
            mainLayoutPanel.Controls.Add(confirmButton, 0, 2);      // 第2行：确认按钮

            // 将布局添加到窗体
            this.Controls.Add(mainLayoutPanel);
            this.AcceptButton = confirmButton; // 设置默认确认按钮
        }

        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void Confirm_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取输入框内容
                var inputText = inputTextBox.Text.Trim();

                // 调用解析方法
                var data = ParseJson(inputText);

                // 设置解析结果
                EncryptedData = data.encryptedData;
                IV = data.iv;
                SessionKey = data.sessionKey;

                // 关闭窗口并返回OK结果
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 解析JSON字符串，支持模糊匹配字段名
        /// </summary>
        /// <param name="jsonText">JSON字符串</param>
        /// <returns>包含加密数据、IV和SessionKey的元组</returns>
        private (string encryptedData, string iv, string sessionKey) ParseJson(string jsonText)
        {
            // 处理未包裹在{}中的JSON格式
            jsonText = jsonText.Trim();
            if (!jsonText.StartsWith("{"))
            {
                jsonText = "{" + jsonText.TrimEnd(',') + "}"; // 移除末尾可能的逗号并补全大括号
            }

            try
            {
                // 配置反序列化选项
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 反序列化为字典
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonText);

                // 模糊匹配字段
                string encryptedData = FindKey(dict, new[] { "encryptedData", "encrypted", "data" });
                string iv = FindKey(dict, new[] { "iv" });
                string sessionKey = FindKey(dict, new[] { "sessionKey", "Session_Key", "Session", "Key" });

                // 检查必要字段是否存在
                if (string.IsNullOrEmpty(encryptedData) ||
                    string.IsNullOrEmpty(iv) ||
                    string.IsNullOrEmpty(sessionKey))
                {
                    throw new Exception("未找到必要的字段");
                }

                return (encryptedData, iv, sessionKey);
            }
            catch (JsonException ex)
            {
                throw new Exception($"JSON格式错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 模糊匹配字段名
        /// </summary>
        /// <param name="dict">字典对象</param>
        /// <param name="candidates">候选字段名数组</param>
        /// <returns>匹配到的字段值，若未找到则返回null</returns>
        private string FindKey(Dictionary<string, object> dict, string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                // 先检查完全匹配
                if (dict.TryGetValue(candidate, out var value))
                    return value.ToString();

                // 再检查模糊匹配
                foreach (var key in dict.Keys)
                {
                    if (key.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                        return dict[key].ToString();
                }
            }
            return null;
        }
    }
}