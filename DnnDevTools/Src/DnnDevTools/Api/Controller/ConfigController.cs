﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using weweave.DnnDevTools.Dto;
using weweave.DnnDevTools.Service;

namespace weweave.DnnDevTools.Api.Controller
{
    [ValidateAntiForgeryToken]
    [SuperUserAuthorize]
    public class ConfigController : DnnApiController
    {

        private static ServiceLocator ServiceLocator => ServiceLocatorFactory.Instance;

        [HttpPut]
        public HttpResponseMessage Enable(bool status)
        {
            ServiceLocator.ConfigService.SetEnable(status);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut]
        public HttpResponseMessage EnableMailCatch(bool status)
        {
            if (status && !ServiceLocator.ConfigService.GetEnable())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT_ENABLED");
            }

            ServiceLocator.ConfigService.SetEnableMailCatch(status);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public HttpResponseMessage List()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new Config()
            {
                Enable = ServiceLocator.ConfigService.GetEnable(),
                EnableMailCatch = ServiceLocator.ConfigService.GetEnableMailCatch()
            });
        }
    }
}
