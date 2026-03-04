using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messenger.Client
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Добавляем обработчик непойманных исключений
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show($"Ошибка: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
                    "Необработанное исключение",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Ошибка: {((Exception)e.ExceptionObject).Message}",
                    "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Открываем окно вывода для отладки
            Console.WriteLine("🚀 Запуск приложения...");

            Application.Run(new MainForm());
        }
    }
}
