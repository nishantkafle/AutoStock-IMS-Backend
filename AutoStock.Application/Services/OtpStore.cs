namespace AutoStock.Application.Services;

// Holds OTPs in memory with expiry time
public static class OtpStore
{
    private static readonly Dictionary<string, (string Otp, DateTime Expiry)> _store = new();

    public static void Save(string email, string otp)
    {
        _store[email] = (otp, DateTime.UtcNow.AddMinutes(10));
    }

    public static bool Verify(string email, string otp)
    {
        if (!_store.TryGetValue(email, out var entry)) return false;
        if (DateTime.UtcNow > entry.Expiry) return false;
        return entry.Otp == otp;
    }

    public static void Remove(string email)
    {
        _store.Remove(email);
    }
}