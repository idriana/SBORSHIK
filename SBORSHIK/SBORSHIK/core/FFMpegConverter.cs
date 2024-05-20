using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;

namespace SBORSHIK.core
{
    public class FFMpegConverter
    {
        CompileData data;
        string tempPath = "\\temp";
        string fileName = "temp.aac";

        public FFMpegConverter(CompileData data) 
        {
            this.data = data;
            this.tempPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\temp";
        }

        public string Convert()
        {
            try
            {
                FileSystemHelper.CreateDirectoryIfNotExists(tempPath);
                string filePath = tempPath + "\\" + fileName;
                FFMpegArguments
                    .FromFileInput(data.NewAudioFile)
                    .OutputToFile(filePath, true, options => options
                    .WithAudioBitrate(AudioQuality.VeryHigh) // Эквивалент -b:a 256k, согласно документации битрейт должен быть константным
                    .WithAudioCodec(AudioCodec.Aac)
                    .OverwriteExisting()
                    )
                    .ProcessSynchronously();
                return filePath;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void ClearTemp()
        {
            string[] tempFiles = Directory.GetFiles(tempPath, "*");
            foreach ( string file in tempFiles )
            {
                File.Delete(file);
            }
        }
    }
}
