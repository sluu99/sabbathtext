using SabbathText.Core.Entities;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace SabbathText.Core
{
    public class ZipSunsetLocationProvider : ILocationProvider
    {
        const string UrlFormat = "http://zipsunset.azurewebsites.net/sunset/{0}/{1:MMddyy}";

        public async Task<Location> GetLocationByZipCode(string zipCode)
        {
            XmlNode rootNode = await this.GetLocationData(zipCode, DateTime.UtcNow);

            if (rootNode == null)
            {
                return null;
            }

            return new Location
            {
                City = rootNode.SelectNodeInnerText("city", string.Empty),
                State = rootNode.SelectNodeInnerText("state", string.Empty),
                Country = rootNode.SelectNodeInnerText("country", string.Empty),
                Latitude = double.Parse(rootNode.SelectNodeInnerText("lat", "0"), CultureInfo.InvariantCulture),
                Longitude = double.Parse(rootNode.SelectNodeInnerText("long", "0"), CultureInfo.InvariantCulture),
                TimeZoneName = rootNode.SelectNodeInnerText("timezone", string.Empty),
                TimeZoneOffset = double.Parse(rootNode.SelectNodeInnerText("offset", "0"), CultureInfo.InvariantCulture),
                ZipCode = zipCode,                
            };
        }

        public async Task<LocationTimeInfo> GetTimeInfoByZipCode(string zipCode, DateTime date)
        {
            XmlNode data = await this.GetLocationData(zipCode, date.Date);

            if (data == null)
            {
                return null;
            }

            return new LocationTimeInfo
            {
                ZipCode = zipCode,
                InfoDate = date.Date,
                Sunrise = DateTime.Parse(data.SelectNodeInnerText("sunrise", string.Empty)).ToUniversalTime(),
                Sunset = DateTime.Parse(data.SelectNodeInnerText("sunset", string.Empty)).ToUniversalTime(),
            };
        }

        private async Task<XmlNode> GetLocationData(string zipCode, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                throw new ArgumentException("zipCode cannot be null or white space");
            }

            string url = string.Format(UrlFormat, zipCode, date);

            HttpWebRequest request = WebRequest.CreateHttp(url);

            WebResponse response = null;

            try
            {
                response = await request.GetResponseAsync();
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            XmlDocument document = new XmlDocument();

            using (Stream responseStream = response.GetResponseStream())
            {
                document.Load(responseStream);
            }

            return document.SelectSingleNode("/xml");
        }
    }
}
