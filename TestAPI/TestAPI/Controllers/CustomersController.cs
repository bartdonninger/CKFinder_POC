﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TestAPI.Controllers
{
    public class CustomersController : ApiController
    {

        public IHttpActionResult Get()
        {
            return Ok<string>("Dit is een bericht");
        }

    }
}
