using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogic
{
    /// <summary>
    /// Clase que proporciona métodos de validación genéricos para diferentes tipos de datos y reglas de negocio.
    /// </summary>
    public class AllValidations
    {
        /// <summary>
        /// Verifica si una cadena no está vacía ni contiene solo espacios en blanco.
        /// </summary>
        /// <param name="input">Cadena a validar</param>
        /// <returns>True si la cadena no es vacía, False si lo es</returns>
        public bool EsStringNoVacio(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Verifica si un número entero es positivo (mayor que cero).
        /// </summary>
        /// <param name="numero">Número a validar</param>
        /// <returns>True si el número es positivo, False si no lo es</returns>
        public bool EsNumeroPositivo(int numero)
        {
            return numero > 0;
        }

        /// <summary>
        /// Verifica si una fecha es igual o anterior a la fecha actual.
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <returns>True si la fecha es pasada o actual, False si es futura</returns>
        public bool EsFechaPasada(DateTime fecha)
        {
            return fecha <= DateTime.Now;
        }

        /// <summary>
        /// Verifica si una cadena tiene un formato de correo electrónico válido.
        /// </summary>
        /// <param name="email">Correo electrónico a validar</param>
        /// <returns>True si el formato es válido, False si no lo es</returns>
        public bool EsFormatoEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patronEmail);
        }

        /// <summary>
        /// Verifica si una cadena tiene un formato de hora válido (HH:MM, 24 horas).
        /// </summary>
        /// <param name="hora">Hora a validar</param>
        /// <returns>True si el formato es válido (ej. "08:30"), False si no lo es</returns>
        public bool EsFormatoHoraValido(string hora)
        {
            if (string.IsNullOrWhiteSpace(hora))
            {
                return false;
            }

            string patronHora = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$";
            return Regex.IsMatch(hora, patronHora);
        }

        /// <summary>
        /// Verifica si un elemento existe en una lista o arreglo.
        /// </summary>
        /// <typeparam name="T">Tipo del elemento y de la lista</typeparam>
        /// <param name="elemento">Elemento a buscar</param>
        /// <param name="lista">Arreglo donde buscar</param>
        /// <returns>True si el elemento existe en la lista, False si no</returns>
        public bool ExisteEnLaLista<T>(T elemento, T[] lista)
        {
            return Array.Exists(lista, item => item.Equals(elemento));
        }

        /// <summary>
        /// Verifica si una persona es mayor de edad (18 años o más) según su fecha de nacimiento.
        /// </summary>
        /// <param name="fechaNacimiento">Fecha de nacimiento de la persona</param>
        /// <returns>True si la persona tiene 18 años o más, False si no</returns>
        public bool EsMayorDeEdad(DateTime fechaNacimiento)
        {
            int edad = DateTime.Now.Year - fechaNacimiento.Year;
            if (fechaNacimiento > DateTime.Now.AddYears(-edad))
            {
                edad--;
            }
            return edad >= 18;
        }

        /// <summary>
        /// Verifica si la longitud de una cadena está dentro de un rango específico.
        /// </summary>
        /// <param name="input">Cadena a validar</param>
        /// <param name="min">Longitud mínima permitida</param>
        /// <param name="max">Longitud máxima permitida</param>
        /// <returns>True si la longitud está en el rango, False si no</returns>
        public bool EsLongitudValida(string input, int min, int max)
        {
            return input.Length >= min && input.Length <= max;
        }

        /// <summary>
        /// Verifica si una lista no está vacía ni es nula.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la lista</typeparam>
        /// <param name="lista">Lista a validar</param>
        /// <returns>True si la lista tiene elementos, False si está vacía o es nula</returns>
        public bool EsListaNoVacia<T>(IEnumerable<T> lista)
        {
            return lista != null && lista.Any();
        }

        /// <summary>
        /// Verifica si una cadena contiene solo letras (incluyendo acentuadas, ñ y espacios).
        /// </summary>
        /// <param name="input">Cadena a validar</param>
        /// <returns>True si contiene solo letras y espacios, False si no</returns>
        public bool EsSoloLetras(string input)
        {
            //return Regex.IsMatch(input, @"^[a-zA-Z]+$");
            // Acepta solo letras (incluyendo acentuadas, con diéresis y la "ñ") y espacios.
            return Regex.IsMatch(input, @"^[a-zA-ZÀ-ÿüÜñÑ\s]+$");
        }

        /// <summary>
        /// Verifica si una cadena contiene solo números.
        /// </summary>
        /// <param name="input">Cadena a validar</param>
        /// <returns>True si contiene solo números, False si no</returns>
        public bool EsSoloNumeros(string input)
        {
            return Regex.IsMatch(input, @"^\d+$");
        }

        /// <summary>
        /// Verifica si una cadena tiene un formato de teléfono válido (ej. "(123) 456-7890").
        /// </summary>
        /// <param name="telefono">Teléfono a validar</param>
        /// <returns>True si el formato es válido, False si no</returns>
        public bool EsTelefonoValido(string telefono)
        {
            // Ejemplo para números de teléfono con formato (123) 456-7890
            // Esto es un problema si el teléfono tiene 4 dígitos para la
            // característica (por ejemplo los de Carlos Paz)
            return Regex.IsMatch(telefono, @"^\(\d{3}\) \d{3}-\d{4}$");
        }

        /// <summary>
        /// Verifica si una fecha está dentro de un rango específico.
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <returns>True si la fecha está en el rango, False si no</returns>
        public bool EsFechaEnRango(DateTime fecha, DateTime fechaInicio, DateTime fechaFin)
        {
            return fecha >= fechaInicio && fecha <= fechaFin;
        }

        /// <summary>
        /// Verifica si una cadena no contiene letras (es decir, es numérica).
        /// </summary>
        /// <param name="input">Cadena a validar</param>
        /// <returns>True si la cadena es numérica, False si contiene letras</returns>
        public bool ContieneLetras(string input)
        {
            return int.TryParse(input, out int ip);
        }

        /// <summary>
        /// Verifica si una contraseña cumple con criterios de seguridad (mínimo 8 caracteres, letras mayúsculas, minúsculas, números y caracteres especiales).
        /// </summary>
        /// <param name="contrasena">Contraseña a validar</param>
        /// <returns>True si la contraseña es segura, False si no</returns>
        public bool EsContrasenaSegura(string contrasena)
        {
            return Regex.IsMatch(contrasena, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[~!@#$%^&*()_+=\[{\]};:<>|./?,-]).{8,}$");
        }

        /// <summary>
        /// Verifica si una fecha de nacimiento es válida (no puede ser futura).
        /// </summary>
        /// <param name="fechaNacimiento">Fecha de nacimiento a validar</param>
        /// <returns>True si la fecha es válida, False si es futura</returns>
        public bool EsFechaNacimientoValida(DateTime fechaNacimiento)
        {
            return fechaNacimiento <= DateTime.Now;
        }

    }


}
