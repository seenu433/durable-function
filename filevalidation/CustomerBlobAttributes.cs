using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace filevalidation
{
    public class CustomerBlobAttributes
    {
        static readonly Regex blobUrlRegexExtract = new Regex(@"^\S*/([^/]+)/orders/([^_]+)_([\w]+)\.csv$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        public string FullUrl { get; private set; }
        public string OrderId { get; private set; }
        public string TransactionType { get; private set; }
        public string ContainerName { get; private set; }

        public static CustomerBlobAttributes Parse(string fullUri, bool detectSubfolder = false)
        {
            var regexMatch = blobUrlRegexExtract.Match(fullUri);
            if (regexMatch.Success)
            {
                return new CustomerBlobAttributes
                {
                    FullUrl = regexMatch.Groups[0].Value,
                    ContainerName = regexMatch.Groups[1].Value,
                    OrderId = regexMatch.Groups[2].Value,
                    TransactionType = regexMatch.Groups[3].Value
                };
            }

            return null;
        }
    }
}
