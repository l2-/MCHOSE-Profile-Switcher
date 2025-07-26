using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driver;

public enum CommandType
{
    Info = 3,
    Base = 4,
    Lighting = 6,
}

public static class CommandTypeExtensions
{
    public static byte As(this CommandType commandType)
        => (byte)commandType;
}
