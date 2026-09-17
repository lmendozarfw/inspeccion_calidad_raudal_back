namespace Calidad_API.DTOs.Usuario;

public class CambiarPasswordDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNuevo { get; set; } = string.Empty;
}