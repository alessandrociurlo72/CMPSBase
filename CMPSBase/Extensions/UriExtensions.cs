
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FrmNetCore.Extensions
{
    public static class UriExtensions
    {
        private static readonly Regex Regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");


        public static IReadOnlyDictionary<string, string> ParseQueryStringEx(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                var match = Regex.Match(uri.PathAndQuery);
                var paramaters = new Dictionary<string, string>();
                while (match.Success)
                {
                    paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                    match = match.NextMatch();
                }
                return paramaters;
            }
            else
            {
                int idx = uri.ToString().IndexOf('?');
                string query = idx >= 0 ? uri.ToString().Substring(idx) : "";
                NameValueCollection nvc = HttpUtility.ParseQueryString(query);
                return nvc.Cast<string>().ToDictionary(s => s, s => nvc[s]);
            }
        }

        public static string GetQueryValue(this Uri uri, string key)
        {
            try
            {
                string value;
                IReadOnlyDictionary<string, string> dict = uri.ParseQueryStringEx();
                if (dict.TryGetValue(key, out value))
                    return value;
                else
                    return String.Empty;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string GetLastComponent(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
                return uri.Segments.Last().TrimEnd('/');
            else
                return uri.ToString().Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).LastOrDefault();
        }

        public static string RemoveLastComponent(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                var noLastSegment = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
                for (int i = 0; i < uri.Segments.Length - 1; i++)
                    noLastSegment += uri.Segments[i];

                return noLastSegment.Trim("/".ToCharArray());
            }
            else
            {
                if (uri.ToString().Contains('/'))
                    return uri.ToString().Remove(uri.ToString().LastIndexOf('/'));
                else
                    return String.Empty;


            }
        }
        public static string GetLeftPartFromComponent(this Uri uri, string component)
        {
            try
            {
                int idx = uri.ToString().IndexOf(component);
                return uri.ToString().Substring(0, idx);

            }
            catch (Exception)
            {
                return String.Empty;
            }

        }

        public static Uri AddParameter(this Uri uri, string key, string value)
        {
            try
            {
                if (uri.IsAbsoluteUri)
                {
                    UriBuilder builder = new UriBuilder(uri);
                    var query = HttpUtility.ParseQueryString(builder.Query);
                    query[key] = value;
                    builder.Query = query.ToString();
                    return builder.Uri;
                }
                else
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add(key, value);
                    return uri.ExtendQuery(dict);


                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Uri AddParameter(this Uri uri, string key, bool value)
        {
            return uri.AddParameter(key, value.ToString());
        }

        public static Uri AddParameter(this Uri uri, string key, int value)
        {
            return uri.AddParameter(key, value.ToString());
        }

        public static Uri ExtendQuery(this Uri uri, IDictionary<string, string> values)
        {
            var baseUrl = uri.ToString();
            var queryString = string.Empty;
            if (baseUrl.Contains("?"))
            {
                var urlSplit = baseUrl.Split('?');
                baseUrl = urlSplit[0];
                queryString = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
            }

            NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
            foreach (var kvp in values ?? new Dictionary<string, string>())
            {
                queryCollection[kvp.Key] = kvp.Value;
            }
            var uriKind = uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;
            return queryCollection.Count == 0
              ? new Uri(baseUrl, uriKind)
              : new Uri(string.Format("{0}?{1}", baseUrl, queryCollection), uriKind);
        }

        public static string RemoveQueryString(this Uri uri)
        {
            if (uri.IsAbsoluteUri)
                return uri.GetLeftPart(UriPartial.Path);
            else
                return uri.ToString().Split('?')[0];
        }

        public static Uri AppendUriTralingBackSlash(this Uri uri)
        {
            Uri result;
            string struri = uri.ToString();
            if (String.IsNullOrWhiteSpace(struri))
                return null;

            struri.Trim();
            if (struri.Last() != '/') //to have one and only one backslash at the end of uri
                struri += '/';

            if (Uri.TryCreate(struri, UriKind.RelativeOrAbsolute, out result))
                return result;

            return null;
        }



    }
}
