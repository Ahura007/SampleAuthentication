using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SampleAuthentication.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (ValidationException exception)
            {
                await ConfigureResponse(context, HttpStatusCode.BadRequest, exception.Message);
            }
            catch (KeyNotFoundException exception)
            {
                await ConfigureResponse(context, HttpStatusCode.NotFound, exception.Message);
            }
            catch (ArgumentException exception)
            {
                await ConfigureResponse(context, HttpStatusCode.BadRequest, exception.Message);
            }
            catch (Exception exception)
            {
                await ConfigureResponse(context, HttpStatusCode.InternalServerError, exception.Message);
            }
        }


        private static async Task ConfigureResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int) statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(new ResponseMessage(message).ToString());
        }
    }


    public class ResponseMessage
    {
        public ResponseMessage()
        {
        }

        public ResponseMessage(string message)
        {
            this.message = message;
        }

        public ResponseMessage(string message, object content)
        {
            this.message = message;
            this.content = content;
        }

        public string message { get; set; }

        public object content { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}