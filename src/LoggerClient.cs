using AuthEventTrackers.Domains.Entities;
using AuthEventTrackers.Domains.Response;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace AuthEventTrackers
{
    public class LoggerClient
    {
        private readonly string       _aplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME");
        private readonly RabbitEntity _configRabbit   = JsonConvert.DeserializeObject<RabbitEntity>(Environment.GetEnvironmentVariable("MQ_RABBITMQ_GERAL") ?? "");

        private void SetLog(
            string type,
            string category,
            AuthorizationResponse authorization,
            string sourceFilePath   = "",
            string memberName       = "",
            int    sourceLineNumber = 0,
            string tags             = null,
            object request          = null,
            object response         = null,
            object exception        = null,
            object codeNumber       = null)
        {
            var date            = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var memberClassName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var tagSplit        = tags?.Split(",");

            var json = new
            {
                AplicacaoNome   = _aplicationName,
                Type            = type,
                Category        = category,
                Date            = date,
                Source          = new
                {               
                    ClassName   = memberClassName,
                    MethodName  = memberName,
                    LineNumber  = sourceLineNumber
                },              
                Authorization   = authorization,
                Tags            = tagSplit,
                                
                CodeNumber      = codeNumber,
                                
                Request         = request,
                Response        = response,
                Exception       = exception
            };

            RabbitClient.SendMessage(json, _configRabbit.Queue.QueueLoggers);
        }

        public void Info(
            string                    category,
            AuthorizationResponse     authorization,
            [CallerFilePath] string   sourceFilePath   = "",
            [CallerMemberName] string memberName       = "",
            [CallerLineNumber] int    sourceLineNumber = 0,
            string                    tags             = null,
            object                    request          = null,
            object                    response         = null,
            object                    exception        = null,
            object                    codeNumber       = null
        ) => SetLog("INFO",
                    category,
                    authorization,
                    sourceFilePath,
                    memberName,
                    sourceLineNumber,
                    tags,
                    request,
                    response,
                    exception,
                    codeNumber
            );

        public void Success(
            string                    category,
            AuthorizationResponse     authorization,
            [CallerFilePath] string   sourceFilePath   = "",
            [CallerMemberName] string memberName       = "",
            [CallerLineNumber] int    sourceLineNumber = 0,
            string                    tags             = null,
            object                    request          = null,
            object                    response         = null,
            object                    exception        = null,
            object                    codeNumber       = null
        ) => SetLog("SUCCESS",
                    category,
                    authorization,
                    sourceFilePath,
                    memberName,
                    sourceLineNumber,
                    tags,
                    request,
                    response,
                    exception,
                    codeNumber
            );

        public void Error(
            string                    category,
            AuthorizationResponse     authorization,
            [CallerFilePath] string   sourceFilePath   = "",
            [CallerMemberName] string memberName       = "",
            [CallerLineNumber] int    sourceLineNumber = 0,
            string                    tags             = null,
            object                    request          = null,
            object                    response         = null,
            object                    exception        = null,
            object                    codeNumber       = null
        ) => SetLog("ERROR",
                    category,
                    authorization,
                    sourceFilePath,
                    memberName,
                    sourceLineNumber,
                    tags,
                    request,
                    response,
                    exception,
                    codeNumber
            );

        public void Warning(
            string                    category,
            AuthorizationResponse     authorization,
            [CallerFilePath] string   sourceFilePath   = "",
            [CallerMemberName] string memberName       = "",
            [CallerLineNumber] int    sourceLineNumber = 0,
            string                    tags             = null,
            object                    request          = null,
            object                    response         = null,
            object                    exception        = null,
            object                    codeNumber       = null
        ) => SetLog("WARNING",
                    category,
                    authorization,
                    sourceFilePath,
                    memberName,
                    sourceLineNumber,
                    tags,
                    request,
                    response,
                    exception,
                    codeNumber
        );
    }
}
