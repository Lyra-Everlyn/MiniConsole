using System;
using System.Collections.Generic;
using System.Text;

namespace MiniConsole.Models
{
    public class JsonStreamState : IDisposable
    {
        public StreamWriter Writer { get; }
        public string FilePath { get; }
        public bool IsFirstItem { get; set; } = true;
        public long WrittenCount { get; set; } = 0;

        public JsonStreamState(StreamWriter writer, string filePath)
        {
            Writer = writer;
            FilePath = filePath;
        }

        public void Dispose()
        {
            try
            {
                Writer.Flush();
                Writer.Close();
                Writer.Dispose();
            }
            catch
            {
                // Suppress I/O errors during shutdown
            }
        }
    }
}
