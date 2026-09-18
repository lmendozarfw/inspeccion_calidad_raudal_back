namespace Calidad_API.Interfaces;

public interface ICodigoCipher
{
    string Cifrar(string textoPlano);
    string Descifrar(string textoCifrado);
}