using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PropertyGenerator
{
    public static class SourceCreator
    {
        private static readonly string GeneratePath = Path.Combine("Assets", "Scripts", "CoreModule", "PropertyGenerator", "Generated");

        public static void CreateFile(string fileName, CodeBuilder builder)
        {
            string path = Path.Combine(GeneratePath, fileName + ".cs");

            using (var stream = File.Create(path))
            {
                var generatedCode = builder.ToString();
                var bytes = new UTF8Encoding(true).GetBytes(generatedCode);
                stream.Write(bytes);
            }

            AssetDatabase.Refresh();

            Debug.Log(path + "にファイルを生成しました。");
        }
    }
}