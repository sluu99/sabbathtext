namespace SabbathText.Location.V1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A stream reader which will puts the seek position to the right place after each line read.
    /// </summary>
    internal class LineReader
    {
        private Stream stream;
        private Encoding encoding;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <param name="encoding">The encoding.</param>
        public LineReader(Stream s, Encoding encoding)
        {
            this.stream = s;
            this.encoding = encoding;
        }

        /// <summary>
        /// Read a line from the file.
        /// </summary>
        /// <returns>The line, or null.</returns>
        public string ReadLine()
        {
            List<byte> bytes = new List<byte>();
            int totalBytesUsed = 0;
            long totalBytesRead = 0;

            bool lineStarted = false;
            bool lineEnded = false;

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = this.stream.Read(buffer, 0, 1024);
                totalBytesRead += bytesRead;

                if (bytesRead == 0)
                {
                    break;
                }
                
                // go through each byte read
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == '\r' || buffer[i] == '\n')
                    {
                        if (lineStarted)
                        {
                            // this is the second time we see these characters
                            lineEnded = true;
                        }
                    }
                    else
                    {
                        lineStarted = true;

                        if (lineEnded)
                        {
                            // we ended the last line, read the first character of the next line
                            break;
                        }

                        bytes.Add(buffer[i]);
                    }

                    totalBytesUsed++;
                }

                if (lineEnded)
                {
                    // found the end of a line                    
                    break;
                }
            }

            this.stream.Seek(totalBytesUsed - totalBytesRead, SeekOrigin.Current);

            return this.encoding.GetString(bytes.ToArray());
        }
    }
}
