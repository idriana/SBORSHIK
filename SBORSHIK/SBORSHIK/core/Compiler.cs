using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SBORSHIK.core
{
    public class Compiler
    {
        CompileData data;
        PropertyChangedEventHandler propertyChangedEventHandler;
        object sender;
        string properyName;
        StreamReader sr;
        int skipCounter = 0;

        public Compiler(CompileData data, PropertyChangedEventHandler propertyChangedEventHandler, object sender, string properyName)
        {
            this.data = data;
            this.propertyChangedEventHandler = propertyChangedEventHandler;
            this.sender = sender;
            this.properyName = properyName;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public bool CheckFiles()
        {
            return true;
        }

        public void Compile() 
        {
            data.Logs = "";
            UpdateLogs("Конвертируем аудио файл (это может занять некоторое время)...");
            FFMpegConverter converter = new FFMpegConverter(data);
            string audioPath = converter.Convert();
            if (audioPath == null)
            {
                UpdateLogs("FFMpeg не может конвертировать файл, проверьте правильность введенных данных");
                UpdateLogs("Отмена операции");
                return;
            }
            UpdateLogs("Аудио файл сконвертирован в " + audioPath);

            CompileCommand compileCommand = new CompileCommand(data, audioPath);
            string MKVCommand = compileCommand.GetString();

            UpdateLogs("Создаем процесс cmd.exe...");
            Process cmd = new Process();
            Encoding encoding = Encoding.GetEncoding(855);
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.StandardInputEncoding = Encoding.GetEncoding(855);
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(65001);
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            
            cmd.OutputDataReceived += (senderOutput, e) => UpdateLogs(e.Data);
            cmd.Start();
            cmd.BeginOutputReadLine();
            cmd.StandardInput.WriteLine("CHCP 855");
            MKVCommand = Encoding.UTF8.GetString(Encoding.Default.GetBytes(MKVCommand));
            cmd.StandardInput.WriteLine(MKVCommand);
            cmd.StandardInput.WriteLine("exit");
            cmd.WaitForExit();
            string errors = cmd.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errors))
            {
                UpdateLogs("Процесс cmd.exe завершен с ошибкой: ");
                UpdateLogs(errors);
            }
            else
            {
                UpdateLogs("Процесс cmd.exe завершен");
            }
            UpdateLogs("Очищаем временные файлы...");
            converter.ClearTemp();
            UpdateLogs("Сборка завершена");

        } 

        private void UpdateLogs(string line)
        {
            if (line != null && !line.Contains("Microsoft"))
            {
                data.Logs += line + "\n";
                propertyChangedEventHandler.Invoke(sender, new PropertyChangedEventArgs(properyName));
            }
        }
    }
}
