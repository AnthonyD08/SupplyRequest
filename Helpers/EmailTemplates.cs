namespace WebApi.Helpers
{
    public class EmailTemplates
    {
        public static string RequestConfirmation(string name, int requestId)
        {
            var template = $"Hola {name},\n\n" +

                           $"Tu solicitud de suministros ha sido recibida y está siendo procesada.\n\n" +

                           $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

                           $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

                           $"Gracias por usar Supply Request.";

            return template;
        }

        public static string BossApproved(string name, int requestId)
        {
            var template = $"Hola {name},\n\n" +

                           $"Tu solicitud de suministros ha sido aprobada por tu jefe.\n\n" +

                           $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

                           $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

                           $"Gracias por usar Supply Request."; 

            return template;
        }

        public static string BossRejected(string name, int requestId)
        {
            var template = $"Hola {name},\n\n" +

                           $"Tu solicitud de suministros ha sido rechazada por tu jefe.\n\n" +

                           $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

                           $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

                           $"Gracias por usar Supply Request.";

            return template;
        }

        public static string AccountantApproved(string name, int requestId)
        {
            var template = $"Hola {name},\n\n" +

                           $"Tu solicitud de suministros ha sido aprobada por el contador.\n\n" +

                           $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

                           $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

                           $"Gracias por usar Supply Request.";

            return template;
        }

        public static string AccountantRejected(string name, int requestId)
        {
            var template = $"Hola {name},\n\n" +

                           $"Tu solicitud de suministros ha sido rechazada por el contador.\n\n" +

                           $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

                           $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

                           $"Gracias por usar Supply Request.";

            return template;
        }

        public static string UserCancelled(string name, int requestId)
        {
           var template = $"Hola {name},\n\n" +

               $"Tu solicitud de suministros ha sido cancelada.\n\n" +

               $"Puedes ver el estado de tu solicitud en la sección de solicitudes de tu perfil o haciendo click en el siguiente enalce:\n\n" +

               $"https://localhost:7021/Request/Status/Index?requestId={requestId}\n\n" +

               $"Gracias por usar Supply Request.";

            return template;
        }
    }
}
