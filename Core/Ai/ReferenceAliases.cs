namespace Core.Ai
{
    /// <summary>
    /// Maps spoken/written unit and symptom names (EN + PL, including common speech shortcuts)
    /// onto seeded reference ids from <c>FodmapLogDbContext</c>.
    /// </summary>
    public static class ReferenceAliases
    {
        private static readonly Dictionary<string, int> Units = new(StringComparer.OrdinalIgnoreCase)
        {
            ["gram"] = 1, ["grams"] = 1, ["g"] = 1, ["gr"] = 1, ["gramy"] = 1, ["gramów"] = 1, ["gramow"] = 1,
            ["kilogram"] = 2, ["kilograms"] = 2, ["kg"] = 2, ["kilo"] = 2, ["kilogramy"] = 2,
            ["milligram"] = 3, ["milligrams"] = 3, ["mg"] = 3, ["miligram"] = 3, ["miligramy"] = 3,
            ["liter"] = 4, ["litre"] = 4, ["liters"] = 4, ["l"] = 4, ["litr"] = 4, ["litry"] = 4,
            ["milliliter"] = 5, ["millilitre"] = 5, ["milliliters"] = 5, ["ml"] = 5, ["mililitr"] = 5, ["mililitry"] = 5,
            ["teaspoon"] = 6, ["teaspoons"] = 6, ["tsp"] = 6, ["łyżeczka"] = 6, ["lyzeczka"] = 6, ["łyżeczki"] = 6,
            ["tablespoon"] = 7, ["tablespoons"] = 7, ["tbsp"] = 7, ["łyżka"] = 7, ["lyzka"] = 7, ["łyżka stołowa"] = 7, ["łyżki"] = 7,
            ["cup"] = 8, ["cups"] = 8, ["filiżanka"] = 8, ["filizanka"] = 8, ["szklanka"] = 8, ["szklanki"] = 8,
            ["piece"] = 9, ["pieces"] = 9, ["sztuka"] = 9, ["sztuki"] = 9, ["szt"] = 9, ["pcs"] = 9,
            ["slice"] = 10, ["slices"] = 10, ["plasterek"] = 10, ["plasterki"] = 10, ["kromka"] = 10,
            ["drop"] = 11, ["drops"] = 11, ["kropla"] = 11, ["krople"] = 11,
            ["pinch"] = 12, ["pinches"] = 12, ["szczypta"] = 12,
            ["ounce"] = 13, ["ounces"] = 13, ["oz"] = 13, ["uncja"] = 13,
            ["pound"] = 14, ["pounds"] = 14, ["lb"] = 14, ["funt"] = 14,
            ["fluid ounce"] = 15, ["fl oz"] = 15, ["uncja płynu"] = 15, ["uncja plynu"] = 15,
            ["pint"] = 16, ["pinta"] = 16,
            ["quart"] = 17, ["kwarta"] = 17,
            ["gallon"] = 18, ["galon"] = 18,
            ["can"] = 19, ["cans"] = 19, ["puszka"] = 19, ["puszki"] = 19,
            ["package"] = 20, ["pack"] = 20, ["opakowanie"] = 20, ["paczka"] = 20,
            ["bottle"] = 21, ["bottles"] = 21, ["butelka"] = 21, ["butelki"] = 21
        };

        private static readonly Dictionary<string, int> Symptoms = new(StringComparer.OrdinalIgnoreCase)
        {
            ["nausea"] = 1, ["nudności"] = 1, ["nudnosci"] = 1, ["mdłości"] = 1, ["mdlosci"] = 1,
            ["burping"] = 2, ["odbijanie"] = 2, ["odbijania"] = 2, ["belching"] = 2,
            ["diarrhea"] = 3, ["diarrhoea"] = 3, ["biegunka"] = 3, ["rozwolnienie"] = 3,
            ["constipation"] = 4, ["zaparcie"] = 4, ["zaparcia"] = 4,
            ["bloating"] = 5, ["wzdęcia"] = 5, ["wzdecia"] = 5, ["wzdęcie"] = 5, ["wzdecie"] = 5,
            ["abdominal pain"] = 6, ["stomach pain"] = 6, ["belly pain"] = 6,
            ["ból brzucha"] = 6, ["bol brzucha"] = 6, ["brzuch"] = 6, ["ból"] = 6, ["bol"] = 6,
            ["heartburn"] = 7, ["zgaga"] = 7,
            ["gas"] = 8, ["gazy"] = 8, ["wiaty"] = 8,
            ["cramps"] = 9, ["skurcze"] = 9, ["skurcz"] = 9,
            ["vomiting"] = 10, ["wymioty"] = 10,
            ["appetite"] = 11, ["apetyt"] = 11,
            ["headache"] = 12, ["ból głowy"] = 12, ["bol glowy"] = 12, ["ból glowy"] = 12,
            ["fatigue"] = 13, ["zmęczenie"] = 13, ["zmeczenie"] = 13, ["tired"] = 13,
            ["mood"] = 14, ["nastrój"] = 14, ["nastroj"] = 14,
            ["energy"] = 15, ["energia"] = 15,
            ["sleep quality"] = 16, ["sleep"] = 16, ["jakość snu"] = 16, ["jakosc snu"] = 16, ["sen"] = 16,
            ["stress"] = 17, ["stres"] = 17,
            ["concentration"] = 18, ["koncentracja"] = 18,
            ["motivation"] = 19, ["motywacja"] = 19,
            ["physical activity"] = 20, ["aktywność fizyczna"] = 20, ["aktywnosc fizyczna"] = 20, ["ruch"] = 20,
            ["general well-being"] = 21, ["general wellbeing"] = 21, ["wellbeing"] = 21,
            ["ogólne samopoczucie"] = 21, ["ogolne samopoczucie"] = 21, ["samopoczucie"] = 21,
            ["brain fog"] = 22, ["mgła mózgowa"] = 22, ["mgla mozgowa"] = 22, ["mgła mozgowa"] = 22,
            ["dizziness"] = 23, ["zawroty głowy"] = 23, ["zawroty glowy"] = 23, ["lightheaded"] = 23,
            ["anxiety"] = 24, ["lęk"] = 24, ["lek"] = 24, ["niepokój"] = 24, ["niepokoj"] = 24,
            ["joint pain"] = 25, ["ból stawów"] = 25, ["bol stawow"] = 25, ["stawy"] = 25,
            ["itching"] = 26, ["itch"] = 26, ["swędzenie"] = 26, ["swedzenie"] = 26, ["świąd"] = 26, ["swiad"] = 26,
            ["urgency"] = 27, ["bowel urgency"] = 27, ["parcie"] = 27, ["parcie na stolec"] = 27,
            ["incomplete evacuation"] = 28, ["incomplete bowel movement"] = 28,
            ["uczucie niepełnego wypróżnienia"] = 28, ["niepelne wyproznienie"] = 28,
            ["mucus"] = 29, ["śluz"] = 29, ["sluz"] = 29,
            ["palpitations"] = 30, ["kołatanie serca"] = 30, ["kolatanie serca"] = 30, ["heart racing"] = 30,
            ["irritability"] = 31, ["drażliwość"] = 31, ["drazliwosc"] = 31, ["irritable"] = 31
        };

        public static int? ResolveUnitId(string? name) => Lookup(Units, name);

        public static int? ResolveSymptomTypeId(string? name) => Lookup(Symptoms, name);

        public static int ClampScale(int scale)
        {
            if (scale >= 0 && scale <= 5)
            {
                return scale;
            }

            if (scale >= 6 && scale <= 10)
            {
                return (int)Math.Round(scale / 2.0);
            }

            return Math.Clamp(scale, 0, 5);
        }

        private static int? Lookup(Dictionary<string, int> map, string? name)
        {
            var key = Normalize(name);
            if (key.Length == 0)
            {
                return null;
            }

            if (map.TryGetValue(key, out var id))
            {
                return id;
            }

            return null;
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim().Trim('.', ',', ';').ToLowerInvariant();
        }
    }
}
