public static class ConsoleUtils
{
    public static bool? GetBoolean(string arg)
    {
        arg = arg.ToUpper();
        if (arg == "TRUE" || arg == "1" || arg == "T")
            return true;
        else if (arg == "FALSE" || arg == "0" || arg == "F")
            return false;
        else
            return null;
    }
}