
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;



namespace ProyectoFinal.Models
{
    public class LoginViewModel: IValidatableObject
    {
        [Required]
        public string Dni { get; set; }
        public string Clave { get; set; }
        public string Nuevo { get; set; }
        public string NombreCompleto { get; set; }
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            string strLetra = string.Empty;
            int tmpInt = 0;

            try
            {

            }
            catch (Exception ex)
            {

            }


            yield return ValidationResult.Success;

        }
    }
}
