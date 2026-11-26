using System.Globalization;
using System.Text;

namespace Core.Insights
{
    /// <summary>
    /// Insights-only folding of free-text product names. Stored rows are left as the user typed them.
    /// Lowercase, trim, strip diacritics, then a small EN/PL synonym list (onion / cebula / cebulka).
    /// </summary>
    public static class FoodNameNormalizer
    {
        public static string? CanonicalKey(string? raw)
        {
            var cleaned = Clean(raw);
            if (cleaned.Length == 0)
            {
                return null;
            }

            return Synonyms.TryGetValue(cleaned, out var key) ? key : cleaned;
        }

        public static string Clean(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var lower = StripDiacritics(raw.Trim().ToLowerInvariant());
            var sb = new StringBuilder(lower.Length);
            var pendingSpace = false;
            foreach (var c in lower)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (pendingSpace && sb.Length > 0)
                    {
                        sb.Append(' ');
                    }

                    pendingSpace = false;
                    sb.Append(c);
                }
                else
                {
                    pendingSpace = true;
                }
            }

            return sb.ToString();
        }

        private static string StripDiacritics(string value)
        {
            // ł does not decompose under FormD.
            value = value.Replace('ł', 'l').Replace('Ł', 'L');
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static readonly Dictionary<string, string> Synonyms = BuildSynonyms();

        private static Dictionary<string, string> BuildSynonyms()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            void Add(string key, params string[] forms)
            {
                foreach (var form in forms)
                {
                    map[form] = key;
                }
            }

            Add("onion", "onion", "onions", "cebula", "cebulka", "cebulke", "cebulki", "cebuli", "cebule");
            Add("garlic", "garlic", "garlics", "czosnek", "czosnku", "czosnkiem");
            Add("wheat", "wheat", "pszenica", "pszenicy");
            Add("bread", "bread", "breads", "chleb", "chleba", "pieczywo");
            Add("milk", "milk", "mleko", "mleka");
            Add("cheese", "cheese", "cheeses", "ser", "sera", "sery", "serek", "twarog");
            Add("butter", "butter", "maslo");
            Add("egg", "egg", "eggs", "jajko", "jajka", "jajek", "jajkiem");
            Add("yogurt", "yogurt", "yoghurt", "yogurts", "jogurt", "jogurtu");
            Add("apple", "apple", "apples", "jablko", "jablka");
            Add("pear", "pear", "pears", "gruszka", "gruszki");
            Add("banana", "banana", "bananas", "banan", "banany");
            Add("honey", "honey", "miod", "miodu");
            Add("chocolate", "chocolate", "czekolada", "czekolady", "czekolade");
            Add("coffee", "coffee", "kawa", "kawy");
            Add("tea", "tea", "herbata", "herbaty");
            Add("rice", "rice", "ryz", "ryzu");
            Add("pasta", "pasta", "makaron", "makaronu", "spaghetti", "noodles");
            Add("beans", "bean", "beans", "fasola", "fasoli");
            Add("lentils", "lentil", "lentils", "soczewica", "soczewicy");
            Add("chickpeas", "chickpea", "chickpeas", "ciecierzyca", "ciecierzycy");
            Add("tomato", "tomato", "tomatoes", "pomidor", "pomidory", "pomidora");
            Add("potato", "potato", "potatoes", "ziemniak", "ziemniaki", "ziemniaka");
            Add("chicken", "chicken", "kurczak", "kurczaka", "kurczakiem");
            Add("beef", "beef", "wolowina");
            Add("pork", "pork", "wieprzowina", "wieprzowiny");
            Add("avocado", "avocado", "awokado");
            Add("cauliflower", "cauliflower", "kalafior", "kalafiora");
            Add("broccoli", "broccoli", "brokul", "brokuly");
            Add("cabbage", "cabbage", "kapusta", "kapusty");
            Add("mushroom", "mushroom", "mushrooms", "pieczarka", "pieczarki", "grzyb", "grzyby");
            Add("cucumber", "cucumber", "cucumbers", "ogorek", "ogorki");
            Add("carrot", "carrot", "carrots", "marchew", "marchewka", "marchewki");
            Add("oats", "oats", "oatmeal", "owsianka", "platki owsiane");
            Add("corn", "corn", "kukurydza", "kukurydzy");
            Add("soy", "soy", "soya", "soja", "soi");
            Add("almond", "almond", "almonds", "migdal", "migdaly");
            Add("peanut", "peanut", "peanuts", "orzeszki ziemne", "orzech ziemny");
            Add("walnut", "walnut", "walnuts", "orzech wloski", "orzechy wloskie");
            Add("ice cream", "ice cream", "icecream", "lody");
            Add("beer", "beer", "piwo", "piwa");
            Add("wine", "wine", "wino", "wina");
            Add("cream", "cream", "smietana");
            Add("ham", "ham", "szynka", "szynki");
            Add("sausage", "sausage", "kielbasa", "kielbasy");

            return map;
        }
    }
}
