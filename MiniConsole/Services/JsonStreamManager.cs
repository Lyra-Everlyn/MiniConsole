using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniConsole.Models;
using MiniConsole.Views;

namespace MiniConsole.Services
{
    public class JsonStreamManager : IDisposable
    {
        private readonly Dictionary<string, JsonStreamState> _activeStreams = new(StringComparer.OrdinalIgnoreCase);

        public long TotalWrittenAcrossStreams
        {
            get
            {
                long total = 0;
                foreach (var state in _activeStreams.Values)
                {
                    total += state.WrittenCount;
                }
                return total;
            }
        }

        public void OpenStream(string fileKey, string fullPath)
        {
            try
            {
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (_activeStreams.ContainsKey(fileKey))
                {
                    CloseStream(fileKey);
                }

                var writer = new StreamWriter(fullPath, false, new UTF8Encoding(false), 65536)
                {
                    AutoFlush = true
                };
                writer.WriteLine("[");

                _activeStreams[fileKey] = new JsonStreamState(writer, fullPath);
                ConsoleRenderer.PrintSystemEvent($"[JSON Engine] Opened write stream [{fileKey}] -> {fullPath}");
            }
            catch (Exception ex)
            {
                ConsoleRenderer.PrintError($"Cannot create file '{fullPath}': {ex.Message}");
            }
        }

        public void WriteBatch(string fileKey, string batchPayload)
        {
            if (!_activeStreams.TryGetValue(fileKey, out var state))
            {
                return;
            }

            string[] items = batchPayload.Split("~~", StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in items)
            {
                string trimmed = item.Trim();
                if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
                {
                    if (!state.IsFirstItem)
                    {
                        state.Writer.WriteLine(",");
                    }

                    state.Writer.Write("  " + trimmed);
                    state.IsFirstItem = false;
                    state.WrittenCount++;
                }
            }
        }

        public void FlushAll()
        {
            foreach (var state in _activeStreams.Values)
            {
                state.Writer.Flush();
            }
        }

        public void CloseStream(string fileKey)
        {
            if (_activeStreams.TryGetValue(fileKey, out var state))
            {
                state.Writer.WriteLine();
                state.Writer.WriteLine("]");
                state.Dispose();
                _activeStreams.Remove(fileKey);

                ConsoleRenderer.PrintSystemEvent(
                    $"[JSON Engine] Closed stream [{fileKey}]. Total records written: {state.WrittenCount:N0}.",
                    ConsoleColor.Green);
            }
        }

        public void CloseAllStreams()
        {
            foreach (var key in new List<string>(_activeStreams.Keys))
            {
                CloseStream(key);
            }
        }

        public void Dispose()
        {
            CloseAllStreams();
        }
    }
}
