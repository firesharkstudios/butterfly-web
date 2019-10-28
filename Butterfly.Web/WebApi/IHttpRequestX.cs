/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Butterfly.Util;

namespace Butterfly.Web.WebApi {
    public static class IHttpRequestX {
        public static async Task<Dictionary<string, string>> ParseAsUrlEncodedAsync(this IHttpRequest me) {
            string text = await me.ReadAsTextAsync();
            return HttpUtility.ParseQueryString(text).ToDictionary();
        }

        public static async Task<T> ParseAsJsonAsync<T>(this IHttpRequest me) {
            string json = await me.ReadAsTextAsync();
            return JsonUtil.Deserialize<T>(json);
        }

        public static Dictionary<string, string> GetCookies(this IHttpRequest me) {
            var text = me.Headers.GetAs("COOKIE", "");
            if (string.IsNullOrWhiteSpace(text)) return new Dictionary<string, string>();
            return text.Split(";").Select(x => x.Split('=')).ToDictionary(x => x[0].Trim(), x => x.Length>1 ? x[1].Trim() : null);
        }
    }
}
