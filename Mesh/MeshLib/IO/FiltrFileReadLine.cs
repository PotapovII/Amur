//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System.Collections.Generic;
    using System.IO;

    public class FiltrFileReadLine
    {
        List<string> lines = new List<string>();
        int Count = 0;
        public FiltrFileReadLine(StreamReader file)
        {
            for (string line = file.ReadLine(); line != null; line = file.ReadLine())
            {
                line = line.Trim();
                if (line[0] != '#')
                    lines.Add(line);
            }
            Count = 0;
        }
        public void SetPosition(int p = 0)
        {
            Count = 0;
        }
        public string ReadLine()
        {
            return lines[Count++];
        }
    }
}
