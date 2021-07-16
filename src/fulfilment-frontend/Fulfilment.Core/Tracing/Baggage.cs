using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Fulfilment.Core.Tracing
{
    /// <summary>
    /// HACK - this manually sets baggage in the Activity and HTTP header
    /// The library currently uses Correlation-Context as the header key
    /// </summary>
    /// <see cref="https://github.com/w3c/baggage/issues/13"/>
    public static class Baggage
    {
        public static void AddFromIncoming(IHeaderDictionary headers)
        {
            if (Activity.Current == null)
            {
                return;
            }
            if (headers.ContainsKey("baggage"))
            {
                foreach (var value in headers["baggage"])
                {
                    var parts = value.Trim().Split('=');
                    Activity.Current.AddBaggage(parts[0], parts[1]);
                }
            }
        }

        public static void AddToOutgoing(HttpRequestHeaders headers)
        {            
            if (Activity.Current == null)
            {
                return;
            }
            var value = string.Empty;
            foreach(var item in Activity.Current.Baggage)
            {
                value += $"{item.Key.Trim()}={item.Value.Trim()},";
            }
            if (value.Length > 0)
            {
                headers.Add("baggage", value.Trim(','));
                headers.Add("traceparent", Activity.Current.Id);
            }
        }
    }
}
