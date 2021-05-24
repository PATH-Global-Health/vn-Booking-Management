using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Services.Utils
{
    public static class LocationUtils
    {
        public static string GetProvinceLabel(this string value)
        {
            return Locations.Provinces.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }

        public static string GetDistrictLabel(this string value)
        {
            return Locations.Districts.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }

        public static string GetWardLabel(this string value)
        {
            return Locations.Wards.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }
        //

        public static string GetProvinceValue(this string label)
        {
            label = label.Trim();
            return Locations.Provinces.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }

        public static string GetDistrictValue(this string label)
        {
            label = label.Trim();
            return Locations.Districts.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }

        public static string GetWardValue(this string label)
        {
            label = label.Trim();
            return Locations.Wards.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }
    }

    public class Locations
    {
        public static readonly ICollection<Province> Provinces;
        public static readonly ICollection<District> Districts;
        public static readonly ICollection<Ward> Wards;

        static Locations()
        {
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            Provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            Districts = Provinces.SelectMany(p => p.districts).ToList();
            Wards = Districts.SelectMany(p => p.wards).ToList();
        }
    }
}
