using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WCFServiceWebRole1.Classes
{
    class ParserFile
    {
        public string FileName { get; private set; }

        public string FileType { get; private set; }

        public Boolean Success { get; private set; }

        public byte[] FileContents { get; private set; }

        public void Parse(Stream stream)
        {
            Encoding encoding = Encoding.UTF8;

            // Convert stream to byte array
            byte[] data = StreamToByteArray(stream);

            // Convert byte array to string
            string content = encoding.GetString(data);

            // Get first line delimiter
            int delimitIndex = content.IndexOf("\r\n");

            if (delimitIndex > -1)
            {
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                // Get Content-Type
                Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                Match contentTypeMatch = re.Match(content);

                // Get filename
                re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                Match filenameMatch = re.Match(content);

                // If all regex matchs
                if (contentTypeMatch.Success && filenameMatch.Success)
                {
                    // Set properties fileName and fileType
                    this.FileType = contentTypeMatch.Value.Trim();
                    this.FileName = filenameMatch.Value.Trim();

                    // Get the start and the end indexes contents file
                    int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                    byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                    int endIndex = IndexOf(data, delimiterBytes, startIndex);

                    int contentLength = endIndex - startIndex;

                    // Extract the file contents from the byte array
                    byte[] fileData = new byte[contentLength];

                    Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                    this.FileContents = fileData;

                    this.Success = true;
                }
            }
            
        }

        private int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }

        private byte[] StreamToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}
