namespace 小程序解密
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing); // ❗ 必须调用基类的 Dispose 方法
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            ClientSize = new Size(284, 261);
            Name = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);
        }
    }
}