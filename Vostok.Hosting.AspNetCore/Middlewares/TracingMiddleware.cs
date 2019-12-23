﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Models;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware that creates <see cref="WellKnownSpanKinds.HttpRequest.Server"/> span.
    /// </summary>
    internal class TracingMiddleware : IMiddleware
    {
        private readonly TracingMiddlewareSettings settings;

        public TracingMiddleware(TracingMiddlewareSettings settings)
        {
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();
            
            using (var spanBuilder = settings.Tracer.BeginHttpServerSpan())
            {
                spanBuilder.SetClientDetails(requestInfo.ClientApplicationIdentity, requestInfo.ClientIpAddress);

                if (Uri.TryCreate(context.Request.Path, UriKind.Relative, out var url))
                    spanBuilder.SetRequestDetails(url, context.Request.Method, context.Request.ContentLength);
                
                if (settings.ResponseTraceIdHeader != null)
                {
                    var traceId = spanBuilder.CurrentSpan.TraceId;

                    context.Response.OnStarting(state =>
                    {
                        var ctx = (HttpContext)state;
                        ctx.Response.Headers[settings.ResponseTraceIdHeader] = traceId.ToString();
                        return Task.CompletedTask;
                    }, context);
                }

                await next(context).ConfigureAwait(false);

                spanBuilder.SetResponseDetails(context.Response.StatusCode, context.Response.ContentLength);
            }
        }
    }
}