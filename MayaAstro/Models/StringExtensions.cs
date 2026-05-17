using System.Text.RegularExpressions;
using System.Text;

namespace MayaAstro.DatabaseEntities
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        public static string ToTitleCase(this string str)
        {
            var tokens = str.Split(new[] { " " }, StringSplitOptions.None);
            var stringBuilder = new StringBuilder();
            foreach (var token in tokens)
            {
                if (token == token.ToUpper())
                    stringBuilder.Append(token + " ");
                else
                {
                    var previousWasSeperator = false;
                    var previousWasNumber = false;
                    var ignoreNumber = false;
                    for (var i = 0; i < token.Length; i++)
                    {

                        if (char.IsNumber(token[i]))
                        {
                            stringBuilder.Append(token[i]);
                            previousWasNumber = true;
                        }
                        else if (!char.IsLetter(token[i]))
                        {
                            stringBuilder.Append(token[i]);
                            previousWasSeperator = true;
                        }
                        else if ((previousWasNumber && !ignoreNumber) || previousWasSeperator)
                        {
                            stringBuilder.Append(char.ToUpper(token[i]));
                            previousWasSeperator = false;
                            previousWasNumber = false;
                        }
                        else if (i == 0)
                        {
                            ignoreNumber = true;
                            stringBuilder.Append(char.ToUpper(token[i]));
                        }
                        else
                        {
                            ignoreNumber = true;
                            stringBuilder.Append(char.ToLower(token[i]));
                        }
                    }
                    stringBuilder.Append(" ");
                }
            }
            return stringBuilder.ToString().TrimEnd();
        }
    }
}
