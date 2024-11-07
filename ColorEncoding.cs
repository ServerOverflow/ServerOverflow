namespace ServerOverflow; 

/// <summary>
/// Minecraft color encoding
/// </summary>
public static class ColorEncoding {
    /// <summary>
    /// Color code to CSS color dictionary
    /// </summary>
    private static readonly Dictionary<char, string> _mapping = new() {
        { '0' , "#000000" },
        { '1' , "#0000AA" },
        { '2' , "#00AA00" },
        { '3' , "#00AAAA" },
        { '4' , "#AA0000" },
        { '5' , "#AA00AA" },
        { '6' , "#FFAA00" },
        { '7' , "#AAAAAA" },
        { '8' , "#555555" },
        { '9' , "#5555FF" },
        { 'a' , "#55FF55" },
        { 'b' , "#55FFFF" },
        { 'c' , "#FF5555" },
        { 'd' , "#FF55FF" },
        { 'e' , "#FFFF55" },
        { 'f' , "#FFFFFF" },
        { 'r' , "reset" },
        { 'k' , "obf" },
        { 'o' , "em" },
        { 'l' , "b" },
        { 'm' , "s" },
        { 'n' , "u" }
    };

    /// <summary>
    /// Converts colored text to HTML
    /// </summary>
    /// <param name="str">Colored text</param>
    /// <param name="clean">Strip out color</param>
    /// <returns>Converted HTML</returns>
    public static string ToHtml(string? str, bool clean = false) {
        if (str == null) return "";
        var output = "<a>";
        var expectChar = false;
        var ending = "";
        var character = '&';
        foreach (var i in str) {
            if (expectChar) {
                if (i == 'k') {
                    expectChar = false;
                    continue;
                }

                if (!clean) {
                    if (_mapping.TryGetValue(i, out var color)) {
                        if (color == "reset") {
                            output += ending;
                            ending = "";
                        } else if (!color.StartsWith("#")) {
                            output += $"<{color}>";
                            ending = $"</{color}>" + ending;
                        } else output += $"</a><a style=\"color: {color};\">";
                    } else output += character + i;
                } 
                
                if (!_mapping.ContainsKey(i)) 
                    output += character + i;

                expectChar = false;
                continue;
            }

            switch (i) {
                case '\n':
                    output += "</a></h5><h5><a>";
                    break;
                case '&':
                    character = '&';
                    expectChar = true;
                    continue;
                case '§':
                    character = '§';
                    expectChar = true;
                    continue;
            }

            output += i;
        }

        return output + ending + "</a>";
    }
}
