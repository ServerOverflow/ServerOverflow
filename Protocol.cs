using AngleSharp;
using AngleSharp.Html.Dom;
using Serilog;

namespace ServerOverflow; 

/// <summary>
/// Minecraft protocol version mapping
/// </summary>
public class Protocol {
    /// <summary>
    /// Protocol version number dictionary
    /// </summary>
    public static readonly Dictionary<int, string> Mapping = new();
    
    /// <summary>
    /// Generates the PVN mapping
    /// </summary>
    public static async Task Generate() {
        var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        var document = await context.OpenAsync("https://wiki.vg/index.php?title=Protocol_version_numbers");
        var table = document.QuerySelector("table tbody");
        if (table == null) {
            Log.Error("Failed to find table's body!");
            Environment.Exit(0);
        }

        try {
            var rowSpan = 0;
            foreach (var element in table.Children) {
                var el = (HtmlElement)element;
                if (el.Children[0].TagName == "TH") continue;
                var name = el.Children[0].TextContent.TrimEnd();
                if (rowSpan == 0) {
                    if (el.Children[1].HasAttribute("rowspan"))
                        rowSpan = int.Parse(el.Children[1].Attributes["rowspan"]!.Value) - 1;
                    var str = el.Children[1].TextContent;
                    var number = !str.StartsWith("Snapshot") ? int.Parse(str)
                        : int.Parse(str.Split(" ")[1]) + 0x40000000;
                    Mapping.TryAdd(number, name);
                    continue;
                }

                rowSpan--;
            }
        } catch (Exception e) {
            Log.Error("Failed to parse the table: {0}", e);
            Environment.Exit(0);
        }
        
        Log.Information("Successfully mapped PVNs with {0} entries", Mapping.Count);
    }
}