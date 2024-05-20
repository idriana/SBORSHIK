using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace SBORSHIK.core
{
    public class CompileCommand
    {
        CompileData data;
        string audioFilePath;

        public CompileCommand(CompileData data, string audioFilePath) 
        {
            this.data = data;
            this.audioFilePath = audioFilePath;
        }
        
        public string GetString()
        {
            if (data.Subtitles)
            {
                return GetStringWithSubs();
            }
            else
            {
                return GetStringNoSubs();
            }
        }
        private string GetStringNoSubs()
        {
            string result = "\"{0}\" " +
            "--ui-language ru " +
            "--priority lower " +
            "--output ^\"{1}^\" " +
            "--language 0:{2} " +
            "--track-name ^\"0:Видеоряд [{3}]^\" ^\"^(^\" ^\"{4}^\" ^\"^)^\" " +
            "--sync 0:0 " +
            "--language 0:ru " +
            "--track-name ^\"0:Русский [Дубляжная]^\" ^\"^(^\" ^\"{5}^\" ^\"^)^\" " +
            "--sync 0:0 " +
            "--language 0:{6} " +
            "--track-name ^\"0:Оригинал [{7}]^\" " +
            "--default-track-flag 0:no ^\"^(^\" ^\"{8}^\" ^\"^)^\" " +
            "--track-order 0:0,1:0,2:0 ";

            result = string.Format(result,
                [data.MKVToolNixFile.Trim(),
                data.OutputFile.Trim(),
                data.SourceLanguage.Trim(),
                data.StudioName.Trim(),
                data.VideoFile.Trim(),
                audioFilePath.Trim(),
                data.SourceLanguage.Trim(),
                data.StudioName.Trim(),
                data.SourceAudioFile.Trim()
            ]);
            return result;
        }
        private string GetStringWithSubs()
        {
            string result = "\"{0}\" " +
            "--ui-language ru " +
            "--priority lower " +
            "--output ^\"{1}^\" " +
            "--enable-legacy-font-mime-types " +
            "--language 0:{2} " +
            "--track-name ^\"0:Видеоряд [{3}]^\" ^\"^(^\" ^\"{4}^\" ^\"^)^\" " +
            "--sync 0:0 " +
            "--language 0:ru " +
            "--track-name ^\"0:Русский [Дубляжная]^\" ^\"^(^\" ^\"{5}^\" ^\"^)^\" " +
            "--sync 0:0 " +
            "--language 0:{6} " +
            "--track-name ^\"0:Оригинал [{7}]^\" " +
            "--default-track-flag 0:no ^\"^(^\" ^\"{8}^\" ^\"^)^\" " +
            "--language 0:ru " +
            "--track-name ^\"0:Субтитры [{9}]^\" " +
            "--default-track-flag 0:no ^\"^(^\" ^\"{10}^\" ^\"^)^\" " +
            "--language 0:ru " +
            "--track-name ^\"0:Надписи [{11}]^\" " +
            "--forced-display-flag 0:yes ^\"^(^\" ^\"{12}^\" ^\"^)^\" ";

            result = string.Format(result,
                [data.MKVToolNixFile.Trim(),
                data.OutputFile.Trim(),
                data.SourceLanguage.Trim(),
                data.StudioName.Trim(),
                data.VideoFile.Trim(),
                audioFilePath.Trim(),
                data.SourceLanguage.Trim(),
                data.StudioName.Trim(),
                data.SourceAudioFile.Trim(),
                data.Translation.Trim(),
                data.SubtitlesFile.Trim(),
                data.Translation.Trim(),
                data.InsctiptionFile.Trim()
            ]);

            for (int i = 0; i < data.FontsFile.Count; i++)
            {
                string template = "--attachment-name {0} --attachment-mime-type application/x-truetype-font --attach-file ^\"{1}^\" ";
                template = string.Format(template, [System.IO.Path.GetFileName(data.FontsFile[i].Trim()), data.FontsFile[i].Trim()]);
                result += template;
            }

            result += "--track-order 0:0,1:0,2:0 ";
            return result;
        }
    }
}
