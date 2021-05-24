using Data.MongoCollections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Utils
{
    public static class Utils
    {
        public static string GetUnitUsernameForExamination(this string originalUsername)
        {
            var newUsername = originalUsername;
            //var newUsername = originalUsername.Replace("hcdc.", "");
            if (originalUsername.Contains("hcdc.hcdc."))
                newUsername = originalUsername.Substring(5);

            return newUsername;
        }

        public static string GetFullAddress(this Customer customer)
        {
            string result = null;
            if (customer != null)
            {
                result = customer.Address;
                if (!string.IsNullOrEmpty(customer.WardCode))
                    result += $", {customer.WardCode.GetWardLabel()}";
                if (!string.IsNullOrEmpty(customer.DistrictCode))
                    result += $", {customer.DistrictCode.GetDistrictLabel()}";
                if (!string.IsNullOrEmpty(customer.ProvinceCode))
                    result += $", {customer.ProvinceCode.GetProvinceLabel()}";
            }
            return result;
        }
    }
}
