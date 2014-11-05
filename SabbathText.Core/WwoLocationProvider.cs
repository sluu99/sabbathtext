using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SabbathText.Core
{
    /// <summary>
    /// LocationProvider implementation using WorldWeatherOnline.com API
    /// </summary>
    public class WwoLocationProvider : ILocationProvider
    {
        private string accessKey = null;

        public WwoLocationProvider()
        {
            this.accessKey = Environment.GetEnvironmentVariable("ST_WWO_ACCESS_KEY");
        }


        public async Task<Entities.Location> GetLocationByZipCode(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                throw new ArgumentException("zipCode cannot be null or white space");
            }

            string url = "http://api.worldweatheronline.com/free/v2/search.ashx?key={0}&query={1}&timezone=yes&format=xml";
            url = string.Format(url, this.accessKey, System.Uri.EscapeDataString(zipCode));

            HttpWebRequest request = WebRequest.CreateHttp(url);
            WebResponse response = await request.GetResponseAsync();

            XmlDocument document = new XmlDocument();

            using (Stream responseStream = response.GetResponseStream())
            {
                /*if (responseStream.Length == 0)
                {
                    return null;
                }*/

                document.Load(responseStream);
            }

            XmlNodeList resultNodes = document.SelectNodes("/search_api/result");

            foreach (XmlNode node in resultNodes)
            {
                XmlNode countryNode = node.SelectSingleNode("country");

                if (countryNode == null)
                {
                    continue;
                }

                string country = countryNode.InnerText.Trim();
                if (!"United States of America".Equals(country, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                return new Location
                {
                    LocationName = SelectNodeInnerText(node, "areaName", "UNKNOWN"),
                    Country = country,
                    Region = SelectNodeInnerText(node, "region", "UNKNOWN"),
                    ZipCode = zipCode,
                    Latitude = decimal.Parse(SelectNodeInnerText(node, "latitude", "0")),
                    Longitude = decimal.Parse(SelectNodeInnerText(node, "longitude", "0")),
                    TimeZoneOffset = decimal.Parse(SelectNodeInnerText(node, "timezone/offset", "0")),
                };
            }

            return null;
        }

        private static string SelectNodeInnerText(XmlNode node, string xpath, string defaultValue)
        {
            XmlNode innerNode = node.SelectSingleNode(xpath);
            if (innerNode == null)
            {
                return defaultValue;
            }

            return innerNode.InnerText.Trim();
        }
    }
}
