﻿// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BitcoinLib.RPC.RequestResponse
{
    public class JsonRpcRequestArray
    {
        public JsonRpcRequestArray(int id, string method, params string[] parameters)
        {
            Id = id;
            Method = method;
            jsonrpc = "2.0";
            Parameters = parameters;
        }

        [JsonProperty(PropertyName = "method", Order = 0)]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params", Order = 1)]
        public string[] Parameters { get; set; }

        [JsonProperty(PropertyName = "id", Order = 2)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "jsonrpc", Order = 3)]
        public string jsonrpc { get; set; }

        public byte[] GetBytes()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}