namespace LICORERIA.Core.DTOs
{
    public class ApiResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Success(T data, string mensaje = "Operación exitosa") =>
            new ApiResponse<T> { Exito = true, Mensaje = mensaje, Data = data };

        public static ApiResponse<T> Error(string mensaje) =>
            new ApiResponse<T> { Exito = false, Mensaje = mensaje, Data = default };
    }
}