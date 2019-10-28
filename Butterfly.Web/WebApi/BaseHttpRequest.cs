/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using HttpMultipartParser;

using NLog;

namespace Butterfly.Web.WebApi {
    public abstract class BaseHttpRequest : IHttpRequest {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public abstract Stream InputStream { get; }

        public abstract string ClientIp {
            get;
        }

        public abstract string Method {
            get;
        }

        public abstract Uri RequestUrl { get; }

        public abstract Dictionary<string, string> Headers { get; }

        public abstract Dictionary<string, string> PathParams { get; }

        public abstract Dictionary<string, string> QueryParams { get; }

        public async Task<string> ReadAsTextAsync() {
            using (StreamReader reader = new StreamReader(this.InputStream)) {
                return await reader.ReadToEndAsync();
            }
        }

        public void ParseAsMultipartStream(Action<string, string, string, string, byte[], int> onData, Action<string, string> onParameter = null) {
            var parser = new StreamingMultipartFormDataParser(this.InputStream);
            if (onParameter != null) {
                parser.ParameterHandler += parameter => onParameter(parameter.Name, parameter.Data);
            }
            parser.FileHandler += (name, fileName, type, disposition, buffer, bytes) => onData(name, fileName, type, disposition, buffer, bytes);
            parser.Run();
        }
    }
}
