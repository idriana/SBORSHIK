using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SBORSHIK.core
{
    public class CompileData
    {
        public string MKVToolNixFile = "test";
        public string VideoFile = "";
        public string SourceAudioFile = "";
        public string NewAudioFile = "";
        public string StudioName = "";
        public string SourceLanguage = "";

        public bool Subtitles = false;
        public string SubtitlesFile = "";
        public string InsctiptionFile = "";
        public string Translation = "Crunchyroll";
        public List<string> FontsFile = new();
        public string OutputFile = "";

        public string Logs = "Ахахахахаххаха. ЖЕСТЬ";

        public CompileData()
        {
        }

        // чего только не сделаешь чтобы не писать под каждую кнопку команду
        public void SetVariable(string name, object value)
        {
            FieldInfo field = this.GetType().GetField(name);
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                List<string> variable = field.GetValue(this) as List<string>;
                variable.Clear();
                List<string> valueList = value as List<string>;
                foreach (string x in valueList)
                {
                    if (!string.IsNullOrEmpty(x))
                        variable.Add(x);
                }
            }
            else
            {
                field?.SetValue(this, value);
            }
        }
    }
}
