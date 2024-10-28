using System.Text;

namespace MrX.ProxyServer.Tools;

public class ReadFromStream : BinaryReader
{
    public ReadFromStream(Stream stream, Encoding encoding) : base(stream, encoding)
    {
    }

    public string ReadLine()
    {
        try
        {
            char[] buf = new char[1];
            StringBuilder _readBuffer = new StringBuilder();
            var charsRead = 0;
            char lastChar = new char();
            while ((charsRead = base.Read(buf, 0, 1)) > 0)
            {
                if (lastChar == '\r' && buf[0] == '\n')
                {
                    return _readBuffer.Remove(_readBuffer.Length - 1, 1).ToString();
                }
                else if (buf[0] == '\0')
                {
                    return _readBuffer.ToString();
                }
                else
                    _readBuffer.Append(buf[0]);
                lastChar = buf[0];
            }
            return _readBuffer.ToString();
        }
        catch
        {
            throw new EndOfStreamException();
        }
    }
}