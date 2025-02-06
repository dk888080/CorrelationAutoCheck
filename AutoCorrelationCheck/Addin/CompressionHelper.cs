using System.IO;
using System.IO.Compression;

namespace ClothoSharedItems
{
    public static class CompressionHelper
    {
        public static void Compress(string inputFilePath, string outputFilePath)
        {
            using (var inputFile = File.OpenRead(inputFilePath))
            using (var outputFile = File.Create(outputFilePath))
            using (var gzip = new GZipStream(outputFile, CompressionMode.Compress))
            {
                inputFile.CopyTo(gzip);
            }
        }

        public static void Decompress(string inputFilePath, string outputFilePath)
        {
            using (var inputFile = File.OpenRead(inputFilePath))
            using (var outputFile = File.Create(outputFilePath))
            using (var gzip = new GZipStream(inputFile, CompressionMode.Decompress))
            {
                gzip.CopyTo(outputFile);
            }
        }
    }
}