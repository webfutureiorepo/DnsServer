﻿/*
Technitium DNS Server
Copyright (C) 2024  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LogExporter.Strategy
{
    public class HttpExportStrategy : IExportStrategy
    {
        #region variables

        private readonly string _endpoint;

        private readonly Dictionary<string, string>? _headers;

        private readonly HttpClient _httpClient;

        private readonly string _method;

        private bool disposedValue;

        #endregion variables

        #region constructor

        public HttpExportStrategy(string endpoint, string method, Dictionary<string, string>? headers)
        {
            _endpoint = endpoint;
            _method = method;
            _headers = headers;
            _httpClient = new HttpClient();
        }

        #endregion constructor

        #region public

        public async Task ExportAsync(List<LogEntry> logs)
        {
            var buffer = new GrowableBuffer<char>();
            foreach (var log in logs)
            {
                buffer.Append(log.AsSpan());
                buffer.Append('\n');
            }
            var content = buffer.ToString() ?? string.Empty;
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(_endpoint),
                Method = new HttpMethod(_method),
                Content = new StringContent( content, Encoding.UTF8, "application/json")
            };

            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to export logs to {_endpoint}: {response.StatusCode}");
            }
        }

        #endregion public

        #region IDisposable

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable
    }
}