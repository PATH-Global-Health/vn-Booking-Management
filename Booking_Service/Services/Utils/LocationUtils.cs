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
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            return provinces.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }

        public static string GetDistrictLabel(this string value)
        {
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            var districts = provinces.SelectMany(p => p.districts);
            return districts.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }

        public static string GetWardLabel(this string value)
        {
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            var districts = provinces.SelectMany(p => p.districts);
            var wards = districts.SelectMany(d => d.wards);
            return wards.Where(_ => _.value == value).Select(_ => _.label).FirstOrDefault();
        }
        //

        public static string GetProvinceValue(this string label)
        {
            label = label.Trim();
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            return provinces.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }

        public static string GetDistrictValue(this string label)
        {
            label = label.Trim();
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            var districts = provinces.SelectMany(p => p.districts);
            return districts.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }

        public static string GetWardValue(this string label)
        {
            label = label.Trim();
            var embeded = Properties.Resources.locations;
            var ros = new ReadOnlySpan<byte>(embeded);
            var provinces = JsonSerializer.Deserialize<List<Province>>(ros);
            var districts = provinces.SelectMany(p => p.districts);
            var wards = districts.SelectMany(d => d.wards);
            return wards.Where(_ => _.label.Equals(label, StringComparison.OrdinalIgnoreCase)).Select(_ => _.value).FirstOrDefault();
        }
    }
}
