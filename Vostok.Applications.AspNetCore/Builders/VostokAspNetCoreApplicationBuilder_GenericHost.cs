﻿#if NETCOREAPP && !NET6_0
using System;
using Microsoft.Extensions.Hosting;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.Builders
{
    internal partial class VostokAspNetCoreApplicationBuilder
    {
        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
            => Setup(() => hostFactory.SetupHost(setup));

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
            => Setup(webHostBuilder.Disable);
    }
}
#endif