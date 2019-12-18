using DecodificadorCaptcha.Business;
using DecodificadorCaptcha.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DecodificadorCaptcha.Controllers
{
    [RoutePrefix("api/index")]
    public class IndexController : ApiController
    {
        [HttpPost]
        [Route("decodificar")]
        public HttpResponseMessage Decodificar(ModelBase64 Model)
        {
            try
            {
                if (ModelState.IsValid && !string.IsNullOrEmpty(Model.Base64))
                    return Request.CreateResponse(HttpStatusCode.OK, new DecodificadorBusiness(Model.Base64).Init());
                else
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["MensagemErro"]);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message.ToString());
            }
        }
    }
}
