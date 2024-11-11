namespace MineProtocol; 

/// <summary>
/// Minecraft color encoding
/// </summary>
public static class ColorEncoding {
    /// <summary>
    /// Color code to CSS color dictionary
    /// </summary>
    private static readonly Dictionary<char, string> _mapping = new() {
        { '0' , "black" },
        { '1' , "dark_blue" },
        { '2' , "dark_green" },
        { '3' , "dark_aqua" },
        { '4' , "dark_red" },
        { '5' , "dark_purple" },
        { '6' , "gold" },
        { '7' , "gray" },
        { '8' , "dark_gray" },
        { '9' , "blue" },
        { 'a' , "green" },
        { 'b' , "aqua" },
        { 'c' , "red" },
        { 'd' , "light_purple" },
        { 'e' , "yellow" },
        { 'f' , "white" },
        { 'r' , "reset" },
        { 'k' , "obf" },
        { 'o' , "italic" },
        { 'l' , "bold" },
        { 'm' , "strike" },
        { 'n' , "underline" }
    };

    /// <summary>
    /// Parses decorated string into a text component
    /// </summary>
    /// <param name="str">Decorated string</param>
    /// <returns>Text component</returns>
    public static TextComponent Parse(string? str) {
        if (str == null) return new TextComponent();
        var parent = new TextComponent();
        var component = new TextComponent();
        parent.Extra.Add(component);
        var expectChar = false;
        foreach (var i in str) {
            if (expectChar) {
                if (_mapping.TryGetValue(i, out var color)) {
                    if (color == "reset") {
                        component = new TextComponent();
                        parent.Extra.Add(component);
                        expectChar = false;
                        continue;
                    }

                    var old = component;
                    component = color switch {
                        "obf" => new TextComponent {
                            Obfuscated = true
                        },
                        "italic" => new TextComponent {
                            Italic = true
                        },
                        "bold" => new TextComponent {
                            Bold = true
                        },
                        "strike" => new TextComponent {
                            Strikethrough = true
                        },
                        "underline" => new TextComponent {
                            Underlined = true
                        },
                        _ => new TextComponent {
                            Color = color
                        }
                    };
                        
                    old.Extra.Add(component);
                    expectChar = false;
                    continue;
                } 
                
                expectChar = false;
                continue;
            }

            switch (i) {
                case '&':
                    expectChar = true;
                    continue;
                case '§':
                    expectChar = true;
                    continue;
            }

            component.Text += i;
        }

        return parent;
    }
}
