namespace SabbathText.Location.V1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;

    /// <summary>
    /// This class provides the raw location related data.
    /// </summary>
    internal static class RawData
    {
        private static readonly string ResourceStreamName = typeof(RawData).Namespace + ".ZipCodeDatabase.csv";

        /// <summary>
        /// A list of data headers in order.
        /// </summary>
        private static readonly string[] Headers;

        /// <summary>
        /// A cache map of the ZIP codes and the file seek-position of their data rows.
        /// This cache allows us to quickly jump to a file location instead of having to
        /// scan through each row every time for a ZIP code.
        /// For example:
        /// "00601" : 288
        /// </summary>
        private static readonly Dictionary<string, int> ZipCodeFilePosition;

        /// <summary>
        /// Initializes the static fields
        /// </summary>
        static RawData()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            using (Stream stream = thisAssembly.GetManifestResourceStream(ResourceStreamName))
            {
                LineReader reader = new LineReader(stream, Encoding.UTF8);

                // read the headers
                string header = reader.ReadLine();
                Headers = header.Split(',');

                // index the ZIP locations
                ZipCodeFilePosition = new Dictionary<string, int>(42523 /* there are this many zip codes in the file */);

                while (true)
                {
                    int position = (int)stream.Position; // cast to int, the file is small enough
                    string line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }

                    line = line.Trim();
                    int comma = line.IndexOf(',');
                    string zipCode = line.Substring(0, comma);

                    ZipCodeFilePosition.Add(zipCode, position);
                }
            }
        }

        /// <summary>
        /// Gets data for a specific ZIP code.
        /// </summary>
        /// <param name="zipCode">The ZIP code.</param>
        /// <returns>A dictionary of data with the field names as the keys.</returns>
        public static Dictionary<string, string> GetZipCodeData(string zipCode)
        {
            if (ZipCodeFilePosition.ContainsKey(zipCode) == false)
            {
                return null;
            }

            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            CsvConfiguration csvConfig = new CsvConfiguration { HasHeaderRecord = false };

            using (Stream stream = thisAssembly.GetManifestResourceStream(ResourceStreamName))
            using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
            using (CsvReader csvReader = new CsvReader(streamReader, csvConfig))
            {
                stream.Seek(ZipCodeFilePosition[zipCode], SeekOrigin.Begin);                

                if (csvReader.Read() == false)
                {
                    return null;
                }
                
                Dictionary<string, string> result = new Dictionary<string, string>(Headers.Length);

                for (int i = 0; i < Headers.Length; i++)
                {
                    result[Headers[i]] = csvReader.GetField(i);
                }

                return result;
            }
        }
    }
}
