using System.Collections.Frozen;

namespace Perch.Core.Scanner;

public static class DefaultFontFamilies
{
    private static readonly FrozenSet<string> Defaults = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Core Windows fonts
        "arial", "arialbd", "arialbi", "ariali", "ariblk",
        "bahnschrift",
        "calibri", "calibrib", "calibrii", "calibril", "calibrili", "calibriz",
        "cambria", "cambriab", "cambriai", "cambriaz", "cambria-math",
        "candara", "candarab", "candarai", "candaral", "candarali", "candaraz",
        "comic", "comicbd", "comici", "comicz",
        "consola", "consolab", "consolai", "consolaz",
        "constan", "constanb", "constani", "constanz",
        "corbel", "corbelb", "corbeli", "corbell", "corbelli", "corbelz",
        "cour", "courbd", "courbi", "couri",
        "ebrima", "ebrimabd",
        "framd", "framdit",
        "gadugi", "gadugib",
        "georgia", "georgiab", "georgiai", "georgiaz",
        "himalaya",
        "impact",
        "inkfree",
        "javatext",
        "l_10646",
        "leelawui", "leelauib", "leelauis",
        "lucon",
        "malgun", "malgunbd", "malgunsl",
        "marlett",
        "micross",
        "mingliub",
        "monbaiti",
        "msgothic", "msjh", "msjhbd", "msjhl", "msyh", "msyhbd", "msyhl", "msyi",
        "mvboli",
        "nirmala", "nirmalab", "nirmalas",
        "ntailu", "ntailub",
        "pala", "palab", "palabi", "palai",
        "phagspa", "phagspab",
        "segmdl2",
        "segoepr", "segoeprb",
        "segoesc", "segoescb",
        "segoeui", "segoeuib", "segoeuii", "segoeuil", "segoeuisl", "segoeuiz",
        "seguibl", "seguibli", "seguiemj", "seguihis", "seguili", "seguisb", "seguisbi", "seguisli", "seguisym",
        "simsun", "simsunb",
        "sitka",
        "sylfaen",
        "symbol",
        "tahoma", "tahomabd",
        "times", "timesbd", "timesbi", "timesi",
        "trebuc", "trebucbd", "trebucbi", "trebucit",
        "verdana", "verdanab", "verdanai", "verdanaz",
        "webdings",
        "wingding",
        "yumindb", "yuminl", "yumin",
        "yugothb", "yugothl", "yugothm", "yugothr",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsDefault(string fileNameWithoutExtension)
        => Defaults.Contains(fileNameWithoutExtension);
}
