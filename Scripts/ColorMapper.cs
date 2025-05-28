using Godot;

public partial class ColorMapper : RefCounted
{
    public enum ColorScheme
    {
        Grayscale,
        Heat,
        Ocean,
        Rainbow,
        Neon,
        Plasma
    }
    
    public static Color MapValue(float value, ColorScheme scheme)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        
        return scheme switch
        {
            ColorScheme.Grayscale => new Color(value, value, value),
            ColorScheme.Heat => MapHeat(value),
            ColorScheme.Ocean => MapOcean(value),
            ColorScheme.Rainbow => MapRainbow(value),
            ColorScheme.Neon => MapNeon(value),
            ColorScheme.Plasma => MapPlasma(value),
            _ => new Color(value, value, value)
        };
    }
    
    private static Color MapHeat(float t)
    {
        if (t < 0.25f)
        {
            return new Color(0, 0, t * 4);
        }
        else if (t < 0.5f)
        {
            return new Color(0, (t - 0.25f) * 4, 1);
        }
        else if (t < 0.75f)
        {
            return new Color((t - 0.5f) * 4, 1, 1 - (t - 0.5f) * 4);
        }
        else
        {
            return new Color(1, 1 - (t - 0.75f) * 4, 0);
        }
    }
    
    private static Color MapOcean(float t)
    {
        var r = Mathf.Lerp(0.0f, 0.2f, t);
        var g = Mathf.Lerp(0.1f, 0.8f, t);
        var b = Mathf.Lerp(0.2f, 1.0f, t);
        return new Color(r, g, b);
    }
    
    private static Color MapRainbow(float t)
    {
        var hue = t * 360.0f;
        return Color.FromHsv(hue / 360.0f, 1.0f, 1.0f);
    }
    
    private static Color MapNeon(float t)
    {
        if (t < 0.5f)
        {
            var intensity = t * 2;
            return new Color(0, intensity, intensity * 0.5f);
        }
        else
        {
            var intensity = (t - 0.5f) * 2;
            return new Color(intensity, 1, 0.5f + intensity * 0.5f);
        }
    }
    
    private static Color MapPlasma(float t)
    {
        // Plasma colormap approximation
        var r = Mathf.Sin(Mathf.Pi * t) * 0.5f + 0.5f;
        var g = Mathf.Sin(Mathf.Pi * t + Mathf.Pi / 3) * 0.5f + 0.5f;
        var b = Mathf.Sin(Mathf.Pi * t + 2 * Mathf.Pi / 3) * 0.5f + 0.5f;
        
        return new Color(
            0.5f + 0.5f * r,
            0.2f + 0.6f * g,
            0.8f + 0.2f * b
        );
    }
    
    public static string GetSchemeName(ColorScheme scheme)
    {
        return scheme switch
        {
            ColorScheme.Grayscale => "Grayscale",
            ColorScheme.Heat => "Heat Map",
            ColorScheme.Ocean => "Ocean Blue",
            ColorScheme.Rainbow => "Rainbow",
            ColorScheme.Neon => "Neon Glow",
            ColorScheme.Plasma => "Plasma",
            _ => "Unknown"
        };
    }
}