using System.Security.Cryptography;
using System.Text;
using Calidad_API.Interfaces;

namespace Calidad_API.Services;

public class AesGcmCodigoCipher : ICodigoCipher
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;

    private readonly byte[] _key;

    public AesGcmCodigoCipher(IConfiguration configuration)
    {
        var keyBase64 = configuration["Codigos:AesKey"]
            ?? throw new InvalidOperationException("Falta configurar 'Codigos:AesKey'.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != KeySize)
        {
            throw new InvalidOperationException("'Codigos:AesKey' debe ser una clave de 32 bytes (AES-256) en base64.");
        }
    }

    public string Cifrar(string textoPlano)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintext = Encoding.UTF8.GetBytes(textoPlano);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var resultado = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, resultado, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, resultado, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, resultado, NonceSize + TagSize, ciphertext.Length);

        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        var datos = Convert.FromBase64String(textoCifrado);

        var nonce = datos.AsSpan(0, NonceSize);
        var tag = datos.AsSpan(NonceSize, TagSize);
        var ciphertext = datos.AsSpan(NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}