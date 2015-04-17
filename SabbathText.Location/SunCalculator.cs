namespace SabbathText.Location
{
    using System;

    /// <summary>
    /// This class provides a way to calculate Sun times.
    /// This code is ported over from
    /// <c>http://www.esrl.noaa.gov/gmd/grad/solcalc/</c>
    /// </summary>
    public class SunCalculator
    {
        /// <summary>
        /// Calculates the Sunset for a specific day, at a specific location.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The Sun set time in UTC.</returns>
        public static DateTime CalculateSunSetUtc(int year, int month, int day, double latitude, double longitude)
        {
            double julianDay = CalcJulianDay(year, month, day);
            double sunsetUtc = CalcSunSetUtc(julianDay, latitude, longitude);

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc).AddMinutes(sunsetUtc);
        }

        private static double CalcJulianDay(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            double a = Math.Floor(year / 100.0);
            double b = 2 - a + Math.Floor(a / 4);

            return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;
        }

        /// <summary>
        /// Calculates the UTC of sunset for a given day at the given location on earth.
        /// </summary>
        /// <param name="julianDay">Julian da.y</param>
        /// <param name="latitude">Latitude of observer in degrees.</param>
        /// <param name="longitude">Longitude of observer in degrees.</param>
        /// <returns>Time in minutes from zero Z.</returns>
        private static double CalcSunSetUtc(double julianDay, double latitude, double longitude)
        {
            double julianCentury = CalcJulianCent(julianDay);
            double equationOfTime = CalcEquationOfTime(julianCentury);
            double solarDec = CalcSunDeclination(julianCentury);
            double hourAngle = CalcHourAngleSunRise(latitude, solarDec);
            hourAngle = -hourAngle;
            double delta = longitude + RadToDeg(hourAngle);
            double timeUTC = 720 - (4.0 * delta) - equationOfTime; // in minutes
            return timeUTC;
        }

        /// <summary>
        /// Converts radian angle to degrees.
        /// </summary>
        /// <param name="angleRad">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        private static double RadToDeg(double angleRad)
        {
            return 180.0 * angleRad / Math.PI;
        }

        /// <summary>
        /// Converts degree angle to radians.
        /// </summary>
        /// <param name="angleDeg">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        private static double DegToRad(double angleDeg)
        {
            return Math.PI * angleDeg / 180.0;
        }

        /// <summary>
        /// Calculates the hour angle of the sun set sunrise for the latitude.
        /// </summary>
        /// <param name="latitude">Latitude of observer in degrees.</param>
        /// <param name="solarDec">Declination angle of the Sun in degrees.</param>
        /// <returns>The hour angle of Sun rise in radians.</returns>
        private static double CalcHourAngleSunRise(double latitude, double solarDec)
        {
            double latRad = DegToRad(latitude);
            double solarDecRad = DegToRad(solarDec);

            double hourAngle = Math.Acos(
                (Math.Cos(DegToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(solarDecRad))) -
                (Math.Tan(latRad) * Math.Tan(solarDecRad)));

            return hourAngle;  // in radians
        }

        /// <summary>
        /// Converts Julian day to centuries since J2000.0.
        /// </summary>
        /// <param name="julianDay">The Julian day.</param>
        /// <returns>The T value according to the Julian day.</returns>
        private static double CalcJulianCent(double julianDay)
        {
            return (julianDay - 2451545.0) / 36525.0;
        }

        /// <summary>
        /// Calculates the corrected obliquity of the ecliptic.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The corrected obliquity in degrees.</returns>
        private static double CalcObliquityCorrection(double julianCentury)
        {
            double e0 = CalcMeanObliquityOfEcliptic(julianCentury);

            double omega = 125.04 - (1934.136 * julianCentury);
            double e = e0 + (0.00256 * Math.Cos(DegToRad(omega)));
            return e;  // in degrees
        }

        /// <summary>
        /// Calculates the mean obliquity of the ecliptic.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The mean obliquity in degrees.</returns>
        private static double CalcMeanObliquityOfEcliptic(double julianCentury)
        {
            double seconds = 21.448 - (julianCentury * (46.8150 + (julianCentury * (0.00059 - (julianCentury * 0.001813)))));
            double e0 = 23.0 + ((26.0 + (seconds / 60.0)) / 60.0);
            return e0;  // in degrees
        }

        /// <summary>
        /// Calculates the declination of the sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The Sun's declination in degrees.</returns>
        private static double CalcSunDeclination(double julianCentury)
        {
            double e = CalcObliquityCorrection(julianCentury);
            double lambda = CalcSunApparentLong(julianCentury);

            double sint = Math.Sin(DegToRad(e)) * Math.Sin(DegToRad(lambda));
            double theta = RadToDeg(Math.Asin(sint));
            return theta;  // in degrees
        }

        /// <summary>
        /// Calculates the apparent longitude of the Sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The Sun's apparent longitude in degrees.</returns>
        private static double CalcSunApparentLong(double julianCentury)
        {
            double o = CalcSunTrueLong(julianCentury);

            double omega = 125.04 - (1934.136 * julianCentury);
            double lambda = (o - 0.00569) - (0.00478 * Math.Sin(DegToRad(omega)));
            return lambda;  // in degrees
        }

        /// <summary>
        /// Calculates the geometric mean longitude of the Sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The geometric mean longitude of the Sun in degrees.</returns>
        private static double CalcSunGeomMeanLong(double julianCentury)
        {
            double lng = 280.46646 + (julianCentury * (36000.76983 + (0.0003032 * julianCentury)));
            while (lng > 360.0)
            {
                lng -= 360.0;
            }

            while (lng < 0.0)
            {
                lng += 360.0;
            }

            return lng;  // in degrees
        }

        /// <summary>
        /// Calculates the equation of center for the Sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The equation of center for the Sun in degrees.</returns>
        private static double CalcSunEqOfCenter(double julianCentury)
        {
            double m = CalcSunGeomMeanAnomaly(julianCentury);

            double mrad = DegToRad(m);
            double sinm = Math.Sin(mrad);
            double sin2m = Math.Sin(mrad + mrad);
            double sin3m = Math.Sin(mrad + mrad + mrad);

            return // in degrees
                (sinm * (1.914602 - (julianCentury * (0.004817 + (0.000014 * julianCentury))))) +
                (sin2m * (0.019993 - (0.000101 * julianCentury))) +
                (sin3m * 0.000289);
        }

        /// <summary>
        /// Calculates the geometric mean anomaly of the Sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The geometric mean anomaly of the Sun in degrees.</returns>
        private static double CalcSunGeomMeanAnomaly(double julianCentury)
        {
            // in degrees
            return 357.52911 + (julianCentury * (35999.05029 - (0.0001537 * julianCentury)));
        }

        /// <summary>
        /// Calculates the true longitude of the Sun.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The Sun's true longitude in degrees.</returns>
        private static double CalcSunTrueLong(double julianCentury)
        {
            double meanLongitude = CalcSunGeomMeanLong(julianCentury);
            double center = CalcSunEqOfCenter(julianCentury);

            return meanLongitude + center; // in degrees
        }

        /// <summary>
        /// Calculates the difference between true solar time and mean solar time.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The equation of time in minutes of time.</returns>
        private static double CalcEquationOfTime(double julianCentury)
        {
            double epsilon = CalcObliquityCorrection(julianCentury);
            double l0 = CalcSunGeomMeanLong(julianCentury);
            double e = CalcEccentricityEarthOrbit(julianCentury);
            double m = CalcSunGeomMeanAnomaly(julianCentury);

            double y = Math.Tan(DegToRad(epsilon) / 2.0);
            y *= y;

            double sin2l0 = Math.Sin(2.0 * DegToRad(l0));
            double sinm = Math.Sin(DegToRad(m));
            double cos2l0 = Math.Cos(2.0 * DegToRad(l0));
            double sin4l0 = Math.Sin(4.0 * DegToRad(l0));
            double sin2m = Math.Sin(2.0 * DegToRad(m));

            double etime = (y * sin2l0) - (2.0 * e * sinm) + (4.0 * e * y * sinm * cos2l0) - (0.5 * y * y * sin4l0) - (1.25 * e * e * sin2m);

            return RadToDeg(etime) * 4.0; // in minutes of time
        }

        /// <summary>
        /// Calculates the eccentricity of earth's orbit.
        /// </summary>
        /// <param name="julianCentury">The T value (number of Julian centuries since J2000.0).</param>
        /// <returns>The unit-less eccentricity.</returns>
        private static double CalcEccentricityEarthOrbit(double julianCentury)
        {
            return 0.016708634 - (julianCentury * (0.000042037 + (0.0000001267 * julianCentury)));
        }
    }
}
