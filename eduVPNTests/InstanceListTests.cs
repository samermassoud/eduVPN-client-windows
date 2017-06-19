﻿/*
    eduVPN - End-user friendly VPN

    Copyright: 2017, The Commons Conservancy eduVPN Programme
    SPDX-License-Identifier: GPL-3.0+
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace eduVPN.Tests
{
    [TestClass()]
    public class InstanceListTests
    {
        [TestMethod()]
        public void InstanceListTest()
        {
            // .NET 3.5 allows Schannel to use SSL 3 and TLS 1.0 by default. Instead of hacking user computer's registry, extend it in runtime.
            // System.Net.SecurityProtocolType lacks appropriate constants prior to .NET 4.5.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)0x0C00;

            // Load list of instances.
            var instances = new InstanceList();
            instances.Load(new Uri("https://static.eduvpn.nl/instances.json"), Convert.FromBase64String("E5On0JTtyUVZmcWd+I/FXRm32nSq8R2ioyW7dcu/U88="));

            // Load all instance API(s) in parallel.
            Task.WhenAll(instances.Select(i => API.LoadAsync(i.Base))).Wait();
        }

#if PLATFORM_AnyCPU
        private static bool is_resolver_active = MultiplatformDllLoader.Enable = true;
#endif
    }
}