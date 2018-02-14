using System;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;


namespace WsFedCore.SamlBearerAuthentication
{
    internal class DeflatedSamlEncoder
    {
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly DeflateCookieTransform _deflateCookieTransform = new DeflateCookieTransform();

        public virtual string Encode(string token)
        {
            // Before it’s sent, the message is deflated, base64-encoded, and URL-encoded, in that order.
            var bytes = _encoding.GetBytes(token);
            var deflatedBytes = _deflateCookieTransform.Encode(bytes);
            var base64String = Convert.ToBase64String(deflatedBytes);
            var encodedToken = System.Net.WebUtility.UrlEncode(base64String);
            return encodedToken;
        }

        public virtual string Decode(string encodedToken)
        {
            var base64String = System.Net.WebUtility.UrlDecode(encodedToken);
            var deflatedBytes = Convert.FromBase64String(base64String);
            var bytes = _deflateCookieTransform.Decode(deflatedBytes);
            var token = _encoding.GetString(bytes);
            return token;
        }

        /// <summary>Provides cookie compression using <see cref="T:System.IO.Compression.DeflateStream" />.</summary>
        /// Adapted from Decompiled .NET 4.5 sources
        private sealed class DeflateCookieTransform
        {
            /// <summary>Gets or sets the maximum size, in bytes, of a decompressed cookie.</summary>
            /// <returns>The maximum size, in bytes, of a decompressed cookie.</returns>
            public int MaxDecompressedSize { get; set; } = 1048576;

            /// <summary>Inflates data.</summary>
            /// <param name="encoded">
            ///     Data previously returned from
            ///     <see cref="M:System.IdentityModel.DeflateCookieTransform.Encode(System.Byte[])" />
            /// </param>
            /// <returns>The decoded data.</returns>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="value" /> is <see langword="null" />.
            /// </exception>
            /// <exception cref="T:System.ArgumentException">
            ///     <paramref name="value" /> contains zero bytes.
            /// </exception>
            /// <exception cref="T:System.IdentityModel.Tokens.SecurityTokenException">
            ///     The decompressed length is larger than the
            ///     maximum length specified by the <see cref="P:System.IdentityModel.DeflateCookieTransform.MaxDecompressedSize" />
            ///     property.
            /// </exception>
            public byte[] Decode(byte[] encoded)
            {
                if (encoded == null) throw new ArgumentNullException(nameof(encoded));
                if (encoded.Length == 0) throw new ArgumentException(nameof(encoded), "ID6045");
                using (var deflateStream = new DeflateStream(new MemoryStream(encoded), CompressionMode.Decompress, false))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var buffer = new byte[1024];
                        int count;
                        do
                        {
                            count = deflateStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);
                            if (memoryStream.Length > MaxDecompressedSize)
                                throw new SecurityException($"ID1068: {MaxDecompressedSize}");
                            ;
                        } while (count > 0);
                        return memoryStream.ToArray();
                    }
                }
            }

            /// <summary>Deflates data.</summary>
            /// <param name="value">The data to be compressed.</param>
            /// <returns>The compressed data.</returns>
            /// <exception cref="T:System.ArgumentNullException">
            ///     <paramref name="value" /> is <see langword="null" />.
            /// </exception>
            /// <exception cref="T:System.ArgumentException">
            ///     <paramref name="value" /> contains zero bytes.
            /// </exception>
            public byte[] Encode(byte[] value)
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length == 0) throw new ArgumentException(nameof(value), "ID6044");
                using (var memoryStream = new MemoryStream())
                {
                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                    {
                        deflateStream.Write(value, 0, value.Length);
                    }
                    var array = memoryStream.ToArray();
                    return array;
                }
            }
        }
    }
}