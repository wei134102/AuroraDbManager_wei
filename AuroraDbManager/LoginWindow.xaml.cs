using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AuroraDbManager
{
    public partial class LoginWindow : Window
    {
        // 重要提示：这只是一个示例！在实际应用中，不应将密码或哈希值硬编码。
        // 更好的做法是从一个安全的配置文件或专门的密码管理服务中读取。
        // 这里存储的是 "admin" 经过 SHA256 哈希后的值。
        private const string StoredPasswordHash = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918";

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
            // 新增：将可能的全角数字转换为半角数字，以提高用户体验
            string normalizedPassword = ToHalfWidth(password);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedPassword));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                return string.Equals(builder.ToString().Trim(), StoredPasswordHash.Trim(), System.StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 将字符串中的全角字符转换为半角字符
        /// </summary>
        private string ToHalfWidth(string input)
        {
            StringBuilder sb = new StringBuilder(input);
            for (int i = 0; i < sb.Length; i++)
            {
                // 全角空格为12288，半角空格为32
                // 其他全角字符（33-126）与半角字符相差65248
                if (sb[i] >= 65281 && sb[i] <= 65374)
                    sb[i] = (char)(sb[i] - 65248);
            }
            return sb.ToString();
        }
    }
}