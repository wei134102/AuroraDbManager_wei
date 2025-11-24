using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AuroraDbManager
{
    public partial class LoginWindow : Window
    {
        // 重要提示：这只是一个示例！在实际应用中，不应将密码或哈希值硬编码。
        // 更好的做法是从一个安全的配置文件或专门的密码管理服务中读取。
        // 这里存储的是 "123456" 经过 SHA256 哈希后的值。
        private const string StoredPasswordHash = "8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92";

        public LoginWindow()
        {
            InitializeComponent();
            PasswordBox.Focus(); // 窗口打开时，光标自动聚焦到密码框
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string enteredPassword = PasswordBox.Password;
            if (VerifyPassword(enteredPassword))
            {
                // 设置 DialogResult 为 true，表示登录成功
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误！", "登录失败", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult 默认是 false，直接关闭窗口即可
            this.Close();
        }

        private bool VerifyPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                return builder.ToString() == StoredPasswordHash;
            }
        }
    }
}