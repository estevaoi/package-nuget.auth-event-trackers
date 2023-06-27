using AuthEventTrackers.Domains.Enum;
using AuthEventTrackers.Domains.Response;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AuthEventTrackers
{
    public static class LoggerClient
    {
        public static void Log(
            LogTypeEnum type,
            Guid correlationId,
            AuthorizationResponse authorization     = null,
            [CallerFilePath] string sourceFilePath  = "",
            [CallerMemberName] string memberName    = "",
            [CallerLineNumber] int sourceLineNumber = 0,
            string tags                             = null,
            object request                          = null,
            object response                         = null,
            object exception                        = null)
        {
            var date             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var memberClassName  = Path.GetFileNameWithoutExtension(sourceFilePath);
            var tagSplit         = tags?.Split(",");
            LogTypeEnum typeName = (LogTypeEnum)type;

            var json = new
            {
                type           = typeName.ToString(),
                date,
                correlationId,
                application    = Process.GetCurrentProcess().ProcessName,
                source = new
                {
                    className  = memberClassName,
                    methodName = memberName,
                    lineNumber = sourceLineNumber
                },
                authorization,
                tags           = tagSplit,
                request,
                response,
                exception
            };

            try
            {
                RabbitClient.SendMessage(json, RabbitClient.GetQueueRabbit("queueLoggers"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Não foi possível carregar as configurações do RabbitMQ");
            }

        }
    }
}
