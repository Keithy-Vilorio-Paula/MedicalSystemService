using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSystemServidor
{
    internal class Program
    {
        private static List<string> pacientes = new List<string>();
        private static List<string> citas = new List<string>();

        static async Task Main(string[] args)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:1200/");
            httpListener.Start();
            Console.WriteLine("Se ha iniciado el servidor");

            while (true)
            {
                try
                {
                    var context = await httpListener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        _ = Task.Run(() => HandleWebSocketAsync(webSocketContext.WebSocket));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en la operación del servidor: {ex.Message}");
                }
            }
        }

        static async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Mensaje recibido: {message}");

                    // Aquí deberías manejar el mensaje de acuerdo a tu lógica de aplicación
                    string respuesta = ProcesarMensaje(message);

                    // Envía una respuesta al cliente
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(respuesta);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

                    buffer = new byte[1024];
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Error en la conexión WebSocket: {ex.Message}");
            }
            finally
            {
                // Cierra la conexión WebSocket
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexión", CancellationToken.None);
            }
        }

        static string ProcesarMensaje(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            string comando = partes[0];

            switch (comando)
            {
                case "HacerCita":
                    return HacerCita(partes);
                case "VerCitas":
                    return VerCitas();
                case "BuscarPaciente":
                    return BuscarPaciente();
                case "RegistrarNuevoPaciente":
                    return RegistrarNuevoPaciente(partes);
                case "ModificarCancelarCitas":
                    return ModificarCancelarCitas(partes);
                default:
                    return "Mensaje no reconocido.";
            }
        }

        static string HacerCita(string[] partes)
        {
            string nombre = partes[1];
            string cedula = partes[2];
            string telefono = partes[3];

            // Aquí puedes almacenar la cita en tu sistema y realizar validaciones necesarias
            string cita = $"{nombre} - Cédula: {cedula} - Teléfono: {telefono}";
            citas.Add(cita);

            return "Cita realizada exitosamente.";
        }

        static string VerCitas()
        {
            // Devuelve la lista de citas almacenadas
            return string.Join(Environment.NewLine, citas);
        }

        static string BuscarPaciente()
        {
            // Devuelve la lista de pacientes almacenados
            return string.Join(Environment.NewLine, pacientes);
        }

        static string RegistrarNuevoPaciente(string[] partes)
        {
            string nuevoNombre = partes[1];

            // Aquí puedes almacenar el nuevo paciente en tu sistema y realizar validaciones necesarias
            pacientes.Add(nuevoNombre);

            return "Nuevo paciente registrado exitosamente.";
        }

        static string ModificarCancelarCitas(string[] partes)
        {
            string numeroCita = partes[1];

            // Aquí puedes implementar la lógica para modificar o cancelar la cita con el número proporcionado
            // Retorna un mensaje indicando el resultado de la operación
            if (int.TryParse(numeroCita, out int numero))
            {
                if (numero > 0 && numero <= citas.Count)
                {
                    string citaEliminada = citas[numero - 1];
                    citas.RemoveAt(numero - 1);
                    return $"Cita {numero} eliminada: {citaEliminada}";
                }
                else
                {
                    return "Número de cita inválido.";
                }
            }
            else
            {
                return "Número de cita inválido.";
            }
        }
    }
}
